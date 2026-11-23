// 수락한 의뢰 드롭다운 바(UI_SPEC 10-1). 왼쪽 화살표로 목록을 즉시 펼치고 접으며(아래 ↔ 위 화살표), "수락한 의뢰 (n)"을 보여 준다.
// 오른쪽 X는 바와 목록을 끄고, 책 의뢰 탭의 토글로 다시 켠다(10-1). 바는 시간을 멈추지 않는다. 위치는 책·가방 아이콘 줄 아래.
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class QuestBarView : ThemedWidget, IGameContextBinder
    {
        [SerializeField] private ThemedPanel _background;
        [SerializeField] private ThemedButton _arrow;
        [SerializeField] private ThemedButton _close;
        [SerializeField] private ThemedText _title;

        private IAcceptedRequestList _requests;
        private HudState _hud;
        private int _shownCount = -1;

        public void Bind(GameContext context)
        {
            _requests = context.Requests;
            _hud = context.Hud;
            _requests.AcceptedListChanged += RefreshTitle;
            _hud.QuestListToggled += OnToggled;
            _arrow.Button.onClick.AddListener(_hud.ToggleQuestList);
            // X: 바(와 목록) 전체를 끈다. 다시 켜기는 책 의뢰 탭의 "화면에 의뢰 목록 표시"(10-1).
            _close.Button.onClick.AddListener(() => _hud.SetQuestBarVisible(false));
            _hud.QuestBarVisibilityChanged += OnBarVisibility;
            OnBarVisibility(_hud.IsQuestBarVisible);
            RefreshArrow();
            RefreshTitle();
        }

        // 의뢰·HUD 상태는 씬을 넘어 유지될 수 있으므로 꼭 구독을 푼다.
        private void OnDestroy()
        {
            if (_requests != null) _requests.AcceptedListChanged -= RefreshTitle;
            if (_hud != null) _hud.QuestListToggled -= OnToggled;
            if (_hud != null) _hud.QuestBarVisibilityChanged -= OnBarVisibility;
        }

        private void OnToggled(bool _) => RefreshArrow();

        private void OnBarVisibility(bool visible)
        {
            if (gameObject.activeSelf != visible) gameObject.SetActive(visible);
        }

        private void RefreshArrow()
        {
            if (_hud == null) return;
            _arrow.SetSkin(_hud.IsQuestListExpanded ? UiButtonSkin.ArrowUp : UiButtonSkin.ArrowDown);
        }

        // 개수가 바뀔 때만 글자를 다시 만든다(4-A-5).
        private void RefreshTitle()
        {
            if (this == null) { OnDestroy(); return; } // 꺼진 채 씬이 내려가 OnDestroy가 불리지 않은 구독자: 여기서 구독을 푼다
            int count = _requests != null ? _requests.AcceptedCount : 0;
            if (count == _shownCount || Theme == null || Theme.Texts == null) return;
            _shownCount = count;
            _title.SetText(string.Format(Theme.Texts.Get(UiTextKeys.QuestBarTitle), count));
        }

        protected override void Apply(UiTheme theme)
        {
            var values = theme.TopLeft;
            var root = (RectTransform)transform;
            int iconHeight = IconRowHeight(theme);
            if (_background == null || _arrow == null || _close == null || _title == null) return;
            _title.ApplyTheme();
            var style = theme.TextStyle(_title.Style);
            int lineHeight = style != null ? style.LineHeightFrom(theme.Fonts) : 16;

            PixelLayout.Stretch((RectTransform)_background.transform);
            _arrow.ApplyTheme();
            _close.ApplyTheme();
            var arrowSize = Vector2Int.RoundToInt(((RectTransform)_arrow.transform).sizeDelta);
            var closeSize = Vector2Int.RoundToInt(((RectTransform)_close.transform).sizeDelta);
            // 바 높이 = 글자 줄 높이(또는 버튼 높이 중 큰 값) + 위아래 여백.
            int height = Mathf.Max(lineHeight, Mathf.Max(arrowSize.y, closeSize.y)) + values.BarPaddingY * 2;
            PixelLayout.PlaceTopLeft(root, values.Margin, values.Margin + iconHeight + values.BarGap, values.BarWidth, height);
            PixelLayout.PlaceTopLeft((RectTransform)_arrow.transform, values.BarButtonInset, PixelLayout.CenterOffset(height, arrowSize.y), 0, 0);
            PixelLayout.PlaceTopRight((RectTransform)_close.transform, values.BarButtonInset, PixelLayout.CenterOffset(height, closeSize.y), 0, 0);

            int titleLeft = values.BarButtonInset + arrowSize.x + values.BarTitleGap;
            int titleRight = values.BarButtonInset + closeSize.x + values.BarTitleGap;
            PixelLayout.PlaceTopLeft((RectTransform)_title.transform, titleLeft, PixelLayout.CenterOffset(height, lineHeight), values.BarWidth - titleLeft - titleRight, lineHeight);
            _shownCount = -1;
            RefreshTitle();
        }

        private static int IconRowHeight(UiTheme theme)
        {
            var book = theme.Button(UiButtonSkin.MenuBook);
            return book != null && book.Normal != null ? Mathf.RoundToInt(book.Normal.rect.height) * theme.TopLeft.IconScale : 0;
        }
    }
}
