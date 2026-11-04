// 씨앗을 들고 텃밭을 볼 때 심을 자리를 칸 크기만큼 보여 준다. 심을 수 있으면 초록, 아니면 빨강.
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class PlacementPreview : MonoBehaviour
    {
        [SerializeField] private FarmField _field;
        [SerializeField] private Renderer _quad;
        [SerializeField] private float _height = 0.12f;

        private MaterialPropertyBlock _block;
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        private void Awake()
        {
            _block = new MaterialPropertyBlock();
            _quad.enabled = false;
        }

        private void LateUpdate()
        {
            var action = _field.CurrentAction;
            bool show = _field.Config != null && _field.IsTargetedThisFrame && action.Type == FarmActionType.Plant;
            _quad.enabled = show;
            if (!show) return;

            var config = _field.Config;
            var t = _quad.transform;
            t.position = _field.AreaCenter(action.Origin, action.Size) + Vector3.up * _height;
            t.localScale = new Vector3(action.Size.x * config.CellSize, 1f, action.Size.y * config.CellSize) * 0.1f;

            _block.SetColor(BaseColor, action.CanExecute ? config.PreviewValidColor : config.PreviewInvalidColor);
            _quad.SetPropertyBlock(_block);
        }
    }
}
