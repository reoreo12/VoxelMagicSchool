// 타일 바닥 설정: 칸 크기(m), 타일 픽셀, 샘플 구역, 굽기 조각·아틀라스 여백, 바닥 종류 표, 새 맵 기본값. 값이 바뀌면 Changed로 알린다.
using System;
using UnityEngine;

namespace VoxelMagic.Core
{
    [CreateAssetMenu(menuName = "VoxelMagic/Config/Tile Map", fileName = "TileMapConfig")]
    public sealed class TileMapConfig : ScriptableObject
    {
        [Tooltip("타일 한 칸의 월드 크기 (m). 바꾸면 테스트 씬 바닥이 바로 다시 맞춰진다")]
        [SerializeField, Range(0.25f, 4f)] private float _cellSize = 1f;
        [Tooltip("빠르게 바꿔 볼 칸 크기 후보 (m)")]
        [SerializeField] private float[] _cellSizePresets = { 2.133f, 1.067f, 1f };
        [Tooltip("타일 그림 한 장의 픽셀 수 (임포트 Pixels Per Unit = 이 값 → 그림 1장 = 1칸)")]
        [SerializeField, Min(1)] private int _tilePixels = 32;
        [Tooltip("테스트 씬 샘플 구역 한 변의 칸 수")]
        [SerializeField, Min(1)] private int _sampleSize = 10;
        [Tooltip("굽기: 메시 조각 한 변의 칸 수")]
        [SerializeField, Min(1)] private int _chunkSize = 16;
        [Tooltip("굽기: 아틀라스에서 타일마다 둘레에 늘여 붙이는 여백 (px)")]
        [SerializeField, Range(0, 8)] private int _atlasPadding = 2;
        [Tooltip("타일 → 바닥 종류 표 (굽기 때 칸마다 바닥 종류를 함께 굽는다)")]
        [SerializeField] private GroundSurfaceTable _surfaceTable;
        [Tooltip("새 맵 기본 크기 (가로 × 세로 칸)")]
        [SerializeField] private Vector2Int _newMapSize = new Vector2Int(40, 40);
        [Tooltip("새 맵을 기존 맵 동쪽 끝에서 몇 칸 띄워 놓을지")]
        [SerializeField, Min(0)] private int _newMapGapCells = 4;

        public event Action Changed;

        public float CellSize => _cellSize;
        public float[] CellSizePresets => (float[])_cellSizePresets.Clone();
        public int TilePixels => _tilePixels;
        public int SampleSize => _sampleSize;
        public int ChunkSize => _chunkSize;
        public int AtlasPadding => _atlasPadding;
        public GroundSurfaceTable SurfaceTable => _surfaceTable;
        public Vector2Int NewMapSize => _newMapSize;

        public void SetSurfaceTable(GroundSurfaceTable table) => _surfaceTable = table;
        public int NewMapGapCells => _newMapGapCells;

        public void SetCellSize(float size)
        {
            _cellSize = Mathf.Clamp(size, 0.25f, 4f);
            Changed?.Invoke();
        }

        private void OnValidate() => Changed?.Invoke();
    }
}
