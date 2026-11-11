// 공용 이동 상태 머신·바닥 평면 이동기 단위 테스트: 텃밭 입력으로 Idle↔Walk↔Run, 가속·감속, 대각선 정규화, 상태 이벤트.
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Tests
{
    public class LocomotionTests
    {
        private const float Dt = 0.02f;
        private CharacterMoveConfig _config;
        private PlanarMotor _motor;
        private LocomotionStateMachine _machine;
        private List<(CharacterStateId from, CharacterStateId to)> _changes;

        [SetUp]
        public void SetUp()
        {
            _config = TestAssets.Create<CharacterMoveConfig>(
                ("_walkSpeed", 4f), ("_runSpeed", 9f), ("_acceleration", 40f), ("_deceleration", 50f));
            _motor = new PlanarMotor();
            _machine = new LocomotionStateMachine(_motor, _config);
            _changes = new List<(CharacterStateId, CharacterStateId)>();
            _machine.StateChanged += (from, to) => _changes.Add((from, to));
        }

        [TearDown] public void TearDown() => TestAssets.DestroyAll();

        private void Run(MoveIntent intent, int frames)
        {
            for (int i = 0; i < frames; i++) _machine.Update(intent, Dt);
        }

        [Test]
        public void Planar_IdleToWalkToRunAndBack()
        {
            Run(new MoveIntent(Vector2.up, false), 1);
            Assert.AreEqual(CharacterStateId.Walk, _machine.CurrentId);
            Run(new MoveIntent(Vector2.up, true), 1);
            Assert.AreEqual(CharacterStateId.Run, _machine.CurrentId);
            Run(new MoveIntent(Vector2.zero, true), 1);
            Assert.AreEqual(CharacterStateId.Idle, _machine.CurrentId);
            CollectionAssert.AreEqual(new[]
            {
                (CharacterStateId.Idle, CharacterStateId.Walk),
                (CharacterStateId.Walk, CharacterStateId.Run),
                (CharacterStateId.Run, CharacterStateId.Idle)
            }, _changes);
        }

        [Test]
        public void Planar_ReachesWalkAndRunSpeed()
        {
            Run(new MoveIntent(Vector2.right, false), 50);
            Assert.AreEqual(4f, _motor.Speed, 0.001f);
            Assert.Greater(_motor.Velocity.x, 0f);
            Run(new MoveIntent(Vector2.right, true), 50);
            Assert.AreEqual(9f, _motor.Speed, 0.001f);
        }

        [Test]
        public void Planar_DiagonalIsNotFaster()
        {
            Run(new MoveIntent(new Vector2(1f, 1f), false), 50);
            Assert.AreEqual(4f, _motor.Speed, 0.001f);
        }

        [Test]
        public void Planar_StopsWithDeceleration()
        {
            Run(new MoveIntent(Vector2.right, false), 50);
            Run(new MoveIntent(Vector2.zero, false), 1);
            Assert.AreEqual(4f - 50f * Dt, _motor.Speed, 0.001f);
            Run(new MoveIntent(Vector2.zero, false), 50);
            Assert.AreEqual(0f, _motor.Speed, 0.0001f);
        }

        [Test]
        public void SideScrollIntent_HasPlanarAlongX()
        {
            var intent = new MoveIntent(-1, true);
            Assert.AreEqual(new Vector2(-1f, 0f), intent.Planar);
            Assert.IsTrue(intent.HasMove);
            Assert.IsFalse(new MoveIntent(0, true).HasMove);
        }

        [Test]
        public void AnimationScale_PerState()
        {
            var config = TestAssets.Create<CharacterMoveConfig>(
                ("_walkAnimationScale", 2f), ("_runAnimationScale", 4.5f));
            Assert.AreEqual(1f, config.AnimationScaleFor(CharacterStateId.Idle));
            Assert.AreEqual(2f, config.AnimationScaleFor(CharacterStateId.Walk));
            Assert.AreEqual(4.5f, config.AnimationScaleFor(CharacterStateId.Run));
        }
    }
}
