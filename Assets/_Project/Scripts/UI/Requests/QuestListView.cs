// 의뢰 드롭다운 목록 패널(UI_SPEC 10-2~10-4): 의뢰 바 바로 아래, 가로 고정·세로는 항목에 맞춰, 대략 4개를 넘으면 휠·스크롤바로 스크롤.
// 정렬(완료 → 진행률 높은 순 → 먼저 수락한 순)과 높이 계산은 목록이 바뀔 때만 하고, 항목은 보이는 만큼만 풀에서 재사용한다(가상 스크롤).
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class QuestListView : ThemedWidget, IGameContextBinder, IScrollHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField, Tooltip("UI_Root의 의뢰 바 (이 아래에 붙는다)")] private RectTransform _bar;
        [SerializeField] private ThemedPanel _background;
        [SerializeField, Tooltip("RectMask2D로 자르는 보이는 영역")] private RectTransform _viewport;
        [SerializeField] private QuestListItemView _itemTemplate;
        [SerializeField] private Scrollbar _scrollbar;

        private readonly List<AcceptedRequestEntry> _sorted = new List<AcceptedRequestEntry>();
        private readonly List<int> _tops = new List<int>();
        private readonly List<QuestListItemView> _pool = new List<QuestListItemView>();
        private IAcceptedRequestList _requests;
        private HudState _hud;
        private BookState _book;
        private ScreenStack _screens;
        private int _contentHeight;
        private int _viewHeight;
        private int _itemWidth;
        private int _scroll;
        private int _titleLine = 16;
        private int _requirementLine = 16;
        private bool _settingScrollbar;

        /// <summary>항목을 눌렀을 때 (자세히 패널이 받는다, 10-5).</summary>
        public event Action<AcceptedRequestEntry> ItemClicked;

        public void Bind(GameContext context)
        {
            _book = context.Book;
            _screens = context.Screens;
            Bind(context.Requests, context.Hud);
        }

        /// <summary>목록 원본을 바꿔 연결한다(게임 컨텍스트, 또는 에디터 미리보기의 가짜 목록).</summary>
        public void Bind(IAcceptedRequestList requests, HudState hud)
        {
            if (_requests != null) _requests.AcceptedListChanged -= Rebuild;
            if (_hud != null) _hud.QuestListToggled -= OnToggled;
            if (_hud != null) _hud.QuestBarVisibilityChanged -= OnToggled;
            _requests = requests;
            _hud = hud;
            _scroll = 0;
            if (_requests != null) _requests.AcceptedListChanged += Rebuild;
            if (_hud != null) _hud.QuestListToggled += OnToggled;
            if (_hud != null) _hud.QuestBarVisibilityChanged += OnToggled;
            Rebuild();
        }

        private void OnDestroy()
        {
            if (_requests != null) _requests.AcceptedListChanged -= Rebuild;
            if (_hud != null) _hud.QuestListToggled -= OnToggled;
            if (_hud != null) _hud.QuestBarVisibilityChanged -= OnToggled;
        }

        private void OnToggled(bool _) => Rebuild();

        protected override void Apply(UiTheme theme)
        {
            _titleLine = LineHeight(theme, theme.QuestList.TitleStyle);
            _requirementLine = LineHeight(theme, theme.QuestList.RequirementStyle);
            if (_itemTemplate != null) _itemTemplate.gameObject.SetActive(false);
            if (_scrollbar != null)
            {
                ThemedPanel.EnableScrollbarInput(_scrollbar);
                _scrollbar.onValueChanged.RemoveListener(OnScrollbar);
                _scrollbar.onValueChanged.AddListener(OnScrollbar);
            }
            Rebuild();
        }

        // 목록이 바뀌거나 펼침 상태·테마가 바뀔 때만: 정렬 → 항목 위치 누적 → 패널 크기 → 보이는 항목 배치.
        private void Rebuild()
        {
            if (this == null) { OnDestroy(); return; } // 꺼진 채 씬이 내려가 OnDestroy가 불리지 않은 구독자: 여기서 구독을 푼다
            if (Theme == null || _bar == null || _viewport == null || _background == null) return;
            var values = Theme.QuestList;
            bool expanded = _hud == null || (_hud.IsQuestListExpanded && _hud.IsQuestBarVisible);

            _sorted.Clear();
            if (_requests != null) _sorted.AddRange(_requests.Accepted);
            _sorted.Sort(Compare);

            // 항목이 없으면 패널을 보이지 않는다(빈 목록 문구는 문서에 없음).
            bool visible = expanded && _sorted.Count > 0;
            SetVisible(visible);
            if (!visible) return;

            _tops.Clear();
            int y = 0;
            for (int i = 0; i < _sorted.Count; i++)
            {
                if (i > 0) y += values.ItemGap;
                _tops.Add(y);
                y += QuestListItemView.HeightOf(_sorted[i], _titleLine, _requirementLine);
            }
            _contentHeight = y;

            // 최대 높이: 제목 + 아이템 한 줄짜리 항목이 MaxVisibleItems개 보이는 높이(10-4).
            int typical = _titleLine + _requirementLine;
            int maxView = values.MaxVisibleItems * typical + (values.MaxVisibleItems - 1) * values.ItemGap;
            _viewHeight = Mathf.Min(_contentHeight, maxView);
            bool overflow = _contentHeight > _viewHeight;

            var barRect = _bar;
            int barX = Mathf.RoundToInt(barRect.anchoredPosition.x);
            int barY = -Mathf.RoundToInt(barRect.anchoredPosition.y);
            int width = Mathf.RoundToInt(barRect.sizeDelta.x);
            int barHeight = Mathf.RoundToInt(barRect.sizeDelta.y);
            int height = _viewHeight + values.PaddingY * 2;

            var root = (RectTransform)transform;
            PixelLayout.PlaceTopLeft(root, barX, barY + barHeight + values.BarGap, width, height);
            PixelLayout.Stretch((RectTransform)_background.transform);

            int scrollbarSpace = overflow ? values.ScrollbarWidth + values.ScrollbarGap : 0;
            _itemWidth = width - values.PaddingX * 2 - scrollbarSpace;
            PixelLayout.PlaceTopLeft(_viewport, values.PaddingX, values.PaddingY, _itemWidth, _viewHeight);

            if (_scrollbar != null)
            {
                _scrollbar.gameObject.SetActive(overflow);
                if (overflow)
                    PixelLayout.PlaceTopLeft((RectTransform)_scrollbar.transform, width - values.PaddingX - values.ScrollbarWidth,
                        values.PaddingY, values.ScrollbarWidth, _viewHeight);
            }
            SetScroll(_scroll);
        }

        // 완료 → 진행률 높은 순 → 먼저 수락한 순 (10-3, 사용자 결정).
        private static int Compare(AcceptedRequestEntry a, AcceptedRequestEntry b)
        {
            if (a.IsComplete != b.IsComplete) return a.IsComplete ? -1 : 1;
            int progress = b.Progress.CompareTo(a.Progress);
            return progress != 0 ? progress : a.AcceptedOrder.CompareTo(b.AcceptedOrder);
        }

        private void SetScroll(int value)
        {
            _scroll = Mathf.Clamp(value, 0, Mathf.Max(0, _contentHeight - _viewHeight));
            if (_scrollbar != null && _scrollbar.gameObject.activeSelf)
            {
                _settingScrollbar = true;
                _scrollbar.size = _contentHeight > 0 ? (float)_viewHeight / _contentHeight : 1f;
                int range = _contentHeight - _viewHeight;
                _scrollbar.value = range > 0 ? 1f - (float)_scroll / range : 1f;
                _settingScrollbar = false;
            }
            PlaceVisibleItems();
        }

        // 보이는 범위에 걸친 항목만 풀에서 꺼내 놓는다. 위치는 정수 px.
        private void PlaceVisibleItems()
        {
            int used = 0;
            for (int i = 0; i < _sorted.Count; i++)
            {
                int top = _tops[i] - _scroll;
                int h = QuestListItemView.HeightOf(_sorted[i], _titleLine, _requirementLine);
                if (top + h <= 0 || top >= _viewHeight) continue;
                var item = Take(used++);
                item.Show(_sorted[i], Theme, _itemWidth, _titleLine, _requirementLine);
                PixelLayout.PlaceTopLeft(item.Rect, 0, top, _itemWidth, h);
            }
            for (int i = used; i < _pool.Count; i++)
                if (_pool[i].gameObject.activeSelf) _pool[i].gameObject.SetActive(false);
        }

        private QuestListItemView Take(int index)
        {
            while (_pool.Count <= index)
            {
                var item = Instantiate(_itemTemplate, _viewport);
                item.name = $"Item{_pool.Count + 1}";
                item.Clicked += OnItemClicked;
                _pool.Add(item);
            }
            var view = _pool[index];
            if (!view.gameObject.activeSelf) view.gameObject.SetActive(true);
            return view;
        }

        // 항목을 누르면 책을 의뢰 탭 + 그 의뢰로 연다(자세히 패널 대신, 사용자 결정 2026-11-20).
        private void OnItemClicked(AcceptedRequestEntry entry)
        {
            ItemClicked?.Invoke(entry);
            if (_book == null || _screens == null || _screens.HasAny) return;
            _book.PrepareForRequest(entry.Request);
            _screens.Open(GameScreen.Book);
        }

        private void OnScrollbar(float value)
        {
            if (_settingScrollbar) return;
            int range = _contentHeight - _viewHeight;
            SetScroll(Mathf.RoundToInt((1f - value) * range));
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (_contentHeight <= _viewHeight) return;
            int steps = Mathf.RoundToInt(eventData.scrollDelta.y);
            if (steps == 0) steps = eventData.scrollDelta.y > 0 ? 1 : -1;
            SetScroll(_scroll - steps * Theme.QuestList.WheelStep);
        }

        // 목록 위에서는 휠을 카메라 줌 대신 스크롤에 쓴다(퀵슬롯과 같은 방식).
        public void OnPointerEnter(PointerEventData eventData) => _hud?.SetPointerOverScrollHud(true);
        public void OnPointerExit(PointerEventData eventData) => _hud?.SetPointerOverScrollHud(false);

        private void SetVisible(bool visible)
        {
            if (_background.gameObject.activeSelf != visible) _background.gameObject.SetActive(visible);
            if (_viewport.gameObject.activeSelf != visible) _viewport.gameObject.SetActive(visible);
            if (_scrollbar != null && !visible && _scrollbar.gameObject.activeSelf) _scrollbar.gameObject.SetActive(false);
            if (!visible && _hud != null) _hud.SetPointerOverScrollHud(false);
        }

        private static int LineHeight(UiTheme theme, UiTextStyle id)
        {
            var style = theme.TextStyle(id);
            return style != null ? style.LineHeightFrom(theme.Fonts) : 16;
        }
    }
}
