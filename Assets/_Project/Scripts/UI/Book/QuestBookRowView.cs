// 책 의뢰 탭 왼쪽 목록의 한 칸: 의뢰 제목 한 줄 + 아래 구분선. 고른 칸은 진한 종이 바탕 + 굵은 글씨, 완료(납품 가능)는 파란 글자.
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class QuestBookRowView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image _hit;
        [SerializeField] private ThemedPanel _selection;
        [SerializeField] private ThemedText _title;
        [SerializeField] private Image _line;

        private AcceptedRequestEntry _entry;

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

            if (_selection.gameObject.activeSelf != selected) _selection.gameObject.SetActive(selected);
            PixelLayout.PlaceTopLeft((RectTransform)_selection.transform, 0, 0, width, height - 1);

            var style = selected ? UiTextStyle.BodyBold : UiTextStyle.Body;
            if (_title.Style != style) _title.SetStyle(style);
            _title.SetText(entry.Title);
            _title.SetColorOverride(entry.IsComplete, theme.QuestList.CompletedColor);
            var textStyle = theme.TextStyle(style);
            int line = textStyle != null ? textStyle.LineHeightFrom(theme.Fonts) : 16;
            int inset = values.RowTextInset;
            PixelLayout.PlaceTopLeft((RectTransform)_title.transform, inset, PixelLayout.CenterOffset(height - 1, line), width - inset * 2, line);

            _line.sprite = null;
            _line.color = values.LineColor;
            _line.raycastTarget = false;
            PixelLayout.PlaceTopLeft(_line.rectTransform, 0, height - 1, width, 1);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_entry != null && eventData.button == PointerEventData.InputButton.Left) Clicked?.Invoke(_entry);
        }
    }
}
