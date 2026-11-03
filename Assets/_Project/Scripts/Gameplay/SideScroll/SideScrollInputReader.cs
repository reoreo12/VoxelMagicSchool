// 새 Input System의 SideScroll 맵(A/D·←/→, Shift, P)을 읽는 유일한 곳. 이동은 MoveIntent로 바꾸고, P는 도트 렌더링을 켜고 끈다.
// W/S는 바인딩 자체가 없다.
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

        public void Bind(SideScrollContext context) => _context = context;

        private void Awake()
        {
            _map = _actions.FindActionMap(_mapName, throwIfNotFound: true);
            _move = _map.FindAction("Move", true);
            _run = _map.FindAction("Run", true);
            _map.FindAction("TogglePixel", true).performed += _ => _context?.PixelRender.Toggle();
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
