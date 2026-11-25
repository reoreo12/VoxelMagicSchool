// 크리처에게 전달할 의뢰를 고르는 임시 창(QUEST_TIMER 7-7). 화면 스택이 DeliveryChoice를 열면 DeliverySession의 완료 가능 목록을 버튼으로 보여 준다.
// 고르면 세션에 알리고(그 순간 다시 확인), 고르지 않고 닫으면(ESC·닫기) 대화를 끝낸다. 모양은 코드로 만든다(정식 디자인 전).
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class DeliveryChoiceWindow : UIPanel, IGameContextBinder
    {
        [SerializeField] private TMP_FontAsset _font;
        [Tooltip("다른 UI 위에 오도록 정렬 순서")]
        [SerializeField] private int _sortingOrder = 400;
        [SerializeField] private Vector2 _windowSize = new Vector2(620f, 420f);
        [SerializeField] private float _rowHeight = 64f;

        private readonly List<Button> _rows = new List<Button>();
        private readonly List<TMP_Text> _rowLabels = new List<TMP_Text>();
        private DeliverySession _session;
        private RequestLog _log;
        private ScreenStack _screens;
        private RectTransform _list;
        private TMP_FontAsset _fontCache;

        public void Bind(GameContext context)
        {
            _session = context.Delivery;
            _log = context.Requests;
            _screens = context.Screens;
            Build();
            _screens.Opened += OnOpened;
            _screens.Closed += OnClosed;
            _session.StepChanged += OnStep;
            SetVisible(false);
        }

        private void OnDestroy()
        {
            if (_screens != null)
            {
                _screens.Opened -= OnOpened;
                _screens.Closed -= OnClosed;
            }
            if (_session != null) _session.StepChanged -= OnStep;
        }

        private void OnOpened(GameScreen screen)
        {
            if (screen != GameScreen.DeliveryChoice) return;
            Refresh();
            SetVisible(true);
        }

        private void OnClosed(GameScreen screen)
        {
            if (screen != GameScreen.DeliveryChoice) return;
            SetVisible(false);
            // 고르지 않고 닫았으면(ESC·닫기) 대화를 끝낸다. 골라서 대사로 넘어간 경우는 Talking이라 그대로.
            if (_session.Step == DeliveryStep.Choosing) _session.Cancel();
        }

        private void OnStep()
        {
            if (IsVisible && _session.Step == DeliveryStep.Choosing) Refresh();
        }

        private void Refresh()
        {
            var choices = _session.Choices;
            for (int i = 0; i < choices.Count; i++)
            {
                if (i >= _rows.Count) AddRow();
                int id = choices[i].Id;
                _rows[i].gameObject.SetActive(true);
                _rows[i].onClick.RemoveAllListeners();
                _rows[i].onClick.AddListener(() => _session.Choose(id));
                _rowLabels[i].text = $"{choices[i].Title}  <size=80%>{string.Format(GameTexts.RewardFormat, _log.RewardOf(choices[i].Request))}</size>";
            }
            for (int i = choices.Count; i < _rows.Count; i++) _rows[i].gameObject.SetActive(false);
        }

        private void Build()
        {
            var font = _font != null ? _font : TempUiBuilder.FindFont(transform);
            TempUiBuilder.Root(transform, _sortingOrder);
            var window = TempUiBuilder.Panel("Window", transform, TempUiBuilder.Window, _windowSize);
            var title = TempUiBuilder.Label("Title", window, font, 30f, TextAlignmentOptions.Center);
            title.text = GameTexts.DeliveryChoiceTitle;
            TempUiBuilder.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -16f), new Vector2(_windowSize.x - 40f, 44f));

            _list = new GameObject("List", typeof(RectTransform), typeof(VerticalLayoutGroup)).GetComponent<RectTransform>();
            _list.SetParent(window, false);
            TempUiBuilder.Place(_list, new Vector2(0.5f, 1f), new Vector2(0f, -72f), new Vector2(_windowSize.x - 60f, _windowSize.y - 160f));
            var layout = _list.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 10f;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var close = TempUiBuilder.Button("CloseButton", window, font, 24f, out var closeLabel);
            closeLabel.text = GameTexts.Close;
            TempUiBuilder.Place((RectTransform)close.transform, new Vector2(1f, 0f), new Vector2(-20f, 20f), new Vector2(150f, 52f));
            close.onClick.AddListener(() => _screens.Close(GameScreen.DeliveryChoice));
            _fontCache = font;
        }

        private void AddRow()
        {
            var row = TempUiBuilder.Button($"Choice{_rows.Count}", _list, _fontCache, 24f, out var label);
            ((RectTransform)row.transform).sizeDelta = new Vector2(0f, _rowHeight);
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.margin = new Vector4(16f, 0f, 16f, 0f);
            _rows.Add(row);
            _rowLabels.Add(label);
        }
    }
}
