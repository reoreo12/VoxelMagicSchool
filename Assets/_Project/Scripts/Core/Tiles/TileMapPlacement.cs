// 새 맵 놓을 자리 계산: 기존 맵들의 동쪽(+X) 끝에서 칸 몇 개 띄운 곳. 순수 계산.
namespace VoxelMagic.Core
{
    public static class TileMapPlacement
    {
        /// <summary>기존 맵 하나의 X 범위 (월드 m)</summary>
        public readonly struct MapSpan
        {
            public readonly float MinX;
            public readonly float MaxX;
            public MapSpan(float minX, float maxX) { MinX = minX; MaxX = maxX; }

            /// <summary>맵 루트 X + 칠한 칸 범위(칸) × 칸 크기</summary>
            public static MapSpan From(float rootX, int cellMinX, int cellMaxX, float cellSize) =>
                new MapSpan(rootX + cellMinX * cellSize, rootX + cellMaxX * cellSize);
        }

        /// <summary>새 맵 루트 X: 모든 기존 맵의 동쪽 끝 + 간격. 맵이 없으면 0</summary>
        public static float NextOriginX(MapSpan[] existing, int gapCells, float cellSize)
        {
            if (existing == null || existing.Length == 0) return 0f;
            float east = existing[0].MaxX;
            foreach (var span in existing)
                if (span.MaxX > east) east = span.MaxX;
            return east + gapCells * cellSize;
        }
    }
}
