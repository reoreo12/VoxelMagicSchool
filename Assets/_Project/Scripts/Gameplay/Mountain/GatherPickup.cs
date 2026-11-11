// 서식지 슬롯 하나 = 줍기 대상. 놓인 재료의 기본 도형·색을 보여 주고, 가까이 가면 "E: 줍기", E로 가방에 넣는다.
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
            var item = _itemFilter.transform;
            item.localScale = scale;
            // 메시 바닥이 슬롯(지면)에 닿게 올린다.
            float bottom = mesh != null ? mesh.bounds.min.y * scale.y : -scale.y * 0.5f;
            item.localPosition = new Vector3(0f, -bottom, 0f);

            _block ??= new MaterialPropertyBlock();
            _block.SetColor(BaseColorId, material.IconColor);
            _itemRenderer.SetPropertyBlock(_block);
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
