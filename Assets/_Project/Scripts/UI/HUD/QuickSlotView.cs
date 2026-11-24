// 퀵슬롯 한 칸(UI_SPEC 7-1): 밝은 칸 배경 + 아이템 아이콘(16×16, 정수 px 가운데). 아이콘이 없는 아이템은 단색 칸 + 첫 글자로 보인다(아이콘 제작 전 임시).
// 클릭하면 이 칸 선택을 요청한다. 개수는 2개 이상일 때만 작은 글씨로 오른쪽 아래에 보인다(표시 방식은 [미정], 임시).
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class QuickSlotView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private ThemedPanel _background;
        [SerializeField] private Image _icon;
        [SerializeField] private Image _fallback;
        [SerializeField] private ThemedText _letter;
        [SerializeField] private ThemedText _count;

        private int _index;
        private Action<int> _onClick;
        private ItemStack _shown;
        private bool _hasShown;

        public RectTransform Rect => (RectTransform)transform;

        public void Setup(int index, Action<int> onClick)
        {
            _index = index;
            _onClick = onClick;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) _onClick?.Invoke(_index);
        }

        /// <summary>내용이 바뀐 칸만 다시 그린다(4-A-5).</summary>
        public void Show(ItemStack stack)
        {
            if (_hasShown && stack.Item == _shown.Item && stack.Count == _shown.Count) return;
            _hasShown = true;
            _shown = stack;

            bool hasItem = !stack.IsEmpty;
            bool hasIcon = hasItem && stack.Item.Icon != null;
            _icon.enabled = hasIcon;
            if (hasIcon) _icon.sprite = stack.Item.Icon;

            bool useFallback = hasItem && !hasIcon;
            _fallback.enabled = useFallback;
            _letter.gameObject.SetActive(useFallback);
            if (useFallback)
            {
                _fallback.color = stack.Item.IconColor;
                _letter.SetText(stack.Item.DisplayName.Length > 0 ? stack.Item.DisplayName.Substring(0, 1) : string.Empty);
            }

            bool showCount = hasItem && stack.Count > 1;
            _count.gameObject.SetActive(showCount);
            if (showCount) _count.SetText(stack.Count.ToString());
        }

        /// <summary>칸 크기와 아이콘 크기에 맞춰 안쪽 요소를 정수 px로 배치한다(칸의 평평한 안쪽 영역 가운데).</summary>
        public void Layout(Vector2Int slotSize, int iconSize, int borderSide, int borderTop, int borderBottom)
        {
            PixelLayout.Stretch((RectTransform)_background.transform);
            int innerHeight = slotSize.y - borderTop - borderBottom;
            int iconX = PixelLayout.CenterOffset(slotSize.x, iconSize);
            int iconY = borderTop + PixelLayout.CenterOffset(innerHeight, iconSize);
            PixelLayout.PlaceTopLeft(_icon.rectTransform, iconX, iconY, iconSize, iconSize);
            PixelLayout.PlaceTopLeft(_fallback.rectTransform, iconX, iconY, iconSize, iconSize);
            PixelLayout.PlaceTopLeft((RectTransform)_letter.transform, iconX, iconY, iconSize, iconSize);
            PixelLayout.PlaceBottomRight((RectTransform)_count.transform, borderSide, borderBottom, slotSize.x - borderSide * 2, iconSize);
        }
    }
}
