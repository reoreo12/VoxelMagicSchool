// 화면 하단 인벤토리 줄. 칸 수만큼 칸을 만들고, Inventory 이벤트로만 갱신한다. 선택한 아이템 이름도 보여 준다.
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class InventoryBar : MonoBehaviour, IGameContextBinder
    {
        [SerializeField] private RectTransform _slotRoot;
        [SerializeField] private InventorySlotView _slotPrefab;
        [SerializeField] private TMP_Text _selectedName;

        private readonly List<InventorySlotView> _slots = new List<InventorySlotView>();
        private Inventory _inventory;

        public void Bind(GameContext context)
        {
            _inventory = context.Inventory;
            for (int i = 0; i < _inventory.SlotCount; i++)
            {
                var slot = Instantiate(_slotPrefab, _slotRoot);
                slot.Setup(i, _inventory.Select);
                _slots.Add(slot);
            }
            _inventory.SlotChanged += OnSlotChanged;
            _inventory.SelectionChanged += OnSelectionChanged;

            for (int i = 0; i < _slots.Count; i++) _slots[i].Show(_inventory.GetSlot(i));
            OnSelectionChanged(_inventory.SelectedIndex);
        }

        private void OnDestroy()
        {
            if (_inventory == null) return;
            _inventory.SlotChanged -= OnSlotChanged;
            _inventory.SelectionChanged -= OnSelectionChanged;
        }

        private void OnSlotChanged(int index)
        {
            _slots[index].Show(_inventory.GetSlot(index));
            if (index == _inventory.SelectedIndex) RefreshSelectedName();
        }

        private void OnSelectionChanged(int selected)
        {
            for (int i = 0; i < _slots.Count; i++) _slots[i].SetSelected(i == selected);
            RefreshSelectedName();
        }

        private void RefreshSelectedName()
        {
            var item = _inventory.SelectedItem;
            _selectedName.text = item != null ? item.DisplayName : string.Empty;
        }
    }
}
