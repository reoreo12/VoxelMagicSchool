// 채집 재료 아이콘 굽기 설정: 아이콘 크기·촬영 각도·조명·축소 기준·색 수·외곽선 색. GatherIconBaker가 읽는다.
using UnityEngine;

namespace VoxelMagic.EditorTools
{
    [CreateAssetMenu(menuName = "VoxelMagic/Tools/Gather Icon Settings", fileName = "GatherIconSettings")]
    public sealed class GatherIconSettings : ScriptableObject
    {
        [Tooltip("출력 PNG 한 변 (외곽선 포함, UI_SPEC 7-3: 16)")]
        [SerializeField, Min(8)] private int _iconSize = 16;
        [Tooltip("모델이 차지할 안쪽 칸 수 = 아이콘 크기 − 2 × 이 값 (외곽선 1px 자리)")]
        [SerializeField, Min(1)] private int _margin = 1;
        [Tooltip("촬영 해상도 = 아이콘 크기 × 이 값")]
        [SerializeField, Min(2)] private int _supersample = 16;
        [Tooltip("카메라 내려다보는 각도 (도)")]
        [SerializeField] private float _pitch = 30f;
        [Tooltip("카메라 좌우 각도 (도)")]
        [SerializeField] private float _yaw = 35f;
        [SerializeField] private Vector3 _lightEuler = new Vector3(40f, -30f, 0f);
        [SerializeField, Min(0f)] private float _lightIntensity = 1.1f;
        [SerializeField] private Color _ambient = new Color(0.55f, 0.55f, 0.55f);
        [Tooltip("칸 안에서 모델이 이 비율 이상 덮어야 칠한다")]
        [SerializeField, Range(0.05f, 0.95f)] private float _coverage = 0.4f;
        [Tooltip("아이콘 한 장에 쓸 색 수 (외곽선 제외)")]
        [SerializeField, Range(2, 16)] private int _colors = 7;
        [Tooltip("외곽선 색 (지팡이 아이콘과 같은 진한 갈색)")]
        [SerializeField] private Color32 _outline = new Color32(43, 29, 34, 255);
        [SerializeField] private string _outputFolder = "Assets/_Project/Art/UI/item_icons/gather";

        public int IconSize => _iconSize;
        public int Margin => _margin;
        public int Supersample => _supersample;
        public float Pitch => _pitch;
        public float Yaw => _yaw;
        public Vector3 LightEuler => _lightEuler;
        public float LightIntensity => _lightIntensity;
        public Color Ambient => _ambient;
        public float Coverage => _coverage;
        public int Colors => _colors;
        public Color32 Outline => _outline;
        public string OutputFolder => _outputFolder;
    }
}
