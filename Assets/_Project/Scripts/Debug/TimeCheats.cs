// 시간 치트 동작 모음 (패널 버튼과 단축키가 같이 부른다): 일시 정지, 배속, 시각·날짜 이동, 다음 날 시작 보정 시험.
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using VoxelMagic.Core;
using VoxelMagic.Gameplay;

namespace VoxelMagic.DebugTools
{
    public static class TimeCheats
    {
        public const string ShiftSource = "치트";
        public static readonly float[] Speeds = { 1f, 2f, 4f, 8f, 16f };

        public static bool IsPaused(GameClock clock) => clock != null && clock.Holds.Has(PauseCauses.Cheat);

        public static void TogglePause(GameClock clock)
        {
            if (clock == null) return;
            clock.Holds.Set(!IsPaused(clock), PauseLevel.GameClock, PauseCauses.Cheat);
        }

        public static void SetSpeed(GameClock clock, float speed, bool worldToo)
        {
            if (clock == null) return;
            clock.SpeedMultiplier = speed;
            TimeScaleDriver.DebugWorldSpeed = worldToo ? speed : 1f;
        }

        /// <summary>배속 단계를 한 칸 올리거나 내린다.</summary>
        public static void StepSpeed(GameClock clock, int direction, bool worldToo)
        {
            if (clock == null) return;
            int index = 0;
            for (int i = 0; i < Speeds.Length; i++)
                if (clock.SpeedMultiplier >= Speeds[i] - 0.01f) index = i;
            index = System.Math.Max(0, System.Math.Min(Speeds.Length - 1, index + direction));
            SetSpeed(clock, Speeds[index], worldToo);
        }

        public static void JumpTo(GameClock clock, int hour, int minute = 0) => clock?.JumpTo(hour * 60 + minute);

        public static void AddHour(GameClock clock)
        {
            if (clock == null) return;
            clock.JumpTo(UnityEngine.Mathf.Floor(clock.Minute / 60f) * 60f + 60f);
        }

        public static void NextDay(DayFlow days) => days?.EndDay(DayEndReason.Cheat);

        public static void GoToDay(DayFlow days, int day)
        {
            if (days == null || !days.Clock.Calendar.Has(day)) return;
            days.StartDay(day);
        }

        public static void RequestShift(DayFlow days, int minutes) => days?.RequestNextDayShift(ShiftSource, minutes);

        public static void ClearShift(DayFlow days) => days?.NextDayShift.Clear();
    }
}
#endif
