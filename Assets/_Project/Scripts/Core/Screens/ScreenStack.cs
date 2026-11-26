// 열린 팝업 화면 스택(UI_SPEC 4-A-2). 화면을 열고 닫고, ESC를 맨 위부터 처리하고, 시간 정지를 참조 카운트로 맡긴다.
// Gameplay는 열기 요청과 상태 확인만, UI는 Opened/Closed를 받아 화면을 보이고 숨긴다. 화면은 직접 ESC를 읽지 않는다.
using System;
using System.Collections.Generic;

namespace VoxelMagic.Core
{
    public sealed class ScreenStack
    {
        private readonly List<GameScreen> _stack = new List<GameScreen>();
        private readonly Dictionary<GameScreen, ScreenPolicy> _overrides = new Dictionary<GameScreen, ScreenPolicy>();
        private readonly TimePause _pause;

        public ScreenStack(TimePause pause) => _pause = pause ?? throw new ArgumentNullException(nameof(pause));

        public event Action<GameScreen> Opened;
        public event Action<GameScreen> Closed;
        /// <summary>스택이 바뀔 때마다 (열기·닫기 후).</summary>
        public event Action Changed;
        /// <summary>열린 화면이 없을 때 ESC를 누름 → 책(메뉴 탭)을 연다 (5-2).</summary>
        public event Action MenuRequested;
        /// <summary>화면이 열린 상태에서 E/Space를 누름 (대사 넘기기 등).</summary>
        public event Action Submitted;
        /// <summary>ESC를 눌렀지만 맨 위 화면이 ESC로 닫히지 않는 화면일 때 (대사창 선택지의 "그만두기" 등).</summary>
        public event Action CancelIgnored;
        /// <summary>화면이 열린 상태에서 위(−1)·아래(+1) 이동 키 (선택지 고르기).</summary>
        public event Action<int> Navigated;

        public int Count => _stack.Count;
        public bool HasAny => _stack.Count > 0;
        public GameScreen Top => _stack.Count > 0 ? _stack[_stack.Count - 1] : GameScreen.None;
        public IReadOnlyList<GameScreen> Screens => _stack;

        /// <summary>열린 화면 중 하나라도 플레이어 조작을 막는지.</summary>
        public bool BlocksGameplay
        {
            get
            {
                foreach (var screen in _stack)
                    if (PolicyOf(screen).BlocksGameplay) return true;
                return false;
            }
        }

        public bool IsOpen(GameScreen screen) => _stack.Contains(screen);

        public ScreenPolicy PolicyOf(GameScreen screen)
            => _overrides.TryGetValue(screen, out var policy) ? policy : ScreenPolicy.DefaultFor(screen);

        /// <summary>기본 동작과 다르게 쓰고 싶은 화면만 등록한다. 열려 있는 동안에는 바꾸지 않는다.</summary>
        public void OverridePolicy(GameScreen screen, ScreenPolicy policy)
        {
            if (IsOpen(screen)) throw new InvalidOperationException($"{screen}이(가) 열려 있는 동안에는 동작을 바꿀 수 없습니다.");
            _overrides[screen] = policy;
        }

        /// <summary>맨 위에 연다. 이미 열려 있으면 아무것도 하지 않고 false.</summary>
        public bool Open(GameScreen screen)
        {
            if (screen == GameScreen.None || IsOpen(screen)) return false;
            _stack.Add(screen);
            _pause.Acquire(PolicyOf(screen).Pause, PauseCauses.ForScreen(screen));
            Opened?.Invoke(screen);
            Changed?.Invoke();
            return true;
        }

        /// <summary>스택 어디에 있든 닫는다. 열려 있지 않으면 false.</summary>
        public bool Close(GameScreen screen)
        {
            int index = _stack.LastIndexOf(screen);
            if (index < 0) return false;
            _stack.RemoveAt(index);
            _pause.Release(PauseCauses.ForScreen(screen));
            Closed?.Invoke(screen);
            Changed?.Invoke();
            return true;
        }

        public void CloseAll()
        {
            while (_stack.Count > 0) Close(Top);
        }

        /// <summary>
        /// ESC. 맨 위 화면이 ESC로 닫히는 화면이면 닫고 true. 맨 위 화면이 ESC를 받지 않으면 아무것도 하지 않고 true.
        /// 열린 화면이 없으면 false (호출한 쪽이 낚시 취소 등을 먼저 처리한 뒤 RequestMenu를 부른다).
        /// </summary>
        public bool Cancel()
        {
            if (!HasAny) return false;
            if (PolicyOf(Top).ClosesOnCancel) Close(Top);
            else CancelIgnored?.Invoke();
            return true;
        }

        public void RequestMenu()
        {
            if (!HasAny) MenuRequested?.Invoke();
        }

        public void Submit()
        {
            if (HasAny) Submitted?.Invoke();
        }

        public void Navigate(int step)
        {
            if (HasAny && step != 0) Navigated?.Invoke(step > 0 ? 1 : -1);
        }
    }
}
