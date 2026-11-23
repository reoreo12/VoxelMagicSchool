// 화면 왼쪽 위 책·가방 버튼(UI_SPEC 5-4, 7-4). 책은 책 화면을(마지막에 보던 탭), 가방은 확장 인벤토리를 화면 스택에 연다.
// 아직 만들어지지 않은 화면은 열지 않는다(빈 화면으로 시간만 멈추지 않게). 위치·간격은 테마 값(정수 px).
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class TopLeftMenuView : ThemedWidget, IGameContextBinder
    {
        [SerializeField] private ThemedButton _book;
        [SerializeField] private ThemedButton _bag;
        [SerializeField, Tooltip("열 화면이 만들어져 있는지 확인용")] private UiScreenRouter _router;

        private ScreenStack _screens;
        private BookState _bookState;

        public void Bind(GameContext context)
        {
            _screens = context.Screens;
            _bookState = context.Book;
            _book.Button.onClick.AddListener(() => OpenIfReady(GameScreen.Book));
            _bag.Button.onClick.AddListener(() => OpenIfReady(GameScreen.Inventory));
        }

        private void OpenIfReady(GameScreen screen)
        {
            if (_screens == null || _screens.HasAny) return; // 다른 팝업이 열려 있으면 입력 막기 판이 먼저 받는다
            if (_router != null && !_router.HasScreen(screen))
            {
                Debug.Log($"[UI] {screen} 화면은 아직 만들어지지 않았습니다.");
                return;
            }
            if (screen == GameScreen.Book) _bookState?.PrepareForIcon(); // 책 아이콘은 마지막에 직접 고른 탭으로(5-2)
            _screens.Open(screen);
        }

        protected override void Apply(UiTheme theme)
        {
            var values = theme.TopLeft;
            var root = (RectTransform)transform;
            PixelLayout.PlaceTopLeft(root, values.Margin, values.Margin, 0, 0);

            if (_book == null || _bag == null) return;
            // 아이콘은 정수 배로만 키운다(도트가 고르게 커짐). 배수는 버튼에 넣어 두어 버튼이 다시 그려져도 유지된다.
            _book.SetSizeScale(values.IconScale);
            _bag.SetSizeScale(values.IconScale);
            _book.ApplyTheme();
            _bag.ApplyTheme();
            var bookSize = ((RectTransform)_book.transform).sizeDelta;
            var bagSize = ((RectTransform)_bag.transform).sizeDelta;
            PixelLayout.PlaceTopLeft((RectTransform)_book.transform, 0, 0, (int)bookSize.x, (int)bookSize.y);
            PixelLayout.PlaceTopLeft((RectTransform)_bag.transform, (int)bookSize.x + values.IconGap, 0, (int)bagSize.x, (int)bagSize.y);
            root.sizeDelta = new Vector2(bookSize.x + values.IconGap + bagSize.x, Mathf.Max(bookSize.y, bagSize.y));
        }
    }
}
