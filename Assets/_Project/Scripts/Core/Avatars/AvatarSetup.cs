// 아바타 한 벌의 데이터: 모델, 목표 키, Idle/Walk/Run 클립과 상태별 재생 속도, 생성된 Controller·프리팹.
// 새 아바타는 메뉴(VoxelMagic/아바타/1. 선택한 캐릭터 모델 준비)가 이 에셋을 만들고, 재생 속도는 플레이 중 바꿔도 바로 반영된다.
using UnityEngine;

namespace VoxelMagic.Core
{
    [CreateAssetMenu(menuName = "VoxelMagic/Avatar Setup", fileName = "AvatarSetup")]
    public sealed class AvatarSetup : ScriptableObject
    {
        [Header("모델")]
        [Tooltip("Art/Characters/<이름>/ 의 Humanoid FBX")]
        [SerializeField] private GameObject _model;
        [Tooltip("임포트 배율을 이 키(유닛)에 맞춰 계산한다")]
        [SerializeField, Min(0.1f)] private float _targetHeight = 1.6f;
        [SerializeField, Min(0.01f)] private float _colliderRadius = 0.25f;

        [Header("상태별 클립 (Humanoid)")]
        [SerializeField] private AnimationClip _idle;
        [SerializeField] private AnimationClip _walk;
        [SerializeField] private AnimationClip _run;

        [Header("상태별 재생 속도 (플레이 중 바로 반영)")]
        [SerializeField, Range(0.1f, 3f)] private float _idleSpeed = 1f;
        [SerializeField, Range(0.1f, 3f)] private float _walkSpeed = 1f;
        [SerializeField, Range(0.1f, 3f)] private float _runSpeed = 1f;

        [Header("2D 버전 (있으면 복도에서 2D/3D 전환)")]
        [SerializeField] private SpriteCharacterSet _sprites;

        [Header("소켓 (손 등 장착 위치, 아바타별 보정)")]
        [SerializeField] private SocketProfile _socketProfile;

        [Header("생성됨 (메뉴 3)")]
        [SerializeField] private RuntimeAnimatorController _controller;
        [SerializeField] private GameObject _characterPrefab;

        public GameObject Model => _model;
        public float TargetHeight => _targetHeight;
        public float ColliderRadius => _colliderRadius;
        public SpriteCharacterSet Sprites => _sprites;
        public SocketProfile SocketProfile => _socketProfile;
        public RuntimeAnimatorController Controller => _controller;
        public GameObject CharacterPrefab => _characterPrefab;

        public AnimationClip ClipFor(CharacterStateId state) =>
            state == CharacterStateId.Run ? _run : state == CharacterStateId.Walk ? _walk : _idle;

        public float SpeedFor(CharacterStateId state) =>
            state == CharacterStateId.Run ? _runSpeed : state == CharacterStateId.Walk ? _walkSpeed : _idleSpeed;

        /// <summary>Animator Controller에서 상태별 재생 속도를 받는 float 파라미터 이름.</summary>
        public static string SpeedParameter(CharacterStateId state) => state + "Speed";
    }
}
