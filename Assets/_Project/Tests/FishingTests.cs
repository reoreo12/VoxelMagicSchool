// 낚시 상태 머신 단위 테스트: 상태 전이, 입질 놓침, 미니게임 성공·실패, 취소, 결과 중 다시 던지기.
using System.Collections.Generic;
using NUnit.Framework;
using VoxelMagic.Core;

namespace VoxelMagic.Tests
{
    public class FishingTests
    {
        // FixedRandom(0.5) 기준: 던지기 1초, 대기 2~6초 → 4초, 입질 1초
        private const float Cast = 1f;
        private const float Wait = 4f;

        private FishDefinition _crucian;
        private Inventory _inventory;
        private GameMessages _messages;
        private FishingSession _session;

        [SetUp]
        public void SetUp()
        {
            _crucian = TestAssets.Fish("crucian");
            var config = TestAssets.Create<FishingConfig>(
                ("_castSeconds", Cast),
                ("_biteWaitSeconds", new FloatRange(2f, 6f)),
                ("_biteWindowSeconds", 1f),
                ("_rewaitOnMissedBite", true),
                ("_catchBarSize", 0.25f),
                ("_gaugeStart", 0.3f),
                ("_gaugeFillPerSecond", 0.35f),
                ("_gaugeDrainPerSecond", 0.25f),
                ("_scrollSensitivity", 0.1f),
                ("_pondFish", new List<FishWeight> { new FishWeight(_crucian, 1f) }));
            _inventory = new Inventory(5);
            _messages = new GameMessages();
            _session = new FishingSession(config, _inventory, new FixedRandom(0.5f), _messages, 3f);
        }

        [TearDown] public void TearDown() => TestAssets.DestroyAll();

        private void Run(float seconds, float step = 0.05f)
        {
            for (float t = 0f; t < seconds; t += step) _session.Tick(step);
        }

        private void ReachBite()
        {
            _session.TryCast();
            Run(Cast + 0.01f);
            Run(Wait + 0.01f);
        }

        [Test]
        public void Cast_GoesThroughCastingAndWaiting_ToBiting()
        {
            Assert.IsTrue(_session.TryCast());
            Assert.AreEqual(FishingState.Casting, _session.State);

            Run(Cast + 0.01f);
            Assert.AreEqual(FishingState.Waiting, _session.State);

            Run(Wait + 0.01f);
            Assert.AreEqual(FishingState.Biting, _session.State);
            Assert.AreSame(_crucian, _session.CurrentFish);
        }

        [Test]
        public void MissedBite_ReturnsToWaiting()
        {
            ReachBite();
            string toast = null;
            _messages.ToastRequested += m => toast = m;

            Run(1.1f);

            Assert.AreEqual(FishingState.Waiting, _session.State);
            Assert.AreEqual(GameTexts.BiteMissed, toast);
        }

        [Test]
        public void Minigame_FishStaysInBar_CatchesAndAddsToInventory()
        {
            ReachBite();
            _session.PressInteract();
            Assert.AreEqual(FishingState.Minigame, _session.State);

            Run(2.5f);

            Assert.AreEqual(FishingState.Result, _session.State);
            Assert.AreEqual(FishingOutcome.Caught, _session.LastOutcome);
            Assert.AreEqual(1, _inventory.Count(_crucian));
        }

        [Test]
        public void Minigame_BarAwayFromFish_Escapes()
        {
            ReachBite();
            _session.PressInteract();
            _session.Scroll(-10f);

            Run(2f);

            Assert.AreEqual(FishingOutcome.Escaped, _session.LastOutcome);
            Assert.AreEqual(0, _inventory.Count(_crucian));
        }

        [Test]
        public void Result_PressInteract_RecastsImmediately_ThenIdleAfterHold()
        {
            ReachBite();
            _session.PressInteract();
            Run(2.5f);
            bool dismissed = false;
            _messages.AcquiredDismissed += () => dismissed = true;

            _session.PressInteract();
            Assert.AreEqual(FishingState.Casting, _session.State);
            Assert.IsTrue(dismissed);

            _session.Cancel();
            ReachBite();
            _session.PressInteract();
            Run(2.5f);
            Run(3.1f);
            Assert.AreEqual(FishingState.Idle, _session.State);
        }

        [Test]
        public void Cancel_FromAnyState_ReturnsToIdle()
        {
            ReachBite();
            _session.Cancel();

            Assert.AreEqual(FishingState.Idle, _session.State);
            Assert.IsFalse(_session.IsBusy);
        }
    }
}
