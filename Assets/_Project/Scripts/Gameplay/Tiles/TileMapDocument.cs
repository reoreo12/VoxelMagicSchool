// 맵 하나의 루트(Map_<이름>)에 붙는 맵 정보: 이름, Grid·Paint·구운 바닥 참조, 구운 칸 크기. 루트를 옮기거나 끄면 맵 전체가 따라간다.
// 플레이 중에는 칠하기 층 대신 구운 바닥을 켠다.
using UnityEngine;
using UnityEngine.Tilemaps;

namespace VoxelMagic.Gameplay
{
    public sealed class TileMapDocument : MonoBehaviour
    {
        public const string RootPrefix = "Map_";

        [Tooltip("굽기 결과 이름: Art/Map/Baked/<이름>/<이름>_Floor.prefab")]
        [SerializeField] private string _mapName = "FarmSample";
        [SerializeField] private Grid _grid;
        [SerializeField] private Tilemap _paint;
        [Tooltip("칠한 칸이 없을 때 맵 가운데를 정하는 크기 (칸)")]
        [SerializeField] private Vector2Int _sizeHint = new Vector2Int(10, 10);
        [SerializeField, HideInInspector] private float _bakedCellSize;
        [SerializeField, HideInInspector] private GameObject _bakedFloor;

        public string MapName => _mapName;
        public Grid Grid => _grid;
        public Tilemap Paint => _paint;
        public Vector2Int SizeHint => _sizeHint;
        public float BakedCellSize => _bakedCellSize;
        public GameObject BakedFloor => _bakedFloor;
        public bool HasBake => _bakedFloor != null;

        private void Awake()
        {
            if (Application.isPlaying && _bakedFloor != null) _bakedFloor.SetActive(true);
        }

        public void Setup(string mapName, Grid grid, Tilemap paint, Vector2Int sizeHint)
        {
            _mapName = mapName;
            _grid = grid;
            _paint = paint;
            _sizeHint = sizeHint;
        }

        public void SetMapName(string mapName) => _mapName = mapName;

        public void RecordBake(float cellSize, GameObject bakedFloor)
        {
            _bakedCellSize = cellSize;
            _bakedFloor = bakedFloor;
        }

        /// <summary>칠한 칸 범위 (칸 좌표, 최소·크기). 칠한 칸이 없으면 (0,0)부터 크기 힌트</summary>
        public RectInt PaintedCellRect()
        {
            if (_paint != null && _paint.GetUsedTilesCount() > 0)
            {
                var b = _paint.cellBounds;
                return new RectInt(b.xMin, b.yMin, b.size.x, b.size.y);
            }
            return new RectInt(0, 0, _sizeHint.x, _sizeHint.y);
        }

        /// <summary>칸 좌표(소수 가능) → 월드 위치. 칸 (x, y) = 맵 루트 기준 (x·칸, 0, y·칸)</summary>
        public Vector3 CellToWorld(Vector2 cell, float cellSize) =>
            transform.TransformPoint(new Vector3(cell.x * cellSize, 0f, cell.y * cellSize));
    }
}
