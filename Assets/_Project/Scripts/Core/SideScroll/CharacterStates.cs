// 캐릭터 상태 패턴: 공통 인터페이스와 Idle / Walk / Run 상태. 상태마다 Tick에서 할 일과 다음 상태 판단을 가진다.
// 새 상태는 ICharacterState(또는 CharacterStateBase)를 구현해 상태 머신에 등록하고, 들어갈 상태의 Evaluate에 조건을 더한다.
namespace VoxelMagic.Core
{
    public interface ICharacterState
    {
        CharacterStateId Id { get; }
        void Enter();
        void Tick(float deltaTime);
        void Exit();

        /// <summary>이번 프레임 입력을 보고 다음 상태 Id를 돌려준다. 그대로면 자기 Id.</summary>
        CharacterStateId Evaluate(MoveIntent intent);
    }

    /// <summary>상태들이 함께 쓰는 것: 이동기(복도·텃밭 공용 계약), 이동 설정, 이번 프레임 입력.</summary>
    public sealed class CharacterBody
    {
        public CharacterBody(ICharacterMotor motor, CharacterMoveConfig config)
        {
            Motor = motor;
            Config = config;
        }

        public ICharacterMotor Motor { get; }
        public CharacterMoveConfig Config { get; }
        public MoveIntent Intent { get; internal set; }
    }

    public abstract class CharacterStateBase : ICharacterState
    {
        protected CharacterStateBase(CharacterBody body) => Body = body;

        protected CharacterBody Body { get; }

        public abstract CharacterStateId Id { get; }
        public virtual void Enter() { }
        public abstract void Tick(float deltaTime);
        public virtual void Exit() { }
        public abstract CharacterStateId Evaluate(MoveIntent intent);
    }

    public sealed class IdleState : CharacterStateBase
    {
        public IdleState(CharacterBody body) : base(body) { }

        public override CharacterStateId Id => CharacterStateId.Idle;

        public override void Tick(float deltaTime) =>
            Body.Motor.Drive(Body.Intent, 0f, Body.Config.Acceleration, Body.Config.Deceleration, deltaTime);

        // Shift를 누른 채 출발하면 Walk를 한 프레임 거치지 않고 바로 Run (애니메이션 깜빡임 방지).
        public override CharacterStateId Evaluate(MoveIntent intent)
        {
            if (!intent.HasMove) return Id;
            return intent.Run ? CharacterStateId.Run : CharacterStateId.Walk;
        }
    }

    /// <summary>Walk/Run 공통: 입력 방향으로 목표 속도까지 가속. 목표 속도만 자식이 정한다.</summary>
    public abstract class MoveStateBase : CharacterStateBase
    {
        protected MoveStateBase(CharacterBody body) : base(body) { }

        protected abstract float TargetSpeed { get; }

        public override void Tick(float deltaTime) =>
            Body.Motor.Drive(Body.Intent, TargetSpeed, Body.Config.Acceleration, Body.Config.Deceleration, deltaTime);
    }

    public sealed class WalkState : MoveStateBase
    {
        public WalkState(CharacterBody body) : base(body) { }

        public override CharacterStateId Id => CharacterStateId.Walk;
        protected override float TargetSpeed => Body.Config.WalkSpeed;

        public override CharacterStateId Evaluate(MoveIntent intent)
        {
            if (!intent.HasMove) return CharacterStateId.Idle;
            return intent.Run ? CharacterStateId.Run : Id;
        }
    }

    public sealed class RunState : MoveStateBase
    {
        public RunState(CharacterBody body) : base(body) { }

        public override CharacterStateId Id => CharacterStateId.Run;
        protected override float TargetSpeed => Body.Config.RunSpeed;

        public override CharacterStateId Evaluate(MoveIntent intent)
        {
            if (!intent.HasMove) return CharacterStateId.Idle;
            return intent.Run ? Id : CharacterStateId.Walk;
        }
    }
}
