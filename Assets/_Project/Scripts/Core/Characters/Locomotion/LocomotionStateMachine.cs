// 이동 상태 머신 (복도·텃밭 공용): 입력 → 상태 판단(Idle/Walk/Run) → 전환 → 현재 상태 Tick(이동기에 목표 속도). 상태가 바뀔 때만 이벤트.
// 복도 전용 규칙(좌우 방향 전환·복도 끝 막힘)은 CharacterStateMachine이 이 위에 얹는다.
using System;
using System.Collections.Generic;

namespace VoxelMagic.Core
{
    public sealed class LocomotionStateMachine : ILocomotionSource
    {
        private readonly Dictionary<CharacterStateId, ICharacterState> _states = new Dictionary<CharacterStateId, ICharacterState>();

        public LocomotionStateMachine(ICharacterMotor motor, CharacterMoveConfig config)
        {
            Body = new CharacterBody(motor, config);
            Register(new IdleState(Body));
            Register(new WalkState(Body));
            Register(new RunState(Body));
            Current = _states[CharacterStateId.Idle];
            Current.Enter();
        }

        public event Action<CharacterStateId, CharacterStateId> StateChanged;

        public CharacterBody Body { get; }
        public ICharacterState Current { get; private set; }
        public CharacterStateId CurrentId => Current.Id;

        /// <summary>상태 추가·교체용. 같은 Id가 있으면 바꾼다.</summary>
        public void Register(ICharacterState state) => _states[state.Id] = state;

        public void Update(MoveIntent intent, float deltaTime)
        {
            Body.Intent = intent;
            var nextId = Current.Evaluate(intent);
            if (nextId != Current.Id && _states.TryGetValue(nextId, out var next))
            {
                var previous = Current;
                previous.Exit();
                Current = next;
                Current.Enter();
                StateChanged?.Invoke(previous.Id, nextId);
            }
            Current.Tick(deltaTime);
        }
    }
}
