// 의뢰와 점수 단위 테스트(QUEST_TIMER 2장, 완료 기준 4·5·6): 최대 3개 수락·씨앗 지급, 게시판 연결, 진행 계산, 겹치는 재료 전달, 포기, 씨앗 다시 받기, 전달 대화, 희귀도 점수, 상한 없음.
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Tests
{
    public class RequestAndScoreTests
    {
        private ProduceDefinition _radish;      // 흔함 2점
        private FishDefinition _catfish;        // 보통 4점
        private SeedDefinition _radishSeed;
        private RequestDefinition _broth;       // 무 2 + 메기 1, 씨앗 2
        private RequestDefinition _second;      // 무 1
        private RequestDefinition _third;       // 메기 1
        private RequestDefinition _fourth;      // 무 1
        private Inventory _inventory;
        private ScoreBook _score;
        private QuestConfig _quest;
        private RequestLog _log;

        [SetUp]
        public void SetUp()
        {
            _radish = TestAssets.Produce("radish");
            _catfish = TestAssets.Create<FishDefinition>(("_id", "catfish"), ("_displayName", "catfish"), ("_rarity", ItemRarity.Uncommon));
            _radishSeed = TestAssets.Seed("radishSeed", _radish, 1, 1);
            _broth = TestAssets.Request("국물", new[] { new ItemAmount(_radish, 2), new ItemAmount(_catfish, 1) },
                new[] { new ItemAmount(_radishSeed, 2) });
            _second = TestAssets.Request("두 번째", new[] { new ItemAmount(_radish, 1) }, new ItemAmount[0]);
            _third = TestAssets.Request("세 번째", new[] { new ItemAmount(_catfish, 1) }, new ItemAmount[0]);
            _fourth = TestAssets.Request("네 번째", new[] { new ItemAmount(_radish, 1) }, new ItemAmount[0]);

            _inventory = new Inventory(8);
            _score = new ScoreBook();
            _quest = TestAssets.Create<QuestConfig>();
            _log = new RequestLog(_inventory, _score, new MaterialJournal(), _quest);
        }

        [TearDown] public void TearDown() => TestAssets.DestroyAll();

        private RequestBoardState BoardWith(params RequestDefinition[] offers)
        {
            var board = new RequestBoardState();
            foreach (var r in offers) board.Add(r);
            _log.Connect(_quest, board, null, null);
            return board;
        }

        [Test]
        public void Accept_GivesSeeds_AndBuildsProgress()
        {
            Assert.AreEqual(AcceptResult.Success, _log.Accept(_broth));

            Assert.AreEqual(2, _inventory.Count(_radishSeed));
            Assert.AreEqual(1, _log.AcceptedCount);
            var progress = _log.ProgressOf(_log.Active[0]);
            Assert.AreEqual("radish 0/2", progress[0].ToString());
            Assert.AreEqual(RequestState.InProgress, _log.GetState(_broth));
        }

        [Test]
        public void 네_번째_의뢰는_칸이_꽉_차서_거절()
        {
            _log.Accept(_broth);
            _log.Accept(_second);
            _log.Accept(_third);

            Assert.AreEqual(AcceptResult.SlotsFull, _log.Accept(_fourth));
            Assert.AreEqual(3, _log.AcceptedCount);
            Assert.AreEqual(AcceptResult.AlreadyAccepted, _log.CanAccept(_second));
        }

        [Test]
        public void 씨앗_넣을_자리가_없으면_거절()
        {
            var small = new Inventory(1);
            small.TryAdd(_catfish, 1);
            var log = new RequestLog(small, _score, new MaterialJournal(), _quest);

            Assert.AreEqual(AcceptResult.InventoryFull, log.Accept(_broth));
            Assert.AreEqual(0, log.AcceptedCount);
        }

        [Test]
        public void 수락하면_게시판에서_빠지고_게시판에_없는_의뢰는_거절()
        {
            var board = BoardWith(_broth, _second);

            Assert.AreEqual(AcceptResult.Success, _log.Accept(_broth));
            CollectionAssert.AreEqual(new[] { _second }, board.Offers);
            Assert.AreEqual(AcceptResult.NotOnBoard, _log.Accept(_third));
            Assert.AreEqual(RequestState.Offered, _log.GetState(_second));
        }

        [Test]
        public void 진행은_인벤토리를_따라가고_완료_알림은_한_번()
        {
            _log.Accept(_broth);
            var completed = new List<AcceptedRequestEntry>();
            _log.RequestBecameComplete += completed.Add;

            _inventory.TryAdd(_radish, 2);
            Assert.AreEqual(RequestState.InProgress, _log.Active[0].State);
            Assert.AreEqual(DeliverResult.MissingItems, _log.CanDeliver(_log.Active[0].Id));
            Assert.AreEqual("catfish 0/1", _log.DescribeMissing(_log.Active[0]));

            _inventory.TryAdd(_catfish, 1);
            _inventory.TryAdd(_radish, 1);
            Assert.AreEqual(RequestState.Ready, _log.Active[0].State);
            Assert.AreEqual(1, completed.Count);

            _inventory.TryRemove(_catfish, 1);
            Assert.AreEqual(RequestState.InProgress, _log.Active[0].State, "재료가 줄면 진행 중으로 돌아감");
        }

        [Test]
        public void 전달하면_재료가_빠지고_희귀도_점수_합을_받고_칸이_빈다()
        {
            _log.Accept(_broth);
            _inventory.TryAdd(_radish, 3);
            _inventory.TryAdd(_catfish, 1);
            int id = _log.Active[0].Id;

            Assert.AreEqual(DeliverResult.Success, _log.Deliver(id, out var report));

            Assert.AreEqual(1, _inventory.Count(_radish));
            Assert.AreEqual(0, _inventory.Count(_catfish));
            Assert.AreEqual(2 * 2 + 4, report.ScoreGained, "흔함 2 × 2 + 보통 4");
            Assert.AreEqual(8, _score.Written);
            Assert.AreEqual(0, _log.AcceptedCount);
            Assert.AreEqual(RequestState.Delivered, _log.GetState(_broth));
            Assert.AreEqual(8, _log.RewardOf(_broth));
        }

        [Test]
        public void 같은_재료를_요구하는_두_의뢰_하나를_내면_다른_하나는_빠진다()
        {
            _log.Accept(_second);  // 무 1
            _log.Accept(_fourth);  // 무 1
            _inventory.TryAdd(_radish, 1);

            Assert.AreEqual(2, _log.ReadyRequests().Count, "둘 다 완료 가능으로 보임");
            Assert.AreEqual(DeliverResult.Success, _log.Deliver(_log.Active[0].Id, out _));

            Assert.AreEqual(0, _log.ReadyRequests().Count);
            Assert.AreEqual(RequestState.InProgress, _log.Active[0].State);
            Assert.AreEqual(DeliverResult.MissingItems, _log.CanDeliver(_log.Active[0].Id));
        }

        [Test]
        public void 포기하면_칸이_비고_씨앗은_남는다()
        {
            _log.Accept(_broth);
            AcceptedRequest abandoned = null;
            _log.Abandoned += a => abandoned = a;

            Assert.IsTrue(_log.Abandon(_log.Active[0].Id));

            Assert.AreEqual(0, _log.AcceptedCount);
            Assert.AreEqual(2, _inventory.Count(_radishSeed));
            Assert.AreSame(_broth, abandoned.Request);
            Assert.AreEqual(RequestState.Abandoned, _log.GetState(_broth));
            Assert.IsFalse(_log.Abandon(999));
        }

        [Test]
        public void 씨앗_다시_받기는_필요한_만큼만()
        {
            _log.Accept(_broth); // 무 2 필요, 씨앗 2
            Assert.IsEmpty(_log.RefillableSeeds(), "씨앗을 가진 동안은 줄 것이 없음");

            _inventory.TryRemove(_radishSeed, 2); // 버림
            var farm = new FarmGrid(6, new InstantGrowthRule());
            _log.Connect(_quest, null, null, farm);
            farm.TryPlant(_radishSeed, Vector2Int.zero, out _); // 하나는 심어 둠

            Assert.AreEqual(1, _log.GiveRefillSeeds(), "2 − 심은 것 1 = 1");
            Assert.AreEqual(1, _inventory.Count(_radishSeed));
            Assert.AreEqual(0, _log.GiveRefillSeeds(), "다시 눌러도 더 안 줌");

            _inventory.TryRemove(_radishSeed, 1);
            _inventory.TryAdd(_radish, 1); // 하나는 이미 거둠
            Assert.AreEqual(0, _log.GiveRefillSeeds(), "2 − 작물 1 − 심은 것 1 = 0");
        }

        [Test]
        public void 밤에만_열리고_마감_뒤에는_전달_못_함()
        {
            var time = TestAssets.Create<TimeConfig>();
            var calendar = TestAssets.Create<CalendarConfig>();
            var clock = new GameClock(time, calendar);
            var board = new RequestBoardState();
            board.Add(_second);
            _log.Connect(_quest, board, clock, null);

            clock.BeginDay(2, 16 * 60);
            Assert.AreEqual(QuestAccess.NotNight, _log.BoardAccess());
            Assert.AreEqual(AcceptResult.NotNight, _log.Accept(_second));

            clock.JumpTo(20 * 60);
            Assert.AreEqual(AcceptResult.Success, _log.Accept(_second));
            _inventory.TryAdd(_radish, 1);

            clock.BeginDay(7, 20 * 60);
            Assert.AreEqual(QuestAccess.Closed, _log.BoardAccess());
            Assert.AreEqual(DeliverResult.TooLate, _log.CanDeliver(_log.Active[0].Id));
        }

        [Test]
        public void 전달_대화는_하나뿐이면_바로_대사_둘이면_고르기()
        {
            var session = new DeliverySession(_log);
            _log.Accept(_second);
            _log.Accept(_third);
            _inventory.TryAdd(_radish, 1);

            Assert.IsTrue(session.Begin());
            Assert.AreEqual(DeliveryStep.Talking, session.Step, "완료 가능 하나 → 바로 대사");
            session.Cancel();
            Assert.AreEqual(2, _log.AcceptedCount, "취소하면 아무것도 안 바뀜");

            _inventory.TryAdd(_catfish, 1);
            Assert.IsTrue(session.Begin());
            Assert.AreEqual(DeliveryStep.Choosing, session.Step);
            Assert.AreEqual(2, session.Choices.Count);

            Assert.IsTrue(session.Choose(session.Choices[1].Id));
            Assert.AreEqual(DeliveryStep.Talking, session.Step);
            session.CompleteTalk();
            Assert.AreEqual(DeliveryStep.Talking, session.Step, "남은 하나 → 바로 대사");
            session.CompleteTalk();
            Assert.AreEqual(DeliveryStep.Idle, session.Step);
            Assert.AreEqual(2, session.DeliveredCount);
            Assert.AreEqual(0, _log.AcceptedCount);
        }

        [Test]
        public void 고르는_순간_다시_확인한다()
        {
            var session = new DeliverySession(_log);
            _log.Accept(_second);
            _log.Accept(_fourth);
            _inventory.TryAdd(_radish, 1);
            session.Begin();
            Assert.AreEqual(DeliveryStep.Choosing, session.Step);

            _inventory.TryRemove(_radish, 1); // 고르기 전에 재료가 사라짐
            Assert.IsFalse(session.Choose(session.Choices.Count > 0 ? session.Choices[0].Id : -1));
            Assert.AreEqual(DeliveryStep.Idle, session.Step, "완료 가능이 없어 대화 끝");
        }

        [Test]
        public void 점수에는_상한이_없다()
        {
            var book = new ScoreBook();
            book.AddWritten(90);

            Assert.AreEqual(25, book.AddWritten(25));
            Assert.AreEqual(115, book.Written);
            Assert.AreEqual(0, book.AddWritten(0));
        }
    }
}
