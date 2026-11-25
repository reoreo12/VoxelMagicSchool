// TimePause의 "전체 정지"를 Time.timeScale에 반영한다(책·가방·패널이 열리면 0, 닫히면 원래 값 × 개발용 월드 배속).
// 부트스트랩이 씬마다 만들고 씬이 내려갈 때 Dispose해 timeScale이 0으로 남지 않게 한다.
using System;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class TimeScaleDriver : IDisposable
    {
        private static float s_debugWorldSpeed = 1f;
        private static event Action DebugWorldSpeedChanged;

        private readonly TimePause _pause;
        private readonly float _normalScale;

        public TimeScaleDriver(TimePause pause, float normalScale)
        {
            _pause = pause;
            _normalScale = normalScale;
            _pause.Changed += Apply;
            DebugWorldSpeedChanged += Apply;
            Apply();
        }

        /// <summary>개발용(치트): 월드 시간(timeScale)도 배속. 촬영 때 반복 구간을 빠르게 넘기는 용도. 평소 1.</summary>
        public static float DebugWorldSpeed
        {
            get => s_debugWorldSpeed;
            set
            {
                s_debugWorldSpeed = Mathf.Clamp(value, 0.1f, 100f);
                DebugWorldSpeedChanged?.Invoke();
            }
        }

        public void Dispose()
        {
            _pause.Changed -= Apply;
            DebugWorldSpeedChanged -= Apply;
            Time.timeScale = _normalScale;
        }

        private void Apply() => Time.timeScale = _pause.IsFullyPaused ? 0f : _normalScale * s_debugWorldSpeed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            s_debugWorldSpeed = 1f;
            DebugWorldSpeedChanged = null;
        }
    }
}
