// 구운 맵의 바닥 종류 맵(굽기 결과 <맵>_Surfaces.asset): 칸마다 종류 번호 1바이트 + 번호 → GroundSurface 목록.
// 플레이 중 발밑 칸의 바닥 종류를 배열 한 번 읽기로 찾는다. 번호 0 = 없음(칠하지 않았거나 표에 없음).
using System.Collections.Generic;
using UnityEngine;

namespace VoxelMagic.Core
{
    public sealed class GroundSurfaceMap : ScriptableObject
    {
        public const int MaxSurfaces = 255;

        [SerializeField] private Vector2Int _origin;
        [SerializeField] private int _width;
        [SerializeField] private int _height;
        [SerializeField] private byte[] _cells = new byte[0];
        [SerializeField] private List<GroundSurface> _surfaces = new List<GroundSurface>();

        public Vector2Int Origin => _origin;
        public int Width => _width;
        public int Height => _height;
        public int SurfaceKinds => _surfaces.Count;

        /// <param name="cells">가로 × 세로, 아래 줄부터. 0 = 없음, n = surfaces[n-1]</param>
        public void Store(Vector2Int origin, int width, int height, byte[] cells, List<GroundSurface> surfaces)
        {
            if (cells.Length != width * height) throw new System.ArgumentException("칸 수가 가로 × 세로와 다름");
            if (surfaces.Count > MaxSurfaces) throw new System.ArgumentException($"바닥 종류는 {MaxSurfaces}개까지");
            _origin = origin;
            _width = width;
            _height = height;
            _cells = (byte[])cells.Clone();
            _surfaces = new List<GroundSurface>(surfaces);
        }

        /// <summary>칸 좌표의 바닥 종류. 범위 밖이거나 없으면 null</summary>
        public GroundSurface SurfaceAt(Vector2Int cell)
        {
            int x = cell.x - _origin.x, y = cell.y - _origin.y;
            if (x < 0 || y < 0 || x >= _width || y >= _height) return null;
            int code = _cells[y * _width + x];
            return code == 0 ? null : _surfaces[code - 1];
        }

        /// <summary>바닥 로컬 위치(칸 단위, x = 동, z = 북)의 바닥 종류</summary>
        public GroundSurface SurfaceAtLocal(Vector3 localCells) =>
            SurfaceAt(new Vector2Int(Mathf.FloorToInt(localCells.x), Mathf.FloorToInt(localCells.z)));
    }
}
