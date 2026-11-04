// 상호작용 가능한 모든 대상의 공통 계약. 플레이어는 대상 종류를 몰라도 안내를 띄우고 E로 실행한다.
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public readonly struct InteractionContext
    {
        public InteractionContext(ItemDefinition selectedItem, Vector3 playerPosition, Vector3 playerForward)
        {
            SelectedItem = selectedItem;
            PlayerPosition = playerPosition;
            PlayerForward = playerForward;
        }

        public ItemDefinition SelectedItem { get; }
        public Vector3 PlayerPosition { get; }
        public Vector3 PlayerForward { get; }
    }

    public interface IInteractable
    {
        bool CanBeTargeted { get; }
        /// <summary>플레이어 위치에서 가장 가까운 대상 표면 위의 점 (거리 비교용).</summary>
        Vector3 ClosestPoint(Vector3 from);
        InteractionPrompt GetPrompt(in InteractionContext context);
        void Interact(in InteractionContext context);
    }
}
