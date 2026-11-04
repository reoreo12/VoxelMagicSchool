// 복도 맵 설정: 이미지 임포트, 레이어 Z·정렬 순서, 패럴랙스, 카메라 따라가기(부드럽게/고정), 오른쪽 문 포털, 낮/밤·바닥 빛·천장 조명, 디버그 표시.
using UnityEngine;
using UnityEngine.Serialization;

namespace VoxelMagic.Core
{
    [CreateAssetMenu(menuName = "VoxelMagic/SideScroll/Corridor Config", fileName = "CorridorConfig")]
    public sealed class CorridorConfig : ScriptableObject
    {
        [Header("이미지 임포트 (바꾸면 메뉴로 다시 적용)")]
        [SerializeField, Min(1f)] private float _pixelsPerUnit = 300f;
        [SerializeField] private FilterMode _corridorFilter = FilterMode.Point;
        [SerializeField] private FilterMode _backdropFilter = FilterMode.Point;
        [SerializeField] private int _maxTextureSize = 4096;

        [Header("레이어")]
        [SerializeField] private float _corridorZ = 1f;
        [SerializeField] private int _corridorSortingOrder = 0;
        [SerializeField] private float _backdropZ = 5f;
        [SerializeField] private int _backdropSortingOrder = -10;

        [Header("창밖 배경")]
        [Tooltip("1 = 복도와 함께 지나감, 0 = 화면에 붙어 안 움직임")]
        [SerializeField, Range(0f, 1f)] private float _backdropParallax = 0.3f;
        [SerializeField] private float _backdropOffsetY = 0f;
        [SerializeField, Min(0.1f)] private float _backdropScale = 1f;
        [SerializeField, Min(2)] private int _backdropCopies = 3;

        [Header("이동 범위·카메라")]
        [Tooltip("복도 양 끝에서 캐릭터가 멈추는 안쪽 거리 (유닛)")]
        [SerializeField, Min(0f)] private float _endMargin = 2.56f;
        [SerializeField] private float _cameraZ = -10f;
        [Tooltip("부드럽게 따라갈 때 늦게 따라가는 시간 (초)")]
        [SerializeField, Min(0f)] private float _cameraFollowTime = 0.15f;
        [Tooltip("켜면 플레이어를 화면 가운데에 고정해 따라가며 시작 (C 키로 부드럽게와 전환)")]
        [SerializeField] private bool _cameraLockedAtStart = true;

        [Header("캐릭터")]
        [Tooltip("복도에 놓을 캐릭터 프리팹. 비어 있으면 큐브 캐릭터 (메뉴: VoxelMagic/아바타/3)")]
        [SerializeField] private GameObject _characterPrefab;
        [Tooltip("2D 버전이 있는 캐릭터면 2D 스프라이트로 시작 (V 키로 3D와 전환)")]
        [SerializeField] private bool _startIn2D = true;
        [Tooltip("2D 캐릭터 스프라이트의 정렬 순서 (복도 타일보다 커야 앞에 보임)")]
        [SerializeField] private int _characterSpriteSortingOrder = 10;

        [Header("도트 렌더링 (화면 전체)")]
        [SerializeField] private bool _pixelOnAtStart = false;
        [Tooltip("화면 픽셀 몇 개를 도트 하나로 그릴지")]
        [SerializeField, Min(1)] private int _pixelSize = 4;
        [SerializeField] private Vector2Int _pixelSizeRange = new Vector2Int(2, 8);

