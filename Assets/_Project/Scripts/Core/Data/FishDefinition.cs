// 물고기 정의. 크기 범위와 미니게임에서의 움직임(속도, 변덕, 급이동)을 데이터로 정한다.
using UnityEngine;

namespace VoxelMagic.Core
{
    [CreateAssetMenu(menuName = "VoxelMagic/Items/Fish", fileName = "Fish_")]
    public sealed class FishDefinition : ItemDefinition, ISizedItem
    {
        [SerializeField] private FloatRange _sizeRangeCm = new FloatRange(10f, 25f);
        [Tooltip("미니게임 바 높이(0~1) 기준 초당 이동량")]
        [SerializeField, Min(0.01f)] private float _moveSpeed = 0.6f;
        [Tooltip("목표 지점을 다시 정하는 간격(초). 짧을수록 변덕스럽다")]
        [SerializeField] private FloatRange _retargetIntervalSeconds = new FloatRange(0.8f, 1.6f);
        [Tooltip("목표를 바꿀 때 급히 움직일 확률")]
        [SerializeField, Range(0f, 1f)] private float _dartChance;
        [SerializeField, Min(1f)] private float _dartSpeedMultiplier = 3f;

        public FloatRange SizeRangeCm => _sizeRangeCm;
        public float MoveSpeed => _moveSpeed;
        public FloatRange RetargetIntervalSeconds => _retargetIntervalSeconds;
        public float DartChance => _dartChance;
        public float DartSpeedMultiplier => _dartSpeedMultiplier;
    }
}
