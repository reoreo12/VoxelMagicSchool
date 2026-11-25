// 채집 규칙 테스트(QUEST_TIMER 3-2, 완료 기준 7): 서식지 종류마다 자리 8곳 중 4곳만 켜짐, 자기 서식지 재료만, 희귀도 비율 60/30/10,
// 보장 재료, 씬 슬롯 → 자리 번호 잇기, 줍기와 하루 유지. 그리고 인벤토리 칸 비우기.
using System.Collections.Generic;
using NUnit.Framework;
using VoxelMagic.Core;

namespace VoxelMagic.Tests
{
    public sealed class GatheringTests
    {
        [TearDown] public void TearDown() => TestAssets.DestroyAll();

        private static GatherMaterialDefinition Mat(string id, GatherHabitatKind kind, ItemRarity rarity)
            => TestAssets.Create<GatherMaterialDefinition>(("_id", id), ("_displayName", id), ("_habitat", kind),
                ("_rarity", rarity), ("_source", ItemSource.Gathering), ("_sizeRangeCm", new FloatRange(3f, 5f)));

        private static GatheringConfig Config(int spots, int active, RarityTable rarity, params GatherMaterialDefinition[] materials)
        {
            var config = TestAssets.Create<GatheringConfig>();
            config.Set(spots, active, rarity, materials);
            return config;
        }

        [Test]
        public void 종류마다_켜지는_자리_수만큼만_자기_서식지_재료가_놓인다()
        {
            var shade = Mat("shade", GatherHabitatKind.TreeShade, ItemRarity.Common);
            var berry = Mat("berry", GatherHabitatKind.Bush, ItemRarity.Common);
            var config = Config(8, 4, RarityTable.Default, shade, berry);

            var random = new SystemRandom(1);
            for (int run = 0; run < 50; run++)
            {
                var spots = GatherDayRoller.Roll(config, random);
                Assert.AreEqual(4, Count(spots[GatherHabitatKind.TreeShade], shade));
                Assert.AreEqual(4, Count(spots[GatherHabitatKind.Bush], berry));
                Assert.AreEqual(0, Count(spots[GatherHabitatKind.Bush], shade));
                Assert.AreEqual(0, Count(spots[GatherHabitatKind.RockCrevice], null) - 8, "재료가 없는 서식지는 비어 있다");
            }
        }

        [Test]
        public void 켜지는_자리는_매번_바뀐다()
        {
            var shade = Mat("shade", GatherHabitatKind.TreeShade, ItemRarity.Common);
            var config = Config(8, 4, RarityTable.Default, shade);
            var random = new SystemRandom(7);
            var seen = new HashSet<int>();
            for (int run = 0; run < 30; run++)
            {
                var slots = GatherDayRoller.Roll(config, random)[GatherHabitatKind.TreeShade];
                for (int i = 0; i < slots.Length; i++) if (slots[i] != null) seen.Add(i);
            }
            Assert.AreEqual(8, seen.Count, "여러 날 동안 8자리가 모두 한 번은 켜진다");
        }

        [Test]
        public void 희귀도_비율은_설정_값을_따른다()
        {
            var common = Mat("c", GatherHabitatKind.Clearing, ItemRarity.Common);
            var uncommon = Mat("u", GatherHabitatKind.Clearing, ItemRarity.Uncommon);
            var rare = Mat("r", GatherHabitatKind.Clearing, ItemRarity.Rare);
            var config = Config(8, 4, RarityTable.Default, common, uncommon, rare);
            var random = new SystemRandom(42);

            int c = 0, u = 0, r = 0, total = 0;
            for (int run = 0; run < 5000; run++)
            {
                foreach (var placed in GatherDayRoller.Roll(config, random)[GatherHabitatKind.Clearing])
                {
                    if (placed == null) continue;
                    total++;
                    if (placed == common) c++;
                    else if (placed == uncommon) u++;
                    else if (placed == rare) r++;
                }
            }
            Assert.AreEqual(0.60f, c / (float)total, 0.02f);
            Assert.AreEqual(0.30f, u / (float)total, 0.02f);
            Assert.AreEqual(0.10f, r / (float)total, 0.015f);
        }

        [Test]
        public void 그_희귀도_재료가_없으면_낮은_희귀도로_대신한다()
        {
            var common = Mat("c", GatherHabitatKind.Bush, ItemRarity.Common);
            var config = Config(8, 4, new RarityTable(0f, 0f, 1f), common); // 늘 희귀를 뽑지만 희귀 재료가 없음
            var slots = GatherDayRoller.Roll(config, new SystemRandom(3))[GatherHabitatKind.Bush];
            Assert.AreEqual(4, Count(slots, common));
        }

