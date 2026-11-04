// 텃밭 설정(격자 크기, 칸 크기, 미리보기 색, 성장 연출 시간).
using UnityEngine;

namespace VoxelMagic.Core
{
    [CreateAssetMenu(menuName = "VoxelMagic/Config/Farm", fileName = "FarmConfig")]
    public sealed class FarmConfig : ScriptableObject
    {
        [SerializeField, Min(1)] private int _gridSize = 6;
        [SerializeField, Min(0.1f)] private float _cellSize = 1f;
        [Tooltip("플레이어 앞 몇 m 지점의 칸을 대상으로 삼을지")]
        [SerializeField, Min(0f)] private float _targetForwardOffset = 0.8f;
        [SerializeField] private Color _previewValidColor = new Color(0.2f, 1f, 0.3f, 0.5f);
        [SerializeField] private Color _previewInvalidColor = new Color(1f, 0.2f, 0.2f, 0.5f);
        [SerializeField, Min(0f)] private float _growAnimationSeconds = 0.4f;

        public int GridSize => _gridSize;
        public float CellSize => _cellSize;
        public float TargetForwardOffset => _targetForwardOffset;
        public Color PreviewValidColor => _previewValidColor;
        public Color PreviewInvalidColor => _previewInvalidColor;
        public float GrowAnimationSeconds => _growAnimationSeconds;
    }
}
