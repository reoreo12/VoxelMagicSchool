// 뒷산 포획 동물. 마법진 정확도가 요구 정확도 이상이면 잡힌다. 잡으면 인벤토리 아이템이 된다.
using UnityEngine;

namespace VoxelMagic.Core
{
    [CreateAssetMenu(menuName = "VoxelMagic/Items/Animal", fileName = "Animal_")]
    public sealed class AnimalDefinition : ItemDefinition, ISizedItem
    {
        [Tooltip("마법진 정확도(0~1)가 이 값 이상이면 포획 성공")]
        [SerializeField, Range(0f, 1f)] private float _requiredAccuracy = 0.5f;
        [SerializeField] private FloatRange _sizeRangeCm = new FloatRange(20f, 40f);

        public float RequiredAccuracy => _requiredAccuracy;
        public FloatRange SizeRangeCm => _sizeRangeCm;
    }
}
