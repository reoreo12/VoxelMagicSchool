// 캐릭터 상태 머신 단위 테스트: Idle↔Walk↔Run 전이, 즉시 방향 전환, 복도 끝 막힘, 이벤트.
using System.Collections.Generic;
using NUnit.Framework;
using VoxelMagic.Core;

namespace VoxelMagic.Tests
{
    public class CharacterStateMachineTests
    {
        private const float Dt = 0.02f;
        private static readonly MoveIntent None = new MoveIntent(0, false);
        private static readonly MoveIntent Right = new MoveIntent(1, false);
        private static readonly MoveIntent Left = new MoveIntent(-1, false);
        private static readonly MoveIntent RunRight = new MoveIntent(1, true);
        private static readonly MoveIntent ShiftOnly = new MoveIntent(0, true);

        private CharacterMoveConfig _config;
        private CharacterStateMachine _machine;
        private List<(CharacterStateId from, CharacterStateId to)> _changes;
        private List<Facing> _turns;

        [SetUp]
        public void SetUp()
        {
            _config = TestAssets.Create<CharacterMoveConfig>(
                ("_walkSpeed", 2f), ("_runSpeed", 4f), ("_acceleration", 10f), ("_deceleration", 20f));
            _machine = Create(startX: 5f);
        }

        private CharacterStateMachine Create(float startX)
        {
            var machine = new CharacterStateMachine(new SideScrollMotor(startX, 0f, 10f), _config);
            _changes = new List<(CharacterStateId, CharacterStateId)>();
            _turns = new List<Facing>();
            machine.StateChanged += (from, to) => _changes.Add((from, to));
            machine.FacingChanged += f => _turns.Add(f);
            return machine;
        }

        [Test]
        public void StartsIdleFacingRight()
        {
            Assert.AreEqual(CharacterStateId.Idle, _machine.CurrentId);
            Assert.AreEqual(Facing.Right, _machine.Facing);
        }

        [Test]
        public void Idle_ToWalk_OnMove_AndBack_OnRelease()
        {
            _machine.Update(Right, Dt);
            Assert.AreEqual(CharacterStateId.Walk, _machine.CurrentId);
            _machine.Update(None, Dt);
            Assert.AreEqual(CharacterStateId.Idle, _machine.CurrentId);
            CollectionAssert.AreEqual(new[] { (CharacterStateId.Idle, CharacterStateId.Walk), (CharacterStateId.Walk, CharacterStateId.Idle) }, _changes);
        }

        [Test]
        public void Walk_ToRun_WithShift_AndBack()
        {
            _machine.Update(Right, Dt);
            _machine.Update(RunRight, Dt);
            Assert.AreEqual(CharacterStateId.Run, _machine.CurrentId);
            _machine.Update(Right, Dt);
            Assert.AreEqual(CharacterStateId.Walk, _machine.CurrentId);
        }

        [Test]
        public void Idle_ToRun_Directly_WhenShiftHeld()
        {
            _machine.Update(RunRight, Dt);
            Assert.AreEqual(CharacterStateId.Run, _machine.CurrentId);
            Assert.AreEqual(1, _changes.Count, "Walk를 거치지 않는다");
        }

        [Test]
        public void Run_ToIdle_OnRelease()
        {
            _machine.Update(RunRight, Dt);
            _machine.Update(None, Dt);
            Assert.AreEqual(CharacterStateId.Idle, _machine.CurrentId);
        }

        [Test]
        public void ShiftWithoutDirection_StaysIdle()
        {
            _machine.Update(ShiftOnly, Dt);
            Assert.AreEqual(CharacterStateId.Idle, _machine.CurrentId);
            Assert.IsEmpty(_changes);
        }

        [Test]
        public void Facing_FlipsInSameFrame_WithoutStateChange()
        {
            _machine.Update(Right, Dt);
            _machine.Update(Left, Dt);
            Assert.AreEqual(Facing.Left, _machine.Facing);
            Assert.AreEqual(CharacterStateId.Walk, _machine.CurrentId);
            CollectionAssert.AreEqual(new[] { Facing.Left }, _turns);
        }

        [Test]
        public void Facing_FlipsFromIdle_InFirstFrame()
        {
            _machine.Update(Left, Dt);
            Assert.AreEqual(Facing.Left, _machine.Facing);
            Assert.AreEqual(CharacterStateId.Walk, _machine.CurrentId);
        }

        [Test]
        public void Facing_KeptWhenInputReleased()
        {
            _machine.Update(Left, Dt);
            _machine.Update(None, Dt);
            Assert.AreEqual(Facing.Left, _machine.Facing);
            Assert.AreEqual(1, _turns.Count);
        }

        [Test]
        public void PushingIntoCorridorEnd_IsIdle_ButCanTurnAround()
        {
            _machine = Create(startX: 10f);
            _machine.Update(Right, Dt);
            Assert.AreEqual(CharacterStateId.Idle, _machine.CurrentId, "끝에서 바깥으로 누르면 제자리 걸음 대신 Idle");
            Assert.AreEqual(10f, _machine.Motor.X, 0.0001f);

            _machine.Update(Left, Dt);
            Assert.AreEqual(Facing.Left, _machine.Facing);
            Assert.AreEqual(CharacterStateId.Walk, _machine.CurrentId);
            Assert.Less(_machine.Motor.X, 10f);
        }

        [Test]
        public void Walking_MovesTowardFacing()
        {
            for (int i = 0; i < 10; i++) _machine.Update(Right, Dt);
            Assert.Greater(_machine.Motor.X, 5f);
            for (int i = 0; i < 60; i++) _machine.Update(Left, Dt);
            Assert.Less(_machine.Motor.X, 5f);
        }

        [Test]
        public void RegisteredState_ReplacesExisting()
        {
            var custom = new StubState(CharacterStateId.Walk);
            _machine.Register(custom);
            _machine.Update(Right, Dt);
            Assert.AreSame(custom, _machine.Current);
            Assert.AreEqual(1, custom.Entered);
        }

        private sealed class StubState : ICharacterState
        {
            public StubState(CharacterStateId id) => Id = id;
            public int Entered { get; private set; }
            public CharacterStateId Id { get; }
            public void Enter() => Entered++;
            public void Tick(float deltaTime) { }
            public void Exit() { }
            public CharacterStateId Evaluate(MoveIntent intent) => Id;
        }
    }
}
