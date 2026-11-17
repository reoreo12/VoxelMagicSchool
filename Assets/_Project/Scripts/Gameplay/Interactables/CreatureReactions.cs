// 크리처에게 말을 걸었을 때(의뢰 미완료) 반응 동작을 무작위로 하나 재생하고, 끝나면 Idle로 돌아간다.
// 한 번 재생 동작은 컨트롤러 전이로 돌아가고, 반복 동작은 머무는 시간이 지나면 여기서 Idle로 부드럽게 넘긴다.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelMagic.Gameplay
{
    public sealed class CreatureReactions : MonoBehaviour
    {
        [Serializable]
        private struct Reaction
        {
            [Tooltip("시작할 Animator 상태 이름")]
            public string State;
            [Tooltip("반복 동작이면 이 시간(초, 최소~최대) 뒤 Idle로. 0이면 컨트롤러 전이에 맡김 (한 번 재생 동작)")]
            public Vector2 HoldSeconds;
        }

        [SerializeField] private Animator _animator;
        [SerializeField] private string _idleState = "Cat_Idle";
        [SerializeField, Min(0f)] private float _crossFadeSeconds = 0.2f;
        [SerializeField] private List<Reaction> _reactions = new List<Reaction>
        {
            new Reaction { State = "Cat_Sleep_Start", HoldSeconds = new Vector2(6f, 9f) }, // 잠들기 3.5초 + 잠 2.5~5.5초
            new Reaction { State = "Cat_Jump" },
            new Reaction { State = "Cat_Sitting_Idle", HoldSeconds = new Vector2(3f, 5f) },
            new Reaction { State = "Cat_Attack_L" },
            new Reaction { State = "Cat_Attack_R" },
        };

        private int _last = -1;
        private float _returnAt = -1f;

        public event Action<string> Reacted;

        private void Awake()
        {
            if (_animator == null) _animator = GetComponentInChildren<Animator>();
        }

        /// <summary>직전과 다른 반응을 하나 골라 재생한다 (반응 중이면 새 반응으로 바뀜).</summary>
        public void PlayRandom()
        {
            if (_animator == null || _reactions.Count == 0) return;
            int index = UnityEngine.Random.Range(0, _reactions.Count);
            if (_reactions.Count > 1 && index == _last) index = (index + 1 + UnityEngine.Random.Range(0, _reactions.Count - 1)) % _reactions.Count;
            _last = index;

            var reaction = _reactions[index];
            _animator.CrossFadeInFixedTime(reaction.State, _crossFadeSeconds, 0);
            float hold = Mathf.Lerp(reaction.HoldSeconds.x, reaction.HoldSeconds.y, UnityEngine.Random.value);
            _returnAt = hold > 0f ? Time.time + hold : -1f;
            Reacted?.Invoke(reaction.State);
        }

        private void Update()
        {
            if (_returnAt < 0f || Time.time < _returnAt) return;
            _returnAt = -1f;
            _animator.CrossFadeInFixedTime(_idleState, _crossFadeSeconds, 0);
        }
    }
}
