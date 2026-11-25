// 책의 마지막 탭 "나가기/종료 메뉴"(UI_SPEC 5-2, 13장): 왼쪽 = 장 표지(아이콘·이름·한 줄), 오른쪽 = 제목 + 메인 화면으로 / 게임 종료 / 책 닫기 버튼.
// 지금은 배치만: 책 닫기만 동작하고, 메인 화면·게임 종료는 기능이 생기면 MainMenuRequested·QuitRequested에 연결한다(확인 창 [잠정] 포함).
using System;
using UnityEngine;
using UnityEngine.UI;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class MenuBookPage : BookPage, IGameContextBinder
    {
        [SerializeField, Tooltip("왼쪽 페이지 장 표지")] private BookCoverView _cover;
        [SerializeField, Tooltip("오른쪽 페이지 제목")] private ThemedText _header;
        [SerializeField] private Image _headerLine;
        [SerializeField] private BookMenuButton _mainMenu;
        [SerializeField] private BookMenuButton _quit;
        [SerializeField] private BookMenuButton _close;

        private ScreenStack _screens;

        /// <summary>메인 화면으로 나가기를 눌렀을 때 (씬 이동 기능이 생기면 연결).</summary>
        public event Action MainMenuRequested;
        /// <summary>게임 종료를 눌렀을 때 (종료 기능이 생기면 연결).</summary>
        public event Action QuitRequested;

        public void Bind(GameContext context)
        {
            _screens = context.Screens;
            Hook(_mainMenu, OnMainMenu);
            Hook(_quit, OnQuit);
            Hook(_close, OnClose);
        }

        private static void Hook(BookMenuButton button, Action action)
        {
            if (button == null) return;
            button.Clicked -= action;
            button.Clicked += action;
        }

        private void OnMainMenu()
        {
            if (MainMenuRequested != null) MainMenuRequested.Invoke();
            else Debug.Log("[UI] 메인 화면으로 나가기: 아직 기능이 없습니다.");
        }

        private void OnQuit()
        {
            if (QuitRequested != null) QuitRequested.Invoke();
            else Debug.Log("[UI] 게임 종료: 아직 기능이 없습니다.");
        }

        private void OnClose() => _screens?.Close(GameScreen.Book);

        // 펼쳐질 때 다시 놓는다: 숨겨진 동안은 글자 모양(textBounds)을 잴 수 없어 가운데 맞춤이 정확하지 않다.
        public override void OnOpened() => Relayout();

        protected override void Relayout()
        {
            var theme = Theme;
            if (theme == null || Left == null || Right == null || _header == null) return;
            var values = theme.Book;
            var texts = theme.Texts;

            if (_cover != null)
                _cover.Show(values.Tab(Tab)?.Icon, texts?.Get(UiTextKeys.BookMenuTitle), texts?.Get(UiTextKeys.BookMenuCaption), theme, LeftSize);

            int width = RightSize.x, height = RightSize.y;
            int headerLine = LineOf(theme, UiTextStyle.HudTitle);
            _header.SetStyle(UiTextStyle.HudTitle);
            if (texts != null) _header.SetText(texts.Get(UiTextKeys.BookMenuHeader));
            PixelLayout.PlaceTopLeft((RectTransform)_header.transform, 0, 0, width, headerLine);
            int y = headerLine + values.HeaderGap;
            if (_headerLine != null)
            {
                _headerLine.sprite = null;
                _headerLine.color = values.LineColor;
                _headerLine.raycastTarget = false;
                PixelLayout.PlaceTopLeft(_headerLine.rectTransform, 0, y, width, 1);
            }
            y += 1 + values.SectionGap;

            // 위에서부터 메인 화면 / 게임 종료, 책 닫기는 페이지 맨 아래에 떨어뜨려 둔다.
            int h = values.MenuButtonHeight;
            if (_mainMenu != null) _mainMenu.Show(texts?.Get(UiTextKeys.BookMenuMainMenu), values, 0, y, width, h);
            y += h + values.MenuButtonGap;
            if (_quit != null) _quit.Show(texts?.Get(UiTextKeys.BookMenuQuit), values, 0, y, width, h);
            if (_close != null) _close.Show(texts?.Get(UiTextKeys.BookMenuClose), values, 0, height - h, width, h);
        }

        private static int LineOf(UiTheme theme, UiTextStyle id)
        {
            var style = theme.TextStyle(id);
            return style != null ? style.LineHeightFrom(theme.Fonts) : 16;
        }
    }
}
