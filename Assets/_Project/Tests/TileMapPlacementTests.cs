// 새 맵 놓을 자리 테스트: 기존 맵과 겹치지 않음, 칸 크기·간격 반영, 맵 없음.
using NUnit.Framework;
using VoxelMagic.Core;

namespace VoxelMagic.Tests
{
    public class TileMapPlacementTests
    {
        [Test]
        public void NoMaps_StartsAtZero()
        {
            Assert.AreEqual(0f, TileMapPlacement.NextOriginX(new TileMapPlacement.MapSpan[0], 4, 1f));
        }

        [Test]
        public void PlacesEastOfFarthestMap_WithGap()
        {
            var spans = new[]
            {
                TileMapPlacement.MapSpan.From(0f, 0, 10, 1f),    // 0 ~ 10
                TileMapPlacement.MapSpan.From(14f, 0, 40, 1f),   // 14 ~ 54
            };
            Assert.AreEqual(58f, TileMapPlacement.NextOriginX(spans, 4, 1f), 1e-5f);
        }

        [Test]
        public void UsesCellSize()
        {
            var spans = new[] { TileMapPlacement.MapSpan.From(0f, 0, 10, 2f) }; // 0 ~ 20
            Assert.AreEqual(28f, TileMapPlacement.NextOriginX(spans, 4, 2f), 1e-5f);
        }

        [Test]
        public void NegativeCells_AreCounted()
        {
            var span = TileMapPlacement.MapSpan.From(5f, -3, 2, 1f);
            Assert.AreEqual(2f, span.MinX, 1e-5f);
            Assert.AreEqual(7f, span.MaxX, 1e-5f);
        }
    }
}
