// 플레이어 이동과 쿼터뷰 카메라 설정. 아바타를 지정하면 씬 생성 시 기본 도형 대신 아바타를 입힌다 (비우면 기본 도형).
using UnityEngine;

namespace VoxelMagic.Core
{
    [CreateAssetMenu(menuName = "VoxelMagic/Config/Player", fileName = "PlayerConfig")]
    public sealed class PlayerConfig : ScriptableObject
    {
        [SerializeField, Min(0.1f)] private float _moveSpeed = 4f;
        [SerializeField, Min(1f)] private float _turnSpeedDegrees = 720f;
        [SerializeField, Min(0.1f)] private float _interactRadius = 1.6f;
        [Tooltip("시작할 때 카메라 거리 (스크롤로 줌 범위 안에서 바뀜)")]
        [SerializeField, Min(1f)] private float _cameraDistance = 8f;
        [Tooltip("줌 거리 범위 (가까이, 멀리)")]
        [SerializeField] private Vector2 _cameraZoomRange = new Vector2(4f, 16f);
        [Tooltip("스크롤 한 칸에 바뀌는 거리")]
        [SerializeField, Min(0.1f)] private float _cameraZoomStep = 1f;
        [SerializeField, Range(10f, 89f)] private float _cameraPitch = 45f;
        [SerializeField] private float _cameraYaw = 45f;
        [SerializeField, Min(0.1f)] private float _cameraFollowSharpness = 8f;

        [Header("아바타 (임시, 비우면 기본 도형 · 바꾸면 씬 다시 만들기)")]
        [SerializeField] private AvatarSetup _avatar;
        [Tooltip("Walk 클립을 재생 속도 1로 틀 때 걷는 속도 (m/s, 애니메이션 랩의 디딤발 속도). 실제 이동 속도 ÷ 이 값 = Walk 재생 속도")]
        [SerializeField, Min(0.1f)] private float _walkAnimationReferenceSpeed = 1f;
        [Tooltip("이 속도(m/s)보다 빠르게 움직이면 Walk, 아니면 Idle")]
        [SerializeField, Min(0f)] private float _walkStartSpeed = 0.1f;
        [SerializeField, Min(0f)] private float _animationBlendSeconds = 0.15f;

        public float MoveSpeed => _moveSpeed;
        public float TurnSpeedDegrees => _turnSpeedDegrees;
        public float InteractRadius => _interactRadius;
        public float CameraDistance => _cameraDistance;
        public Vector2 CameraZoomRange => _cameraZoomRange;
        public float CameraZoomStep => _cameraZoomStep;
        public float CameraPitch => _cameraPitch;
        public float CameraYaw => _cameraYaw;
        public float CameraFollowSharpness => _cameraFollowSharpness;
        public AvatarSetup Avatar => _avatar;
        public float WalkAnimationReferenceSpeed => _walkAnimationReferenceSpeed;
        public float WalkStartSpeed => _walkStartSpeed;
        public float AnimationBlendSeconds => _animationBlendSeconds;
    }
}
