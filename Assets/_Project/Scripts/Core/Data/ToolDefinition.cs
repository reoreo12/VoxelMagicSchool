// 도구 아이템 정의. 지팡이는 마법 도구이자 물뿌리개, 낚싯대 역할을 한다.
using UnityEngine;

namespace VoxelMagic.Core
{
    public enum ToolKind
    {
        Wand
    }

    [CreateAssetMenu(menuName = "VoxelMagic/Items/Tool", fileName = "Tool_")]
    public sealed class ToolDefinition : ItemDefinition
    {
        [SerializeField] private ToolKind _kind = ToolKind.Wand;

        public ToolKind Kind => _kind;

        public static bool IsWand(ItemDefinition item) => item is ToolDefinition tool && tool.Kind == ToolKind.Wand;
    }
}