        [Header("오른쪽 문 포털 (바꾸면 복도 씬 다시 만들기)")]
        [SerializeField] private bool _portalEnabled = true;
        [Tooltip("E로 열 씬 이름 (Build Settings 씬 목록에 있어야 함)")]
        [SerializeField] private string _portalSceneName = "Prototype_FarmFishing";
        [Tooltip("복도 오른쪽 끝에서 안쪽으로 떨어진 거리 (끝 여유와 같으면 캐릭터가 멈추는 자리)")]
        [SerializeField, Min(0f)] private float _portalInsetFromRight = 2.56f;
        [SerializeField, Min(0.1f)] private float _portalHeight = 1f;
        [SerializeField, Min(0.05f)] private float _portalDiameter = 0.6f;
        [Tooltip("이 반지름 안에 캐릭터 중심이 들어오면 E로 이동 (씬에서는 포털의 CapsuleCollider 반지름)")]
        [SerializeField, Min(0.01f)] private float _portalTriggerRadius = 0.2f;
        [SerializeField] private Color _portalColor = new Color(0.45f, 0.8f, 1f, 0.35f);
        [Tooltip("캐릭터 발에서 이 높이의 점으로 포털 안인지 잰다")]
        [SerializeField, Min(0f)] private float _interactProbeHeight = 0.5f;

        [Header("낮/밤 (N 키)")]
        [SerializeField] private bool _nightAtStart = false;
        [SerializeField] private BackdropSet _dayBackdrop;
        [SerializeField] private BackdropSet _nightBackdrop;
        [Tooltip("복도 타일 기본 밝기. 흰색 = 그림 그대로")]
        [SerializeField] private Color _dayCorridorAmbient = Color.white;
        [SerializeField] private Color _nightCorridorAmbient = new Color(0.12f, 0.14f, 0.26f);
        [Tooltip("밤에 해(방향 조명) 세기에 곱하는 값 (3D 캐릭터용)")]
        [SerializeField, Range(0f, 1f)] private float _nightSunScale = 0.15f;
        [Tooltip("밤에 주변광에 곱하는 값 (3D 캐릭터용)")]
        [SerializeField, Range(0f, 1f)] private float _nightAmbientScale = 0.3f;

        [Header("밤 조명 배치 (바꾸면 복도 씬 다시 만들기)")]
        [Tooltip("이 간격 이하로 고르게 놓는다")]
        [SerializeField, Min(0.5f)] private float _lampSpacing = 4f;
        [Tooltip("복도 양 끝에서 안쪽으로 떨어진 거리")]
        [SerializeField, Min(0f)] private float _lampInset = 1.5f;

        [Header("밤 바닥 빛 (조명이 아닌 도트 원 그림)")]
        [Tooltip("바닥 빛 가운데 높이 (캐릭터 발은 약 0.12)")]
        [SerializeField] private float _glowCenterY = 0.2f;
        [Tooltip("벽 앞으로 띄우는 거리 (캐릭터보다 뒤)")]
        [SerializeField, Min(0f)] private float _glowOffsetZ = 0.05f;
        [SerializeField, Min(0.05f)] private float _glowWidth = 2.4f;
        [SerializeField, Min(0.05f)] private float _glowHeight = 0.4f;
        [FormerlySerializedAs("_lampColor")]
        [SerializeField] private Color _glowColor = new Color(1f, 0.82f, 0.55f);
        [Tooltip("진하기 (0 = 안 보임, 1 = 가장 진함)")]
        [SerializeField, Range(0f, 1f)] private float _glowOpacity = 0.35f;
        [Tooltip("도트 원 그림 크기 (픽셀). 작을수록 도트가 굵다")]
        [FormerlySerializedAs("_lampCookieSize")]
        [SerializeField, Min(8)] private int _glowTextureSize = 32;
        [Tooltip("도트 원 밝기 단계 수")]
        [FormerlySerializedAs("_lampCookieSteps")]
        [SerializeField, Min(1)] private int _glowSteps = 4;

