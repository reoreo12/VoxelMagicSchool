// 캐릭터 이동 설정(복도·텃밭이 각자 에셋을 가짐): 걷기·달리기 속도, 가속·감속, 장면별 애니메이션 재생 배율,
// 그리고 복도 전용 값(시작 위치, 발 높이, 좌우 회전 각도).
using UnityEngine;

namespace VoxelMagic.Core
{
    [CreateAssetMenu(menuName = "VoxelMagic/SideScroll/Character Move Config", fileName = "CharacterMoveConfig")]
    public sealed class CharacterMoveConfig : ScriptableObject
    {
        [Header("속도 (유닛/초)")]
        [SerializeField, Min(0.1f)] private float _walkSpeed = 1.6f;
        [SerializeField, Min(0.1f)] private float _runSpeed = 3.6f;
        [Tooltip("목표 속도까지 빨라지는 정도 (유닛/초²)")]
        [SerializeField, Min(0.1f)] private float _acceleration = 12f;
        [Tooltip("멈추거나 느려지는 정도 (유닛/초²)")]
        [SerializeField, Min(0.1f)] private float _deceleration = 16f;

        [Header("애니메이션 재생 배율 (AvatarSetup 상태별 속도에 곱함, 플레이 중 바로 반영)")]
        [Tooltip("같은 아바타라도 장면마다 이동 속도가 달라 발걸음 빠르기를 따로 맞춘다")]
        [SerializeField, Range(0.1f, 6f)] private float _idleAnimationScale = 1f;
        [SerializeField, Range(0.1f, 6f)] private float _walkAnimationScale = 1f;
        [SerializeField, Range(0.1f, 6f)] private float _runAnimationScale = 1f;

        [Header("위치 (복도 전용)")]
        [SerializeField] private float _startX = 6f;
        [Tooltip("발이 닿는 높이. 바닥 띠(0~0.25) 안")]
        [SerializeField] private float _groundY = 0.12f;
        [SerializeField] private float _z = 0f;

        [Header("방향 (복도 전용)")]
        [Tooltip("오른쪽을 볼 때 Y 회전. 왼쪽은 부호를 바꾼 값")]
        [SerializeField] private float _facingYaw = 90f;

        public float WalkSpeed => _walkSpeed;
        public float AnimationScaleFor(CharacterStateId state) =>
            state == CharacterStateId.Run ? _runAnimationScale : state == CharacterStateId.Walk ? _walkAnimationScale : _idleAnimationScale;
        public float RunSpeed => _runSpeed;
        public float Acceleration => _acceleration;
        public float Deceleration => _deceleration;
        public float StartX => _startX;
        public float GroundY => _groundY;
        public float Z => _z;
        public float FacingYaw => _facingYaw;
    }
}
