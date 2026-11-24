// 게임 안 날짜·시각을 UI 시간대 HUD가 읽는 창구(UI_SPEC 8장). 시간이 흐르는 규칙(속도·구간·정지)은 게임 규칙 쪽에서 정한다.
using System;

namespace VoxelMagic.Core
{
    public interface IGameTime
    {
        /// <summary>며칠째인지 (1부터).</summary>
        int Day { get; }

        /// <summary>정시 (0~24). 분은 쓰지 않는다(8-1).</summary>
        int Hour { get; }

        /// <summary>지금 시간대. Day = 방과후, Night = 밤.</summary>
        TimeOfDay Period { get; }

        event Action TimeChanged;
    }
}
