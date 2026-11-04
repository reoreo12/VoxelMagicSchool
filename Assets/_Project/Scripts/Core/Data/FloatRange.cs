// 최소~최대 범위 값. 크기(cm)나 대기 시간처럼 무작위로 굴리는 수치에 쓴다.
using System;
using UnityEngine;

namespace VoxelMagic.Core
{
    [Serializable]
    public struct FloatRange
    {
        [SerializeField] private float _min;
        [SerializeField] private float _max;

        public FloatRange(float min, float max)
        {
            _min = min;
            _max = max;
        }

        public float Min => _min;
        public float Max => _max;

        public float Roll(IRandom random) => random.Range(_min, _max);
    }
}
