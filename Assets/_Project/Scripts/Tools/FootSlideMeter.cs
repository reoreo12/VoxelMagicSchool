// 제자리(In Place) 클립에서 디딤발(더 낮은 발)이 뒤로 미끄러지는 속도를 잰다 = 이 애니메이션이 원래 걷는 속도.
// 재생 속도로 나눠 "재생 속도 1 기준" 값으로 맞추고, 골반이 앞뒤로 움직인 폭(최대-최소)으로 제자리 여부도 알려 준다.
using UnityEngine;

namespace VoxelMagic.Tools
{
    public sealed class FootSlideMeter
    {
        private Vector3 _lastLeft;
        private Vector3 _lastRight;
        private bool _hasLast;
        private int _samples;
        private float _hipMin;
        private float _hipMax;

        public float NaturalSpeed { get; private set; }
        public float HipDrift { get; private set; }
        public bool HasValue => _samples > 0;

        public void Reset()
        {
            _hasLast = false;
            _samples = 0;
            NaturalSpeed = 0f;
            HipDrift = 0f;
            _hipMin = float.MaxValue;
            _hipMax = float.MinValue;
        }

        public void Sample(Transform root, Animator animator, float deltaTime, float playbackSpeed, float smoothing)
        {
            var leftBone = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            var rightBone = animator.GetBoneTransform(HumanBodyBones.RightFoot);
            var hips = animator.GetBoneTransform(HumanBodyBones.Hips);
            if (leftBone == null || rightBone == null || hips == null) return;

            var hipLocal = root.InverseTransformPoint(hips.position);
            // 걸음마다 생기는 흔들림은 작고, 제자리가 아닌 클립은 한 주기 동안 크게 앞으로 나간다.
            _hipMin = Mathf.Min(_hipMin, hipLocal.z);
            _hipMax = Mathf.Max(_hipMax, hipLocal.z);
            HipDrift = _hipMax - _hipMin;

            var left = root.InverseTransformPoint(leftBone.position);
            var right = root.InverseTransformPoint(rightBone.position);
            if (_hasLast && deltaTime > 0f && playbackSpeed > 0.01f)
            {
                bool leftPlanted = left.y < right.y;
                float dz = leftPlanted ? left.z - _lastLeft.z : right.z - _lastRight.z;
                float speed = -dz / deltaTime / playbackSpeed;   // 앞(+Z)을 보는 아바타에서 뒤로 가는 속도
                if (speed > 0f)
                {
                    NaturalSpeed = _samples == 0 ? speed : Mathf.Lerp(NaturalSpeed, speed, smoothing);
                    _samples++;
                }
            }
            _lastLeft = left;
            _lastRight = right;
            _hasLast = true;
        }
    }
}
