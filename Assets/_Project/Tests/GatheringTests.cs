// 뒷산 채집 배치 규칙 테스트: 자기 서식지·시간대만, 다시 배치하면 슬롯 이동, 의뢰 재료 보장, 줍기와 누적.
using System.Collections.Generic;
using NUnit.Framework;
using VoxelMagic.Core;

namespace VoxelMagic.Tests
{
    public sealed class GatheringTests
    {
        [TearDown] public void TearDown() => TestAssets.DestroyAll();

        private static GatherMaterialDefinition Mat(string id, GatherHabitatKind kind, bool afterSchool, bool night)
            => TestAssets.Create<GatherMaterialDefinition>(("_id", id), ("_displayName", id), ("_habitat", kind),
                ("_appearsAfterSchool", afterSchool), ("_appearsAtNight", night), ("_sizeRangeCm", new FloatRange(3f, 5f)));

        private static GatherSpawner Spawner(params (GatherHabitatKind kind, int slots)[] habitats)
        {
            var kinds = new List<GatherHabitatKind>();
            var counts = new List<int>();
            foreach (var (kind, slots) in habitats) { kinds.Add(kind); counts.Add(slots); }
            return new GatherSpawner(kinds, counts);
        }

        [Test]
        public void Respawn_PlacesOnlyOwnHabitatAndCurrentTime()
        {
            var shade = Mat("shade", GatherHabitatKind.TreeShade, true, false);
            var moon = Mat("moon", GatherHabitatKind.TreeShade, false, true);
            var berry = Mat("berry", GatherHabitatKind.Bush, true, false);
            var spawner = Spawner((GatherHabitatKind.TreeShade, 4), (GatherHabitatKind.Bush, 3));

            spawner.Respawn(new[] { shade, moon, berry }, TimeOfDay.Day, null, 1f, new FixedRandom(0f));

            for (int s = 0; s < 4; s++) Assert.AreEqual(shade, spawner.Get(0, s));
            for (int s = 0; s < 3; s++) Assert.AreEqual(berry, spawner.Get(1, s));
            Assert.AreEqual(0, spawner.CountOf(moon));
        }

        [Test]
        public void Respawn_AtNight_HasNoAfterSchoolOnly()
        {
            var shade = Mat("shade", GatherHabitatKind.TreeShade, true, false);
            var moon = Mat("moon", GatherHabitatKind.TreeShade, false, true);
            var spawner = Spawner((GatherHabitatKind.TreeShade, 4));

            spawner.Respawn(new[] { shade, moon }, TimeOfDay.Night, null, 1f, new FixedRandom(0f));

            Assert.AreEqual(0, spawner.CountOf(shade));
            Assert.AreEqual(4, spawner.CountOf(moon));
        }

        [Test]
        public void Respawn_MovesToPreviouslyEmptySlots()
        {
            var shade = Mat("shade", GatherHabitatKind.TreeShade, true, false);
            var spawner = Spawner((GatherHabitatKind.TreeShade, 4));
            var random = new FixedRandom(0f);

            spawner.Respawn(new[] { shade }, TimeOfDay.Day, null, 0.5f, random);
            var first = new List<int>();
            for (int s = 0; s < 4; s++) if (spawner.Get(0, s) != null) first.Add(s);
            spawner.Respawn(new[] { shade }, TimeOfDay.Day, null, 0.5f, random);

            Assert.AreEqual(2, first.Count);
            foreach (int s in first) Assert.IsNull(spawner.Get(0, s), $"슬롯 {s}에 그대로 남음");
        }

        [Test]
        public void Respawn_GuaranteesRequiredMaterial()
        {
            var common = Mat("common", GatherHabitatKind.Bush, true, false);
            var needed = Mat("needed", GatherHabitatKind.Bush, true, false);
            var spawner = Spawner((GatherHabitatKind.Bush, 3));

            // FixedRandom(0)은 항상 첫 후보(common)를 고른다 → 보장 규칙이 없으면 needed는 0개.
            spawner.Respawn(new[] { common, needed }, TimeOfDay.Day, new ItemDefinition[] { needed }, 0.34f, new FixedRandom(0f));

            Assert.GreaterOrEqual(spawner.CountOf(needed), 1);
        }

        [Test]
        public void AcceptingRequest_PlacesMissingMaterialWithoutRespawn()
        {
            var common = Mat("common", GatherHabitatKind.Bush, true, false);
            var needed = Mat("needed", GatherHabitatKind.Bush, true, false);
            var config = TestAssets.Create<GatheringConfig>(("_fillRatio", 0.34f));
            var inventory = new Inventory(5);
            var tracker = new RequestTracker(new RequestDefinition[0], inventory, new ScoreBook(100), new MaterialJournal(), 0);
            var service = new GatheringService(Spawner((GatherHabitatKind.Bush, 3)), new[] { common, needed }, config,
                new TimeOfDayState(TimeOfDay.Day), inventory, new GameMessages(), tracker, new FixedRandom(0f));
            service.Respawn();
            Assert.AreEqual(0, service.CountOf(needed));

            tracker.Accept(TestAssets.Request("r", 10, new[] { new ItemAmount(needed, 1) }, new ItemAmount[0]));

            Assert.AreEqual(1, service.CountOf(needed));
            Assert.AreEqual(1, service.CountOf(common), "다른 재료는 그대로");
        }

        [Test]
        public void Abandon_ClearsActiveRequest()
        {
            var inventory = new Inventory(5);
            var tracker = new RequestTracker(new RequestDefinition[0], inventory, new ScoreBook(100), new MaterialJournal(), 0);
            var request = TestAssets.Request("r", 10, new[] { new ItemAmount(TestAssets.Wand(), 1) }, new ItemAmount[0]);
            tracker.Accept(request);

            tracker.Abandon();

            Assert.IsFalse(tracker.HasActive);
            Assert.AreEqual(RequestStatus.Available, tracker.GetStatus(request));
        }

        [Test]
        public void DiscardSlot_EmptiesOnlyThatSlot()
        {
            var inventory = new Inventory(5);
            var a = TestAssets.Produce("a");
            var b = TestAssets.Produce("b");
            inventory.TryAdd(a, 3);
            inventory.TryAdd(b, 2);

            var removed = inventory.DiscardSlot(0);

            Assert.AreEqual(a, removed.Item);
            Assert.AreEqual(3, removed.Count);
            Assert.IsTrue(inventory.GetSlot(0).IsEmpty);
            Assert.AreEqual(2, inventory.Count(b));
        }

        [Test]
        public void Pick_AddsToInventoryAndStacks()
        {
            var berry = Mat("berry", GatherHabitatKind.Bush, true, false);
            var config = TestAssets.Create<GatheringConfig>(("_fillRatio", 1f));
            var inventory = new Inventory(5);
            var messages = new GameMessages();
            int acquired = 0;
            messages.ItemAcquired += _ => acquired++;
            var service = new GatheringService(Spawner((GatherHabitatKind.Bush, 3)), new[] { berry }, config,
                new TimeOfDayState(TimeOfDay.Day), inventory, messages, null, new FixedRandom(0f));

            service.Respawn();
            Assert.IsTrue(service.TryPick(0, 0));
            Assert.IsTrue(service.TryPick(0, 1));

            Assert.AreEqual(2, inventory.Count(berry));
            Assert.AreEqual(2, inventory.GetSlot(0).Count, "같은 종류는 한 칸에 누적");
            Assert.IsNull(service.Get(0, 0));
            Assert.IsFalse(service.TryPick(0, 0), "빈 슬롯은 못 줍는다");
            Assert.AreEqual(2, acquired);
        }
    }
}
