// 게임 타이머 설정: 하루 구간(게임 시각·현실 초), 임박 경고 시각, 하루 시작 시각, 경비원 지연, 다음 날 시작 보정 범위.
// 시각은 "시:분"(ClockTime)으로 적고 코드에서는 하루 0시부터의 분으로 쓴다.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelMagic.Core
{
    /// <summary>임박 경고 종류 (UI 화살표 색·깜빡임이 구독).</summary>
    public enum TimeWarning
    {
        None,
        /// <summary>방과 후 → 밤 전환 임박 (기본 19:40~)</summary>
        NightSoon,
        /// <summary>밤의 끝 임박 (기본 23:40~)</summary>
        DayEndSoon
    }

    /// <summary>인스펙터에서 "16:00"처럼 적는 시각. 24:00까지 쓸 수 있다.</summary>
    [Serializable]
    public struct ClockTime
    {
        [SerializeField, Range(0, 24)] private int _hour;
        [SerializeField, Range(0, 59)] private int _minute;

        public ClockTime(int hour, int minute = 0)
        {
            _hour = hour;
            _minute = minute;
        }

        public int Hour => _hour;
        public int Minute => _minute;
        public int TotalMinutes => _hour * 60 + _minute;

        public override string ToString() => $"{_hour:00}:{_minute:00}";
    }

    /// <summary>하루 구간 하나: 시간대, 게임 시작·끝 시각, 그 구간이 흐르는 현실 시간(초).</summary>
    [Serializable]
    public struct DaySegment
    {
        [SerializeField] private TimeOfDay _period;
        [SerializeField] private ClockTime _start;
        [SerializeField] private ClockTime _end;
        [SerializeField, Min(1f), Tooltip("이 구간 전체가 흐르는 현실 시간(초)")] private float _realSeconds;

        public DaySegment(TimeOfDay period, ClockTime start, ClockTime end, float realSeconds)
        {
            _period = period;
            _start = start;
            _end = end;
            _realSeconds = realSeconds;
        }

        public TimeOfDay Period => _period;
        public int StartMinute => _start.TotalMinutes;
        public int EndMinute => _end.TotalMinutes;
        /// <summary>현실 1초에 흐르는 게임 분.</summary>
        public float MinutesPerSecond => Mathf.Max(1, EndMinute - StartMinute) / Mathf.Max(1f, _realSeconds);
    }

    /// <summary>경고가 켜지는 구간 (시작 ≤ 시각 &lt; 끝).</summary>
    [Serializable]
    public struct TimeWarningWindow
    {
        [SerializeField] private TimeWarning _warning;
        [SerializeField] private ClockTime _start;
        [SerializeField] private ClockTime _end;

        public TimeWarningWindow(TimeWarning warning, ClockTime start, ClockTime end)
        {
            _warning = warning;
            _start = start;
            _end = end;
        }

        public TimeWarning Warning => _warning;
        public int StartMinute => _start.TotalMinutes;
        public int EndMinute => _end.TotalMinutes;
    }

    [CreateAssetMenu(menuName = "VoxelMagic/Config/Time", fileName = "TimeConfig")]
    public sealed class TimeConfig : ScriptableObject
    {
        [Tooltip("하루 구간. 시각 순서대로 이어서 적는다 (마지막 구간의 끝 = 하루 끝, 경비원 시각)")]
        [SerializeField] private List<DaySegment> _segments = new List<DaySegment>
        {
            new DaySegment(TimeOfDay.Day, new ClockTime(16), new ClockTime(20), 210f),
            new DaySegment(TimeOfDay.Night, new ClockTime(20), new ClockTime(24), 150f)
        };

        [Tooltip("임박 경고 구간 (UI 화살표 색·깜빡임)")]
        [SerializeField] private List<TimeWarningWindow> _warnings = new List<TimeWarningWindow>
        {
            new TimeWarningWindow(TimeWarning.NightSoon, new ClockTime(19, 40), new ClockTime(20)),
            new TimeWarningWindow(TimeWarning.DayEndSoon, new ClockTime(23, 40), new ClockTime(24))
        };

        [Tooltip("보정이 없을 때 방과 후(하루)가 시작되는 시각")]
        [SerializeField] private ClockTime _dayStart = new ClockTime(16);
        [Tooltip("하루 끝까지 버텨 경비원에게 걸리면 다음 날 방과 후를 이만큼 늦게 시작 (분)")]
        [SerializeField] private int _guardDelayMinutes = 60;
        [Tooltip("다음 날 시작 보정 합계의 최소·최대 (분). 늦추기 최댓값 + 앞당기기 최댓값을 이 범위로 자른다")]
        [SerializeField] private int _minStartShiftMinutes = -90;
        [SerializeField] private int _maxStartShiftMinutes = 120;
        [Tooltip("프레임 하나가 아무리 길어도 시계에 넣는 최대 현실 초 (멈칫거림 대비, 배속 곱하기 전)")]
        [SerializeField, Min(0.01f)] private float _maxFrameSeconds = 0.25f;

        public IReadOnlyList<DaySegment> Segments => _segments;
        public IReadOnlyList<TimeWarningWindow> Warnings => _warnings;
        public int DayStartMinute => _dayStart.TotalMinutes;
        public int GuardDelayMinutes => _guardDelayMinutes;
        public int MinStartShiftMinutes => Mathf.Min(_minStartShiftMinutes, 0);
        public int MaxStartShiftMinutes => Mathf.Max(_maxStartShiftMinutes, 0);
        public float MaxFrameSeconds => _maxFrameSeconds;

        /// <summary>하루 끝(마지막 구간의 끝, 기본 24:00)의 분.</summary>
        public int DayEndMinute => _segments.Count > 0 ? _segments[_segments.Count - 1].EndMinute : 24 * 60;

        /// <summary>그 시각이 속한 구간. 첫 구간보다 이르면 첫 구간, 마지막보다 늦으면 마지막 구간.</summary>
        public DaySegment SegmentAt(float minute)
        {
            if (_segments.Count == 0) return new DaySegment(TimeOfDay.Day, new ClockTime(16), new ClockTime(24), 360f);
            for (int i = 0; i < _segments.Count; i++)
                if (minute < _segments[i].EndMinute) return _segments[i];
            return _segments[_segments.Count - 1];
        }

        public TimeWarning WarningAt(float minute)
        {
            foreach (var window in _warnings)
                if (minute >= window.StartMinute && minute < window.EndMinute) return window.Warning;
            return TimeWarning.None;
        }

        /// <summary>그 시간대가 시작되는 시(UI 룰렛 각도 계산용). 없으면 -1.</summary>
        public int PeriodStartHour(TimeOfDay period)
        {
            foreach (var segment in _segments)
                if (segment.Period == period) return segment.StartMinute / 60;
            return -1;
        }

        /// <summary>테스트·에디터 도구에서 값을 채운다.</summary>
        public void Set(IEnumerable<DaySegment> segments, IEnumerable<TimeWarningWindow> warnings, ClockTime dayStart, int guardDelayMinutes)
        {
            _segments = new List<DaySegment>(segments);
            _warnings = new List<TimeWarningWindow>(warnings);
            _dayStart = dayStart;
            _guardDelayMinutes = guardDelayMinutes;
        }
    }
}
