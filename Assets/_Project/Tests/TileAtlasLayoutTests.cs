// 타일 굽기 아틀라스 배치 테스트: 크기, 칸 위치, 여백, UV.
using NUnit.Framework;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Tests
{
    public class TileAtlasLayoutTests
    {
        [Test]
        public void FiveTiles_MakeThreeByTwoGrid()
        {
            var layout = new TileAtlasLayout(5, 32, 2);
            Assert.AreEqual(3, layout.Columns);
            Assert.AreEqual(2, layout.Rows);
            Assert.AreEqual(36 * 3, layout.Width);
            Assert.AreEqual(36 * 2, layout.Height);
        }

        [Test]
        public void TileRect_SkipsPaddingAndSlots()
        {
            var layout = new TileAtlasLayout(5, 32, 2);
            Assert.AreEqual(new RectInt(2, 2, 32, 32), layout.TileRect(0));
            Assert.AreEqual(new RectInt(36 + 2, 36 + 2, 32, 32), layout.TileRect(4));
        }

        [Test]
        public void TileUv_ExcludesPadding()
        {
            var layout = new TileAtlasLayout(1, 32, 2);
            var uv = layout.TileUv(0);
            Assert.AreEqual(2f / 36f, uv.x, 1e-6f);
            Assert.AreEqual(32f / 36f, uv.width, 1e-6f);
        }

        [Test]
        public void ZeroPadding_TilesTouch()
        {
            var layout = new TileAtlasLayout(4, 32, 0);
            Assert.AreEqual(new RectInt(32, 0, 32, 32), layout.TileRect(1));
            Assert.AreEqual(64, layout.Width);
        }
    }
}
