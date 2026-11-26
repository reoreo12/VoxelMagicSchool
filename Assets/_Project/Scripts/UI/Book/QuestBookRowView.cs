// 책 의뢰 탭 왼쪽 목록의 한 칸: 의뢰 제목 한 줄 + 아래 구분선. 고른 칸은 깔끔한 진한 종이 바탕 + 굵은 글씨, 마우스를 올린 칸은 옅은 바탕, 완료(납품 가능)는 파란 글자.
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class QuestBookRowView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image _hit;
        [SerializeField] private ThemedPanel _selection;
        [SerializeField] private ThemedText _title;
        [SerializeField] private Image _line;

        private AcceptedRequestEntry _entry;
        private bool _selected;
        private bool _hovered;

        public RectTransform Rect => (RectTransform)transform;
        public event Action<AcceptedRequestEntry> Clicked;

        public void Show(AcceptedRequestEntry entry, bool selected, UiTheme theme, int width)
        {
            var values = theme.Book;
            int height = values.RowHeight;
            _entry = entry;
            Rect.sizeDelta = new Vector2(width, height);
            PixelLayout.PlaceTopLeft(_hit.rectTransform, 0, 0, width, height);
            _hit.color = new Color(1f, 1f, 1f, 0f);
            _hit.raycastTarget = true;

            _selected = selected;
            ApplyBacking();
            PixelLayout.PlaceTopLeft((RectTransform)_selection.transform, 0, 0, width, height - 1);

            var style = selected ? UiTextStyle.BodyBold : UiTextStyle.Body;
            if (_title.Style != style) _title.SetStyle(style);
            _title.SetText(entry.Title);
            _title.SetColorOverride(entry.IsComplete, theme.QuestList.CompletedColor);
            var textStyle = theme.TextStyle(style);
            int line = textStyle != null ? textStyle.LineHeightFrom(theme.Fonts) : 16;
            int inset = values.RowTextInset;
            PixelLayout.PlaceTopLeft((RectTransform)_title.transform, inset, PixelLayout.CenterOffset(height - 1, line), width - inset * 2, line);
            // 줄 높이가 아니라 글자 모양으로 바탕 그림(테두리 1px 안쪽) 세로 가운데에 맞춘다(위로 치우쳐 보이던 것).
            UiTextInk.CenterVertically(_title.GetComponent<TMPro.TMP_Text>(), 1, height - 3);

            _line.sprite = null;
            _line.color = values.LineColor;
            _line.raycastTarget = false;
            PixelLayout.PlaceTopLeft(_line.rectTransform, 0, height - 1, width, 1);
        }

        // 고른 칸 > 마우스 올린 칸 > 바탕 없음.
        private void ApplyBacking()
        {
            bool show = _selected || _hovered;
            if (_selection.gameObject.activeSelf != show) _selection.gameObject.SetActive(show);
            if (show) _selection.SetSkin(_selected ? UiPanelSkin.BookRowSelected : UiPanelSkin.BookRowHover);
        }

        public void OnPointerEnter(PointerEventData eventData) { _hovered = true; ApplyBacking(); }
        public void OnPointerExit(PointerEventData eventData) { _hovered = false; ApplyBacking(); }
        private void OnDisable() => _hovered = false;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_entry != null && eventData.button == PointerEventData.InputButton.Left) Clicked?.Invoke(_entry);
        }
    }
}
