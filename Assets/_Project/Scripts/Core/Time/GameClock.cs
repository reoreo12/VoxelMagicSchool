// 게임 시계: 날짜와 하루 0시부터의 분으로 시각을 들고, Advance(현실 초)로 구간 속도에 맞춰 흐른다.
// 정시·구간·경고 경계를 한 프레임에 여러 개 지나도 순서대로 하나씩 알린다(배속 대비). 하루 끝에 닿으면 멈추고 DayEndReached를 낸다.
using System;
using UnityEngine;

namespace VoxelMagic.Core
{
    public sealed class GameClock : IGameTime
    {
        private const int MaxStepsPerAdvance = 256;
        private const float Epsilon = 0.0001f;

        private readonly TimeConfig _config;
        private readonly CalendarConfig _calendar;
        private float _speed = 1f;

        public GameClock(TimeConfig config, CalendarConfig calendar)
        {
            _config = config ? config : throw new ArgumentNullException(nameof(config));
            _calendar = calendar ? calendar : throw new ArgumentNullException(nameof(calendar));
            Holds = new TimePause();
            Holds.Changed += () => HeldChanged?.Invoke();
        }

        public event Action TimeChanged;
        public event Action<int> HourChanged;
        public event Action<TimeOfDay> PeriodChanged;
        public event Action WarningChanged;
        public event Action HeldChanged;
        /// <summary>시각이 바뀔 때마다 (분 단위 포함, 작물 성장 확인용). 표시에는 쓰지 않는다.</summary>
        public event Action MinuteChanged;
        /// <summary>하루 끝(기본 24:00)에 닿은 순간. DayFlow가 받아 경비원 처리·다음 날로 넘긴다.</summary>
        public event Action DayEndReached;

        public TimeConfig Config => _config;
        public CalendarConfig Calendar => _calendar;

        /// <summary>화면 밖 이유(스토리·튜토리얼·맵 이동·치트)로 시계를 잡는 원인 목록. 씬을 넘어 유지된다.</summary>
        public TimePause Holds { get; }

        public int Day { get; private set; }
        /// <summary>하루 0시부터의 분 (분 단위는 디버그 표시에만).</summary>
        public float Minute { get; private set; }
        /// <summary>1일 0시부터의 누적 분 (날이 바뀌어도 이어지는 시각, 작물 성장 기준).</summary>
        public float AbsoluteMinute => (Day - 1) * 24f * 60f + Minute;
        public int Hour { get; private set; }
        public TimeOfDay Period { get; private set; }
        public TimeWarning Warning { get; private set; }
        public bool IsHeld => Holds.IsClockPaused;
        /// <summary>하루 끝에 닿아 다음 날 시작을 기다리는 중.</summary>
        public bool HasReachedDayEnd { get; private set; }
        public int DaysUntilExam => Math.Max(0, _calendar.ExamDay - Day);
        public int DayEndMinute => _config.DayEndMinute;
        public int DayEndHour => _config.DayEndMinute / 60;
        public int PeriodStartHour(TimeOfDay period) => _config.PeriodStartHour(period);

        /// <summary>시계 배속 (치트). 0 이상.</summary>
        public float SpeedMultiplier
        {
            get => _speed;
            set => _speed = Mathf.Max(0f, value);
        }

        /// <summary>새 날을 그 시각으로 시작한다. 모든 표시 이벤트를 다시 낸다.</summary>
        public void BeginDay(int day, float minute)
        {
            Day = Math.Max(1, day);
            HasReachedDayEnd = false;
            Apply(minute, forceNotify: true);
        }

        /// <summary>같은 날 안에서 시각을 옮긴다 (치트). 하루 끝 이상이면 하루 끝 처리까지 한다. 지나친 경고는 다시 내지 않는다.</summary>
        public void JumpTo(float minute)
        {
            if (HasReachedDayEnd) return;
            Apply(Mathf.Min(minute, DayEndMinute), forceNotify: false);
            if (Minute >= DayEndMinute) ReachDayEnd();
        }

        /// <summary>현실 시간만큼 흐른다 (배속 적용). 멈춤 판단은 부르는 쪽이 한다.</summary>
        public void Advance(float realSeconds)
        {
            if (HasReachedDayEnd || realSeconds <= 0f || _speed <= 0f) return;
            float remaining = realSeconds * _speed;
            for (int step = 0; step < MaxStepsPerAdvance && remaining > 0f; step++)
            {
                float rate = _config.SegmentAt(Minute).MinutesPerSecond;
                float boundary = NextBoundary(Minute);
                float need = (boundary - Minute) / rate;
                if (remaining < need)
                {
                    Apply(Minute + remaining * rate, forceNotify: false);
                    return;
                }
                remaining -= need;
                Apply(boundary, forceNotify: false);
                if (Minute >= DayEndMinute)
                {
                    ReachDayEnd(); // 남은 시간은 버린다 (다음 날은 DayFlow가 새로 시작)
                    return;
                }
            }
        }

        private void ReachDayEnd()
        {
            HasReachedDayEnd = true;
            DayEndReached?.Invoke();
        }

        // 지금보다 뒤의 가장 가까운 경계: 다음 정시, 구간 시작·끝, 경고 시작·끝, 하루 끝.
        private float NextBoundary(float minute)
        {
            float best = DayEndMinute;
            Consider(ref best, minute, (Mathf.Floor(minute / 60f + Epsilon) + 1f) * 60f);
            foreach (var segment in _config.Segments)
            {
                Consider(ref best, minute, segment.StartMinute);
                Consider(ref best, minute, segment.EndMinute);
            }
            foreach (var window in _config.Warnings)
            {
                Consider(ref best, minute, window.StartMinute);
                Consider(ref best, minute, window.EndMinute);
            }
            return best;
        }

        private static void Consider(ref float best, float minute, float candidate)
        {
            if (candidate > minute + Epsilon && candidate < best) best = candidate;
        }

        private void Apply(float minute, bool forceNotify)
        {
            Minute = Mathf.Clamp(minute, 0f, DayEndMinute);
            int hour = Mathf.Clamp(Mathf.FloorToInt(Minute / 60f + Epsilon), 0, 24);
            var period = _config.SegmentAt(Minute).Period;
            var warning = _config.WarningAt(Minute);

            bool hourChanged = forceNotify || hour != Hour;
            bool periodChanged = forceNotify || period != Period;
            bool warningChanged = forceNotify || warning != Warning;
            Hour = hour;
            Period = period;
            Warning = warning;

            if (hourChanged) HourChanged?.Invoke(hour);
            if (periodChanged) PeriodChanged?.Invoke(period);
            if (warningChanged) WarningChanged?.Invoke();
            if (hourChanged || periodChanged) TimeChanged?.Invoke();
            MinuteChanged?.Invoke();
        }
    }
}
