// 애니메이션 랩 설정: 재생·섞기·A↔B 전환 범위, 프레임 단위, 측정 부드럽게, 시작 장착물, 카메라 조작 감도.
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Tools
{
    [CreateAssetMenu(menuName = "VoxelMagic/Tools/Animation Lab Config", fileName = "AnimationLabConfig")]
    public sealed class AnimationLabConfig : ScriptableObject
    {
        [Header("재생")]
        [SerializeField, Min(0.1f)] private float _maxSpeed = 2f;
        [SerializeField, Min(0f)] private float _defaultBlend = 0.25f;
        [SerializeField, Min(0.1f)] private float _maxBlend = 1f;
        [Tooltip("한 프레임 앞/뒤 버튼의 프레임 단위")]
        [SerializeField, Min(1f)] private float _stepFrameRate = 30f;

        [Header("A↔B 반복 전환 (초)")]
        [SerializeField, Min(0.1f)] private float _abDefaultInterval = 2f;
        [SerializeField, Min(0.2f)] private float _abMaxInterval = 6f;

        [Header("측정")]
        [Tooltip("디딤발 속도 평균을 새 값 쪽으로 옮기는 비율 (작을수록 부드러움)")]
        [SerializeField, Range(0.01f, 1f)] private float _meterSmoothing = 0.05f;
        [Tooltip("골반 앞뒤 이동 폭이 이보다 크면 제자리(In Place)가 아니라고 표시 (m)")]
        [SerializeField, Min(0.01f)] private float _inPlaceTolerance = 0.2f;
        [Tooltip("재생 속도 제안에 쓸 게임 이동 속도")]
        [SerializeField] private CharacterMoveConfig _moveConfig;

        [Header("도트 렌더링")]
        [Tooltip("픽셀 크기 슬라이더 범위 (화면 픽셀 몇 개를 도트 하나로 그릴지)")]
        [SerializeField] private Vector2Int _pixelSizeRange = new Vector2Int(2, 8);
        [SerializeField, Min(1)] private int _defaultPixelSize = 4;

        [Header("장착물")]
        [Tooltip("랩을 열 때 장착물 목록의 첫 번째를 손에 들려 둘지")]
        [SerializeField] private bool _equipFirstAttachmentOnStart = true;

        [Header("카메라")]
        [SerializeField, Min(0.01f)] private float _orbitSpeed = 0.25f;
        [SerializeField, Min(0.01f)] private float _zoomSpeed = 0.002f;
        [SerializeField, Min(0.0001f)] private float _panSpeed = 0.003f;
        [SerializeField, Min(0.1f)] private float _minDistance = 0.8f;
        [SerializeField, Min(0.2f)] private float _maxDistance = 10f;
        [Tooltip("시작 거리 = 아바타 키 × 이 값")]
        [SerializeField, Min(0.5f)] private float _distancePerHeight = 2.6f;
        [Tooltip("바라보는 높이 = 아바타 키 × 이 값")]
        [SerializeField, Range(0f, 1f)] private float _focusHeightRatio = 0.45f;
        [SerializeField] private Vector2 _pitchRange = new Vector2(-20f, 80f);

        public float MaxSpeed => _maxSpeed;
        public float DefaultBlend => _defaultBlend;
        public float MaxBlend => _maxBlend;
        public float StepFrameRate => _stepFrameRate;
        public float AbDefaultInterval => _abDefaultInterval;
        public float AbMaxInterval => _abMaxInterval;
        public float MeterSmoothing => _meterSmoothing;
        public float InPlaceTolerance => _inPlaceTolerance;
        public CharacterMoveConfig MoveConfig => _moveConfig;
        public Vector2Int PixelSizeRange => _pixelSizeRange;
        public int DefaultPixelSize => _defaultPixelSize;
        public bool EquipFirstAttachmentOnStart => _equipFirstAttachmentOnStart;
        public float OrbitSpeed => _orbitSpeed;
        public float ZoomSpeed => _zoomSpeed;
        public float PanSpeed => _panSpeed;
        public float MinDistance => _minDistance;
        public float MaxDistance => _maxDistance;
        public float DistancePerHeight => _distancePerHeight;
        public float FocusHeightRatio => _focusHeightRatio;
        public Vector2 PitchRange => _pitchRange;
    }
}
