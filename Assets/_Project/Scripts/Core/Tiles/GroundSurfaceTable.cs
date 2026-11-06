// 타일 → 바닥 종류 표. 굽기 때 읽어 칸마다 종류를 정한다 (에디터용 데이터, 게임은 굽기 결과 GroundSurfaceMap만 씀).
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace VoxelMagic.Core
{
    [CreateAssetMenu(menuName = "VoxelMagic/Tiles/Ground Surface Table", fileName = "GroundSurfaceTable")]
    public sealed class GroundSurfaceTable : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public TileBase Tile;
            public GroundSurface Surface;
        }

        [SerializeField] private List<Entry> _entries = new List<Entry>();

        public int Count => _entries.Count;

        /// <summary>표에 없으면 null</summary>
        public GroundSurface Find(TileBase tile)
        {
            if (tile == null) return null;
            foreach (var entry in _entries)
                if (entry.Tile == tile) return entry.Surface;
            return null;
        }

        public bool Contains(TileBase tile)
        {
            foreach (var entry in _entries)
                if (entry.Tile == tile) return true;
            return false;
        }

        /// <summary>표에 없는 타일만 더한다 (사용자가 고친 줄은 건드리지 않음)</summary>
        public bool AddIfMissing(TileBase tile, GroundSurface surface)
        {
            if (tile == null || Contains(tile)) return false;
            _entries.Add(new Entry { Tile = tile, Surface = surface });
            return true;
        }
    }
}
