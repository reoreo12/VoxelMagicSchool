// 서식지 슬롯 하나 = 줍기 대상. 놓인 재료의 모양(FBX 메시·공용 머티리얼, 없으면 기본 도형+색)을 보여 주고, 가까이 가면 "E: 줍기", E로 가방에 넣는다.
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class GatherPickup : InteractableBase
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private MeshFilter _itemFilter;
        [SerializeField] private MeshRenderer _itemRenderer;

        private GatherHabitat _habitat;
        private int _slot;
        private MaterialPropertyBlock _block;
        private Material _defaultMaterial;

        public GatherMaterialDefinition Material => _habitat != null ? _habitat.Get(_slot) : null;

        public override bool CanBeTargeted => base.CanBeTargeted && Material != null;

        public void Setup(GatherHabitat habitat, int slot)
        {
            _habitat = habitat;
            _slot = slot;
        }

        public void Show(GatherMaterialDefinition material)
        {
            _itemRenderer.enabled = material != null;
            if (material == null) return;

            var mesh = material.WorldMesh;
            _itemFilter.sharedMesh = mesh;
            var scale = material.WorldScale;
            var rotation = material.WorldRotation;
            var item = _itemFilter.transform;
            item.localScale = scale;
            item.localRotation = rotation;
            // 메시 바닥이 슬롯(지면)에 닿게 올린다.
            float bottom = mesh != null ? LowestY(mesh.bounds, scale, rotation) : -scale.y * 0.5f;
            item.localPosition = new Vector3(0f, -bottom, 0f);

            ApplyLook(material);
        }

        // FBX 재료는 공용 머티리얼 그대로(SRP Batcher 묶음 유지), 임시 재료는 기본 머티리얼에 아이콘 색.
        private void ApplyLook(GatherMaterialDefinition material)
        {
            if (_defaultMaterial == null) _defaultMaterial = _itemRenderer.sharedMaterial;

            if (material.WorldMaterial != null)
            {
                _itemRenderer.sharedMaterial = material.WorldMaterial;
                _itemRenderer.SetPropertyBlock(null);
                return;
            }

            _itemRenderer.sharedMaterial = _defaultMaterial;
            _block ??= new MaterialPropertyBlock();
            _block.SetColor(BaseColorId, material.IconColor);
            _itemRenderer.SetPropertyBlock(_block);
        }

        // 회전·크기를 적용한 메시 경계 상자의 가장 낮은 y (로컬 기준).
        private static float LowestY(Bounds bounds, Vector3 scale, Quaternion rotation)
        {
            float lowest = float.MaxValue;
            for (int i = 0; i < 8; i++)
            {
                var corner = new Vector3(
                    (i & 1) == 0 ? bounds.min.x : bounds.max.x,
                    (i & 2) == 0 ? bounds.min.y : bounds.max.y,
                    (i & 4) == 0 ? bounds.min.z : bounds.max.z);
                float y = (rotation * Vector3.Scale(corner, scale)).y;
                if (y < lowest) lowest = y;
            }
            return lowest;
        }

        public override InteractionPrompt GetPrompt(in InteractionContext context)
        {
            if (Material == null) return InteractionPrompt.None;
            return _habitat.CanStore(_slot)
                ? InteractionPrompt.Available(MountainTexts.Pick, PromptAnchor)
                : InteractionPrompt.Blocked(MountainTexts.Pick, GameTexts.InventoryFull, PromptAnchor);
        }

        public override void Interact(in InteractionContext context) => _habitat.TryPick(_slot);
    }
}
