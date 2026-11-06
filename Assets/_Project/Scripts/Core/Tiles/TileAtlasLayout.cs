// 굽기용 아틀라스 배치 계산: 타일 N장을 정사각형에 가깝게 늘어놓고, 타일마다 둘레 여백(가장자리 늘이기)을 둔다. 순수 계산.
using UnityEngine;

namespace VoxelMagic.Core
{
    public sealed class TileAtlasLayout
    {
        public int TileCount { get; }
        public int TilePixels { get; }
        public int Padding { get; }
        public int Columns { get; }
        public int Rows { get; }
        public int SlotPixels => TilePixels + Padding * 2;
        public int Width => Columns * SlotPixels;
        public int Height => Rows * SlotPixels;

        public TileAtlasLayout(int tileCount, int tilePixels, int padding)
        {
            TileCount = Mathf.Max(1, tileCount);
            TilePixels = Mathf.Max(1, tilePixels);
            Padding = Mathf.Max(0, padding);
            Columns = Mathf.CeilToInt(Mathf.Sqrt(TileCount));
            Rows = Mathf.CeilToInt(TileCount / (float)Columns);
        }

        /// <summary>타일 그림이 들어갈 픽셀 위치 (여백 안쪽, 왼쪽 아래 기준)</summary>
        public RectInt TileRect(int index)
        {
            int col = index % Columns;
            int row = index / Columns;
            return new RectInt(col * SlotPixels + Padding, row * SlotPixels + Padding, TilePixels, TilePixels);
        }

        /// <summary>타일 그림 영역의 UV (0~1). 여백은 포함하지 않는다</summary>
        public Rect TileUv(int index)
        {
            var r = TileRect(index);
            return new Rect(r.x / (float)Width, r.y / (float)Height, r.width / (float)Width, r.height / (float)Height);
        }
    }
}
