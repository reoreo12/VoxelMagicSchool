// 예/아니요 임시 확인 창. ConfirmChannel.Ask를 받으면 화면 스택에 Confirm을 열고, 답을 채널에 돌려준다(닫기·ESC = 아니요).
// UI_Root가 없는 씬(Prototype_FarmFishing)에만 남은 예비 창. 정식 확인 창은 UI_Root의 ConfirmScreen(UI_Confirm)이다. 모양은 코드로 만든다.
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class ConfirmDialog : UIPanel, IGameContextBinder
    {
        [SerializeField] private TMP_FontAsset _font;
        [Tooltip("책·게시판 위에 오도록 정렬 순서")]
        [SerializeField] private int _sortingOrder = 450;
        [SerializeField] private Vector2 _windowSize = new Vector2(560f, 240f);

        private ConfirmChannel _channel;
        private ScreenStack _screens;
        private TMP_Text _message;
        private TMP_Text _yesLabel;
        private TMP_Text _noLabel;
        private bool _answer;

        public void Bind(GameContext context)
        {
            _channel = context.Confirm;
            _screens = context.Screens;
            Build();
            _channel.Asked += OnAsked;
            _screens.Closed += OnClosed;
            SetVisible(false);
        }

        private void OnDestroy()
        {
            if (_channel != null) _channel.Asked -= OnAsked;
            if (_screens != null) _screens.Closed -= OnClosed;
        }

        private void OnAsked(string message, string yes, string no)
        {
            _answer = false;
            _message.text = message;
            _yesLabel.text = string.IsNullOrEmpty(yes) ? GameTexts.Yes : yes;
            _noLabel.text = string.IsNullOrEmpty(no) ? GameTexts.No : no;
            if (!_screens.Open(GameScreen.Confirm)) { _channel.Answer(false); return; }
            SetVisible(true);
        }

        private void OnClosed(GameScreen screen)
        {
            if (screen != GameScreen.Confirm) return;
            SetVisible(false);
            _channel.Answer(_answer);
        }

        private void Choose(bool yes)
        {
            _answer = yes;
            _screens.Close(GameScreen.Confirm);
        }

        private void Build()
        {
            var font = _font != null ? _font : TempUiBuilder.FindFont(transform);
            TempUiBuilder.Root(transform, _sortingOrder);
            var window = TempUiBuilder.Panel("Window", transform, TempUiBuilder.Window, _windowSize);
            _message = TempUiBuilder.Label("Message", window, font, 26f, TextAlignmentOptions.Center);
            TempUiBuilder.Place(_message.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(_windowSize.x - 40f, _windowSize.y - 110f));

            var yes = TempUiBuilder.Button("YesButton", window, font, 24f, out _yesLabel);
            TempUiBuilder.Place((RectTransform)yes.transform, new Vector2(0.5f, 0f), new Vector2(-90f, 24f), new Vector2(150f, 52f));
            yes.onClick.AddListener(() => Choose(true));
            var no = TempUiBuilder.Button("NoButton", window, font, 24f, out _noLabel);
            TempUiBuilder.Place((RectTransform)no.transform, new Vector2(0.5f, 0f), new Vector2(90f, 24f), new Vector2(150f, 52f));
            no.onClick.AddListener(() => Choose(false));
        }
    }
}
