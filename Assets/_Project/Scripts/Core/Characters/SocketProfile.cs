// 아바타 한 벌의 소켓 목록. AvatarSetup이 참조하며, 같은 지팡이라도 아바타마다 손 위치·각도 보정을 따로 둔다.
using System.Collections.Generic;
using UnityEngine;

namespace VoxelMagic.Core
{
    [CreateAssetMenu(menuName = "VoxelMagic/Characters/Socket Profile", fileName = "SocketProfile_")]
    public sealed class SocketProfile : ScriptableObject
    {
        [SerializeField] private List<SocketBinding> _bindings = new List<SocketBinding>();

        public IReadOnlyList<SocketBinding> Bindings => _bindings;

        public bool TryGet(SocketId socket, out SocketBinding binding)
        {
            if (socket != null)
            {
                foreach (var candidate in _bindings)
                {
                    if (candidate != null && candidate.Socket == socket)
                    {
                        binding = candidate;
                        return true;
                    }
                }
            }
            binding = null;
            return false;
        }
    }
}
