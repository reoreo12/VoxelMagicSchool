// 뒷산 포획 규칙 테스트: 구역 밖 입질 없음, 간격마다 입질, 흔들림 종료, 지나갈 때 흔들림, 포획 성공·실패·강제.
using System;
using NUnit.Framework;
using VoxelMagic.Core;

namespace VoxelMagic.Tests
{
    public sealed class CaptureTests
    {
        private sealed class FakeDrawer : IMagicCircleDrawer
        {
            public float Accuracy;
            public Action<float> Pending;
            public void Begin(MagicCircleRequest request, Action<float> onFinished) => Pending = onFinished;
            public void Finish() => Pending?.Invoke(Accuracy);
        }

        private Inventory _inventory;
        private MountainMessages _mountain;
        private string _banner;
        private AnimalDefinition _hare;
        private FakeDrawer _drawer;

        [TearDown] public void TearDown() => TestAssets.DestroyAll();

        private CaptureService Create(int tufts = 4)
        {
            var config = TestAssets.Create<CaptureConfig>(("_biteIntervalSeconds", 5f), ("_biteShakeSeconds", 3f),
                ("_bitesPerWave", new UnityEngine.Vector2Int(1, 2)), ("_passShakeSeconds", 0.4f));
            _hare = TestAssets.Create<AnimalDefinition>(("_id", "hare"), ("_displayName", "산토끼"), ("_requiredAccuracy", 0.5f),
                ("_sizeRangeCm", new FloatRange(30f, 40f)));
            _inventory = new Inventory(5);
            _mountain = new MountainMessages();
            _banner = null;
            _mountain.BannerRequested += text => _banner = text;
            _drawer = new FakeDrawer();
            var service = new CaptureService(tufts, new[] { _hare }, config, _inventory, new GameMessages(), _mountain, new FixedRandom(0f));
            service.SetDrawer(_drawer);
            return service;
        }

        private static int BiteTuft(CaptureService service)
        {
            for (int i = 0; i < service.TuftCount; i++) if (service.ShakeOf(i) == TuftShake.Bite) return i;
            return -1;
        }

        [Test]
        public void OutsideZone_NoBites()
        {
            var service = Create();
            for (int i = 0; i < 100; i++) service.Tick(0.1f, false);
            Assert.AreEqual(0, service.ShakingCount);
        }

        [Test]
        public void InsideZone_BiteAfterInterval_ThenStops()
        {
            var service = Create();
            service.Tick(4.9f, true);
            Assert.AreEqual(0, service.ShakingCount);
            service.Tick(0.2f, true);
            Assert.AreEqual(1, service.ShakingCount, "FixedRandom(0) → 최소 1개");
            int tuft = BiteTuft(service);
            Assert.AreEqual(_hare, service.AnimalAt(tuft));

            service.Tick(1f, false);   // 구역을 나가도 흔들림은 계속
            Assert.AreEqual(TuftShake.Bite, service.ShakeOf(tuft));
            service.Tick(2.1f, false); // 3초 지나 멈춤, 새 입질 없음
            Assert.AreEqual(0, service.ShakingCount);
        }

        [Test]
        public void PassBy_FromNearPath_ShakesOnceWithoutAnimal()
        {
            var service = Create();
            service.Tick(0f, false); // 플레이어는 구역 밖 (근처 길)
            service.PassBy(0);
            Assert.AreEqual(TuftShake.Pass, service.ShakeOf(0));
            Assert.IsNull(service.AnimalAt(0));
            Assert.IsFalse(service.CanCapture(0));
            Assert.AreEqual(1, service.ShakingCount, "맨 앞 하나만");

            service.Tick(0.3f, false); // 멀어져도(구역 밖) 끝까지 흔들린다
            Assert.AreEqual(TuftShake.Pass, service.ShakeOf(0));
            service.Tick(0.2f, false);
            Assert.AreEqual(TuftShake.None, service.ShakeOf(0));
        }

        [Test]
        public void PassBy_DoesNotOverrideBite()
        {
            var service = Create(1);
            service.Tick(0f, true);
            service.SpawnBites();
            service.PassBy(0);
            Assert.AreEqual(TuftShake.Bite, service.ShakeOf(0));
        }

        [Test]
        public void Capture_SuccessAddsToInventory()
        {
            var service = Create();
            service.Tick(0f, true);
            service.SpawnBites();
            int tuft = BiteTuft(service);
            Assert.IsTrue(service.BeginCapture(tuft));
            service.Tick(10f, true); // 그리는 동안 그 풀숲은 멈춰 기다린다
            Assert.AreEqual(TuftShake.Bite, service.ShakeOf(tuft));

            _drawer.Accuracy = 0.7f;
            _drawer.Finish();

            Assert.AreEqual(1, _inventory.Count(_hare));
            Assert.AreEqual("산토끼 포획!", _banner);
            Assert.AreEqual(TuftShake.None, service.ShakeOf(tuft));
        }

        [Test]
        public void Capture_LowAccuracyFails_ForcedOverrides()
        {
            var service = Create();
            service.Tick(0f, true);
            service.SpawnBites();
            int tuft = BiteTuft(service);
            service.BeginCapture(tuft);
            _drawer.Accuracy = 0.3f;
            _drawer.Finish();
            Assert.AreEqual(0, _inventory.Count(_hare));
            Assert.AreEqual(MountainTexts.Missed, _banner);
            Assert.AreEqual(TuftShake.None, service.ShakeOf(tuft), "실패하면 흔들림이 멈춘다");

            service.SpawnBites();
            tuft = BiteTuft(service);
            service.ForceNext(ForcedCaptureResult.Success);
            service.BeginCapture(tuft);
            _drawer.Finish(); // 정확도 0.3이어도 강제 성공
            Assert.AreEqual(1, _inventory.Count(_hare));
            Assert.AreEqual(ForcedCaptureResult.None, service.Forced, "강제는 한 번만");
        }
    }
}
