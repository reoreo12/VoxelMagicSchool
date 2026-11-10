// 이동 상태(ILocomotionSource)가 바뀔 때만 Animator 상태를 섞어 바꾼다. 어떤 상태인지는 결정하지 않는다 (복도·텃밭 공용).
// 복도는 ISideScrollBinder로, 텃밭은 PlayerMover가 Initialize로 연결한다. Controller 상태 이름은 CharacterStateId 이름과 같다.
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class CharacterAnimatorView : MonoBehaviour, ISideScrollBinder
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private CharacterAnimationConfig _config;

        private ILocomotionSource _source;

        public void Bind(SideScrollContext context) => Initialize(context.Character);

        public void Initialize(ILocomotionSource source)
        {
            if (_source != null) _source.StateChanged -= OnStateChanged;
            _source = source;
            if (_source == null) return;
            _source.StateChanged += OnStateChanged;
            _animator.Play(_source.CurrentId.ToString(), 0, 0f);
        }

        private void OnDestroy()
        {
            if (_source != null) _source.StateChanged -= OnStateChanged;
        }

        private void OnStateChanged(CharacterStateId previous, CharacterStateId next) =>
            _animator.CrossFadeInFixedTime(next.ToString(), _config.BlendSeconds);
    }
}
