// 구운 맵의 칸 데이터(굽기 결과 <맵>_Cells.asset): 칸 위치 → 칠한 타일(Rule Tile이면 Rule Tile 자체). 에디터에서 다시 불러와 고칠 때 쓴다.
// 같은 타일은 목록에 한 번만 두고 칸마다 번호로 가리킨다.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace VoxelMagic.Core
{
    public sealed class TileMapCells : ScriptableObject
    {
        [SerializeField] private string _mapName;
        [SerializeField] private float _cellSize;
        [SerializeField] private List<TileBase> _tiles = new List<TileBase>();
        [SerializeField] private List<Vector2Int> _positions = new List<Vector2Int>();
        [SerializeField] private List<int> _tileIndices = new List<int>();

        public string MapName => _mapName;
        public float CellSize => _cellSize;
        public int Count => _positions.Count;
        public int TileKinds => _tiles.Count;

        public Vector2Int PositionAt(int index) => _positions[index];
        public TileBase TileAt(int index) => _tiles[_tileIndices[index]];

        public void Store(string mapName, float cellSize, IReadOnlyList<Vector2Int> positions, IReadOnlyList<TileBase> tiles)
        {
            _mapName = mapName;
            _cellSize = cellSize;
            _tiles.Clear();
            _positions.Clear();
            _tileIndices.Clear();
            var lookup = new Dictionary<TileBase, int>();
            for (int i = 0; i < positions.Count; i++)
            {
                var tile = tiles[i];
                if (tile == null) continue;
                if (!lookup.TryGetValue(tile, out int index))
                {
                    index = _tiles.Count;
                    _tiles.Add(tile);
                    lookup[tile] = index;
                }
                _positions.Add(positions[i]);
                _tileIndices.Add(index);
            }
        }
    }
}
