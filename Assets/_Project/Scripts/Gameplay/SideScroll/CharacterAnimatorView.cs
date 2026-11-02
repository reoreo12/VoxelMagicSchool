// 상태 머신의 StateChanged를 받아 Animator 상태를 섞어 바꾸기만 한다. 어떤 상태인지는 결정하지 않는다.
// Animator Controller에는 전이·파라미터가 없고, 상태 이름은 CharacterStateId 이름과 같다.
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class CharacterAnimatorView : MonoBehaviour, ISideScrollBinder
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private CharacterAnimationConfig _config;

        private CharacterStateMachine _machine;

        public void Bind(SideScrollContext context)
        {
            _machine = context.Character;
            _machine.StateChanged += OnStateChanged;
            _animator.Play(_machine.CurrentId.ToString(), 0, 0f);
        }

        private void OnDestroy()
        {
            if (_machine != null) _machine.StateChanged -= OnStateChanged;
        }

        private void OnStateChanged(CharacterStateId previous, CharacterStateId next) =>
            _animator.CrossFadeInFixedTime(next.ToString(), _config.BlendSeconds);
    }
}
