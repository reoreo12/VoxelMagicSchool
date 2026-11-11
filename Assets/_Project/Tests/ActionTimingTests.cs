// 동작 애니메이션과 맞물리는 Core 흐름 단위 테스트: 낚시 "놓는 순간" 기다리기·자동 놓기, 물 주기 동작(시작 → 물 들어감 → 끝, 중복·이벤트 누락 대비).
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Tests
{
    public class ActionTimingTests
    {
        private const float Flight = 1f;
        private const float Timeout = 3f;

        private FishingSession _session;
        private List<FishingState> _states;
        private int _released;

        [SetUp]
        public void SetUp()
        {
            var fish = TestAssets.Fish("crucian");
            var config = TestAssets.Create<FishingConfig>(
                ("_castSeconds", Flight),
                ("_castReleaseTimeoutSeconds", Timeout),
                ("_biteWaitSeconds", new FloatRange(2f, 6f)),
                ("_biteWindowSeconds", 1f),
                ("_pondFish", new List<FishWeight> { new FishWeight(fish, 1f) }));
            _session = new FishingSession(config, new Inventory(5), new FixedRandom(0.5f), new GameMessages(), 3f);
            _states = new List<FishingState>();
            _released = 0;
            _session.StateChanged += _states.Add;
            _session.CastReleased += () => _released++;
        }

        [TearDown] public void TearDown() => TestAssets.DestroyAll();

        private void Run(float seconds, float step = 0.05f)
        {
            for (float t = 0f; t < seconds; t += step) _session.Tick(step);
        }

        [Test]
        public void WithoutAnimation_ReleasesImmediately()
        {
            _session.TryCast();

            Assert.AreEqual(1, _released);
            Run(Flight + 0.01f);
            Assert.AreEqual(FishingState.Waiting, _session.State);
        }

        [Test]
        public void WithAnimation_WaitsForReleaseThenFlies()
        {
            _session.WaitForCastRelease = true;
            _session.TryCast();
            Run(Timeout * 0.5f);
            Assert.AreEqual(FishingState.Casting, _session.State);
            Assert.AreEqual(0, _released);

            _session.ReleaseCast();
            _session.ReleaseCast();   // 두 번 불러도 한 번만
            Assert.AreEqual(1, _released);
            Run(Flight - 0.1f);
            Assert.AreEqual(FishingState.Casting, _session.State);
            Run(0.2f);
            Assert.AreEqual(FishingState.Waiting, _session.State);
        }

        [Test]
        public void WithAnimation_NoEvent_ReleasesAfterTimeout()
        {
            _session.WaitForCastRelease = true;
            _session.TryCast();
            Run(Timeout + 0.01f);
            Assert.AreEqual(1, _released);
            Run(Flight + 0.01f);
            Assert.AreEqual(FishingState.Waiting, _session.State);
        }

        [Test]
        public void ReleaseOutsideCasting_IsIgnored()
        {
            _session.ReleaseCast();
            Assert.AreEqual(0, _released);
            Assert.AreEqual(FishingState.Idle, _session.State);
        }

        // ── 물 주기 ──

        private (FarmService service, FarmGrid grid, FarmAction water) Farm(bool useAction)
        {
            var grid = new FarmGrid(6, new InstantGrowthRule());
            var inventory = new Inventory(5);
            var wand = TestAssets.Wand();
            inventory.TryAdd(wand, 1);
            var seed = TestAssets.Seed("onion_seed", TestAssets.Produce("onion"), 1, 1);
            grid.TryPlant(seed, new Vector2Int(1, 1), out _);
            var service = new FarmService(grid, inventory, new FixedRandom(0.5f), new GameMessages()) { UseWateringAction = useAction };
            return (service, grid, service.Resolve(new Vector2Int(1, 1), wand));
        }

        [Test]
        public void Watering_WithoutAction_IsInstant()
        {
            var (service, grid, water) = Farm(false);
            Assert.IsTrue(service.Execute(water));
            Assert.IsTrue(grid.GetPlotAt(new Vector2Int(1, 1)).IsWatered);
            Assert.IsFalse(service.IsWatering);
        }

        [Test]
        public void Watering_WithAction_WaterGoesInAtEventAndEndsOnFinish()
        {
            var (service, grid, water) = Farm(true);
            FarmPlot started = null;
            int finished = 0;
            service.WateringStarted += p => started = p;
            service.WateringFinished += () => finished++;
            var plot = grid.GetPlotAt(new Vector2Int(1, 1));

            Assert.IsTrue(service.Execute(water));
            Assert.AreSame(plot, started);
            Assert.IsTrue(service.IsWatering);
            Assert.IsFalse(plot.IsWatered);
            Assert.IsFalse(service.Execute(water), "물 주는 중에는 또 시작하지 않는다");

            service.ApplyPendingWater();
            Assert.IsTrue(plot.IsWatered);
            service.FinishWatering();
            Assert.IsFalse(service.IsWatering);
            Assert.AreEqual(1, finished);
        }

        [Test]
        public void Watering_FinishWithoutEvent_StillWaters()
        {
            var (service, grid, water) = Farm(true);
            service.Execute(water);
            service.FinishWatering();
            Assert.IsTrue(grid.GetPlotAt(new Vector2Int(1, 1)).IsWatered);
            Assert.IsFalse(service.IsWatering);
        }
    }
}
