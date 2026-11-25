// 게임 안 날짜·시각을 UI 시간대 HUD가 읽는 창구(UI_SPEC 8장). 시간이 흐르는 규칙(속도·구간·정지)은 GameClock·DayFlow가 정한다.
using System;

namespace VoxelMagic.Core
{
    public interface IGameTime
    {
        /// <summary>며칠째인지 (1부터).</summary>
        int Day { get; }

        /// <summary>정시 (0~24). 분은 쓰지 않는다(8-1).</summary>
        int Hour { get; }

        /// <summary>하루 0시부터의 분(소수). 룰렛 화살표를 부드럽게 돌리는 데만 쓰고, 분을 글자로 보여 주지는 않는다.</summary>
        float Minute { get; }

        /// <summary>지금 시간대. Day = 방과후, Night = 밤.</summary>
        TimeOfDay Period { get; }

        /// <summary>시험까지 남은 날 (시험 날 0 = D-Day).</summary>
        int DaysUntilExam { get; }

        /// <summary>지금 켜진 임박 경고.</summary>
        TimeWarning Warning { get; }

        /// <summary>스토리·튜토리얼·맵 이동 등 화면 밖 이유로 시계가 잡혀 있는지 (화면 멈춤은 TimePause로 따로 본다).</summary>
        bool IsHeld { get; }

        /// <summary>그 시간대가 시작되는 시 (룰렛 각도 계산용). 없으면 -1.</summary>
        int PeriodStartHour(TimeOfDay period);

        /// <summary>하루 끝 시 (기본 24).</summary>
        int DayEndHour { get; }

        /// <summary>날짜·정시·시간대 중 하나가 바뀔 때.</summary>
        event Action TimeChanged;

        event Action WarningChanged;

        event Action HeldChanged;
    }
}
