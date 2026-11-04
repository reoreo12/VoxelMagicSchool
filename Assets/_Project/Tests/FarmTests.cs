// 텃밭 단위 테스트: 칸 크기별 배치, 격자 밖·겹침 판정, 물 주기 성장, 뽑기.
using NUnit.Framework;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Tests
{
    public class FarmTests
    {
        private ProduceDefinition _onion;
        private SeedDefinition _radishSeed;
        private SeedDefinition _onionSeed;
        private SeedDefinition _pumpkinSeed;
        private FarmGrid _grid;

        [SetUp]
        public void SetUp()
        {
            _onion = TestAssets.Produce("onion", 40f, 70f);
            _radishSeed = TestAssets.Seed("radishSeed", TestAssets.Produce("radish"), 1, 1);
            _onionSeed = TestAssets.Seed("onionSeed", _onion, 1, 2);
            _pumpkinSeed = TestAssets.Seed("pumpkinSeed", TestAssets.Produce("pumpkin"), 2, 2);
            _grid = new FarmGrid(6, new InstantGrowthRule());
        }

        [TearDown] public void TearDown() => TestAssets.DestroyAll();

        [Test]
        public void CanPlace_FootprintPastEdge_IsOutOfBounds()
        {
            Assert.AreEqual(PlacementResult.Ok, _grid.CanPlace(new Vector2Int(5, 5), _radishSeed.Footprint));
            Assert.AreEqual(PlacementResult.OutOfBounds, _grid.CanPlace(new Vector2Int(5, 5), _onionSeed.Footprint));
            Assert.AreEqual(PlacementResult.OutOfBounds, _grid.CanPlace(new Vector2Int(-1, 0), _radishSeed.Footprint));
        }

        [Test]
        public void TryPlant_OccupiesWholeFootprint()
        {
            Assert.IsTrue(_grid.TryPlant(_pumpkinSeed, new Vector2Int(1, 1), out var plot));

            Assert.AreSame(plot, _grid.GetPlotAt(new Vector2Int(1, 1)));
            Assert.AreSame(plot, _grid.GetPlotAt(new Vector2Int(2, 2)));
            Assert.IsNull(_grid.GetPlotAt(new Vector2Int(3, 3)));
        }

        [Test]
        public void CanPlace_OverlappingExistingPlot_IsOccupied()
        {
            _grid.TryPlant(_pumpkinSeed, new Vector2Int(1, 1), out _);

            Assert.AreEqual(PlacementResult.Occupied, _grid.CanPlace(new Vector2Int(2, 0), _onionSeed.Footprint));
            Assert.AreEqual(PlacementResult.Ok, _grid.CanPlace(new Vector2Int(3, 0), _onionSeed.Footprint));
        }

        [Test]
        public void Water_GrowsInstantly_OnlyOnce()
        {
            _grid.TryPlant(_radishSeed, Vector2Int.zero, out var plot);

            Assert.IsTrue(_grid.Water(plot));
            Assert.AreEqual(GrowthStage.Grown, plot.Stage);
            Assert.IsFalse(_grid.Water(plot));
        }

        [Test]
        public void Remove_FreesCells()
        {
            _grid.TryPlant(_onionSeed, Vector2Int.zero, out var plot);

            Assert.IsTrue(_grid.Remove(plot));
            Assert.IsNull(_grid.GetPlotAt(new Vector2Int(0, 1)));
            Assert.AreEqual(PlacementResult.Ok, _grid.CanPlace(Vector2Int.zero, _onionSeed.Footprint));
        }

        [Test]
        public void FarmService_PlantWaterHarvest_FullFlow()
        {
            var inventory = new Inventory(5);
            var wand = TestAssets.Wand();
            var messages = new GameMessages();
            var service = new FarmService(_grid, inventory, new FixedRandom(0.5f), messages);
            inventory.TryAdd(wand, 1);
            inventory.TryAdd(_onionSeed, 2);
            AcquiredInfo? acquired = null;
            messages.ItemAcquired += info => acquired = info;
            var cell = new Vector2Int(2, 2);

            var plant = service.Resolve(cell, _onionSeed);
            Assert.AreEqual(FarmActionType.Plant, plant.Type);
            Assert.IsTrue(service.Execute(plant));
            Assert.AreEqual(1, inventory.Count(_onionSeed));

            var blocked = service.Resolve(cell, _onionSeed);
            Assert.IsFalse(blocked.CanExecute);
            Assert.AreEqual(GameTexts.AlreadyPlanted, blocked.BlockedReason);

            var water = service.Resolve(cell, wand);
            Assert.AreEqual(FarmActionType.Water, water.Type);
            Assert.IsTrue(service.Execute(water));

            var harvest = service.Resolve(new Vector2Int(2, 3), wand);
            Assert.AreEqual(FarmActionType.Harvest, harvest.Type);
            Assert.IsTrue(service.Execute(harvest));

            Assert.AreEqual(1, inventory.Count(_onion));
            Assert.IsNull(_grid.GetPlotAt(cell));
            Assert.IsTrue(acquired.HasValue);
            Assert.AreEqual(55f, acquired.Value.SizeCm, 0.001f);
        }

        [Test]
        public void FarmService_SproutWithoutWand_ExplainsWhy()
        {
            var service = new FarmService(_grid, new Inventory(5), new FixedRandom(0.5f), new GameMessages());
            _grid.TryPlant(_radishSeed, Vector2Int.zero, out _);

            var action = service.Resolve(Vector2Int.zero, null);

            Assert.IsFalse(action.CanExecute);
            Assert.AreEqual(GameTexts.NeedWandToWater, action.BlockedReason);
        }
    }
}
