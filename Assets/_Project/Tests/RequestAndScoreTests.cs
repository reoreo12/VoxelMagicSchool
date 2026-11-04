// 의뢰와 점수 단위 테스트: 동시 1개 제한, 씨앗 지급, 진행 계산, 전달 조건, 보너스 점수, 상한.
using NUnit.Framework;
using VoxelMagic.Core;

namespace VoxelMagic.Tests
{
    public class RequestAndScoreTests
    {
        private ProduceDefinition _onion;
        private FishDefinition _crucian;
        private SeedDefinition _onionSeed;
        private RequestDefinition _broth;
        private RequestDefinition _second;
        private Inventory _inventory;
        private ScoreBook _score;
        private RequestTracker _tracker;

        [SetUp]
        public void SetUp()
        {
            _onion = TestAssets.Produce("onion");
            _crucian = TestAssets.Fish("crucian");
            _onionSeed = TestAssets.Seed("onionSeed", _onion, 1, 2);
            _broth = TestAssets.Request("국물 재료가 필요해", 15,
                new[] { new ItemAmount(_onion, 2), new ItemAmount(_crucian, 1) },
                new[] { new ItemAmount(_onionSeed, 2) });
            _second = TestAssets.Request("두 번째", 10,
                new[] { new ItemAmount(_onion, 1) },
                new ItemAmount[0]);

            _inventory = new Inventory(5);
            _score = new ScoreBook(100);
            _tracker = new RequestTracker(new[] { _broth, _second }, _inventory, _score, new MaterialJournal(), 5);
        }

        [TearDown] public void TearDown() => TestAssets.DestroyAll();

        [Test]
        public void Accept_GivesSeedsAndBuildsProgress()
        {
            Assert.AreEqual(AcceptResult.Success, _tracker.Accept(_broth));

            Assert.AreEqual(2, _inventory.Count(_onionSeed));
            Assert.AreEqual(2, _tracker.Progress.Count);
            Assert.AreEqual("onion 0/2", _tracker.Progress[0].ToString());
        }

        [Test]
        public void Accept_WhileActive_IsRejected()
        {
            _tracker.Accept(_broth);

            Assert.AreEqual(AcceptResult.AlreadyActive, _tracker.Accept(_second));
            Assert.AreSame(_broth, _tracker.Active);
        }

        [Test]
        public void Progress_FollowsInventory()
        {
            _tracker.Accept(_broth);
            int raised = 0;
            _tracker.ProgressChanged += () => raised++;

            _inventory.TryAdd(_onion, 1);

            Assert.Greater(raised, 0);
            Assert.AreEqual(1, _tracker.Progress[0].Current);
            Assert.AreEqual(DeliverResult.MissingItems, _tracker.CanDeliver());
            Assert.AreEqual("onion 1/2, crucian 0/1", _tracker.DescribeMissing());
        }

        [Test]
        public void Deliver_RemovesItems_AddsScoreWithNewKindBonus_AndFreesSlot()
        {
            _tracker.Accept(_broth);
            _inventory.TryAdd(_onion, 3);
            _inventory.TryAdd(_crucian, 1);

            Assert.AreEqual(DeliverResult.Success, _tracker.Deliver(out var report));

            Assert.AreEqual(1, _inventory.Count(_onion));
            Assert.AreEqual(0, _inventory.Count(_crucian));
            Assert.AreEqual(15 + 2 * 5, report.ScoreGained);
            Assert.AreEqual(25, _score.Written);
            Assert.AreEqual(0, _score.Practical);
            Assert.IsFalse(_tracker.HasActive);
            Assert.AreEqual(RequestStatus.Completed, _tracker.GetStatus(_broth));
            Assert.AreEqual(AcceptResult.AlreadyCompleted, _tracker.CanAccept(_broth));
            Assert.AreEqual(AcceptResult.Success, _tracker.CanAccept(_second));
        }

        [Test]
        public void Deliver_KnownItemKind_GetsNoBonus()
        {
            _tracker.Accept(_broth);
            _inventory.TryAdd(_onion, 3);
            _inventory.TryAdd(_crucian, 1);
            _tracker.Deliver(out _);

            _tracker.Accept(_second);
            _tracker.Deliver(out var report);

            Assert.AreEqual(10, report.ScoreGained);
        }

        [Test]
        public void Deliver_WithoutActive_ReportsNoActiveRequest()
        {
            Assert.AreEqual(DeliverResult.NoActiveRequest, _tracker.Deliver(out _));
        }

        [Test]
        public void ScoreCalculator_AddsBonusPerNewKind()
        {
            Assert.AreEqual(20, ScoreCalculator.RequestReward(10, 2, 5));
            Assert.AreEqual(10, ScoreCalculator.RequestReward(10, 0, 5));
        }

        [Test]
        public void ScoreBook_ClampsAtMax_AndReportsActualGain()
        {
            var book = new ScoreBook(100);
            book.AddWritten(90);

            Assert.AreEqual(10, book.AddWritten(25));
            Assert.AreEqual(100, book.Written);
            Assert.AreEqual(0, book.AddWritten(5));
        }
    }
}
