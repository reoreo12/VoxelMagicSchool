// 아바타 Animator에 AvatarSetup의 상태별 재생 속도를 매 프레임 넣는다. 플레이 중 값을 바꾸면 바로 보인다 (복도에서 맞추기용).
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
                _animator.SetFloat(_parameters[i], _setup.SpeedFor(States[i]));
        }
    }
}
