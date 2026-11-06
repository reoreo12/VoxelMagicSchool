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
        [Tooltip("이 도구를 고르면 손에 드는 장착물 (비우면 아무것도 들지 않음)")]
        [SerializeField] private AttachmentDefinition _attachment;

        public ToolKind Kind => _kind;
        public AttachmentDefinition Attachment => _attachment;

        public static bool IsWand(ItemDefinition item) => item is ToolDefinition tool && tool.Kind == ToolKind.Wand;
    }
}
