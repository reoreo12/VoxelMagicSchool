// 하루 시작 처리 테스트(QUEST_TIMER 1-4·2-2·3-3, 완료 기준 3·7): 게시판 교체(난이도별, 어려움은 2일부터, 1일 튜토리얼, 7일 닫힘, 반복 방지),
// 낚시 희귀도 비율과 고정, 하루 시작 단계 순서(작물 → 채집 → 게시판).
using System.Collections.Generic;
using NUnit.Framework;
using VoxelMagic.Core;

namespace VoxelMagic.Tests
{
    public sealed class DayStartTests
    {
        private QuestConfig _quest;
        private RequestDefinition _tutorial;
        private List<RequestDefinition> _pool;

        [SetUp]
        public void SetUp()
        {
            _tutorial = Req("tutorial", RequestDifficulty.Tutorial);
            _quest = TestAssets.Create<QuestConfig>(("_tutorialRequest", _tutorial));
            _pool = new List<RequestDefinition>();
            for (int i = 0; i < 3; i++)
            {
                _pool.Add(Req("easy" + i, RequestDifficulty.Easy));
                _pool.Add(Req("normal" + i, RequestDifficulty.Normal));
                _pool.Add(Req("hard" + i, RequestDifficulty.Hard));
            }
        }

        [TearDown] public void TearDown() => TestAssets.DestroyAll();

        private static RequestDefinition Req(string name, RequestDifficulty difficulty)
        {
            var r = TestAssets.Create<RequestDefinition>(("_title", name), ("_difficulty", difficulty));
            r.name = name;
            return r;
        }

        private static List<RequestDifficulty> Difficulties(RequestBoardState board)
        {
            var list = new List<RequestDifficulty>();
            foreach (var r in board.Offers) list.Add(r.Difficulty);
            return list;
        }

        [Test]
        public void 게시판은_1일_튜토리얼_2일부터_쉬움보통어려움_7일_닫힘()
        {
            var board = new RequestBoardState();
            var random = new SystemRandom(1);
            board.Refresh(1, _quest, _pool, null, null, random);
            CollectionAssert.AreEqual(new[] { _tutorial }, board.Offers);
            board.Refresh(2, _quest, _pool, null, null, random);
            CollectionAssert.AreEqual(new[] { RequestDifficulty.Easy, RequestDifficulty.Normal, RequestDifficulty.Hard }, Difficulties(board));
            board.Refresh(3, _quest, _pool, null, null, random);
            CollectionAssert.AreEqual(new[] { RequestDifficulty.Easy, RequestDifficulty.Normal, RequestDifficulty.Hard }, Difficulties(board));
            board.Refresh(7, _quest, _pool, null, null, random);
            Assert.IsEmpty(board.Offers);
        }

        [Test]
        public void 받지_않은_의뢰는_하루_시작_때_사라진다()
        {
            var board = new RequestBoardState();
            IReadOnlyList<RequestDefinition> expired = null;
            board.Expired += list => expired = list;
            board.Refresh(2, _quest, _pool, null, null, new SystemRandom(2));
            var yesterday = new List<RequestDefinition>(board.Offers);
            board.Refresh(3, _quest, _pool, null, null, new SystemRandom(3));
            CollectionAssert.AreEquivalent(yesterday, expired);
        }

        [Test]
        public void 받아_둔_의뢰는_게시하지_않고_최근_게시_의뢰는_피한다()
        {
            var board = new RequestBoardState();
            var accepted = _pool.Find(r => r.name == "easy0");
            var random = new SystemRandom(4);
            for (int day = 2; day <= 6; day++)
            {
                var before = new List<RequestDefinition>(board.Offers);
                board.Refresh(day, _quest, _pool, r => r == accepted, null, random);
                Assert.IsFalse(((List<RequestDefinition>)new List<RequestDefinition>(board.Offers)).Contains(accepted));
                // 난이도마다 후보 3개 중 받아 둔 것·어제 것을 빼도 남으므로 이틀 연속 같은 의뢰가 나오지 않는다
                foreach (var offer in board.Offers) Assert.IsFalse(before.Contains(offer), $"{day}일 {offer.name} 반복");
            }
        }

        [Test]
        public void 후보가_모자라면_최근_게시_제한을_풀어_채운다()
        {
            var pool = new List<RequestDefinition> { Req("onlyEasy", RequestDifficulty.Easy) };
            var board = new RequestBoardState();
            board.Refresh(2, _quest, pool, null, null, new SystemRandom(5));
            board.Refresh(3, _quest, pool, null, null, new SystemRandom(6));
            Assert.AreEqual(1, board.Offers.Count);
            Assert.AreEqual(2, board.TimesOffered(pool[0]));
        }

