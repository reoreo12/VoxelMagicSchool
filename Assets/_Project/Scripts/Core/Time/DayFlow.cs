// 하루의 시작과 끝을 진행한다: 하루 시작 단계(작물·채집·게시판)를 순서대로 실행하고, 날 종류에 따라 시계를 잡거나 풀고,
// 하루 끝(경비원)·스스로 끝내기·스토리 날 끝내기 때 다음 날로 넘긴다. 다음 날 시작 시각 보정 요청도 여기서 받는다.
using System;
using System.Collections.Generic;

namespace VoxelMagic.Core
{
    public enum DayEndReason
    {
        /// <summary>하루 끝(24:00)까지 남아 경비원에게 걸림 → 다음 날 늦게 시작.</summary>
        CaughtByGuard,
        /// <summary>플레이어가 스스로 하루를 끝냄 (방에서 자기 등, 아직 치트로만).</summary>
        Voluntary,
        /// <summary>시계가 흐르지 않는 스토리 날이 끝남.</summary>
        StoryFinished,
        /// <summary>치트로 넘김.</summary>
        Cheat
    }

    /// <summary>하루가 왜 시작됐는지 (작물은 새 게임 첫날에는 자라지 않는다).</summary>
    public enum DayStartReason
    {
        /// <summary>새 게임(또는 시계가 없던 진행 상태)의 첫날.</summary>
        NewGame,
        /// <summary>앞날이 끝나 다음 날로 넘어옴.</summary>
        NextDay,
        /// <summary>치트로 특정 날짜로 이동.</summary>
        Jump
    }

    /// <summary>하루 시작에 할 일 하나. Order가 작은 것부터 실행한다 (작물 1 → 채집 2 → 게시판 3).</summary>
    public interface IDayStartStep
    {
        int Order { get; }
        void Run(int day, DayKind kind, DayStartReason reason);
    }

    public sealed class DayFlow
    {
        private readonly GameClock _clock;
        private readonly List<IDayStartStep> _steps = new List<IDayStartStep>();

        public DayFlow(GameClock clock)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _clock.DayEndReached += OnDayEndReached;
        }

        /// <summary>(끝난 날, 이유)</summary>
        public event Action<int, DayEndReason> DayEnded;
        /// <summary>(시작한 날, 날 종류) — 하루 시작 단계가 모두 끝난 뒤.</summary>
        public event Action<int, DayKind> DayStarted;

        public GameClock Clock => _clock;
        public StartTimeAdjustments NextDayShift { get; } = new StartTimeAdjustments();
        public int Day => _clock.Day;
        public DayKind Kind => _clock.Calendar.Get(_clock.Day).Kind;
        /// <summary>마지막 날까지 끝나 더 넘길 날이 없다.</summary>
        public bool IsFinished { get; private set; }

        public void AddStep(IDayStartStep step)
        {
            if (step == null || _steps.Contains(step)) return;
            _steps.Add(step);
            _steps.Sort((a, b) => a.Order.CompareTo(b.Order));
        }

        public void RemoveStep(IDayStartStep step) => _steps.Remove(step);

        public void BeginNewGame() => StartDay(_clock.Calendar.NewGameStartDay, DayStartReason.NewGame);

        /// <summary>그날을 시작한다: 시각 정하기(보정 적용) → 하루 시작 단계 → 날 종류별 시계 잡기 → DayStarted.</summary>
        public void StartDay(int day, DayStartReason reason = DayStartReason.Jump)
        {
            var config = _clock.Config;
            var info = _clock.Calendar.Get(day);
            int shift = NextDayShift.Consume(config.MinStartShiftMinutes, config.MaxStartShiftMinutes);
            float minute = info.Kind == DayKind.Story ? info.HeldMinute : config.DayStartMinute + shift;

            IsFinished = false;
            _clock.Holds.Release(PauseCauses.GameOver);
            _clock.BeginDay(day, minute);
            foreach (var step in _steps.ToArray())
                step.Run(day, info.Kind, reason);
            // 이벤트 날: 이벤트 본편이 아직 없어 건너뛰고 바로 방과 후부터 흐른다 (이벤트가 생기면 이벤트 동안 Event 원인으로 잡는다).
            _clock.Holds.Set(info.Kind == DayKind.Story, PauseLevel.GameClock, PauseCauses.Story);
            DayStarted?.Invoke(day, info.Kind);
        }

        /// <summary>다음 날 방과 후 시작을 minutes분 늦게(+)/일찍(−). 같은 원인은 덮어쓴다.</summary>
        public void RequestNextDayShift(string source, int minutes) => NextDayShift.Request(source, minutes);

        /// <summary>플레이어가 스스로 하루를 끝낸다 (보정 없음).</summary>
        public void EndDayVoluntarily() => EndDay(DayEndReason.Voluntary);

        /// <summary>스토리 날의 내용이 끝났을 때 (예: 1일 튜토리얼 의뢰 전달). 스토리 날이 아니면 무시.</summary>
        public void FinishStoryDay()
        {
            if (Kind == DayKind.Story) EndDay(DayEndReason.StoryFinished);
        }

        /// <summary>지금 날을 끝내고 다음 날을 시작한다. 마지막 날이면 시계를 잡아 둔다.</summary>
        public void EndDay(DayEndReason reason)
        {
            if (IsFinished) return;
            int day = _clock.Day;
            DayEnded?.Invoke(day, reason);
            if (_clock.Calendar.Has(day + 1))
            {
                StartDay(day + 1, DayStartReason.NextDay);
                return;
            }
            IsFinished = true;
            _clock.Holds.Acquire(PauseLevel.GameClock, PauseCauses.GameOver);
        }

        private void OnDayEndReached()
        {
            RequestNextDayShift(StartTimeAdjustments.GuardSource, _clock.Config.GuardDelayMinutes);
            EndDay(DayEndReason.CaughtByGuard);
        }
    }
}
