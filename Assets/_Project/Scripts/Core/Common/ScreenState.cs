// 열린 창과 의뢰 토글 접힘 상태. 창이 열려 있으면 플레이어 이동을 막는 기준이 된다.
using System;

namespace VoxelMagic.Core
{
    public enum GameWindow
    {
        None,
        RequestBoard,
        CreatureDialog
    }

    public sealed class ScreenState
    {
        public event Action<GameWindow> WindowChanged;
        public event Action<bool> TrackerToggled;
        public event Action Submitted;

        public GameWindow Current { get; private set; }
        public bool IsModalOpen => Current != GameWindow.None;
        public bool IsTrackerCollapsed { get; private set; }

        public void Open(GameWindow window)
        {
            if (Current == window) return;
            Current = window;
            WindowChanged?.Invoke(window);
        }

        public void Close(GameWindow window)
        {
            if (Current != window || window == GameWindow.None) return;
            Current = GameWindow.None;
            WindowChanged?.Invoke(GameWindow.None);
        }

        public void CloseCurrent() => Close(Current);

        public void ToggleTracker()
        {
            IsTrackerCollapsed = !IsTrackerCollapsed;
            TrackerToggled?.Invoke(IsTrackerCollapsed);
        }

        /// <summary>창이 열린 상태에서 E를 누름 (대사 넘기기 등).</summary>
        public void Submit()
        {
            if (IsModalOpen) Submitted?.Invoke();
        }
    }
}
