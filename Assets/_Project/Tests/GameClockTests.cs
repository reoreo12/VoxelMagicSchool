// 게임 시계·하루 흐름 검사 (QUEST_TIMER 1장, 완료 기준 1): 구간 속도, 경고·시간대·자정 순서, 경비원 지연, 스토리 날 정지, 시작 보정 합치기.
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Tests
{
    public sealed class GameClockTests
    {
        private GameClock _clock;
        private DayFlow _days;

        [SetUp]
        public void SetUp()
        {
            // 기본값: 방과후 16~20시 210초, 밤 20~24시 150초, 경고 19·23시, 경비원 +60분, 8일 일정
            var time = TestAssets.Create<TimeConfig>();
            var calendar = TestAssets.Create<CalendarConfig>();
            _clock = new GameClock(time, calendar);
            _days = new DayFlow(_clock);
        }

        [TearDown]
        public void TearDown() => TestAssets.DestroyAll();

        [Test]
        public void 방과후는_게임_1시간이_현실_52_5초()
        {
            _days.StartDay(2);
            _clock.Advance(52.5f);
            Assert.AreEqual(17, _clock.Hour);
            Assert.AreEqual(17 * 60f, _clock.Minute, 0.01f);
        }

        [Test]
        public void 밤은_게임_1시간이_현실_37_5초()
        {
            _days.StartDay(2);
            _clock.JumpTo(20 * 60);
            _clock.Advance(37.5f);
            Assert.AreEqual(21 * 60f, _clock.Minute, 0.01f);
        }

        [Test]
        public void 하루_흐름_경고_밤_경비원_다음날_17시()
        {
            var log = new List<string>();
            _clock.WarningChanged += () => log.Add("경고:" + _clock.Warning);
            _clock.PeriodChanged += p => log.Add("시간대:" + p);
            _days.DayEnded += (day, reason) => log.Add($"끝:{day}:{reason}");
            _days.StartDay(2);
            log.Clear();

            float elapsed = 0f;
            while (_clock.Day == 2 && elapsed < 400f)
            {
                _clock.Advance(0.25f);
                elapsed += 0.25f;
            }

            Assert.AreEqual(3, _clock.Day);
            Assert.AreEqual(360f, elapsed, 0.3f, "하루는 현실 약 6분");
            Assert.AreEqual(17 * 60f, _clock.Minute, 0.01f, "경비원에게 걸리면 다음 날 17:00 시작");
            CollectionAssert.AreEqual(new[]
            {
                "경고:NightSoon", "시간대:Night", "경고:None", "경고:DayEndSoon", "경고:None", "끝:2:CaughtByGuard",
                "시간대:Day", "경고:None" // 3일 시작 때 표시 이벤트를 다시 낸다
            }, log);
        }

        [Test]
        public void 임박_경고는_20분_전부터()
        {
            _days.StartDay(2);
            _clock.JumpTo(19 * 60 + 39);
            Assert.AreEqual(TimeWarning.None, _clock.Warning);
            _clock.JumpTo(19 * 60 + 40);
            Assert.AreEqual(TimeWarning.NightSoon, _clock.Warning);
            _clock.JumpTo(23 * 60 + 39);
            Assert.AreEqual(TimeWarning.None, _clock.Warning);
            _clock.JumpTo(23 * 60 + 40);
            Assert.AreEqual(TimeWarning.DayEndSoon, _clock.Warning);
        }

        [Test]
        public void 배속_16에서도_정시를_빠짐없이_순서대로_알린다()
        {
            var hours = new List<int>();
            _clock.HourChanged += h => hours.Add(h);
            _days.StartDay(2);
            hours.Clear();
            _clock.SpeedMultiplier = 16f;
            _clock.Advance(15f); // 현실 240초 = 방과후 4시간(210초) + 밤 30초
            CollectionAssert.AreEqual(new[] { 17, 18, 19, 20 }, hours);
            Assert.AreEqual(TimeOfDay.Night, _clock.Period);
        }

        [Test]
        public void 스토리_날은_시계가_멈추고_끝내면_다음날_16시()
        {
            _days.StartDay(1);
            Assert.IsTrue(_clock.IsHeld);
            Assert.IsTrue(_clock.Holds.Has(PauseCauses.Story));
            Assert.AreEqual(TimeOfDay.Night, _clock.Period, "1일은 밤 20:00으로 보인다");
            _days.FinishStoryDay();
            Assert.AreEqual(2, _clock.Day);
            Assert.AreEqual(16 * 60f, _clock.Minute, 0.01f);
            Assert.IsFalse(_clock.IsHeld);
        }

        [Test]
        public void 자유_날에는_스토리_끝내기를_무시한다()
        {
            _days.StartDay(2);
            _days.FinishStoryDay();
            Assert.AreEqual(2, _clock.Day);
        }

        [Test]
        public void 시험까지_남은_날()
        {
            _days.StartDay(1);
            Assert.AreEqual(7, _clock.DaysUntilExam);
            _days.StartDay(8);
            Assert.AreEqual(0, _clock.DaysUntilExam);
        }

        [Test]
        public void 마지막_날이_끝나면_멈춘다()
        {
            _days.StartDay(8);
            _days.FinishStoryDay();
            Assert.IsTrue(_days.IsFinished);
            Assert.AreEqual(8, _clock.Day);
            Assert.IsTrue(_clock.Holds.Has(PauseCauses.GameOver));
        }

        [Test]
        public void 시작_보정은_늦추기_최대와_앞당기기_최대를_더한다()
        {
            var shift = new StartTimeAdjustments();
            shift.Request(StartTimeAdjustments.GuardSource, 60);
            shift.Request("석고상", 60);
            Assert.AreEqual(60, shift.Combined(-90, 120), "같은 방향 벌은 겹치지 않는다");
            shift.Request("지각 레이스", -30);
            Assert.AreEqual(30, shift.Combined(-90, 120));
            shift.Request("지각 레이스", -200); // 같은 원인은 덮어쓴다
            Assert.AreEqual(-90, shift.Combined(-90, 120), "범위로 자른다");
        }

        [Test]
        public void 보정은_다음_날_한_번만_쓰인다()
        {
            _days.StartDay(2);
            _days.RequestNextDayShift("지각 레이스", -30);
            _days.EndDayVoluntarily();
            Assert.AreEqual(15 * 60 + 30f, _clock.Minute, 0.01f);
            _days.EndDayVoluntarily();
            Assert.AreEqual(16 * 60f, _clock.Minute, 0.01f);
        }

        [Test]
        public void 시각_이동으로_24시가_되면_경비원_처리()
        {
            _days.StartDay(2);
            _clock.JumpTo(24 * 60);
            Assert.AreEqual(3, _clock.Day);
            Assert.AreEqual(17 * 60f, _clock.Minute, 0.01f);
        }

        [Test]
        public void 하루_시작_단계는_순서대로_실행된다()
        {
            var order = new List<string>();
            _days.AddStep(new Step(3, "게시판", order));
            _days.AddStep(new Step(1, "작물", order));
            _days.AddStep(new Step(2, "채집", order));
            _days.StartDay(2);
            CollectionAssert.AreEqual(new[] { "작물", "채집", "게시판" }, order);
        }

        [Test]
        public void 배속은_시각만_빠르게_하고_0이면_멈춘다()
        {
            _days.StartDay(2);
            _clock.SpeedMultiplier = 0f;
            _clock.Advance(100f);
            Assert.AreEqual(16 * 60f, _clock.Minute, 0.01f);
        }

        private sealed class Step : IDayStartStep
        {
            private readonly string _name;
            private readonly List<string> _log;

            public Step(int order, string name, List<string> log)
            {
                Order = order;
                _name = name;
                _log = log;
            }

            public int Order { get; }
            public void Run(int day, DayKind kind, DayStartReason reason) => _log.Add(_name);
        }
    }
}
