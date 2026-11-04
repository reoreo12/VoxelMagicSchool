// 플레이어 아바타 애니메이션: 움직이면 Walk, 멈춰 있으면 Idle (농사·낚시·대화처럼 전용 애니메이션이 없는 동작도 Idle).
// Walk 재생 속도는 실제 이동 속도 ÷ 기준 속도로 맞춰 발이 미끄러져 보이지 않게 한다.
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerAvatarAnimator : MonoBehaviour, IGameContextBinder
    {
        private static readonly string IdleState = CharacterStateId.Idle.ToString();
        private static readonly string WalkState = CharacterStateId.Walk.ToString();

        [SerializeField] private Animator _animator;

        private CharacterController _controller;
        private PlayerConfig _config;
        private int _walkSpeedParameter;
        private bool _walking;

        public void Bind(GameContext context)
        {
            _config = context.PlayerConfig;
            _animator.Play(IdleState, 0, 0f);
        }

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _walkSpeedParameter = Animator.StringToHash(AvatarSetup.SpeedParameter(CharacterStateId.Walk));
        }

        private void Update()
        {
            if (_config == null) return;

            var velocity = _controller.velocity;
            velocity.y = 0f;
            float speed = velocity.magnitude;
            bool walking = speed > _config.WalkStartSpeed;
            if (walking != _walking)
            {
                _walking = walking;
                _animator.CrossFadeInFixedTime(walking ? WalkState : IdleState, _config.AnimationBlendSeconds);
            }
            if (walking) _animator.SetFloat(_walkSpeedParameter, speed / _config.WalkAnimationReferenceSpeed);
        }
    }
}
