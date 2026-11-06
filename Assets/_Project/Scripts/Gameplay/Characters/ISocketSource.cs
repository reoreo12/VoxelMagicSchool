// 소켓을 붙일 기준 Transform(본)을 찾아 주는 계약. 사람형 본/모델 안 이름 등 찾는 방식만 구현마다 다르다.
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public interface ISocketSource
    {
        bool TryGetAnchor(SocketBinding binding, out Transform anchor);
    }
}
