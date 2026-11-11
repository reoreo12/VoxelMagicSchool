// 플레이어 데이터 유지·저장 단위 테스트: 저장 → JSON → 불러오기 결과가 같음, 모르는 ID 건너뛰기, 밭 보관·복원, 세션 보관소.
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Tests
{
    public class SaveSystemTests
    {
        private GameBalanceConfig _balance;
        private ProduceDefinition _radish;
        private SeedDefinition _radishSeed;
        private ToolDefinition _wand;
        private RequestDefinition _first;
        private RequestDefinition _second;
        private DefinitionCatalog _catalog;

        [SetUp]
        public void SetUp()
        {
            _balance = TestAssets.Create<GameBalanceConfig>(("_inventorySlotCount", 5), ("_maxScore", 100), ("_newItemKindBonus", 5));
            _radish = TestAssets.Produce("radish");
            _radishSeed = TestAssets.Seed("radish_seed", _radish, 1, 1);
            _wand = TestAssets.Wand();
            _first = TestAssets.Request("첫 의뢰", 10, new[] { new ItemAmount(_radish, 1) }, new ItemAmount[0]);
            _first.name = "Request_First";
            _second = TestAssets.Request("둘째 의뢰", 10, new[] { new ItemAmount(_radish, 2) }, new ItemAmount[0]);
            _second.name = "Request_Second";
            _catalog = TestAssets.Create<DefinitionCatalog>();
            _catalog.SetEntries(new ItemDefinition[] { _radish, _radishSeed, _wand }, new[] { _first, _second });
            GameSession.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            GameSession.Clear();
            TestAssets.DestroyAll();
        }

        private PlayerProgress NewProgress() => PlayerProgress.CreateNew(_balance, new[] { _first, _second });

        [Test]
        public void SaveThenLoadThroughJson_RestoresEverything()
        {
            var source = NewProgress();
            source.Inventory.TryAdd(_wand, 1);
            source.Inventory.TryAdd(_radish, 3);
            source.Inventory.Select(1);
            source.Requests.Accept(_first);
            source.Requests.Deliver(out _);
            source.Requests.Accept(_second);
            source.Score.AddPractical(7);
            var grid = new FarmGrid(6, new InstantGrowthRule());
            grid.TryPlant(_radishSeed, new Vector2Int(1, 2), out var watered);
            grid.Water(watered);
            grid.TryPlant(_radishSeed, new Vector2Int(3, 3), out _);
            source.CaptureFarm(grid);

            string json = JsonUtility.ToJson(SaveMapper.ToData(source));
            var loaded = NewProgress();
            var unknown = SaveMapper.Apply(JsonUtility.FromJson<SaveData>(json), loaded, _catalog);

            Assert.IsEmpty(unknown);
            for (int i = 0; i < source.Inventory.SlotCount; i++)
            {
                Assert.AreSame(source.Inventory.GetSlot(i).Item, loaded.Inventory.GetSlot(i).Item, $"칸 {i}");
                Assert.AreEqual(source.Inventory.GetSlot(i).Count, loaded.Inventory.GetSlot(i).Count, $"칸 {i}");
            }
            Assert.AreEqual(1, loaded.Inventory.SelectedIndex);
            Assert.AreEqual(source.Score.Written, loaded.Score.Written);
            Assert.AreEqual(7, loaded.Score.Practical);
            Assert.IsTrue(loaded.Journal.HasDelivered(_radish));
            Assert.AreEqual(RequestStatus.Completed, loaded.Requests.GetStatus(_first));
            Assert.AreSame(_second, loaded.Requests.Active);

            var restored = new FarmGrid(6, new InstantGrowthRule());
            loaded.RestoreFarm(restored);
            Assert.AreEqual(2, restored.Plots.Count);
            Assert.IsTrue(restored.GetPlotAt(new Vector2Int(1, 2)).IsWatered);
            Assert.IsFalse(restored.GetPlotAt(new Vector2Int(3, 3)).IsWatered);
        }

        [Test]
        public void Load_UnknownIds_AreSkippedAndReported()
        {
            var data = new SaveData();
            data.Slots.Add(new SlotRecord { ItemId = "radish", Count = 2 });
            data.Slots.Add(new SlotRecord { ItemId = "deleted_item", Count = 1 });
            data.CompletedRequestIds.Add("Request_Deleted");
            data.FarmPlots.Add(new FarmPlotRecord { SeedId = "deleted_seed" });
            var progress = NewProgress();

            var unknown = SaveMapper.Apply(data, progress, _catalog);

            CollectionAssert.AreEquivalent(new[] { "deleted_item", "Request_Deleted", "deleted_seed" }, unknown);
            Assert.AreSame(_radish, progress.Inventory.GetSlot(0).Item);
            Assert.IsTrue(progress.Inventory.GetSlot(1).IsEmpty);
            Assert.IsEmpty(progress.FarmPlots);
        }

        [Test]
        public void Load_RaisesInventoryEventsForChangedSlots()
        {
            var data = new SaveData();
            data.Slots.Add(new SlotRecord { ItemId = "wand", Count = 1 });
            data.SelectedSlot = 0;
            var progress = NewProgress();
            var changed = new List<int>();
            progress.Inventory.SlotChanged += changed.Add;

            SaveMapper.Apply(data, progress, _catalog);

            CollectionAssert.AreEqual(new[] { 0 }, changed);
        }

        [Test]
        public void Load_ActiveRequest_DoesNotGiveItemsAgain()
        {
            var given = TestAssets.Request("지급 의뢰", 5, new[] { new ItemAmount(_radish, 1) }, new[] { new ItemAmount(_radishSeed, 2) });
            given.name = "Request_Given";
            _catalog.SetEntries(new ItemDefinition[] { _radish, _radishSeed, _wand }, new[] { _first, _second, given });
            var data = new SaveData { ActiveRequestId = "Request_Given" };
            var progress = PlayerProgress.CreateNew(_balance, new[] { given });

            SaveMapper.Apply(data, progress, _catalog);

            Assert.AreSame(given, progress.Requests.Active);
            Assert.AreEqual(0, progress.Inventory.Count(_radishSeed));
        }

        [Test]
        public void Score_RestoreClampsToMax()
        {
            var data = new SaveData { PracticalScore = 500, WrittenScore = -3 };
            var progress = NewProgress();

            SaveMapper.Apply(data, progress, _catalog);

            Assert.AreEqual(100, progress.Score.Practical);
            Assert.AreEqual(0, progress.Score.Written);
        }

        [Test]
        public void GameSession_CaptureAsksSceneToStoreFarm()
        {
            var progress = NewProgress();
            GameSession.Begin(progress);
            var grid = new FarmGrid(6, new InstantGrowthRule());
            grid.TryPlant(_radishSeed, new Vector2Int(0, 0), out _);
            System.Action<PlayerProgress> capture = p => p.CaptureFarm(grid);
            GameSession.Capturing += capture;

            GameSession.Capture();
            GameSession.Capturing -= capture;

            Assert.IsTrue(GameSession.HasProgress);
            Assert.AreEqual(1, progress.FarmPlots.Count);
        }
    }
}
