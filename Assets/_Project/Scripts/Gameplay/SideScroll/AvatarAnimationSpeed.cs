// 아바타 Animator에 상태별 재생 속도(AvatarSetup 값 × 장면 이동 설정의 배율)를 매 프레임 넣는다. 플레이 중 값을 바꾸면 바로 보인다.
// 복도·텃밭 공용. 배율 설정을 비우면 AvatarSetup 값 그대로.
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    [RequireComponent(typeof(Animator))]
    public sealed class AvatarAnimationSpeed : MonoBehaviour
    {
        private static readonly CharacterStateId[] States = { CharacterStateId.Idle, CharacterStateId.Walk, CharacterStateId.Run };

        [SerializeField] private AvatarSetup _setup;
        [SerializeField] private Animator _animator;
        [Tooltip("장면별 재생 배율 (비우면 1)")]
        [SerializeField] private CharacterMoveConfig _moveConfig;

        private int[] _parameters;

        private void Awake()
        {
            _parameters = new int[States.Length];
            for (int i = 0; i < States.Length; i++)
                _parameters[i] = Animator.StringToHash(AvatarSetup.SpeedParameter(States[i]));
        }

        private void Update()
        {
            if (_setup == null) return;
            for (int i = 0; i < States.Length; i++)
                _animator.SetFloat(_parameters[i], _setup.SpeedFor(States[i]) * (_moveConfig != null ? _moveConfig.AnimationScaleFor(States[i]) : 1f));
        }
    }
}
