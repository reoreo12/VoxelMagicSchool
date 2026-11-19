// Core의 EquipmentState를 보고 장착물 프리팹을 소켓에 붙이고 켜고 끈다 (한 번 만든 인스턴스는 캐시해 재사용).
// 장착된 물건 중 끝(Tip)이 있는 것을 캐시해 낚싯줄 시작점(ICastOrigin)으로 제공한다.
// 에디터에서는 플레이 중 장착물 정의의 놓는 위치·회전을 바꾸면 이미 만든 인스턴스에 바로 다시 적용한다.
using System;
using System.Collections.Generic;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class CharacterEquipment : MonoBehaviour, ICastOrigin
    {
        [SerializeField] private SocketHost _sockets;
        [Tooltip("시작할 때 장착할 물건 (NPC·몬스터 기본 장착, 애니메이션 랩)")]
        [SerializeField] private List<AttachmentDefinition> _defaultAttachments = new List<AttachmentDefinition>();
        [Tooltip("끄면 장착 상태는 그대로 두고 모습만 숨긴다")]
        [SerializeField] private bool _visible = true;

        private readonly Dictionary<AttachmentDefinition, AttachmentBase> _instances = new Dictionary<AttachmentDefinition, AttachmentBase>();
        private readonly Dictionary<SocketId, AttachmentBase> _active = new Dictionary<SocketId, AttachmentBase>();
#if UNITY_EDITOR
        private readonly Dictionary<AttachmentDefinition, Action> _editHandlers = new Dictionary<AttachmentDefinition, Action>();
#endif
        private EquipmentState _state;
        private Transform _tip;

        /// <summary>장착 상태 (부트스트랩이 Awake보다 먼저 Bind할 수 있어 처음 쓸 때 만든다).</summary>
        public EquipmentState State
        {
            get
            {
                if (_state == null)
                {
                    _state = new EquipmentState();
                    _state.Changed += OnChanged;
                }
                return _state;
            }
        }

        public bool Visible => _visible;

        /// <summary>장착물 모습이 바뀌었을 때 (소켓, 지금 붙은 장착물 또는 null).</summary>
        public event Action<SocketId, AttachmentBase> AttachmentShown;

        public void Initialize(SocketHost sockets) => _sockets = sockets;

        public void SetVisible(bool visible)
        {
            if (_visible == visible) return;
            _visible = visible;
            foreach (var attachment in _active.Values)
                if (attachment != null) attachment.gameObject.SetActive(visible);
            RefreshTip();
        }

        public bool TryGetOrigin(out Vector3 position)
        {
            position = _tip != null ? _tip.position : default;
            return _tip != null;
        }

        private void Start()
        {
            foreach (var attachment in _defaultAttachments)
                if (attachment != null && State.Get(attachment.Socket) == null)
                    State.Equip(attachment);
        }

        private void OnDestroy()
        {
            if (_state != null) _state.Changed -= OnChanged;
#if UNITY_EDITOR
            foreach (var pair in _editHandlers) pair.Key.Edited -= pair.Value;
            _editHandlers.Clear();
#endif
        }

        private void OnChanged(SocketId socket, AttachmentDefinition previous, AttachmentDefinition now)
        {
            if (_active.TryGetValue(socket, out var old) && old != null)
            {
                old.NotifyDetached();
                old.gameObject.SetActive(false);
            }
            _active.Remove(socket);

            var shown = now != null ? GetOrCreate(now) : null;
            if (shown != null)
            {
                _active[socket] = shown;
                shown.gameObject.SetActive(_visible);
                shown.NotifyAttached(gameObject);
            }
            RefreshTip();
            AttachmentShown?.Invoke(socket, shown);
        }

        private AttachmentBase GetOrCreate(AttachmentDefinition definition)
        {
            if (_instances.TryGetValue(definition, out var cached) && cached != null) return cached;
            if (definition.Prefab == null)
            {
                Debug.LogWarning($"[{name}] 장착물 '{definition.DisplayName}'에 프리팹이 없다.", this);
                return null;
            }
            if (_sockets == null || !_sockets.TryGetSocket(definition.Socket, out var socket))
            {
                Debug.LogWarning($"[{name}] 소켓 '{(definition.Socket != null ? definition.Socket.DisplayName : "없음")}'이 없어 '{definition.DisplayName}'을 붙이지 못했다.", this);
                return null;
            }
            var instance = Instantiate(definition.Prefab, socket, false);
            instance.name = definition.Prefab.name;
            ApplyHold(instance.transform, definition);
            var attachment = instance.GetComponent<AttachmentBase>();
            if (attachment == null)
            {
                Debug.LogWarning($"[{name}] 장착물 프리팹 '{definition.Prefab.name}'에 AttachmentBase 컴포넌트가 없다.", this);
                Destroy(instance);
                return null;
            }
            _instances[definition] = attachment;
#if UNITY_EDITOR
            System.Action handler = () => { if (attachment != null) ApplyHold(attachment.transform, definition); };
            definition.Edited += handler;
            _editHandlers[definition] = handler;
#endif
            return attachment;
        }

        private static void ApplyHold(Transform instance, AttachmentDefinition definition)
        {
            instance.localPosition = definition.HoldPosition;
            instance.localRotation = definition.HoldRotation;
        }

        private void RefreshTip()
        {
            _tip = null;
            if (!_visible) return;
            foreach (var attachment in _active.Values)
            {
                if (attachment is ITipProvider provider && provider.Tip != null)
                {
                    _tip = provider.Tip;
                    return;
                }
            }
        }
    }
}
