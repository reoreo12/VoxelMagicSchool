// 소켓 프로필대로 본 아래에 소켓 Transform을 처음 필요할 때 한 번 만들고 캐시한다. 이후 장착물은 이 소켓의 자식이 되어 애니메이션을 저절로 따라간다.
// 에디터에서는 플레이 중 프로필 값을 바꾸면 이미 만든 소켓의 위치·회전에 바로 다시 적용한다.
using System.Collections.Generic;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class SocketHost : MonoBehaviour
    {
        private const string SocketPrefix = "Socket_";

        [SerializeField] private SocketProfile _profile;
        [Tooltip("ISocketSource 구현 컴포넌트 (보통 모델의 HumanoidSocketSource)")]
        [SerializeField] private MonoBehaviour _source;

        private readonly Dictionary<SocketId, Transform> _sockets = new Dictionary<SocketId, Transform>();
        private bool _built;

        public SocketProfile Profile => _profile;

        /// <summary>코드로 캐릭터를 만들 때(애니메이션 랩 등) 인스펙터 대신 연결한다. 이미 만든 소켓은 다시 만든다.</summary>
        public void Initialize(SocketProfile profile, ISocketSource source)
        {
            Clear();
            _profile = profile;
            _source = source as MonoBehaviour;
        }

        private void OnDestroy() => Clear();

        public bool TryGetSocket(SocketId socket, out Transform socketTransform)
        {
            EnsureBuilt();
            socketTransform = null;
            return socket != null && _sockets.TryGetValue(socket, out socketTransform) && socketTransform != null;
        }

        private void EnsureBuilt()
        {
            if (_built) return;
            _built = true;
#if UNITY_EDITOR
            if (_profile != null) _profile.Edited += ReapplyOffsets;
#endif
            if (_profile == null || !(_source is ISocketSource source))
            {
                Debug.LogWarning($"[{name}] SocketHost: 소켓 프로필 또는 ISocketSource가 없어 소켓을 만들지 않는다.", this);
                return;
            }
            foreach (var binding in _profile.Bindings)
            {
                if (binding == null || binding.Socket == null) continue;
                if (!source.TryGetAnchor(binding, out var anchor))
                {
                    Debug.LogWarning($"[{name}] 소켓 '{binding.Socket.DisplayName}'의 기준 본을 찾지 못했다.", this);
                    continue;
                }
                var socket = new GameObject(SocketPrefix + binding.Socket.name.Replace(SocketPrefix, string.Empty)).transform;
                socket.SetParent(anchor, false);
                _sockets[binding.Socket] = socket;
            }
            ReapplyOffsets();
        }

        // 소켓 위치·회전을 프로필 값으로 맞춘다 (처음 만들 때, 에디터에서 프로필 값이 바뀔 때).
        private void ReapplyOffsets()
        {
            if (_profile == null) return;
            foreach (var binding in _profile.Bindings)
            {
                if (binding == null || binding.Socket == null) continue;
                if (!_sockets.TryGetValue(binding.Socket, out var socket) || socket == null) continue;
                socket.localPosition = binding.PositionOffset;
                socket.localRotation = binding.RotationOffset;
            }
        }

        private void Clear()
        {
#if UNITY_EDITOR
            if (_built && _profile != null) _profile.Edited -= ReapplyOffsets;
#endif
            foreach (var socket in _sockets.Values)
                if (socket != null) Destroy(socket.gameObject);
            _sockets.Clear();
            _built = false;
        }
    }
}
