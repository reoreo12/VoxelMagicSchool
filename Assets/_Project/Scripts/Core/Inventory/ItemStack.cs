// 인벤토리 한 칸의 읽기 전용 상태 (아이템, 개수).
namespace VoxelMagic.Core
{
    public readonly struct ItemStack
    {
        public static readonly ItemStack Empty = default;

        public ItemDefinition Item { get; }
        public int Count { get; }
        public bool IsEmpty => Item == null || Count <= 0;

        public ItemStack(ItemDefinition item, int count)
        {
            Item = count > 0 ? item : null;
            Count = item != null ? count : 0;
        }
    }
}
