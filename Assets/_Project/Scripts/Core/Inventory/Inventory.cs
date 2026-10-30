// 칸 수가 고정된 인벤토리. 같은 아이템은 겹쳐 쌓고, 바뀐 칸과 선택 변경을 이벤트로 알린다.
// 추가·제거는 전부 되거나 전부 안 되거나(부분 성공 없음)로 처리한다.
using System;
using System.Collections.Generic;

namespace VoxelMagic.Core
{
    public sealed class Inventory
    {
        private readonly ItemStack[] _slots;
        private readonly List<int> _changedBuffer = new List<int>();
        private int _selectedIndex;

        public event Action<int> SlotChanged;
        public event Action<int> SelectionChanged;

        public Inventory(int slotCount)
        {
            if (slotCount < 1) throw new ArgumentOutOfRangeException(nameof(slotCount));
            _slots = new ItemStack[slotCount];
        }

        public int SlotCount => _slots.Length;
        public int SelectedIndex => _selectedIndex;
        public ItemDefinition SelectedItem => _slots[_selectedIndex].Item;

        public ItemStack GetSlot(int index) => _slots[index];

        public void Select(int index)
        {
            if (index < 0 || index >= _slots.Length || index == _selectedIndex) return;
            _selectedIndex = index;
            SelectionChanged?.Invoke(index);
        }

        public int Count(ItemDefinition item)
        {
            if (item == null) return 0;
            int total = 0;
            foreach (var slot in _slots)
                if (slot.Item == item) total += slot.Count;
            return total;
        }

        public bool Contains(ItemDefinition item, int amount) => Count(item) >= amount;

        public bool CanAdd(ItemDefinition item, int amount)
        {
            if (item == null || amount <= 0) return false;
            var copy = (ItemStack[])_slots.Clone();
            return Fill(copy, item, amount, null);
        }

        /// <summary>여러 아이템을 한꺼번에 넣을 공간이 있는지 (의뢰 씨앗 지급 확인용).</summary>
        public bool CanAddAll(IReadOnlyList<ItemAmount> items)
        {
            var copy = (ItemStack[])_slots.Clone();
            foreach (var entry in items)
                if (!Fill(copy, entry.Item, entry.Amount, null)) return false;
            return true;
        }

        public bool TryAdd(ItemDefinition item, int amount)
        {
            if (!CanAdd(item, amount)) return false;
            _changedBuffer.Clear();
            Fill(_slots, item, amount, _changedBuffer);
            NotifyChanged();
            return true;
        }

        public bool TryRemove(ItemDefinition item, int amount)
        {
            if (item == null || amount <= 0 || Count(item) < amount) return false;
            _changedBuffer.Clear();
            for (int i = _slots.Length - 1; i >= 0 && amount > 0; i--)
            {
                if (_slots[i].Item != item) continue;
                int take = Math.Min(_slots[i].Count, amount);
                _slots[i] = new ItemStack(item, _slots[i].Count - take);
                amount -= take;
                _changedBuffer.Add(i);
            }
            NotifyChanged();
            return true;
        }

        private static bool Fill(ItemStack[] slots, ItemDefinition item, int amount, List<int> changed)
        {
            if (item == null || amount <= 0) return false;
            int maxStack = Math.Max(1, item.MaxStack);

            for (int i = 0; i < slots.Length && amount > 0; i++)
            {
                if (slots[i].Item != item || slots[i].Count >= maxStack) continue;
                int add = Math.Min(maxStack - slots[i].Count, amount);
                slots[i] = new ItemStack(item, slots[i].Count + add);
                amount -= add;
                changed?.Add(i);
            }
            for (int i = 0; i < slots.Length && amount > 0; i++)
            {
                if (!slots[i].IsEmpty) continue;
                int add = Math.Min(maxStack, amount);
                slots[i] = new ItemStack(item, add);
                amount -= add;
                changed?.Add(i);
            }
            return amount == 0;
        }

        private void NotifyChanged()
        {
            foreach (int index in _changedBuffer)
                SlotChanged?.Invoke(index);
            _changedBuffer.Clear();
        }
    }
}