        [Test]
        public void 보장_재료는_반드시_놓인다()
        {
            var common = Mat("c", GatherHabitatKind.Bush, ItemRarity.Common);
            var tutorial = Mat("t", GatherHabitatKind.Bush, ItemRarity.Rare);
            var config = Config(8, 4, new RarityTable(1f, 0f, 0f), common, tutorial);
            for (int seed = 0; seed < 20; seed++)
            {
                var slots = GatherDayRoller.Roll(config, new SystemRandom(seed), new[] { tutorial })[GatherHabitatKind.Bush];
                Assert.AreEqual(1, Count(slots, tutorial));
                Assert.AreEqual(4, 8 - Count(slots, null), "켜진 자리 수는 그대로");
            }
        }

        [Test]
        public void 씬_슬롯은_같은_종류_서식지에_번갈아_자리_번호가_된다_넘치는_슬롯은_비어_있다()
        {
            var shade = Mat("shade", GatherHabitatKind.TreeShade, ItemRarity.Common);
            var day = new GatherDayState(8);
            var spots = new Dictionary<GatherHabitatKind, GatherMaterialDefinition[]>
            {
                { GatherHabitatKind.TreeShade, new GatherMaterialDefinition[8] }
            };
            spots[GatherHabitatKind.TreeShade][1] = shade;
            spots[GatherHabitatKind.TreeShade][7] = shade;
            day.Set(2, spots);

            // 서식지: 그늘0(3칸) · 덤불(3칸) · 그늘1(3칸) · 그늘2(3칸)
            // 그늘 자리: 0번 슬롯 → 0,1,2 / 1번 슬롯 → 3,4,5 / 2번 슬롯 → 6,7,8(8은 없음)
            var kinds = new[] { GatherHabitatKind.TreeShade, GatherHabitatKind.Bush, GatherHabitatKind.TreeShade, GatherHabitatKind.TreeShade };
            var service = new GatheringService(day, kinds, new[] { 3, 3, 3, 3 }, new Inventory(5), new GameMessages(), new FixedRandom(0f));

            Assert.AreEqual(shade, service.Get(2, 0), "그늘1의 0번 슬롯 = 자리 1");
            Assert.AreEqual(shade, service.Get(2, 2), "그늘1의 2번 슬롯 = 자리 7");
            Assert.AreEqual(-1, service.SpotOf(3, 2), "그늘2의 2번 슬롯 = 자리 8, 설정(8곳) 밖이라 쓰지 않는다");
            Assert.AreEqual(0, service.SpotOf(1, 0), "덤불은 따로 센다");
            Assert.AreEqual(9, service.SceneSpotCounts[GatherHabitatKind.TreeShade]);
        }

        [Test]
        public void 주우면_가방에_들어가고_그날은_다시_생기지_않는다()
        {
            var berry = Mat("berry", GatherHabitatKind.Bush, ItemRarity.Common);
            var day = new GatherDayState(8);
            var spots = new Dictionary<GatherHabitatKind, GatherMaterialDefinition[]> { { GatherHabitatKind.Bush, new GatherMaterialDefinition[8] } };
            spots[GatherHabitatKind.Bush][0] = berry;
            spots[GatherHabitatKind.Bush][1] = berry;
            day.Set(2, spots);
            var inventory = new Inventory(5);
            var service = new GatheringService(day, new[] { GatherHabitatKind.Bush }, new[] { 3 }, inventory, new GameMessages(), new FixedRandom(0f));

            Assert.IsTrue(service.TryPick(0, 0));
            Assert.IsTrue(service.TryPick(0, 1));
            Assert.IsFalse(service.TryPick(0, 0));
            Assert.AreEqual(2, inventory.Count(berry));
            Assert.AreEqual(0, day.CountOf(berry));

            // 다른 씬에서 다시 연결해도(서비스 새로) 주운 자리는 그대로 비어 있다
            var again = new GatheringService(day, new[] { GatherHabitatKind.Bush }, new[] { 3 }, inventory, new GameMessages(), new FixedRandom(0f));
            Assert.IsNull(again.Get(0, 0));
        }

        [Test]
        public void 하루_시작_단계가_그날_자리를_뽑는다()
        {
            var shade = Mat("shade", GatherHabitatKind.TreeShade, ItemRarity.Common);
            var config = Config(8, 4, RarityTable.Default, shade);
            var day = new GatherDayState(8);
            var step = new GatherRespawnStep(day, config, new SystemRandom(5));
            step.Run(3, DayKind.Free, DayStartReason.NextDay);
            Assert.AreEqual(3, day.Day);
            Assert.AreEqual(4, day.CountOf(shade));
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

        private static int Count(GatherMaterialDefinition[] slots, GatherMaterialDefinition material)
        {
            int n = 0;
            foreach (var s in slots) if (s == material) n++;
            return n;
        }
    }
}
