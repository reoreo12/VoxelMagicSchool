// 시간 정지 참조 카운트(UI_SPEC 4-A-2, 13장). 전체 정지(timeScale 0)와 게임 시계만 정지를 따로 센다.
// 화면이 겹쳐 열려도 마지막 화면이 닫힐 때만 시간이 다시 흐른다. 실제 적용은 Gameplay가 Changed를 받아 한다.
using System;

namespace VoxelMagic.Core
{
    public enum PauseLevel
    {
        None,
        /// <summary>게임 안의 시각만 멈춘다. 월드 애니메이션·크리처 동작은 계속된다 (크리처 대화, 수업).</summary>
        GameClock,
        /// <summary>모든 게임 시간을 멈춘다(timeScale 0). UI 연출만 unscaled time으로 돈다 (책, 가방, 패널).</summary>
        Full
    }

    public sealed class TimePause
    {
        private int _clockCount;
        private int _fullCount;

        public event Action Changed;

        /// <summary>게임 시계가 멈춰 있는지 (전체 정지 포함).</summary>
        public bool IsClockPaused => _clockCount > 0 || _fullCount > 0;
        /// <summary>전체 정지(timeScale 0)여야 하는지.</summary>
        public bool IsFullyPaused => _fullCount > 0;

        public void Acquire(PauseLevel level)
        {
            if (level == PauseLevel.None) return;
            var before = State;
            if (level == PauseLevel.Full) _fullCount++;
            else _clockCount++;
            NotifyIfChanged(before);
        }

        public void Release(PauseLevel level)
        {
            if (level == PauseLevel.None) return;
            var before = State;
            if (level == PauseLevel.Full) _fullCount = Math.Max(0, _fullCount - 1);
            else _clockCount = Math.Max(0, _clockCount - 1);
            NotifyIfChanged(before);
        }

        private (bool, bool) State => (IsClockPaused, IsFullyPaused);

        private void NotifyIfChanged((bool, bool) before)
        {
            if (before != State) Changed?.Invoke();
        }
    }
}
