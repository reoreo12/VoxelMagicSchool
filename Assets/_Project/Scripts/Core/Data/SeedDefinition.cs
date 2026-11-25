// 씨앗 정의. 어떤 작물이 되는지, 텃밭에서 몇 칸을 차지하는지, 물 준 뒤 게임 시간 몇 시간 만에 자라는지(0 = 다음 날 아침), 새싹/다 자란 모양을 정한다.
using UnityEngine;

namespace VoxelMagic.Core
{
    [CreateAssetMenu(menuName = "VoxelMagic/Items/Seed", fileName = "Seed_")]
    public sealed class SeedDefinition : ItemDefinition
    {
        [SerializeField] private ProduceDefinition _produce;
        [Tooltip("차지하는 칸 수 (x: 가로, y: 세로)")]
        [SerializeField] private Vector2Int _footprint = Vector2Int.one;
        [Tooltip("물을 준 뒤 게임 시간으로 몇 시간 뒤 다 자라는지. 0이면 다음 날 하루 시작에 자란다 (희귀 작물)")]
        [SerializeField, Min(0f)] private float _growHours = 3f;
        [SerializeField] private GameObject _sproutPrefab;
        [SerializeField] private GameObject _grownPrefab;

        public ProduceDefinition Produce => _produce;
        public Vector2Int Footprint => _footprint;
        public float GrowHours => Mathf.Max(0f, _growHours);
        /// <summary>다음 날 하루 시작에 자라는 작물인지 (GrowHours = 0).</summary>
        public bool GrowsNextDay => _growHours <= 0f;
        public GameObject SproutPrefab => _sproutPrefab;
        public GameObject GrownPrefab => _grownPrefab;

        private void OnValidate()
        {
            _footprint = Vector2Int.Max(_footprint, Vector2Int.one);
        }
    }
}
