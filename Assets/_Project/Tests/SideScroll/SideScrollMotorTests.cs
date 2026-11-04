// 횡스크롤 이동 계산기 단위 테스트: 가속·감속, 최고 속도, 반대 방향 전환, 양 끝 제한.
using NUnit.Framework;
using VoxelMagic.Core;

namespace VoxelMagic.Tests
{
    public class SideScrollMotorTests
    {
        private const float Accel = 10f;
        private const float Decel = 20f;
        private const float Dt = 0.1f;

        [Test]
        public void StartX_IsClampedIntoRange()
        {
            Assert.AreEqual(2f, new SideScrollMotor(-5f, 2f, 8f).X, 0.0001f);
            Assert.AreEqual(8f, new SideScrollMotor(50f, 2f, 8f).X, 0.0001f);
        }

        [Test]
        public void Accelerates_ThenHoldsTargetSpeed()
        {
            var motor = new SideScrollMotor(0f, -100f, 100f);
            motor.Step(2f, Accel, Decel, Dt);
            Assert.AreEqual(1f, motor.Velocity, 0.0001f, "가속값 10 × 0.1초");
            for (int i = 0; i < 10; i++) motor.Step(2f, Accel, Decel, Dt);
            Assert.AreEqual(2f, motor.Velocity, 0.0001f);
        }

        [Test]
        public void Stops_WithDeceleration()
        {
            var motor = new SideScrollMotor(0f, -100f, 100f);
            for (int i = 0; i < 10; i++) motor.Step(2f, Accel, Decel, Dt);
            motor.Step(0f, Accel, Decel, Dt);
            Assert.AreEqual(0f, motor.Velocity, 0.0001f, "감속값 20 × 0.1초 = 2 → 바로 멈춤");
        }

        [Test]
        public void Reversing_UsesDecelerationFirst()
        {
            var motor = new SideScrollMotor(0f, -100f, 100f);
            for (int i = 0; i < 10; i++) motor.Step(2f, Accel, Decel, Dt);
            motor.Step(-2f, Accel, Decel, Dt);
            Assert.AreEqual(0f, motor.Velocity, 0.0001f);
        }

        [Test]
        public void ClampsAtEnds_AndZeroesVelocity()
        {
            var motor = new SideScrollMotor(9.9f, 0f, 10f);
            for (int i = 0; i < 20; i++) motor.Step(4f, Accel, Decel, Dt);
            Assert.AreEqual(10f, motor.X, 0.0001f);
            Assert.AreEqual(0f, motor.Velocity, 0.0001f);
            Assert.IsTrue(motor.IsBlocked(1));
            Assert.IsFalse(motor.IsBlocked(-1));
        }

        [Test]
        public void NotBlocked_WithoutDirection()
        {
            var motor = new SideScrollMotor(0f, 0f, 10f);
            Assert.IsFalse(motor.IsBlocked(0));
            Assert.IsTrue(motor.IsBlocked(-1));
        }
    }
}
