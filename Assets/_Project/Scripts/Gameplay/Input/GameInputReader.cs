// 새 Input System 액션을 읽는 유일한 곳. 이동·E는 이벤트로 내보내고, 나머지 키는 Core 명령으로 바꾼다.
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class GameInputReader : MonoBehaviour, IGameContextBinder
    {
        [SerializeField] private InputActionAsset _actions;
        [SerializeField] private string _mapName = "Gameplay";

        private InputActionMap _map;
        private InputAction _move;
        private InputAction[] _slots;
        private GameContext _context;

        public event Action InteractPressed;

        public Vector2 Move => _move != null ? _move.ReadValue<Vector2>() : Vector2.zero;

        public void Bind(GameContext context) => _context = context;

        private void Awake()
        {
            _map = _actions.FindActionMap(_mapName, throwIfNotFound: true);
            _move = _map.FindAction("Move", true);
            _slots = new InputAction[5];
            for (int i = 0; i < _slots.Length; i++)
            {
                int index = i;
                _slots[i] = _map.FindAction($"Slot{i + 1}", true);
                _slots[i].performed += _ => _context?.Inventory.Select(index);
            }
            _map.FindAction("Interact", true).performed += _ => InteractPressed?.Invoke();
            _map.FindAction("Scroll", true).performed += OnScroll;
            _map.FindAction("ToggleTracker", true).performed += _ => _context?.Screen.ToggleTracker();
            _map.FindAction("Cancel", true).performed += _ => OnCancel();
        }

        private void OnEnable() => _map?.Enable();
        private void OnDisable() => _map?.Disable();

        private void OnScroll(InputAction.CallbackContext ctx)
        {
            float value = ctx.ReadValue<float>();
            if (Mathf.Approximately(value, 0f)) return;
            _context?.Fishing.Scroll(Mathf.Sign(value));
        }

        private void OnCancel()
        {
            if (_context == null) return;
            if (_context.Screen.IsModalOpen) _context.Screen.CloseCurrent();
            else _context.Fishing.Cancel();
        }
    }
}
