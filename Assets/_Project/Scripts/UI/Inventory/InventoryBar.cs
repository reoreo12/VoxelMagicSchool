// 화면 하단 인벤토리 줄. 칸 수만큼 칸을 만들고, Inventory 이벤트로만 갱신한다. 선택한 아이템 이름도 보여 준다.
// 디버그 설정을 켜면 칸 우클릭으로 그 칸을 통째로 버린다.
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
        [Tooltip("디버그: 칸을 우클릭하면 그 칸의 아이템을 전부 버린다")]
        [SerializeField] private bool _debugRightClickDiscard;

        private readonly List<InventorySlotView> _slots = new List<InventorySlotView>();
        private Inventory _inventory;
        private GameMessages _messages;

        public void Bind(GameContext context)
        {
            _inventory = context.Inventory;
            _messages = context.Messages;
            for (int i = 0; i < _inventory.SlotCount; i++)
            {
                var slot = Instantiate(_slotPrefab, _slotRoot);
                slot.Setup(i, _inventory.Select, _debugRightClickDiscard ? Discard : null);
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

        private void Discard(int index)
        {
            var removed = _inventory.DiscardSlot(index);
            if (!removed.IsEmpty)
                _messages.ShowToast(string.Format(GameTexts.DiscardedFormat, removed.Item.DisplayName, removed.Count));
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
