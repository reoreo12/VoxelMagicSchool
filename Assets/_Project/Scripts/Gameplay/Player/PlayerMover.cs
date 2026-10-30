// 카메라 방향 기준 WASD 이동. 창이 열려 있으면 멈추고, 움직이면 낚시 취소·획득 정보 닫기를 요청한다.
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMover : MonoBehaviour, IGameContextBinder
    {
        [SerializeField] private GameInputReader _input;

        private CharacterController _controller;
        private GameContext _context;
        private float _verticalSpeed;
        private bool _wasMoving;

        public void Bind(GameContext context) => _context = context;

        private void Awake() => _controller = GetComponent<CharacterController>();

        private void Update()
        {
            if (_context == null) return;
            var config = _context.PlayerConfig;

            Vector2 input = _context.Screen.IsModalOpen ? Vector2.zero : _input.Move;
            bool moving = input.sqrMagnitude > 0.01f;
            if (moving)
            {
                if (_context.Fishing.IsBusy) _context.Fishing.Cancel();
                if (!_wasMoving) _context.Messages.DismissAcquired();
            }
            _wasMoving = moving;

            var direction = Quaternion.Euler(0f, config.CameraYaw, 0f) * new Vector3(input.x, 0f, input.y);
            if (direction.sqrMagnitude > 1f) direction.Normalize();

            if (moving)
            {
                var targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation,
                    config.TurnSpeedDegrees * Time.deltaTime);
            }

            _verticalSpeed = _controller.isGrounded ? -1f : _verticalSpeed + Physics.gravity.y * Time.deltaTime;
            var velocity = direction * config.MoveSpeed + Vector3.up * _verticalSpeed;
            _controller.Move(velocity * Time.deltaTime);
        }
    }
}
