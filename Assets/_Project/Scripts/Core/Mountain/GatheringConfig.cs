// 뒷산 채집 조절 값. 다시 배치할 때 서식지 슬롯을 얼마나 채울지 정한다.
using UnityEngine;

namespace VoxelMagic.Core
{
    [CreateAssetMenu(menuName = "VoxelMagic/Config/Gathering", fileName = "GatheringConfig")]
    public sealed class GatheringConfig : ScriptableObject
    {
        [Tooltip("서식지마다 슬롯 중 재료를 놓을 비율 (가정 0.5). 나올 재료가 있으면 최소 1개는 놓는다")]
        [SerializeField, Range(0f, 1f)] private float _fillRatio = 0.5f;

        public float FillRatio => _fillRatio;
    }
}
