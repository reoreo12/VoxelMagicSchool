// 텃밭·연못 플레이어 이동: 카메라 방향 기준 WASD + Shift 달리기를 복도와 같은 이동 상태 머신(Idle/Walk/Run)으로 처리하고,
// 바닥 평면 이동기(PlanarMotor)의 속도를 CharacterController로 옮긴다. 창이 열려 있거나 물 주는 중이면 멈추고, 움직이면 낚시 취소·획득 정보 닫기.
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMover : MonoBehaviour, IGameContextBinder
    {
        [SerializeField] private GameInputReader _input;
        [Tooltip("3D 맵 공용 이동 설정 MoveConfig (걷기·달리기 속도, 가속·감속, 애니메이션 재생 배율)")]
        [SerializeField] private CharacterMoveConfig _moveConfig;
        [Tooltip("이동 상태를 애니메이션으로 보여 줄 뷰 (아바타 모델의 CharacterAnimatorView)")]
        [SerializeField] private CharacterAnimatorView _animationView;

        private readonly PlanarMotor _motor = new PlanarMotor();
        private LocomotionStateMachine _locomotion;
        private CharacterController _controller;
        private GameContext _context;
        private float _verticalSpeed;
        private bool _wasMoving;

        public ILocomotionSource Locomotion => _locomotion;

        public void Bind(GameContext context)
        {
            _context = context;
            if (_moveConfig == null)
            {
                Debug.LogError($"[{name}] PlayerMover에 이동 설정(CharacterMoveConfig)이 없습니다 (메뉴 VoxelMagic/캐릭터/텃밭 플레이어 이동·애니 통일).", this);
                return;
            }
            _locomotion = new LocomotionStateMachine(_motor, _moveConfig);
            if (_animationView != null) _animationView.Initialize(_locomotion);
        }

        private void Awake() => _controller = GetComponent<CharacterController>();

        private void Update()
        {
            if (_context == null || _locomotion == null) return;
            var config = _context.PlayerConfig;

            // 창이 열려 있거나 물 주기 동작 중에는 멈춘다.
            Vector2 input = _context.Screen.IsModalOpen || _context.Farm.IsWatering ? Vector2.zero : _input.Move;
            bool moving = input.sqrMagnitude > 0.01f;
            if (moving)
            {
                if (_context.Fishing.IsBusy) _context.Fishing.Cancel();
                if (!_wasMoving) _context.Messages.DismissAcquired();
            }
            _wasMoving = moving;

            // 카메라 Yaw 기준으로 돌린 바닥 평면 방향 (x = 월드 x, y = 월드 z)
            var world = Quaternion.Euler(0f, config.CameraYaw, 0f) * new Vector3(input.x, 0f, input.y);
            var intent = new MoveIntent(moving ? new Vector2(world.x, world.z) : Vector2.zero, _input.Run);
            _locomotion.Update(intent, Time.deltaTime);

            var planar = new Vector3(_motor.Velocity.x, 0f, _motor.Velocity.y);
            if (moving)
            {
                var targetRotation = Quaternion.LookRotation(world, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation,
                    config.TurnSpeedDegrees * Time.deltaTime);
            }

            _verticalSpeed = _controller.isGrounded ? -1f : _verticalSpeed + Physics.gravity.y * Time.deltaTime;
            _controller.Move((planar + Vector3.up * _verticalSpeed) * Time.deltaTime);
        }
    }
}
