// 씬 이동 상태(이동 중인지, 도착 지점 ID). 씬이 바뀌어도 남도록 정적이며, 이동 중에는 게임플레이 입력을 막는 근거가 된다.
using System;
using UnityEngine;

namespace VoxelMagic.Core
{
    public static class SceneTravel
    {
        public static bool IsTraveling { get; private set; }
        public static string ArrivalSpawnId { get; private set; }

        /// <summary>true = 이동 시작(입력 막기), false = 이동 끝(입력 풀기).</summary>
        public static event Action<bool> TravelingChanged;

        /// <summary>이미 이동 중이면 false.</summary>
        public static bool Begin(string arrivalSpawnId)
        {
            if (IsTraveling) return false;
            IsTraveling = true;
            ArrivalSpawnId = arrivalSpawnId;
            TravelingChanged?.Invoke(true);
            return true;
        }

        public static void End()
        {
            if (!IsTraveling) return;
            IsTraveling = false;
            ArrivalSpawnId = null;
            TravelingChanged?.Invoke(false);
        }

        // 도메인 리로드를 끈 플레이 모드에서도 이전 상태가 남지 않게.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            IsTraveling = false;
            ArrivalSpawnId = null;
            TravelingChanged = null;
        }
    }

    /// <summary>화면 전환 연출. coverage 0 = 다 보임, 1 = 다 가림. center = 원 중심(뷰포트 0~1).</summary>
    public interface IScreenFade
    {
        void Configure(SceneTransitionConfig config);
        void SetFade(float coverage, Vector2 center);
    }
}
