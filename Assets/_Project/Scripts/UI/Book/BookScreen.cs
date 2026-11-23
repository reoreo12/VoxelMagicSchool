// 책(메뉴) 팝업 화면(UI_SPEC 5-2): ESC(열린 화면이 없을 때)는 메뉴 탭으로 연다. 책 아이콘·HUD 의뢰 항목은 BookState에 탭을 정한 뒤 연다.
// 열려 있는 동안 시간 정지·입력 막기는 ScreenPolicy(전체 정지)가 맡는다.
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class BookScreen : UiScreen, IGameContextBinder
    {
        [SerializeField] private BookView _view;

        private ScreenStack _screens;
        private BookState _book;

        public void Bind(GameContext context)
        {
            _screens = context.Screens;
            _book = context.Book;
            _screens.MenuRequested += OpenMenu;
        }

        private void OnDestroy()
        {
            if (_screens != null) _screens.MenuRequested -= OpenMenu;
        }

        private void OpenMenu()
        {
            _book.PrepareForMenu();
            _screens.Open(GameScreen.Book);
        }

        protected override void OnShown()
        {
            // 화면 크기가 바뀌었을 수 있으니 가운데 배치부터 다시 한다(Apply 안에서 Refresh까지).
            if (_view != null) _view.ApplyTheme();
        }
    }
}
