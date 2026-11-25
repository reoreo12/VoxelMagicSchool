// 게임 시계를 매 프레임 흐르게 하는 얇은 구동기. timeScale과 무관한 unscaledDeltaTime을 쓰고, 화면 멈춤(씬)과 시계 잡기(진행 상태)가 모두 없을 때만 흐른다.
// 맵 이동(페이드) 중에는 "맵 이동" 원인으로 시계를 잡는다. GameBootstrap이 씬마다 붙인다.
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class GameClockDriver : MonoBehaviour
    {
        private GameClock _clock;
        private TimePause _scenePause;

        public void Initialize(GameClock clock, TimePause scenePause)
        {
            _clock = clock;
            _scenePause = scenePause;
        }

        /// <summary>지금 시계가 흐를 수 있는지 (디버그 표시용).</summary>
        public bool IsRunning => _clock != null && !_clock.IsHeld && (_scenePause == null || !_scenePause.IsClockPaused);

        private void Update()
        {
            if (_clock == null) return;
            _clock.Holds.Set(SceneTravel.IsTraveling, PauseLevel.GameClock, PauseCauses.SceneTravel);
            if (!IsRunning) return;
            _clock.Advance(Mathf.Min(Time.unscaledDeltaTime, _clock.Config.MaxFrameSeconds));
        }
    }
}
