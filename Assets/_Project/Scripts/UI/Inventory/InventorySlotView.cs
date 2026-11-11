// 인벤토리 한 칸 표시: 단색 사각형 + 이름 첫 글자, 개수, 선택 강조. 클릭하면 선택을, 우클릭하면(켜져 있을 때) 버리기를 요청한다.
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class InventorySlotView : MonoBehaviour, IPointerClickHandler
    {
        private int _index;
        private Action<int> _onRightClick;
        [SerializeField] private Image _highlight;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _letter;
        [SerializeField] private TMP_Text _count;
        [SerializeField] private TMP_Text _keyNumber;
        [SerializeField] private Button _button;

        public void Setup(int index, Action<int> onClick, Action<int> onRightClick = null)
        {
            _index = index;
            _onRightClick = onRightClick;
            _keyNumber.text = (index + 1).ToString();
            _button.onClick.AddListener(() => onClick(index));
        }

        // Button은 왼쪽 클릭만 처리하므로 우클릭은 여기서 따로 받는다.
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right) _onRightClick?.Invoke(_index);
        }

        public void Show(ItemStack stack)
        {
            bool hasItem = !stack.IsEmpty;
            _icon.enabled = hasItem;
            _letter.text = hasItem ? stack.Item.DisplayName.Substring(0, 1) : string.Empty;
            _count.text = hasItem && stack.Count > 1 ? stack.Count.ToString() : string.Empty;
            if (!hasItem) return;

            var color = stack.Item.IconColor;
            _icon.color = color;
            float luminance = 0.299f * color.r + 0.587f * color.g + 0.114f * color.b;
            _letter.color = luminance > 0.6f ? new Color(0.15f, 0.1f, 0.05f) : Color.white;
        }

        public void SetSelected(bool selected) => _highlight.enabled = selected;
    }
}
