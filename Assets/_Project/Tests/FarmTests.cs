// 텃밭 단위 테스트: 칸 크기별 배치, 격자 밖·겹침 판정, 물 주기 성장(튜토리얼 즉시 / 기본 하루 단위), 뽑기, 하루 시작 성장 단계.
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
        public void 기본_규칙은_물_준_뒤_게임_시간이_지나면_자라고_희귀는_다음_날_아침에_자란다()
        {
            float now = 16 * 60f;
            var grid = new FarmGrid(6, new TimedGrowthRule());
            grid.SetClock(() => now);
            var common = TestAssets.Create<SeedDefinition>(("_id", "c"), ("_produce", TestAssets.Produce("cp")), ("_footprint", Vector2Int.one), ("_growHours", 2f));
            var uncommon = TestAssets.Create<SeedDefinition>(("_id", "u"), ("_produce", TestAssets.Produce("up")), ("_footprint", Vector2Int.one), ("_growHours", 4f));
            var rare = TestAssets.Create<SeedDefinition>(("_id", "r"), ("_produce", TestAssets.Produce("rp")), ("_footprint", Vector2Int.one), ("_growHours", 0f));
            grid.TryPlant(common, Vector2Int.zero, out var a);
            grid.TryPlant(uncommon, new Vector2Int(1, 0), out var b);
            grid.TryPlant(rare, new Vector2Int(2, 0), out var c);
            grid.TryPlant(common, new Vector2Int(3, 0), out var dry);
            grid.Water(a);
            grid.Water(b);
            grid.Water(c);
            Assert.AreEqual(GrowthStage.Sprout, a.Stage, "물만 줘서는 바로 자라지 않는다");

            now = 17 * 60f + 59f;
            grid.UpdateGrowth();
            Assert.IsFalse(a.IsGrown);
            now = 18 * 60f;
            grid.UpdateGrowth();
            Assert.IsTrue(a.IsGrown, "흔함 2시간 뒤");
            Assert.IsFalse(b.IsGrown);
            now = 20 * 60f;
            grid.UpdateGrowth();
            Assert.IsTrue(b.IsGrown, "보통 4시간 뒤");
            now = 23 * 60f + 59f;
            grid.UpdateGrowth();
            Assert.IsFalse(c.IsGrown, "희귀는 그날 안에는 안 자람");

            now = 24 * 60f + 16 * 60f; // 다음 날 16:00
            grid.AdvanceDay();
            Assert.IsTrue(c.IsGrown, "희귀는 다음 날 하루 시작에 자람");
            Assert.IsFalse(dry.IsGrown, "물을 안 준 작물은 자라지 않는다");
        }

        [Test]
        public void 밤늦게_물_준_흔한_작물은_다음_날_시작에_자라_있다()
        {
            float now = 23 * 60f + 30f;
            var grid = new FarmGrid(6, new TimedGrowthRule());
            grid.SetClock(() => now);
            grid.TryPlant(_radishSeed, Vector2Int.zero, out var plot);
            grid.Water(plot);
            now = 24 * 60f + 16 * 60f;
            var step = new CropGrowthStep(grid);
            step.Run(2, DayKind.Free, DayStartReason.NextDay);
            Assert.IsTrue(plot.IsGrown);
        }

        [Test]
        public void 하루_시작_단계는_새_게임_첫날에는_키우지_않는다()
        {
            var grid = new FarmGrid(6, new TimedGrowthRule());
            var rare = TestAssets.Create<SeedDefinition>(("_id", "r"), ("_produce", TestAssets.Produce("rp")), ("_footprint", Vector2Int.one), ("_growHours", 0f));
            grid.TryPlant(rare, Vector2Int.zero, out var plot);
            grid.Water(plot);
            var step = new CropGrowthStep(grid);
            step.Run(1, DayKind.Story, DayStartReason.NewGame);
            Assert.IsFalse(plot.IsGrown);
            step.Run(2, DayKind.Free, DayStartReason.NextDay);
            Assert.IsTrue(plot.IsGrown);
        }

        [Test]
        public void 되살리기는_물_준_시각과_단계를_그대로_둔다()
        {
            float now = 17 * 60f;
            var grid = new FarmGrid(6, new TimedGrowthRule());
            grid.SetClock(() => now);
            Assert.IsTrue(grid.TryRestore(_radishSeed, Vector2Int.zero, true, 16 * 60f, false, out var sprout));
            Assert.IsTrue(grid.TryRestore(_radishSeed, new Vector2Int(1, 0), true, 10f, true, out var grown));
            Assert.IsFalse(sprout.IsGrown);
            Assert.AreEqual(16 * 60f, sprout.WateredAt);
            Assert.IsTrue(grown.IsGrown);
            now = 16 * 60f + _radishSeed.GrowHours * 60f; // 물 준 시각 + 씨앗 성장 시간
            grid.UpdateGrowth();
            Assert.IsTrue(sprout.IsGrown);
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
