// 바닥 평면(x, z) 이동 계산기 (텃밭·연못 같은 쿼터뷰 맵). 속도 벡터만 계산하고, 실제 위치 이동·충돌은 Gameplay(CharacterController)가 한다.
using UnityEngine;

namespace VoxelMagic.Core
{
    public sealed class PlanarMotor : ICharacterMotor
    {
        /// <summary>바닥 평면 속도 (x = 월드 x, y = 월드 z).</summary>
        public Vector2 Velocity { get; private set; }
        public float Speed => Velocity.magnitude;

        public void Drive(MoveIntent intent, float targetSpeed, float acceleration, float deceleration, float deltaTime)
        {
            var direction = intent.Planar.sqrMagnitude > 1f ? intent.Planar.normalized : intent.Planar;
            var target = direction * targetSpeed;
            // 같은 쪽으로 빨라질 때만 가속값, 멈추기·느려지기·방향 바꾸기는 감속값 (복도 SideScrollMotor와 같은 규칙).
            bool speedingUp = target.sqrMagnitude > Velocity.sqrMagnitude && Vector2.Dot(target, Velocity) >= 0f;
            float rate = speedingUp ? acceleration : deceleration;
            Velocity = Vector2.MoveTowards(Velocity, target, rate * deltaTime);
        }

        /// <summary>속도 없이 멈춘다 (씬 도착·창 열림 등).</summary>
        public void Stop() => Velocity = Vector2.zero;
    }
}
