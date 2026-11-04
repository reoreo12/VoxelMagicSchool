// 크리처 대사 창. 받은 대사를 한 줄씩 보여 주고, E(ScreenState.Submit)나 클릭으로 넘기며, 마지막 줄 뒤에 창을 닫는다.
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class CreatureDialogView : UIPanel, IGameContextBinder
    {
        [SerializeField] private TMP_Text _speaker;
        [SerializeField] private TMP_Text _line;
        [SerializeField] private TMP_Text _hint;
        [SerializeField] private Button _nextButton;

        private readonly List<string> _lines = new List<string>();
        private GameMessages _messages;
        private ScreenState _screen;
        private int _index;

        public void Bind(GameContext context)
        {
            _messages = context.Messages;
            _screen = context.Screen;
            _speaker.text = GameTexts.CreatureName;
            _messages.CreatureSpeechRequested += OnSpeech;
            _screen.WindowChanged += OnWindowChanged;
            _screen.Submitted += Advance;
            _nextButton.onClick.AddListener(Advance);
            SetVisible(false);
        }

        private void OnDestroy()
        {
            if (_screen == null) return;
            _messages.CreatureSpeechRequested -= OnSpeech;
            _screen.WindowChanged -= OnWindowChanged;
            _screen.Submitted -= Advance;
        }

        private void OnSpeech(IReadOnlyList<string> lines)
        {
            _lines.Clear();
            _lines.AddRange(lines);
            _index = 0;
        }

        private void OnWindowChanged(GameWindow window)
        {
            bool open = window == GameWindow.CreatureDialog;
            SetVisible(open);
            if (!open) return;
            if (_lines.Count == 0) _screen.Close(GameWindow.CreatureDialog);
            else ShowCurrent();
        }

        private void Advance()
        {
            if (!IsVisible) return;
            _index++;
            if (_index >= _lines.Count) _screen.Close(GameWindow.CreatureDialog);
            else ShowCurrent();
        }

        private void ShowCurrent()
        {
            _line.text = _lines[_index];
            _hint.text = _index < _lines.Count - 1 ? GameTexts.DialogNext : GameTexts.DialogClose;
        }
    }
}
