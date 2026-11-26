// 확인 창(예/아니요): ConfirmChannel.Ask를 받으면 화면 스택에 Confirm을 열고, 닫힐 때 답을 돌려준다(아니요·ESC·바깥 클릭 = 아니요).
// 깔끔한 종이 9-slice 바탕 + 메시지(가운데, 낱말 단위 줄바꿈) + 책 페이지 버튼 두 개. 창 폭은 메시지에 맞추고 화면 가운데 정수 px에 둔다.
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class ConfirmScreen : UiScreen, IGameContextBinder
    {
        [SerializeField] private UiTheme _theme;
        [SerializeField] private Image _window;
        [SerializeField] private ThemedText _message;
        [SerializeField] private BookMenuButton _yes;
        [SerializeField] private BookMenuButton _no;

        private ConfirmChannel _channel;
        private ScreenStack _screens;
        private string _messageText = string.Empty;
        private string _yesText = GameTexts.Yes;
        private string _noText = GameTexts.No;
        private bool _answer;

        public void Bind(GameContext context)
        {
            _channel = context.Confirm;
            _screens = context.Screens;
            _channel.Asked += OnAsked;
            _screens.Closed += OnClosed;
            _yes.Clicked -= OnYes;
            _yes.Clicked += OnYes;
            _no.Clicked -= OnNo;
            _no.Clicked += OnNo;
        }

        private void OnDestroy()
        {
            if (_channel != null) _channel.Asked -= OnAsked;
            if (_screens != null) _screens.Closed -= OnClosed;
        }

        private void OnAsked(string message, string yes, string no)
        {
            if (this == null) { OnDestroy(); return; } // 꺼진 채 씬이 내려가 OnDestroy가 불리지 않은 구독자
            _answer = false;
            _messageText = message ?? string.Empty;
            _yesText = string.IsNullOrEmpty(yes) ? GameTexts.Yes : yes;
            _noText = string.IsNullOrEmpty(no) ? GameTexts.No : no;
            if (!_screens.Open(GameScreen.Confirm)) _channel.Answer(false);
        }

        private void OnClosed(GameScreen screen)
        {
            if (this == null) { OnDestroy(); return; }
            if (screen == GameScreen.Confirm) _channel.Answer(_answer);
        }

        private void OnYes()
        {
            _answer = true;
            _screens.Close(GameScreen.Confirm);
        }

        private void OnNo()
        {
            _answer = false;
            _screens.Close(GameScreen.Confirm);
        }

        protected override void OnShown() => Layout();

        // 메시지 폭 → 창 폭(범위 안) → 줄바꿈 → 창 높이 → 화면 가운데. 모두 정수 px.
        private void Layout()
        {
            if (_theme == null) return;
            var values = _theme.Confirm;
            int pad = values.Padding;
            var label = _message.GetComponent<TMP_Text>();
            _message.SetStyle(values.MessageStyle);
            label.alignment = TextAlignmentOptions.Top;
            label.textWrappingMode = TextWrappingModes.NoWrap;

            int widest = 0;
            foreach (var line in _messageText.Split('\n'))
                widest = Mathf.Max(widest, Mathf.CeilToInt(label.GetPreferredValues(line).x));
            var range = values.WidthRange;
            var button = values.ButtonSize;
            int buttons = button.x * 2 + values.ButtonGap;
            int width = Mathf.Clamp(Mathf.Max(widest, buttons) + pad * 2, range.x, Mathf.Max(range.x, range.y));
            int inner = width - pad * 2;

            var wrapped = new System.Text.StringBuilder();
            foreach (var line in _messageText.Split('\n'))
            {
                if (wrapped.Length > 0) wrapped.Append('\n');
                wrapped.Append(UiTextWrap.ByWords(label, line, inner));
            }
            _message.SetText(wrapped.ToString());
            var style = _theme.TextStyle(values.MessageStyle);
            int lineHeight = style != null ? style.LineHeightFrom(_theme.Fonts) : 16;
            int lines = 1;
            foreach (char c in wrapped.ToString()) if (c == '\n') lines++;
            int messageHeight = lines * lineHeight;
            int height = pad + messageHeight + values.MessageGap + button.y + pad;

            var parent = Vector2Int.RoundToInt(Root.rect.size);
            var windowRect = _window.rectTransform;
            PixelLayout.PlaceTopLeft(windowRect, PixelLayout.CenterOffset(parent.x, width), PixelLayout.CenterOffset(parent.y, height), width, height);
            _window.sprite = values.Panel;
            _window.type = Image.Type.Sliced;
            _window.raycastTarget = true; // 창 안 클릭이 바깥(닫기)으로 새지 않게

            PixelLayout.PlaceTopLeft((RectTransform)_message.transform, pad, pad, inner, messageHeight);

            int buttonX = (width - buttons) / 2, buttonY = pad + messageHeight + values.MessageGap;
            _yes.Show(_yesText, _theme.Book, buttonX, buttonY, button.x, button.y);
            _no.Show(_noText, _theme.Book, buttonX + button.x + values.ButtonGap, buttonY, button.x, button.y);
        }
    }
}
