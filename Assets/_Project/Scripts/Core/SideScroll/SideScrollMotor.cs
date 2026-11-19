// 횡스크롤 이동 계산기. x 위치와 x 속도만 가지며(Y·Z 없음), 가속·감속과 양 끝 제한을 처리한다.
using UnityEngine;

namespace VoxelMagic.Core
{
    public sealed class SideScrollMotor : ICharacterMotor
    {
        private const float EdgeEpsilon = 0.0001f;

        public SideScrollMotor(float startX, float minX, float maxX)
        {
            MinX = minX;
            MaxX = Mathf.Max(minX, maxX);
            X = Mathf.Clamp(startX, MinX, MaxX);
        }

        public float X { get; private set; }
        public float Velocity { get; private set; }
        public float MinX { get; }
        public float MaxX { get; }
        public float Speed => Mathf.Abs(Velocity);

        /// <summary>상태 머신용: 좌우 입력 방향 × 목표 속도로 Step.</summary>
        public void Drive(MoveIntent intent, float targetSpeed, float acceleration, float deceleration, float deltaTime) =>
            Step(intent.Direction * targetSpeed, acceleration, deceleration, deltaTime);

        /// <summary>속도 없이 x로 바로 옮긴다 (씬 도착 지점). 범위 밖이면 끝에 붙음.</summary>
        public void Teleport(float x)
        {
            X = Mathf.Clamp(x, MinX, MaxX);
            Velocity = 0f;
        }

        /// <summary>그 방향으로 더 갈 수 없으면 true (복도 끝에 닿음).</summary>
        public bool IsBlocked(int direction) =>
            (direction < 0 && X <= MinX + EdgeEpsilon) || (direction > 0 && X >= MaxX - EdgeEpsilon);

        /// <summary>목표 속도로 다가간다. 같은 방향으로 빨라질 때는 가속값, 그 밖(멈춤·감속·반대 방향)은 감속값.</summary>
        public void Step(float targetVelocity, float acceleration, float deceleration, float deltaTime)
        {
            bool speedingUp = targetVelocity != 0f &&
                              (Velocity == 0f || Mathf.Sign(targetVelocity) == Mathf.Sign(Velocity)) &&
                              Mathf.Abs(targetVelocity) > Mathf.Abs(Velocity);
            float rate = speedingUp ? acceleration : deceleration;
            Velocity = Mathf.MoveTowards(Velocity, targetVelocity, rate * deltaTime);
            X += Velocity * deltaTime;

            if (X <= MinX)
            {
                X = MinX;
                if (Velocity < 0f) Velocity = 0f;
            }
            else if (X >= MaxX)
            {
                X = MaxX;
                if (Velocity > 0f) Velocity = 0f;
            }
        }
    }
}
