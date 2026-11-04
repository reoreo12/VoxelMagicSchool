// 텃밭에서 뽑은 작물 결과물. 뽑을 때 크기 범위 안에서 크기가 정해진다.
using UnityEngine;

namespace VoxelMagic.Core
{
    [CreateAssetMenu(menuName = "VoxelMagic/Items/Produce", fileName = "Produce_")]
    public sealed class ProduceDefinition : ItemDefinition, ISizedItem
    {
        [SerializeField] private FloatRange _sizeRangeCm = new FloatRange(15f, 30f);

        public FloatRange SizeRangeCm => _sizeRangeCm;
    }
}
