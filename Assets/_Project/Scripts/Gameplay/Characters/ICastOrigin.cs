// 낚싯줄·던지기 시작점을 주는 계약. 낚시는 지팡이를 몰라도 되고, 시작점이 없으면 자기 기본값을 쓴다.
using UnityEngine;

namespace VoxelMagic.Gameplay
{
    public interface ICastOrigin
    {
        bool TryGetOrigin(out Vector3 position);
    }
}
