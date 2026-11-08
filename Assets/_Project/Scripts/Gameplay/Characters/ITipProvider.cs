// 끝 지점이 있는 장착물(지팡이 등)의 계약. 낚싯줄·마법 발사 시작점으로 쓴다.
using UnityEngine;

namespace VoxelMagic.Gameplay
{
    public interface ITipProvider
    {
        Transform Tip { get; }
    }
}
