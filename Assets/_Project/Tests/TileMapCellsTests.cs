// 칸 데이터 테스트: 저장 후 같은 칸·타일로 읽힘, 같은 타일은 한 번만, 빈 타일 건너뜀, 다시 저장하면 바뀜.
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;
using VoxelMagic.Core;

namespace VoxelMagic.Tests
{
    public class TileMapCellsTests
    {
        private Tile _grass;
        private Tile _soil;
        private TileMapCells _cells;

        [SetUp]
        public void SetUp()
        {
            _grass = ScriptableObject.CreateInstance<Tile>();
            _soil = ScriptableObject.CreateInstance<Tile>();
            _cells = ScriptableObject.CreateInstance<TileMapCells>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_grass);
            Object.DestroyImmediate(_soil);
            Object.DestroyImmediate(_cells);
        }

        [Test]
        public void Store_RoundTrips()
        {
            _cells.Store("Farm", 1.5f, new[] { new Vector2Int(0, 0), new Vector2Int(3, -2) }, new TileBase[] { _grass, _soil });
            Assert.AreEqual("Farm", _cells.MapName);
            Assert.AreEqual(1.5f, _cells.CellSize);
            Assert.AreEqual(2, _cells.Count);
            Assert.AreEqual(new Vector2Int(3, -2), _cells.PositionAt(1));
            Assert.AreSame(_soil, _cells.TileAt(1));
        }

        [Test]
        public void SameTile_StoredOnce()
        {
            _cells.Store("Farm", 1f, new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0) }, new TileBase[] { _grass, _grass, _soil });
            Assert.AreEqual(2, _cells.TileKinds);
            Assert.AreSame(_grass, _cells.TileAt(1));
        }

        [Test]
        public void NullTile_Skipped()
        {
            _cells.Store("Farm", 1f, new[] { new Vector2Int(0, 0), new Vector2Int(1, 0) }, new TileBase[] { null, _soil });
            Assert.AreEqual(1, _cells.Count);
            Assert.AreEqual(new Vector2Int(1, 0), _cells.PositionAt(0));
        }

        [Test]
        public void StoreAgain_Replaces()
        {
            _cells.Store("Farm", 1f, new[] { new Vector2Int(0, 0), new Vector2Int(1, 0) }, new TileBase[] { _grass, _soil });
            _cells.Store("Farm", 2f, new[] { new Vector2Int(5, 5) }, new TileBase[] { _soil });
            Assert.AreEqual(1, _cells.Count);
            Assert.AreEqual(1, _cells.TileKinds);
            Assert.AreEqual(2f, _cells.CellSize);
        }
    }
}
