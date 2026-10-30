// 텃밭 전체를 하나의 상호작용 대상으로 다룬다. 플레이어 앞 칸을 계산해 FarmService에 행동을 묻고 실행한다.
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class FarmField : InteractableBase
    {
        [SerializeField] private float _promptHeight = 1.2f;

        private int _lastQueriedFrame = -1;

        public FarmConfig Config => Context?.FarmConfig;
        /// <summary>격자 (0,0) 칸의 바깥 모서리 = 이 오브젝트 위치.</summary>
        public Vector3 Origin => transform.position;
        /// <summary>이번 프레임에 플레이어가 이 텃밭을 대상으로 삼았을 때의 행동.</summary>
        public FarmAction CurrentAction { get; private set; }
        public bool IsTargetedThisFrame => _lastQueriedFrame >= Time.frameCount - 1;

        public override Vector3 ClosestPoint(Vector3 from)
        {
            if (Context == null) return transform.position;
            float size = Context.FarmGrid.Size * Config.CellSize;
            return new Vector3(
                Mathf.Clamp(from.x, Origin.x, Origin.x + size),
                Origin.y,
                Mathf.Clamp(from.z, Origin.z, Origin.z + size));
        }

        public override InteractionPrompt GetPrompt(in InteractionContext context)
        {
            CurrentAction = ResolveAt(context);
            _lastQueriedFrame = Time.frameCount;

            var action = CurrentAction;
            if (action.Type == FarmActionType.None) return InteractionPrompt.None;
            var anchor = AreaCenter(action.Origin, action.Size) + Vector3.up * _promptHeight;
            return action.CanExecute
                ? InteractionPrompt.Available(action.Label, anchor)
                : InteractionPrompt.Blocked(action.Label, action.BlockedReason, anchor);
        }

        public override void Interact(in InteractionContext context)
        {
            Context.Farm.Execute(ResolveAt(context));
        }

        public Vector2Int WorldToCell(Vector3 world)
        {
            var local = (world - Origin) / Config.CellSize;
            return new Vector2Int(Mathf.FloorToInt(local.x), Mathf.FloorToInt(local.z));
        }

        /// <summary>칸 영역(시작 칸 + 크기)의 월드 중심.</summary>
        public Vector3 AreaCenter(Vector2Int origin, Vector2Int size)
        {
            float s = Config.CellSize;
            return Origin + new Vector3((origin.x + size.x * 0.5f) * s, 0f, (origin.y + size.y * 0.5f) * s);
        }

        private FarmAction ResolveAt(in InteractionContext context)
        {
            var target = context.PlayerPosition + context.PlayerForward * Config.TargetForwardOffset;
            var cell = WorldToCell(target);
            return Context.FarmGrid.IsInside(cell)
                ? Context.Farm.Resolve(cell, context.SelectedItem)
                : FarmAction.None;
        }
    }
}
