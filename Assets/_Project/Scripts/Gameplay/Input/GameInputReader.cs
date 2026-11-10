// 새 Input System 액션을 읽는 유일한 곳. 이동·E·카메라 줌(스크롤)은 이벤트로 내보내고, 나머지 키는 Core 명령으로 바꾼다.
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
        /// <summary>마우스 스크롤 방향 (+1 = 위로 굴림 = 확대).</summary>
        public event Action<float> ZoomRequested;

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

        private void OnEnable()
        {
            SceneTravel.TravelingChanged += OnTravelingChanged;
            if (!SceneTravel.IsTraveling) _map?.Enable();
        }

        private void OnDisable()
        {
            SceneTravel.TravelingChanged -= OnTravelingChanged;
            _map?.Disable();
        }

        // 씬 이동(페이드) 중에는 게임플레이 키 전부 막기. 환경설정 같은 UI는 다른 액션 맵을 쓰면 영향 없음.
        private void OnTravelingChanged(bool traveling)
        {
            if (traveling) _map?.Disable();
            else _map?.Enable();
        }

        // 스크롤은 낚시 미니게임 중이면 잡기 바를 움직이고, 창이 열려 있지 않은 평소에는 카메라 줌으로 쓴다.
        private void OnScroll(InputAction.CallbackContext ctx)
        {
            float value = ctx.ReadValue<float>();
            if (Mathf.Approximately(value, 0f) || _context == null) return;
            if (_context.Fishing.State == FishingState.Minigame) _context.Fishing.Scroll(Mathf.Sign(value));
            else if (!_context.Screen.IsModalOpen) ZoomRequested?.Invoke(Mathf.Sign(value));
        }

        private void OnCancel()
        {
            if (_context == null) return;
            if (_context.Screen.IsModalOpen) _context.Screen.CloseCurrent();
            else _context.Fishing.Cancel();
        }
    }
}
