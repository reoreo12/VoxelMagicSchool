// 책 오른쪽 가장자리의 사이드 탭 하나(UI_SPEC 5-1): 색 탭 + 아이콘. 누르면, 또는 위에서 휠을 굴리면 책에 알린다.
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class BookTabView : MonoBehaviour, IPointerClickHandler, IScrollHandler
    {
        [SerializeField] private BookTab _tab;
        [SerializeField] private Image _background;
        [SerializeField] private Image _icon;

        public BookTab Tab => _tab;
        public RectTransform Rect => (RectTransform)transform;
        public event Action<BookTab> Clicked;
        /// <summary>휠: +1 = 위로 굴림, −1 = 아래로 굴림.</summary>
        public event Action<int> Scrolled;

        /// <summary>탭 왼쪽 위를 (x, y)에 두고 선택 여부에 맞는 그림·크기로 바꾼다. 아이콘은 표지 밖으로 보이는 안쪽 칸의 가운데.</summary>
        public void Show(BookTabValues values, bool selected, int x, int y, BookValues book)
        {
            var sprite = values != null ? (selected ? values.Selected : values.Normal) : null;
            _background.sprite = sprite;
            _background.type = Image.Type.Sliced; // 늘려도 외곽선 1px
            _background.enabled = sprite != null;
            _background.raycastTarget = true;
            var size = book.TabSize(selected);
            PixelLayout.PlaceTopLeft(Rect, x, y, size.x, size.y);
            PixelLayout.Stretch(_background.rectTransform);

            var icon = values != null ? values.Icon : null;
            _icon.sprite = icon;
            _icon.enabled = icon != null;
            _icon.raycastTarget = false;
            if (icon == null) return;
            int scale = book.TabIconScale, line = book.TabOutline;
            int w = Mathf.RoundToInt(icon.rect.width) * scale, h = Mathf.RoundToInt(icon.rect.height) * scale;
            // 안쪽 칸: 가로 = 표지에 가린 폭 뒤 ~ 오른쪽 외곽선 앞, 세로 = 위아래 외곽선 사이.
            int innerX = book.TabTuck, innerW = size.x - book.TabTuck - line;
            int innerY = line, innerH = size.y - line * 2;
            PixelLayout.PlaceTopLeft(_icon.rectTransform, innerX + (innerW - w) / 2, innerY + (innerH - h) / 2, w, h);
        }

        public void OnScroll(PointerEventData eventData)
        {
            float y = eventData.scrollDelta.y;
            if (!Mathf.Approximately(y, 0f)) Scrolled?.Invoke(y > 0f ? 1 : -1);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) Clicked?.Invoke(_tab);
        }
    }
}
