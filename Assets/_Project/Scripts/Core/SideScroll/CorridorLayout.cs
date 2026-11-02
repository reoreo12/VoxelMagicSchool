// 복도 타일 순서 데이터. 항목마다 스프라이트와 좌우 반전 여부를 가진다. 코드 수정 없이 순서·개수를 바꾼다.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelMagic.Core
{
    [Serializable]
    public struct CorridorTile
    {
        [SerializeField] private Sprite _sprite;
        [SerializeField] private bool _flipX;

        public CorridorTile(Sprite sprite, bool flipX)
        {
            _sprite = sprite;
            _flipX = flipX;
        }

        public Sprite Sprite => _sprite;
        public bool FlipX => _flipX;
    }

    [CreateAssetMenu(menuName = "VoxelMagic/SideScroll/Corridor Layout", fileName = "CorridorLayout")]
    public sealed class CorridorLayout : ScriptableObject
    {
        [SerializeField] private List<CorridorTile> _tiles = new List<CorridorTile>();

        public IReadOnlyList<CorridorTile> Tiles => _tiles;
    }
}
