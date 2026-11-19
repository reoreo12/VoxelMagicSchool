// 이동 상태(Idle/Walk/Run)가 정한 목표 속도로 실제 위치·속도를 바꾸는 이동기의 계약. 복도(x축만)와 텃밭(바닥 평면)이 각자 구현한다.
// 상태 쪽은 "얼마나 빨리"만 정하고, 어느 방향·어떤 공간에서 움직일지는 이동기가 정한다.
using System;

namespace VoxelMagic.Core
{
    public interface ICharacterMotor
    {
        /// <summary>입력 방향으로 targetSpeed까지 가속·감속한다 (targetSpeed 0 = 멈춤).</summary>
        void Drive(MoveIntent intent, float targetSpeed, float acceleration, float deceleration, float deltaTime);

        /// <summary>지금 속력 (방향 없이 크기만).</summary>
        float Speed { get; }
    }

    /// <summary>이동 상태가 바뀌는 것을 알려 주는 쪽 (애니메이션 뷰가 듣는다). 복도·텃밭 상태 머신이 구현한다.</summary>
    public interface ILocomotionSource
    {
        CharacterStateId CurrentId { get; }

        /// <summary>이전 상태, 새 상태.</summary>
        event Action<CharacterStateId, CharacterStateId> StateChanged;
    }
}