        [Test]
        public void 다음_날_자라는_작물_의뢰는_마지막_전달_날에는_게시하지_않는다()
        {
            var rareSeed = TestAssets.Create<SeedDefinition>(("_id", "rs"), ("_growHours", 0f));
            var rareCrop = TestAssets.Create<RequestDefinition>(("_title", "rare"), ("_difficulty", RequestDifficulty.Hard),
                ("_givenItems", new List<ItemAmount> { new ItemAmount(rareSeed, 1) }));
            var other = Req("otherHard", RequestDifficulty.Hard);
            var pool = new List<RequestDefinition> { rareCrop, other };
            Assert.IsTrue(rareCrop.NeedsNextDayCrop);

            var board = new RequestBoardState();
            for (int i = 0; i < 20; i++)
            {
                board.Refresh(6, _quest, pool, null, null, new SystemRandom(i));
                CollectionAssert.DoesNotContain(board.Offers, rareCrop, "6일 밤에 받으면 7일 아침에야 자라 전달할 수 없다");
            }
            Assert.IsTrue(_quest.CanFinishInTime(rareCrop, 5), "5일 밤에 받아 심으면 6일 밤 전달 가능");
        }

        [Test]
        public void 낚시_희귀도_비율은_설정_값을_따른다()
        {
            var common = TestAssets.Create<FishDefinition>(("_id", "c"), ("_rarity", ItemRarity.Common));
            var uncommon = TestAssets.Create<FishDefinition>(("_id", "u"), ("_rarity", ItemRarity.Uncommon));
            var rare = TestAssets.Create<FishDefinition>(("_id", "r"), ("_rarity", ItemRarity.Rare));
            var config = TestAssets.Create<FishingConfig>(("_pondFish", new List<FishWeight>
                { new FishWeight(common, 1f), new FishWeight(uncommon, 1f), new FishWeight(rare, 1f) }));
            var random = new SystemRandom(11);
            int c = 0, u = 0, r = 0;
            const int runs = 20000;
            for (int i = 0; i < runs; i++)
            {
                var fish = config.PickFish(random);
                if (fish == common) c++; else if (fish == uncommon) u++; else if (fish == rare) r++;
            }
            Assert.AreEqual(0.60f, c / (float)runs, 0.015f);
            Assert.AreEqual(0.30f, u / (float)runs, 0.015f);
            Assert.AreEqual(0.10f, r / (float)runs, 0.01f);
            Assert.AreSame(rare, config.PickFish(random, ItemRarity.Rare), "희귀도 고정");
        }

        [Test]
        public void 하루_시작은_작물_채집_게시판_순서()
        {
            var time = TestAssets.Create<TimeConfig>();
            var calendar = TestAssets.Create<CalendarConfig>();
            var farm = TestAssets.Create<FarmConfig>();
            var gathering = TestAssets.Create<GatheringConfig>();
            var balance = TestAssets.Create<GameBalanceConfig>();
            var progress = PlayerProgress.CreateNew(balance);
            var pool = TestAssets.Create<RequestPool>();
            pool.Set(_pool);
            progress.EnsureWorld(new WorldSetup
            {
                Time = time, Calendar = calendar, Farm = farm, Gathering = gathering, Quest = _quest, RequestPool = pool,
                Random = new SystemRandom(9)
            });

            Assert.AreEqual(1, progress.Clock.Day);
            CollectionAssert.AreEqual(new[] { _tutorial }, progress.Board.Offers, "새 게임 1일 = 튜토리얼 의뢰");
            Assert.AreEqual(1, progress.Gathering.Day);

            var seed = TestAssets.Seed("s", TestAssets.Produce("p"), 1, 1);
            progress.Farm.TryPlant(seed, UnityEngine.Vector2Int.zero, out var plot);
            progress.Farm.Water(plot);
            progress.Days.FinishStoryDay();
            Assert.AreEqual(2, progress.Clock.Day);
            Assert.IsTrue(plot.IsGrown, "2일 시작에 작물이 자람");
            Assert.AreEqual(2, progress.Gathering.Day);
            Assert.AreEqual(2, progress.Board.Day);
            Assert.AreEqual(3, progress.Board.Offers.Count, "2일부터 쉬움·보통·어려움");
        }
    }
}
