// 캐릭터의 소켓별 장착 상태 (플레이어·NPC·몬스터 공용). 바뀔 때만 Changed 이벤트로 알리고, 실제 모습은 Gameplay가 그린다.
using System;
using System.Collections.Generic;

namespace VoxelMagic.Core
{
    public sealed class EquipmentState
    {
        private readonly Dictionary<SocketId, AttachmentDefinition> _equipped = new Dictionary<SocketId, AttachmentDefinition>();

        /// <summary>소켓, 이전 장착물(없으면 null), 새 장착물(해제면 null).</summary>
        public event Action<SocketId, AttachmentDefinition, AttachmentDefinition> Changed;

        public IEnumerable<KeyValuePair<SocketId, AttachmentDefinition>> Equipped => _equipped;

        public AttachmentDefinition Get(SocketId socket) =>
            socket != null && _equipped.TryGetValue(socket, out var attachment) ? attachment : null;

        /// <summary>장착물의 소켓에 끼운다. 같은 소켓의 이전 장착물은 자동으로 빠진다. 소켓이 없는 정의는 거절한다.</summary>
        public bool Equip(AttachmentDefinition attachment)
        {
            if (attachment == null || attachment.Socket == null) return false;
            var socket = attachment.Socket;
            var previous = Get(socket);
            if (previous == attachment) return false;
            _equipped[socket] = attachment;
            Changed?.Invoke(socket, previous, attachment);
            return true;
        }

        public bool Unequip(SocketId socket)
        {
            var previous = Get(socket);
            if (previous == null) return false;
            _equipped.Remove(socket);
            Changed?.Invoke(socket, previous, null);
            return true;
        }
    }
}
