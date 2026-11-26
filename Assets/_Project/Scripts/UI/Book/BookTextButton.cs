// 책 페이지 안의 글자 버튼(예: 의뢰 탭 "포기"): 평소엔 글자만, 마우스를 올리면 목록 칸과 같은 옅은 바탕, 누르는 동안 진한 바탕 + 글자 1px 내려감.
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace VoxelMagic.UI
{
    public sealed class BookTextButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        [SerializeField] private ThemedPanel _background;
        [SerializeField] private ThemedText _label;

        private bool _hovered;
        private bool _pressed;
        private int _labelY;

        public event Action Clicked;
        public RectTransform Rect => (RectTransform)transform;

        public void Init(ThemedPanel background, ThemedText label)
        {
            _background = background;
            _label = label;
        }

        /// <summary>글자를 넣고 부모 오른쪽 아래에 둔다. 글자 끝이 부모 내용 끝에 맞고, 바탕은 여백만큼 바깥으로 나간다.</summary>
        public void Layout(string text, UiTheme theme, int parentWidth, int parentHeight)
        {
            var values = theme.Book;
            _label.SetStyle(values.TextButtonStyle);
            _label.SetText(text);
            var label = _label.GetComponent<TMP_Text>();
            label.raycastTarget = false;
            var padding = values.TextButtonPadding;
            int textWidth = Mathf.CeilToInt(label.GetPreferredValues(text).x);
            var style = theme.TextStyle(values.TextButtonStyle);
            int line = style != null ? style.LineHeightFrom(theme.Fonts) : 16;
            var labelRect = (RectTransform)_label.transform;
            PixelLayout.PlaceTopLeft(labelRect, padding.x, 0, textWidth + 1, line);
            label.ForceMeshUpdate();
            int ink = UiTextInk.TryInkRows(label, out float inkTop, out float inkBottom) ? Mathf.RoundToInt(inkTop - inkBottom) : line;
            // 높이 = 글자 모양 + 위아래 여백 + 테두리 1px씩 (목록 칸과 같은 느낌). 바탕은 글자보다 padding만큼 바깥으로 나간다.
            int width = textWidth + padding.x * 2, height = ink + padding.y * 2 + 2;
            PixelLayout.PlaceTopLeft(Rect, parentWidth - width + padding.x, parentHeight - height + padding.y, width, height);
            PixelLayout.Stretch((RectTransform)_background.transform);
            UiTextInk.CenterVertically(label, 1, height - 2); // 바탕 테두리 1px 안쪽 가운데
            _labelY = Mathf.RoundToInt(-labelRect.anchoredPosition.y);
            Apply();
        }

        private void Apply()
        {
            bool show = _hovered || _pressed;
            if (_background.gameObject.activeSelf != show) _background.gameObject.SetActive(show);
            if (show) _background.SetSkin(_pressed ? UiPanelSkin.BookRowSelected : UiPanelSkin.BookRowHover);
            var rect = (RectTransform)_label.transform;
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, -(_labelY + (_pressed ? 1 : 0)));
        }

        public void OnPointerEnter(PointerEventData eventData) { _hovered = true; Apply(); }
        public void OnPointerExit(PointerEventData eventData) { _hovered = false; _pressed = false; Apply(); }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            _pressed = true;
            Apply();
        }

        public void OnPointerUp(PointerEventData eventData) { _pressed = false; Apply(); }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) Clicked?.Invoke();
        }

        private void OnDisable()
        {
            _hovered = false;
            _pressed = false;
        }
    }
}
