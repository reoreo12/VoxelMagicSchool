// 크리처 대사 창. 받은 대사를 한 줄씩 보여 주고, E(화면 스택 Submit)나 클릭으로 넘기며, 마지막 줄 뒤에 창을 닫는다.
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
        private ScreenStack _screens;
        private int _index;

        public void Bind(GameContext context)
        {
            _messages = context.Messages;
            _screens = context.Screens;
            _speaker.text = GameTexts.CreatureName;
            _messages.CreatureSpeechRequested += OnSpeech;
            _screens.Opened += OnOpened;
            _screens.Closed += OnClosed;
            _screens.Submitted += Advance;
            _nextButton.onClick.AddListener(Advance);
            SetVisible(false);
        }

        private void OnDestroy()
        {
            if (_screens == null) return;
            _messages.CreatureSpeechRequested -= OnSpeech;
            _screens.Opened -= OnOpened;
            _screens.Closed -= OnClosed;
            _screens.Submitted -= Advance;
        }

        private void OnSpeech(IReadOnlyList<string> lines)
        {
            _lines.Clear();
            _lines.AddRange(lines);
            _index = 0;
        }

        private void OnOpened(GameScreen screen)
        {
            if (screen != GameScreen.CreatureDialog) return;
            SetVisible(true);
            if (_lines.Count == 0) _screens.Close(GameScreen.CreatureDialog);
            else ShowCurrent();
        }

        private void OnClosed(GameScreen screen)
        {
            if (screen == GameScreen.CreatureDialog) SetVisible(false);
        }

        private void Advance()
        {
            if (!IsVisible || _screens.Top != GameScreen.CreatureDialog) return;
            _index++;
            if (_index >= _lines.Count) _screens.Close(GameScreen.CreatureDialog);
            else ShowCurrent();
        }

        private void ShowCurrent()
        {
            _line.text = _lines[_index];
            _hint.text = _index < _lines.Count - 1 ? GameTexts.DialogNext : GameTexts.DialogClose;
        }
    }
}
