// 캐릭터 상태 머신. 매 프레임 방향 전환 → 끝 막힘 판정 → 상태 판단·전환 → 현재 상태 Tick 순서로 처리하고,
// 상태·방향이 바뀌면 이벤트로 알린다. 애니메이션·UI는 이벤트만 듣고 상태를 결정하지 않는다.
using System;
using System.Collections.Generic;

namespace VoxelMagic.Core
{
    public sealed class CharacterStateMachine
    {
        private readonly Dictionary<CharacterStateId, ICharacterState> _states = new Dictionary<CharacterStateId, ICharacterState>();
        private readonly CharacterBody _body;

        public CharacterStateMachine(SideScrollMotor motor, CharacterMoveConfig config, Facing facing = Facing.Right)
        {
            _body = new CharacterBody(motor, config);
            Facing = facing;
            Register(new IdleState(_body));
            Register(new WalkState(_body));
            Register(new RunState(_body));
            Current = _states[CharacterStateId.Idle];
            Current.Enter();
        }

        /// <summary>이전 상태, 새 상태.</summary>
        public event Action<CharacterStateId, CharacterStateId> StateChanged;
        public event Action<Facing> FacingChanged;

        public ICharacterState Current { get; private set; }
        public CharacterStateId CurrentId => Current.Id;
        public Facing Facing { get; private set; }
        public SideScrollMotor Motor => _body.Motor;
        public CharacterBody Body => _body;

        /// <summary>상태 추가·교체용. 같은 Id가 있으면 바꾼다.</summary>
        public void Register(ICharacterState state) => _states[state.Id] = state;

        public void Update(MoveIntent input, float deltaTime)
        {
            // 1) 방향 전환: 막혀 있어도 돌아설 수 있도록 원래 입력으로 먼저 처리한다. 보간 없이 즉시.
            if (input.HasMove)
            {
                var facing = input.Direction > 0 ? Facing.Right : Facing.Left;
                if (facing != Facing)
                {
                    Facing = facing;
                    FacingChanged?.Invoke(facing);
                }
            }

            // 2) 복도 끝에서 바깥으로 누르면 이동 입력이 없는 것으로 본다 (제자리 걸음 대신 Idle).
            var intent = _body.Motor.IsBlocked(input.Direction) ? input.WithDirection(0) : input;
            _body.Intent = intent;

            // 3) 상태 판단·전환
            var nextId = Current.Evaluate(intent);
            if (nextId != Current.Id && _states.TryGetValue(nextId, out var next))
            {
                var previous = Current;
                previous.Exit();
                Current = next;
                Current.Enter();
                StateChanged?.Invoke(previous.Id, nextId);
            }

            // 4) 현재 상태의 프레임 처리 (이동 계산)
            Current.Tick(deltaTime);
        }
    }
}
