// 바닥 종류 맵 테스트: 칸 → 종류, 음수 시작 칸, 범위 밖·없음, 로컬 위치(소수) → 칸, 표 찾기·중복 추가 안 함.
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;
using VoxelMagic.Core;

namespace VoxelMagic.Tests
{
    public class GroundSurfaceMapTests
    {
        private GroundSurface _grass;
        private GroundSurface _soil;
        private GroundSurfaceMap _map;

        [SetUp]
        public void SetUp()
        {
            _grass = ScriptableObject.CreateInstance<GroundSurface>();
            _soil = ScriptableObject.CreateInstance<GroundSurface>();
            _map = ScriptableObject.CreateInstance<GroundSurfaceMap>();
            // 3×2, 시작 칸 (-1, 0): 아래 줄 [풀, 흙, 없음], 위 줄 [흙, 흙, 풀]
            _map.Store(new Vector2Int(-1, 0), 3, 2, new byte[] { 1, 2, 0, 2, 2, 1 }, new List<GroundSurface> { _grass, _soil });
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_grass);
            Object.DestroyImmediate(_soil);
            Object.DestroyImmediate(_map);
        }

        [Test]
        public void CellLookup_UsesOrigin()
        {
            Assert.AreSame(_grass, _map.SurfaceAt(new Vector2Int(-1, 0)));
            Assert.AreSame(_soil, _map.SurfaceAt(new Vector2Int(0, 0)));
            Assert.AreSame(_grass, _map.SurfaceAt(new Vector2Int(1, 1)));
        }

        [Test]
        public void EmptyOrOutside_IsNull()
        {
            Assert.IsNull(_map.SurfaceAt(new Vector2Int(1, 0)));
            Assert.IsNull(_map.SurfaceAt(new Vector2Int(2, 0)));
            Assert.IsNull(_map.SurfaceAt(new Vector2Int(-2, 0)));
            Assert.IsNull(_map.SurfaceAt(new Vector2Int(0, 2)));
        }

        [Test]
        public void LocalPosition_FloorsToCell()
        {
            Assert.AreSame(_soil, _map.SurfaceAtLocal(new Vector3(0.99f, 5f, 1.2f)));
            Assert.AreSame(_grass, _map.SurfaceAtLocal(new Vector3(-0.5f, 0f, 0.1f)));
        }

        [Test]
        public void Table_FindAndAddIfMissing()
        {
            var table = ScriptableObject.CreateInstance<GroundSurfaceTable>();
            var tile = ScriptableObject.CreateInstance<Tile>();
            try
            {
                Assert.IsNull(table.Find(tile));
                Assert.IsTrue(table.AddIfMissing(tile, _soil));
                Assert.IsFalse(table.AddIfMissing(tile, _grass));
                Assert.AreSame(_soil, table.Find(tile));
            }
            finally
            {
                Object.DestroyImmediate(table);
                Object.DestroyImmediate(tile);
            }
        }
    }
}
