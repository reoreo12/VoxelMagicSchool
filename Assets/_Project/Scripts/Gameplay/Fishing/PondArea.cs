// 연못. 지팡이를 들고 E를 누르면 낚싯줄을 던진다. 찌가 떨어질 지점(플레이어 쪽 물가에서 안쪽)을 정한다.
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class PondArea : InteractableBase
    {
        [SerializeField] private Collider _water;
        [Tooltip("물가에서 연못 안쪽으로 찌를 던지는 거리(m)")]
        [SerializeField, Min(0f)] private float _castDistance = 1.5f;
        [SerializeField] private float _waterHeight = 0.06f;
        [SerializeField] private float _promptHeight = 1.2f;

        /// <summary>마지막으로 던진 찌의 착수 지점.</summary>
        public Vector3 CastPoint { get; private set; }

        public override Vector3 ClosestPoint(Vector3 from) => _water.ClosestPoint(from);

        public override InteractionPrompt GetPrompt(in InteractionContext context)
        {
            var anchor = ClosestPoint(context.PlayerPosition) + Vector3.up * _promptHeight;
            return ToolDefinition.IsWand(context.SelectedItem)
                ? InteractionPrompt.Available(GameTexts.CastLine, anchor)
                : InteractionPrompt.Blocked(GameTexts.CastLine, GameTexts.NeedWandToFish, anchor);
        }

        public override void Interact(in InteractionContext context)
        {
            CastPoint = ComputeCastPoint(context.PlayerPosition);
            Context.Fishing.TryCast();
        }

        /// <summary>결과 표시 중 E로 다시 던질 때도 같은 규칙으로 착수 지점을 정한다.</summary>
        public void UpdateCastPoint(Vector3 playerPosition) => CastPoint = ComputeCastPoint(playerPosition);

        private Vector3 ComputeCastPoint(Vector3 playerPosition)
        {
            var bounds = _water.bounds;
            var edge = _water.ClosestPoint(playerPosition);
            var inward = bounds.center - edge;
            inward.y = 0f;
            var point = edge + inward.normalized * Mathf.Min(_castDistance, inward.magnitude);
            point.y = transform.position.y + _waterHeight;
            return point;
        }
    }
}
