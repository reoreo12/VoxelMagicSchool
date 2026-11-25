// 심는 방향 테스트: 바라보는 네 방향 판정, 앞 칸에서 바라보는 쪽으로 펼치기, 옆을 보면 가로세로가 바뀌는 차지 칸, 이미 물 준 작물 막기, 방향 저장·불러오기.
using NUnit.Framework;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Tests
{
    public class FarmFacingTests
    {
        private SeedDefinition _riceSeed;   // 1×2
        private SeedDefinition _potatoSeed; // 2×2
        private FarmGrid _grid;

        [SetUp]
        public void SetUp()
        {
            _riceSeed = TestAssets.Seed("riceSeed", TestAssets.Produce("rice"), 1, 2);
            _potatoSeed = TestAssets.Seed("potatoSeed", TestAssets.Produce("potato"), 2, 2);
            _grid = new FarmGrid(6, new InstantGrowthRule());
        }

        [TearDown] public void TearDown() => TestAssets.DestroyAll();

        [Test]
        public void 바라보는_방향은_가장_가까운_네_방향()
        {
            Assert.AreEqual(FarmFacing.Up, FarmPlacement.FromDirection(new Vector2(0.3f, 0.9f)));
            Assert.AreEqual(FarmFacing.Right, FarmPlacement.FromDirection(new Vector2(0.9f, -0.3f)));
            Assert.AreEqual(FarmFacing.Down, FarmPlacement.FromDirection(new Vector2(-0.2f, -1f)));
            Assert.AreEqual(FarmFacing.Left, FarmPlacement.FromDirection(new Vector2(-1f, 0.4f)));
        }

        [Test]
        public void 옆을_보면_가로세로가_바뀐다()
        {
            Assert.AreEqual(new Vector2Int(1, 2), FarmPlacement.RotatedSize(_riceSeed.Footprint, FarmFacing.Up));
            Assert.AreEqual(new Vector2Int(1, 2), FarmPlacement.RotatedSize(_riceSeed.Footprint, FarmFacing.Down));
            Assert.AreEqual(new Vector2Int(2, 1), FarmPlacement.RotatedSize(_riceSeed.Footprint, FarmFacing.Right));
            Assert.AreEqual(new Vector2Int(2, 1), FarmPlacement.RotatedSize(_riceSeed.Footprint, FarmFacing.Left));
        }

        [Test]
        public void 앞_칸에서_바라보는_쪽으로_펼친다()
        {
            var front = new Vector2Int(3, 3);
            // 1×2 벼: 위를 보면 앞 칸부터 위로, 아래를 보면 앞 칸부터 아래로
            Assert.AreEqual(new Vector2Int(3, 3), FarmPlacement.Origin(front, new Vector2Int(1, 2), FarmFacing.Up, true));
            Assert.AreEqual(new Vector2Int(3, 2), FarmPlacement.Origin(front, new Vector2Int(1, 2), FarmFacing.Down, true));
            // 옆으로 돌린 2×1: 오른쪽은 앞 칸부터 오른쪽, 왼쪽은 앞 칸부터 왼쪽
            Assert.AreEqual(new Vector2Int(3, 3), FarmPlacement.Origin(front, new Vector2Int(2, 1), FarmFacing.Right, true));
            Assert.AreEqual(new Vector2Int(2, 3), FarmPlacement.Origin(front, new Vector2Int(2, 1), FarmFacing.Left, true));
            // 2×2: 옆으로는 플레이어가 선 쪽 절반으로 붙임
            Assert.AreEqual(new Vector2Int(3, 3), FarmPlacement.Origin(front, new Vector2Int(2, 2), FarmFacing.Up, true));
            Assert.AreEqual(new Vector2Int(2, 3), FarmPlacement.Origin(front, new Vector2Int(2, 2), FarmFacing.Up, false));
            Assert.AreEqual(new Vector2Int(2, 2), FarmPlacement.Origin(front, new Vector2Int(2, 2), FarmFacing.Left, false));
        }

        [Test]
        public void 옆으로_돌려_심으면_가로_두_칸을_차지한다()
        {
            Assert.IsTrue(_grid.TryPlant(_riceSeed, new Vector2Int(1, 1), FarmFacing.Right, out var plot));

            Assert.AreEqual(new Vector2Int(2, 1), plot.Size);
            Assert.AreEqual(FarmFacing.Right, plot.Facing);
            Assert.AreSame(plot, _grid.GetPlotAt(new Vector2Int(2, 1)));
            Assert.IsNull(_grid.GetPlotAt(new Vector2Int(1, 2)));
        }

        [Test]
        public void 서비스는_앞_칸과_방향으로_심고_정사각형도_방향을_기억한다()
        {
            var inventory = new Inventory(5);
            inventory.TryAdd(_riceSeed, 1);
            inventory.TryAdd(_potatoSeed, 1);
            var service = new FarmService(_grid, inventory, new FixedRandom(0.5f), new GameMessages());

            var rice = service.Resolve(new Vector2Int(3, 0), _riceSeed, FarmFacing.Left, true);
            Assert.AreEqual(new Vector2Int(2, 0), rice.Origin);
            Assert.AreEqual(new Vector2Int(2, 1), rice.Size);
            Assert.IsTrue(service.Execute(rice));
            Assert.AreEqual(FarmFacing.Left, _grid.GetPlotAt(new Vector2Int(2, 0)).Facing);

            var potato = service.Resolve(new Vector2Int(4, 4), _potatoSeed, FarmFacing.Down, true);
            Assert.AreEqual(new Vector2Int(4, 3), potato.Origin);
            Assert.IsTrue(service.Execute(potato));
            Assert.AreEqual(FarmFacing.Down, _grid.GetPlotAt(new Vector2Int(5, 4)).Facing);
        }

        [Test]
        public void 막힌_자리는_빨간_미리보기용으로_이유를_준다()
        {
            var service = new FarmService(_grid, new Inventory(5), new FixedRandom(0.5f), new GameMessages());
            var edge = service.Resolve(new Vector2Int(5, 2), _riceSeed, FarmFacing.Right, true);
            Assert.IsFalse(edge.CanExecute);
            Assert.AreEqual(GameTexts.NotEnoughCells, edge.BlockedReason);
        }

        [Test]
        public void 이미_물_준_작물은_물_주기가_막힌다()
        {
            var wand = TestAssets.Wand();
            var timed = new FarmGrid(6, new TimedGrowthRule()); // 물을 줘도 바로 자라지 않는 규칙
            timed.TryPlant(_riceSeed, Vector2Int.zero, FarmFacing.Right, out var plot);
            var timedService = new FarmService(timed, new Inventory(5), new FixedRandom(0.5f), new GameMessages());

            var first = timedService.Resolve(new Vector2Int(1, 0), wand, FarmFacing.Left, true);
            Assert.AreEqual(FarmActionType.Water, first.Type);
            Assert.AreEqual(new Vector2Int(2, 1), first.Size, "물 주기 대상은 작물이 차지한 칸 전체");
            Assert.IsTrue(timedService.Execute(first));

            var again = timedService.Resolve(Vector2Int.zero, wand, FarmFacing.Up, true);
            Assert.AreEqual(FarmActionType.Water, again.Type);
            Assert.IsFalse(again.CanExecute);
            Assert.AreEqual(GameTexts.AlreadyWatered, again.BlockedReason);
        }

        [Test]
        public void 심은_방향은_저장하고_되살린다()
        {
            var balance = TestAssets.Create<GameBalanceConfig>(("_inventorySlotCount", 5));
            var catalog = TestAssets.Create<DefinitionCatalog>();
            catalog.SetEntries(new ItemDefinition[] { _riceSeed, _riceSeed.Produce }, new RequestDefinition[0]);
            var source = PlayerProgress.CreateNew(balance);
            _grid.TryPlant(_riceSeed, new Vector2Int(1, 1), FarmFacing.Left, out _);
            source.CaptureFarm(_grid);

            var json = JsonUtility.ToJson(SaveMapper.ToData(source));
            var loaded = PlayerProgress.CreateNew(balance);
            SaveMapper.Apply(JsonUtility.FromJson<SaveData>(json), loaded, catalog);
            var restored = new FarmGrid(6, new InstantGrowthRule());
            loaded.RestoreFarm(restored);

            var plot = restored.GetPlotAt(new Vector2Int(2, 1));
            Assert.IsNotNull(plot);
            Assert.AreEqual(FarmFacing.Left, plot.Facing);
            Assert.AreEqual(new Vector2Int(2, 1), plot.Size);
        }
    }
}
