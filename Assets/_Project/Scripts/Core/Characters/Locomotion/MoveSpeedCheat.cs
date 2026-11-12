// 개발용 임시 치트: 걷기·달리기 목표 속도 배율 (모든 맵 공용, 지금은 이 상태 머신을 플레이어만 씀). 키 입력은 Gameplay의 MoveSpeedCheatKey.
using System;

namespace VoxelMagic.Core
{
    public static class MoveSpeedCheat
    {
        public static float Multiplier { get; private set; } = 1f;
        public static bool IsOn => Multiplier != 1f;

        public static event Action Changed;

        public static void Set(float multiplier)
        {
            if (multiplier <= 0f || multiplier == Multiplier) return;
            Multiplier = multiplier;
            Changed?.Invoke();
        }

        public static void Toggle(float onMultiplier) => Set(IsOn ? 1f : onMultiplier);

        /// <summary>플레이 시작 때 초기화 (도메인 리로드를 꺼도 이전 플레이 값이 남지 않게).</summary>
        public static void Reset()
        {
            Multiplier = 1f;
            Changed = null;
        }
    }
}
