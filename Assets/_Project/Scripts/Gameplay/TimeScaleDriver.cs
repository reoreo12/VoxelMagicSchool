// TimePause의 "전체 정지"를 Time.timeScale에 반영한다(책·가방·패널이 열리면 0, 닫히면 원래 값).
// 부트스트랩이 씬마다 만들고 씬이 내려갈 때 Dispose해 timeScale이 0으로 남지 않게 한다.
using System;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class TimeScaleDriver : IDisposable
    {
        private readonly TimePause _pause;
        private readonly float _normalScale;

        public TimeScaleDriver(TimePause pause, float normalScale)
        {
            _pause = pause;
            _normalScale = normalScale;
            _pause.Changed += Apply;
            Apply();
        }

        public void Dispose()
        {
            _pause.Changed -= Apply;
            Time.timeScale = _normalScale;
        }

        private void Apply() => Time.timeScale = _pause.IsFullyPaused ? 0f : _normalScale;
    }
}
