// 화면 스택(Core)과 팝업 화면(UI)을 잇는다. 스택이 열고 닫으면 화면을 보이고 숨기며 맨 위로 올린다.
// 뒤쪽 입력을 막는 화면이 열려 있으면 그 화면 바로 아래에 입력 막기 판을 둔다(딤은 테마 색으로 나중에 켤 수 있음).
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class UiScreenRouter : MonoBehaviour, IGameContextBinder
    {
        [SerializeField] private UiTheme _theme;
        [SerializeField, Tooltip("팝업 화면들이 들어 있는 부모(입력 막기 판도 여기 둔다)")] private RectTransform _screenParent;
        [SerializeField] private Image _inputBlocker;

        private readonly Dictionary<GameScreen, IUiScreen> _screens = new Dictionary<GameScreen, IUiScreen>();
        private ScreenStack _stack;

        public void Bind(GameContext context)
        {
            _stack = context.Screens;
            _screens.Clear();
            foreach (var screen in _screenParent.GetComponentsInChildren<UiScreen>(true))
            {
                if (_screens.ContainsKey(screen.Id))
                {
                    Debug.LogWarning($"[UI] 같은 화면이 두 개 있습니다: {screen.Id}", screen);
                    continue;
                }
                _screens.Add(screen.Id, screen);
                screen.HideImmediate();
            }
            _inputBlocker.color = _theme.Modal.BlockerColor;
            _inputBlocker.raycastTarget = true;
            // 팝업 바깥(입력 막기 판)을 누르면 ESC처럼 맨 위 화면을 닫는다(10-5 결정을 책 의뢰 탭에도 적용).
            var click = _inputBlocker.GetComponent<UiRaycastBlocker>();
            if (click == null) click = _inputBlocker.gameObject.AddComponent<UiRaycastBlocker>();
            click.Clicked -= OnBlockerClicked;
            click.Clicked += OnBlockerClicked;

            _stack.Opened += OnOpened;
            _stack.Closed += OnClosed;
            _stack.Changed += UpdateBlocker;
            foreach (var open in _stack.Screens) OnOpened(open);
            UpdateBlocker();
        }

        /// <summary>이 화면이 만들어져 있는지 (아직 없는 화면을 열어 빈 채로 시간만 멈추는 일을 막는다).</summary>
        public bool HasScreen(GameScreen id) => _screens.ContainsKey(id);

        private void OnDestroy()
        {
            if (_stack == null) return;
            _stack.Opened -= OnOpened;
            _stack.Closed -= OnClosed;
            _stack.Changed -= UpdateBlocker;
        }

        private void OnBlockerClicked() => _stack?.Cancel();

        private void OnOpened(GameScreen id)
        {
            if (!_screens.TryGetValue(id, out var screen)) return;
            screen.Root.SetAsLastSibling();
            screen.Show();
        }

        private void OnClosed(GameScreen id)
        {
            if (_screens.TryGetValue(id, out var screen)) screen.Hide();
        }

        // 위에서부터 보면서 뒤쪽 입력을 막는 첫 화면을 찾고, 판을 그 화면 바로 아래 형제로 둔다.
        private void UpdateBlocker()
        {
            var opened = _stack.Screens;
            for (int i = opened.Count - 1; i >= 0; i--)
            {
                if (!_stack.PolicyOf(opened[i]).BlocksInputBehind) continue;
                if (!_screens.TryGetValue(opened[i], out var screen)) continue;
                _inputBlocker.gameObject.SetActive(true);
                var blocker = _inputBlocker.rectTransform;
                int target = screen.Root.GetSiblingIndex();
                if (blocker.GetSiblingIndex() < target) target--; // 판이 이미 앞에 있으면 빠진 자리만큼 당겨진다
                blocker.SetSiblingIndex(target);
                return;
            }
            _inputBlocker.gameObject.SetActive(false);
        }
    }
}
