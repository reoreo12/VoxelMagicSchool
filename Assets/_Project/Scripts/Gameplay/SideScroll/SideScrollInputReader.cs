// 새 Input System의 SideScroll 맵(A/D·←/→, Shift, P, V, C, N, E)을 읽는 유일한 곳. 이동은 MoveIntent로 바꾸고,
// P는 도트 렌더링, V는 2D/3D 캐릭터, C는 카메라 고정/부드럽게, N은 낮/밤을 바꾸며, E는 InteractPressed로 알린다.
// W/S는 바인딩 자체가 없다.
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class SideScrollInputReader : MonoBehaviour, ISideScrollBinder
    {
        private const float AxisDeadZone = 0.5f;

        [SerializeField] private InputActionAsset _actions;
        [SerializeField] private string _mapName = "SideScroll";

        private InputActionMap _map;
        private InputAction _move;
        private InputAction _run;
        private SideScrollContext _context;

        public event Action InteractPressed;

        public void Bind(SideScrollContext context) => _context = context;

        private void Awake()
        {
            _map = _actions.FindActionMap(_mapName, throwIfNotFound: true);
            _move = _map.FindAction("Move", true);
            _run = _map.FindAction("Run", true);
            _map.FindAction("TogglePixel", true).performed += _ => _context?.PixelRender.Toggle();
            _map.FindAction("ToggleCharacterView", true).performed += _ => _context?.CharacterView.Toggle();
            _map.FindAction("ToggleCameraFollow", true).performed += _ => _context?.CameraFollow.Toggle();
            _map.FindAction("ToggleNight", true).performed += _ => _context?.TimeOfDay.Toggle();
            _map.FindAction("Interact", true).performed += _ => InteractPressed?.Invoke();
        }

        private void OnEnable() => _map?.Enable();
        private void OnDisable() => _map?.Disable();

        public MoveIntent Read()
        {
            float axis = _move.ReadValue<float>();
            int direction = axis > AxisDeadZone ? 1 : axis < -AxisDeadZone ? -1 : 0;
            return new MoveIntent(direction, _run.IsPressed());
        }
    }
}
