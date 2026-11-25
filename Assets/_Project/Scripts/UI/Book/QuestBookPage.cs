// 책의 의뢰 목록 탭(사용자 결정 2026-11-20, UI_SPEC 10-5를 대신함): 왼쪽 페이지는 수락한 의뢰 버튼 목록 + "화면에 의뢰 목록 표시" 토글,
// 오른쪽 페이지는 고른 의뢰의 제목·크리처 대사·필요한 재료·보상 한 줄 + 임시 "포기" 버튼(확인 창 뒤 RequestLog.Abandon). 정렬과 완료 색은 HUD 드롭다운과 같다.
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class QuestBookPage : BookPage, IGameContextBinder, IScrollHandler
    {
        [Header("왼쪽 페이지")]
        [SerializeField] private ThemedText _header;
        [SerializeField] private Image _headerLine;
        [SerializeField, Tooltip("RectMask2D로 자르는 목록 칸")] private RectTransform _viewport;
        [SerializeField] private QuestBookRowView _rowTemplate;
        [SerializeField] private Scrollbar _scrollbar;
        [SerializeField] private ThemedCheckbox _showHud;
        [SerializeField] private ThemedText _showHudLabel;

        [Header("오른쪽 페이지")]
        [SerializeField] private ThemedText _title;
        [SerializeField] private ThemedText _line;
        [SerializeField] private ThemedText _needsLabel;
        [SerializeField] private ThemedText _needs;
        [SerializeField] private ThemedText _rewardLabel;
        [SerializeField] private ThemedText _reward;
        [Tooltip("비우면 보상 글자를 복제해 오른쪽 아래에 만든다 (임시)")]
        [SerializeField] private ThemedText _abandon;

        private static readonly StringBuilder Lines = new StringBuilder();
        private readonly List<AcceptedRequestEntry> _sorted = new List<AcceptedRequestEntry>();
        private readonly List<QuestBookRowView> _pool = new List<QuestBookRowView>();
        private IAcceptedRequestList _requests;
        private RequestLog _log;
        private ConfirmChannel _confirm;
        private GameMessages _messages;
        private BookState _book;
        private HudState _hud;
        private AcceptedRequestEntry _selected;
        private int _scroll;
        private int _viewHeight;
        private int _listWidth;
        private bool _settingScrollbar;

        public void Bind(GameContext context)
        {
            _requests = context.Requests;
            _log = context.Requests;
            _confirm = context.Confirm;
            _messages = context.Messages;
            _book = context.Book;
            _hud = context.Hud;
            _requests.AcceptedListChanged += Rebuild;
            _book.FocusChanged += OnFocusChanged;
            _hud.QuestBarVisibilityChanged += OnHudVisibility;
            if (_showHud != null)
            {
                _showHud.SetValue(_hud.IsQuestBarVisible);
                _showHud.Changed -= OnShowHudChanged;
                _showHud.Changed += OnShowHudChanged;
            }
            Rebuild();
        }

        private void OnDestroy()
        {
            if (_requests != null) _requests.AcceptedListChanged -= Rebuild;
            if (_book != null) _book.FocusChanged -= OnFocusChanged;
            if (_hud != null) _hud.QuestBarVisibilityChanged -= OnHudVisibility;
        }

        private void OnShowHudChanged(bool visible) => _hud?.SetQuestBarVisible(visible);
        private void OnHudVisibility(bool visible) => _showHud?.SetValue(visible);
        private void OnFocusChanged(RequestDefinition _) => Rebuild();

        public override void OnOpened()
        {
            _scroll = 0;
            Rebuild();
        }

        protected override void Relayout()
        {
            if (_scrollbar != null)
            {
                _scrollbar.onValueChanged.RemoveListener(OnScrollbar);
                _scrollbar.onValueChanged.AddListener(OnScrollbar);
            }
            if (_rowTemplate != null) _rowTemplate.gameObject.SetActive(false);
            Rebuild();
        }

        // 목록이나 고른 의뢰가 바뀔 때만: 정렬 → 왼쪽 배치 → 오른쪽 본문.
        private void Rebuild()
        {
            if (this == null) { OnDestroy(); return; } // 꺼진 채 씬이 내려가 OnDestroy가 불리지 않은 구독자: 여기서 구독을 푼다
            if (Theme == null || Left == null || Right == null || _viewport == null) return;
            _sorted.Clear();
            if (_requests != null) _sorted.AddRange(_requests.Accepted);
            _sorted.Sort(Compare);
            _selected = PickSelected();
            LayoutLeft(Theme);
            LayoutRight(Theme);
        }

        // 먼저 보여 줄 의뢰가 목록에 있으면 그것, 없으면 지금 고른 것, 그것도 없으면 맨 위.
        private AcceptedRequestEntry PickSelected()
        {
            var focus = _book != null ? _book.FocusRequest : null;
            foreach (var e in _sorted) if (focus != null && e.Request == focus) return e;
            return _sorted.Count > 0 ? _sorted[0] : null;
        }

        private static int Compare(AcceptedRequestEntry a, AcceptedRequestEntry b)
        {
            if (a.IsComplete != b.IsComplete) return a.IsComplete ? -1 : 1;
            int progress = b.Progress.CompareTo(a.Progress);
            return progress != 0 ? progress : a.AcceptedOrder.CompareTo(b.AcceptedOrder);
        }

        private void LayoutLeft(UiTheme theme)
        {
            var values = theme.Book;
            int width = LeftSize.x, height = LeftSize.y;
            int headerLine = LineOf(theme, UiTextStyle.HudTitle);
            int bodyLine = LineOf(theme, UiTextStyle.Body);

            _header.SetStyle(UiTextStyle.HudTitle);
            if (theme.Texts != null) _header.SetText(string.Format(theme.Texts.Get(UiTextKeys.QuestBarTitle), _sorted.Count));
            PixelLayout.PlaceTopLeft((RectTransform)_header.transform, 0, 0, width, headerLine);
            int y = headerLine + values.HeaderGap;
            SetLine(_headerLine, values.LineColor, 0, y, width);
            y += 1 + values.HeaderGap;

            // 맨 아래 토글 줄
            int toggleHeight = Mathf.Max(12, bodyLine);
            int toggleY = height - toggleHeight;
            if (_showHud != null)
            {
                _showHud.ApplyTheme();
                var box = Vector2Int.RoundToInt(((RectTransform)_showHud.transform).sizeDelta);
                PixelLayout.PlaceTopLeft((RectTransform)_showHud.transform, 0, toggleY + PixelLayout.CenterOffset(toggleHeight, box.y), 0, 0);
                _showHudLabel.SetStyle(UiTextStyle.Small); // 체크 상자(12px)와 비슷한 크기 (갈무리9 10px)
                if (theme.Texts != null) _showHudLabel.SetText(theme.Texts.Get(UiTextKeys.BookQuestShowHud));
                int labelX = box.x + values.CheckboxGap;
                int smallLine = LineOf(theme, UiTextStyle.Small);
                PixelLayout.PlaceTopLeft((RectTransform)_showHudLabel.transform, labelX, toggleY + PixelLayout.CenterOffset(toggleHeight, smallLine), width - labelX, smallLine);
            }

            _viewHeight = Mathf.Max(0, toggleY - values.SectionGap - y);
            int content = _sorted.Count * values.RowHeight;
            bool overflow = content > _viewHeight;
            var quest = theme.QuestList;
            _listWidth = width - (overflow ? quest.ScrollbarWidth + quest.ScrollbarGap : 0);
            PixelLayout.PlaceTopLeft(_viewport, 0, y, _listWidth, _viewHeight);
            if (_scrollbar != null)
            {
                _scrollbar.gameObject.SetActive(overflow);
                if (overflow) PixelLayout.PlaceTopLeft((RectTransform)_scrollbar.transform, width - quest.ScrollbarWidth, y, quest.ScrollbarWidth, _viewHeight);
            }
            SetScroll(_scroll);
        }

        private void SetScroll(int value)
        {
            int rowHeight = Theme.Book.RowHeight;
            int content = _sorted.Count * rowHeight;
            int range = Mathf.Max(0, content - _viewHeight);
            _scroll = Mathf.Clamp(value, 0, range);
            if (_scrollbar != null && _scrollbar.gameObject.activeSelf)
            {
                _settingScrollbar = true;
                _scrollbar.size = content > 0 ? (float)_viewHeight / content : 1f;
                _scrollbar.value = range > 0 ? 1f - (float)_scroll / range : 1f;
                _settingScrollbar = false;
            }

            int used = 0;
            for (int i = 0; i < _sorted.Count; i++)
            {
                int top = i * rowHeight - _scroll;
                if (top + rowHeight <= 0 || top >= _viewHeight) continue;
                var row = Take(used++);
                row.Show(_sorted[i], _sorted[i] == _selected, Theme, _listWidth);
                PixelLayout.PlaceTopLeft(row.Rect, 0, top, _listWidth, rowHeight);
            }
            for (int i = used; i < _pool.Count; i++)
                if (_pool[i].gameObject.activeSelf) _pool[i].gameObject.SetActive(false);
        }

        private QuestBookRowView Take(int index)
        {
            while (_pool.Count <= index)
            {
                var row = Instantiate(_rowTemplate, _viewport);
                row.name = $"Row{_pool.Count + 1}";
                row.Clicked += OnRowClicked;
                _pool.Add(row);
            }
            var view = _pool[index];
            if (!view.gameObject.activeSelf) view.gameObject.SetActive(true);
            return view;
        }

        private void OnRowClicked(AcceptedRequestEntry entry) => _book?.SetFocus(entry.Request);

        private void OnScrollbar(float value)
        {
            if (_settingScrollbar) return;
            int range = _sorted.Count * Theme.Book.RowHeight - _viewHeight;
            SetScroll(Mathf.RoundToInt((1f - value) * range));
        }

        public void OnScroll(PointerEventData eventData)
        {
            int steps = Mathf.RoundToInt(eventData.scrollDelta.y);
            if (steps == 0) steps = eventData.scrollDelta.y > 0 ? 1 : -1;
            SetScroll(_scroll - steps * Theme.Book.RowHeight);
        }

        // 오른쪽 페이지: 제목(굵게) → 대사(줄바꿈) → "필요한 재료" + 재료 줄 → "보상 필기 점수 +n".
        private void LayoutRight(UiTheme theme)
        {
            var values = theme.Book;
            bool has = _selected != null;
            EnsureAbandon();
            foreach (var t in new[] { _title, _line, _needsLabel, _needs, _rewardLabel, _reward, _abandon })
                if (t != null && t.gameObject.activeSelf != has) t.gameObject.SetActive(has);
            if (!has) return;

            var texts = theme.Texts;
            int width = RightSize.x;
            int bold = LineOf(theme, UiTextStyle.BodyBold);
            int body = LineOf(theme, UiTextStyle.Body);
            var completed = theme.QuestList.CompletedColor;
            int y = 0;

            _title.SetStyle(UiTextStyle.BodyBold);
            _title.SetText(_selected.Title);
            _title.SetColorOverride(_selected.IsComplete, completed);
            PixelLayout.PlaceTopLeft((RectTransform)_title.transform, 0, y, width, bold);
            y += bold + values.TitleGap;

            _line.SetStyle(UiTextStyle.Body);
            string line = _selected.Request != null ? _selected.Request.CreatureLine : string.Empty;
            var lineLabel = _line.GetComponent<TMP_Text>();
            string quoted = texts != null ? string.Format(texts.Get(UiTextKeys.BookQuestLine), line) : line;
            _line.SetText(UiTextWrap.ByWords(lineLabel, quoted, width)); // 낱말 단위 줄바꿈
            int lineHeight = Mathf.Max(body, Mathf.CeilToInt(lineLabel.GetPreferredValues(lineLabel.text, width, 0f).y));
            lineHeight = Mathf.CeilToInt(lineHeight / (float)body) * body; // 줄 높이의 정수 배
            PixelLayout.PlaceTopLeft((RectTransform)_line.transform, 0, y, width, lineHeight);
            y += lineHeight + values.SectionGap;

            _needsLabel.SetStyle(UiTextStyle.BodyBold);
            if (texts != null) _needsLabel.SetText(texts.Get(UiTextKeys.BookQuestNeeds));
            PixelLayout.PlaceTopLeft((RectTransform)_needsLabel.transform, 0, y, width, bold);
            y += bold;

            Lines.Clear();
            string format = texts != null ? texts.Get(UiTextKeys.QuestRequirement) : "{0} {1}/{2}";
            for (int i = 0; i < _selected.Requirements.Count; i++)
            {
                var r = _selected.Requirements[i];
                if (i > 0) Lines.Append('\n');
                Lines.AppendFormat(format, r.Item != null ? r.Item.DisplayName : "?", Mathf.Min(r.Current, r.Required), r.Required);
            }
            _needs.SetStyle(UiTextStyle.Body);
            _needs.SetText(Lines.ToString());
            _needs.SetColorOverride(_selected.IsComplete, completed);
            int needsHeight = Mathf.Max(1, _selected.Requirements.Count) * body;
            PixelLayout.PlaceTopLeft((RectTransform)_needs.transform, values.RequirementIndent, y, width - values.RequirementIndent, needsHeight);
            y += needsHeight + values.SectionGap;

            _rewardLabel.SetStyle(UiTextStyle.BodyBold);
            if (texts != null) _rewardLabel.SetText(texts.Get(UiTextKeys.BookQuestReward));
            var rewardLabel = _rewardLabel.GetComponent<TMP_Text>();
            int labelWidth = Mathf.CeilToInt(rewardLabel.GetPreferredValues(rewardLabel.text).x);
            PixelLayout.PlaceTopLeft((RectTransform)_rewardLabel.transform, 0, y, labelWidth, bold);
            _reward.SetStyle(UiTextStyle.Body);
            int reward = _requests != null ? _requests.ExpectedReward(_selected) : 0;
            _reward.SetText(texts != null ? string.Format(texts.Get(UiTextKeys.BookQuestRewardValue), reward) : reward.ToString());
            int rewardX = labelWidth + values.RewardGap;
            PixelLayout.PlaceTopLeft((RectTransform)_reward.transform, rewardX, y, width - rewardX, body);

            if (_abandon != null)
            {
                _abandon.SetStyle(UiTextStyle.BodyBold);
                _abandon.SetText($"<u>{GameTexts.AbandonButton}</u>");
                var label = _abandon.GetComponent<TMP_Text>();
                label.raycastTarget = true; // 글자 자체가 버튼
                int abandonWidth = Mathf.CeilToInt(label.GetPreferredValues(GameTexts.AbandonButton).x) + 4;
                PixelLayout.PlaceTopLeft((RectTransform)_abandon.transform, width - abandonWidth, RightSize.y - bold, abandonWidth, bold);
            }
        }

        // 포기 버튼(임시): 보상 글자를 복제해 버튼을 붙인다. 정식 디자인은 책 의뢰 상세 페이지 작업에서.
        private void EnsureAbandon()
        {
            if (_abandon == null && _rewardLabel != null)
            {
                _abandon = Instantiate(_rewardLabel, _rewardLabel.transform.parent);
                _abandon.name = "AbandonButton";
            }
            if (_abandon == null || _abandon.GetComponent<Button>() != null) return;
            var button = _abandon.gameObject.AddComponent<Button>();
            button.targetGraphic = _abandon.GetComponent<TMP_Text>();
            button.onClick.AddListener(OnAbandonClicked);
        }

        private void OnAbandonClicked()
        {
            var entry = _selected;
            if (entry == null || _log == null || _confirm == null) return;
            _confirm.Ask(string.Format(GameTexts.AbandonConfirmFormat, entry.Title), () =>
            {
                if (_log.Abandon(entry.Id)) _messages?.ShowToast(string.Format(GameTexts.AbandonedFormat, entry.Title));
            });
        }

        private static void SetLine(Image image, Color color, int x, int y, int width)
        {
            if (image == null) return;
            image.sprite = null;
            image.color = color;
            image.raycastTarget = false;
            PixelLayout.PlaceTopLeft(image.rectTransform, x, y, width, 1);
        }

        private static int LineOf(UiTheme theme, UiTextStyle id)
        {
            var style = theme.TextStyle(id);
            return style != null ? style.LineHeightFrom(theme.Fonts) : 16;
        }
    }
}
