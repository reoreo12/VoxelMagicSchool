// 칠하기 전용 Tilemap: 플레이 중에는 그리지 않는다 (게임에서 보이는 바닥은 구운 메시).
using UnityEngine;
using UnityEngine.Tilemaps;

namespace VoxelMagic.Gameplay
{
    [RequireComponent(typeof(TilemapRenderer))]
    public sealed class PaintOnlyTilemap : MonoBehaviour
    {
        private void Awake()
        {
            if (Application.isPlaying) GetComponent<TilemapRenderer>().enabled = false;
        }
    }
}
