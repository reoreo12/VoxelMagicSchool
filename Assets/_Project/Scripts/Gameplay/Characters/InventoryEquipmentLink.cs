// 인벤토리 선택과 장착을 잇는다: 장착물이 있는 도구 칸을 고르면 손에 들고, 다른 칸을 고르면 내린다.
// Inventory와 EquipmentState는 서로 모르고, 이 부품만 둘을 안다 (인벤토리가 없는 NPC는 이 부품 없이 기본 장착만 쓴다).
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class InventoryEquipmentLink : MonoBehaviour, IGameContextBinder
    {
        [SerializeField] private CharacterEquipment _equipment;

        private Inventory _inventory;
        private AttachmentDefinition _linked;

        public void Bind(GameContext context)
        {
            _inventory = context.Inventory;
            _inventory.SelectionChanged += OnInventoryChanged;
            _inventory.SlotChanged += OnInventoryChanged;
            Apply();
        }

        private void OnDestroy()
        {
            if (_inventory == null) return;
            _inventory.SelectionChanged -= OnInventoryChanged;
            _inventory.SlotChanged -= OnInventoryChanged;
        }

        private void OnInventoryChanged(int _) => Apply();

        private void Apply()
        {
            var wanted = _inventory.SelectedItem is ToolDefinition tool ? tool.Attachment : null;
            if (wanted == _linked) return;

            var state = _equipment.State;
            if (_linked != null && state.Get(_linked.Socket) == _linked)
                state.Unequip(_linked.Socket);
            _linked = wanted;
            if (wanted != null) state.Equip(wanted);
        }
    }
}
