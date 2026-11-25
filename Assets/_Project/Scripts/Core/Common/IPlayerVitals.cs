// 마력·기력 값을 UI 상태 바가 읽는 창구(UI_SPEC 7-2). 값의 규칙(최대치·회복·소모)은 게임 규칙 문서에서 정한다.
using System;

namespace VoxelMagic.Core
{
    public interface IPlayerVitals
    {
        /// <summary>마력 0~1.</summary>
        float ManaRatio { get; }

        /// <summary>기력 0~1.</summary>
        float StaminaRatio { get; }

        event Action VitalsChanged;
    }
}
