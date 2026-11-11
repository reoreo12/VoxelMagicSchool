// 복도(횡스크롤) 캐릭터 상태 머신. 매 프레임 방향 전환 → 끝 막힘 판정 → 공용 이동 상태 머신(상태 판단·전환·Tick) 순서로 처리하고,
// 상태·방향이 바뀌면 이벤트로 알린다. Idle/Walk/Run 규칙은 텃밭과 같은 LocomotionStateMachine을 쓴다.
using System;

namespace VoxelMagic.Core
{
    public sealed class CharacterStateMachine : ILocomotionSource
    {
        private readonly LocomotionStateMachine _locomotion;

        public CharacterStateMachine(SideScrollMotor motor, CharacterMoveConfig config, Facing facing = Facing.Right)
        {
            Motor = motor;
            Facing = facing;
            _locomotion = new LocomotionStateMachine(motor, config);
            _locomotion.StateChanged += (previous, next) => StateChanged?.Invoke(previous, next);
        }

        /// <summary>이전 상태, 새 상태.</summary>
        public event Action<CharacterStateId, CharacterStateId> StateChanged;
        public event Action<Facing> FacingChanged;

        public ICharacterState Current => _locomotion.Current;
        public CharacterStateId CurrentId => _locomotion.CurrentId;
        public Facing Facing { get; private set; }
        public SideScrollMotor Motor { get; }
        public CharacterBody Body => _locomotion.Body;

        /// <summary>상태 추가·교체용. 같은 Id가 있으면 바꾼다.</summary>
        public void Register(ICharacterState state) => _locomotion.Register(state);

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
            var intent = Motor.IsBlocked(input.Direction) ? input.WithDirection(0) : input;

            // 3) 상태 판단·전환·이동 (공용)
            _locomotion.Update(intent, deltaTime);
        }
    }
}
