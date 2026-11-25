// 8일 일정표: 날짜마다 날 종류(스토리·이벤트·자유)와 시계가 멈춘 날에 보여 줄 시각, 시험 날, 새 게임 시작 날을 데이터로 정한다.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelMagic.Core
{
    public enum DayKind
    {
        /// <summary>스토리 날(1·7·8일): 시계가 흐르지 않는다.</summary>
        Story,
        /// <summary>이벤트 날(3·4·5일): 이벤트 중 멈춤, 끝나면 방과 후부터 흐른다 (이번에는 이벤트를 건너뛴다).</summary>
        Event,
        /// <summary>자유 날: 방과 후부터 흐른다.</summary>
        Free
    }

    [Serializable]
    public struct CalendarDay
    {
        [SerializeField] private string _label;
        [SerializeField] private DayKind _kind;
        [Tooltip("스토리 날처럼 시계가 멈춘 날 HUD에 보일 시각")]
        [SerializeField] private ClockTime _heldTime;

        public CalendarDay(string label, DayKind kind, ClockTime heldTime)
        {
            _label = label;
            _kind = kind;
            _heldTime = heldTime;
        }

        public string Label => _label;
        public DayKind Kind => _kind;
        public int HeldMinute => _heldTime.TotalMinutes;
    }

    [CreateAssetMenu(menuName = "VoxelMagic/Config/Calendar", fileName = "CalendarConfig")]
    public sealed class CalendarConfig : ScriptableObject
    {
        [Tooltip("1일부터 차례로. 목록보다 뒤의 날은 없다 (마지막 날이 끝나면 게임 끝)")]
        [SerializeField] private List<CalendarDay> _days = new List<CalendarDay>
        {
            new CalendarDay("크리처와 만남, 튜토리얼", DayKind.Story, new ClockTime(20)),
            new CalendarDay("자유 플레이", DayKind.Free, new ClockTime(16)),
            new CalendarDay("지각 레이스", DayKind.Event, new ClockTime(16)),
            new CalendarDay("마법 대결", DayKind.Event, new ClockTime(16)),
            new CalendarDay("석고상 보스전", DayKind.Event, new ClockTime(16)),
            new CalendarDay("마지막 의뢰 전달", DayKind.Free, new ClockTime(16)),
            new CalendarDay("불꽃 축제 (일과 없음)", DayKind.Story, new ClockTime(20)),
            new CalendarDay("급수 시험 → 엔딩", DayKind.Story, new ClockTime(16))
        };
        [Tooltip("급수 시험 날 (D-n 계산 기준)")]
        [SerializeField, Min(1)] private int _examDay = 8;
        [Tooltip("새 게임이 시작되는 날 (튜토리얼이 생기기 전 시험용으로 2를 넣어 둘 수 있다)")]
        [SerializeField, Min(1)] private int _newGameStartDay = 1;

        public int DayCount => _days.Count;
        public int ExamDay => _examDay;
        public int NewGameStartDay => Mathf.Clamp(_newGameStartDay, 1, Mathf.Max(1, _days.Count));

        public bool Has(int day) => day >= 1 && day <= _days.Count;

        /// <summary>그날 정보. 목록 밖이면 자유 날로 본다.</summary>
        public CalendarDay Get(int day) => Has(day) ? _days[day - 1] : new CalendarDay(string.Empty, DayKind.Free, new ClockTime(16));

        public void Set(IEnumerable<CalendarDay> days, int examDay, int newGameStartDay)
        {
            _days = new List<CalendarDay>(days);
            _examDay = examDay;
            _newGameStartDay = newGameStartDay;
        }
    }
}
