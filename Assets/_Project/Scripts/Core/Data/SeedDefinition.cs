// 씨앗 정의. 어떤 작물이 되는지, 텃밭에서 몇 칸을 차지하는지, 새싹/다 자란 모양을 정한다.
using UnityEngine;

namespace VoxelMagic.Core
{
    [CreateAssetMenu(menuName = "VoxelMagic/Items/Seed", fileName = "Seed_")]
    public sealed class SeedDefinition : ItemDefinition
    {
        [SerializeField] private ProduceDefinition _produce;
        [Tooltip("차지하는 칸 수 (x: 가로, y: 세로)")]
        [SerializeField] private Vector2Int _footprint = Vector2Int.one;
        [SerializeField] private GameObject _sproutPrefab;
        [SerializeField] private GameObject _grownPrefab;

        public ProduceDefinition Produce => _produce;
        public Vector2Int Footprint => _footprint;
        public GameObject SproutPrefab => _sproutPrefab;
        public GameObject GrownPrefab => _grownPrefab;

        private void OnValidate()
        {
            _footprint = Vector2Int.Max(_footprint, Vector2Int.one);
        }
    }
}