        [Header("밤 천장 조명 (칩비를 비춤)")]
        [SerializeField] private float _ceilingLightHeight = 2.7f;
        [Tooltip("조명 위치 z (캐릭터는 0, 음수 = 카메라 쪽)")]
        [SerializeField] private float _ceilingLightZ = -0.8f;
        [Tooltip("아래를 보다가 카메라 반대쪽(벽 쪽)으로 기울이는 각도 → 칩비 앞면까지 비춤")]
        [SerializeField, Range(0f, 89f)] private float _ceilingLightTilt = 25f;
        [SerializeField, Range(1f, 179f)] private float _ceilingLightAngle = 60f;
        [SerializeField, Min(0.1f)] private float _ceilingLightRange = 4f;
        [SerializeField, Min(0f)] private float _ceilingLightIntensity = 4f;
        [SerializeField] private Color _ceilingLightColor = new Color(1f, 0.85f, 0.65f);
        [Tooltip("복도 벽 그림이 천장 조명을 받는 배율 (0 = 안 받음, 벽에는 바닥 빛 그림만)")]
        [SerializeField, Min(0f)] private float _lampOnWall = 0f;

        [Header("디버그")]
        [SerializeField] private bool _showStateDebug = true;

        public float PixelsPerUnit => _pixelsPerUnit;
        public FilterMode CorridorFilter => _corridorFilter;
        public FilterMode BackdropFilter => _backdropFilter;
        public int MaxTextureSize => _maxTextureSize;
        public float CorridorZ => _corridorZ;
        public int CorridorSortingOrder => _corridorSortingOrder;
        public float BackdropZ => _backdropZ;
        public int BackdropSortingOrder => _backdropSortingOrder;
        public float BackdropParallax => _backdropParallax;
        public float BackdropOffsetY => _backdropOffsetY;
        public float BackdropScale => _backdropScale;
        public int BackdropCopies => _backdropCopies;
        public float EndMargin => _endMargin;
        public float CameraZ => _cameraZ;
        public float CameraFollowTime => _cameraFollowTime;
        public bool CameraLockedAtStart => _cameraLockedAtStart;
        public GameObject CharacterPrefab => _characterPrefab;
        public bool StartIn2D => _startIn2D;
        public int CharacterSpriteSortingOrder => _characterSpriteSortingOrder;
        public bool PixelOnAtStart => _pixelOnAtStart;
        public int PixelSize => _pixelSize;
        public Vector2Int PixelSizeRange => _pixelSizeRange;
        public bool PortalEnabled => _portalEnabled;
        public string PortalSceneName => _portalSceneName;
        public float PortalInsetFromRight => _portalInsetFromRight;
        public float PortalHeight => _portalHeight;
        public float PortalDiameter => _portalDiameter;
        public float PortalTriggerRadius => _portalTriggerRadius;
        public Color PortalColor => _portalColor;
        public float InteractProbeHeight => _interactProbeHeight;
        public bool NightAtStart => _nightAtStart;
        public BackdropSet DayBackdrop => _dayBackdrop;
        public BackdropSet NightBackdrop => _nightBackdrop;
        public Color DayCorridorAmbient => _dayCorridorAmbient;
        public Color NightCorridorAmbient => _nightCorridorAmbient;
        public float NightSunScale => _nightSunScale;
        public float NightAmbientScale => _nightAmbientScale;
        public float LampSpacing => _lampSpacing;
        public float LampInset => _lampInset;
        public float GlowCenterY => _glowCenterY;
        public float GlowOffsetZ => _glowOffsetZ;
        public float GlowWidth => _glowWidth;
        public float GlowHeight => _glowHeight;
        public Color GlowColor => _glowColor;
        public float GlowOpacity => _glowOpacity;
        public int GlowTextureSize => _glowTextureSize;
        public int GlowSteps => _glowSteps;
        public float CeilingLightHeight => _ceilingLightHeight;
        public float CeilingLightZ => _ceilingLightZ;
        public float CeilingLightTilt => _ceilingLightTilt;
        public float CeilingLightAngle => _ceilingLightAngle;
        public float CeilingLightRange => _ceilingLightRange;
        public float CeilingLightIntensity => _ceilingLightIntensity;
        public Color CeilingLightColor => _ceilingLightColor;
        public float LampOnWall => _lampOnWall;
        public bool ShowStateDebug => _showStateDebug;
    }
}
