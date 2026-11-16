// 씬이 바뀌어도 남는 플레이어 진행 상태 보관소 (SceneTravel과 같은 정적 방식). 처음 텃밭에 들어올 때 만들어지거나 저장 파일에서 불러온다.
// 저장 직전에는 Capturing으로 씬 소속 데이터(밭)를 PlayerProgress에 옮겨 담을 기회를 준다.
using System;
using UnityEngine;

namespace VoxelMagic.Core
{
    public static class GameSession
    {
        public static PlayerProgress Progress { get; private set; }
        public static bool HasProgress => Progress != null;

        /// <summary>저장·씬 이동 직전. 씬 소속 데이터를 Progress에 옮겨 담는다.</summary>
        public static event Action<PlayerProgress> Capturing;

        public static void Begin(PlayerProgress progress) =>
            Progress = progress ?? throw new ArgumentNullException(nameof(progress));

        public static void Capture()
        {
            if (Progress != null) Capturing?.Invoke(Progress);
        }

        /// <summary>시험용·새 게임: 진행 상태를 비운다.</summary>
        public static void Clear() => Progress = null;

        // 도메인 리로드를 끈 플레이 모드에서도 이전 플레이의 진행 상태가 남지 않게.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Progress = null;
            Capturing = null;
        }
    }
}
