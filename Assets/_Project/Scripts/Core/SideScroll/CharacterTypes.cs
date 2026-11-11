// 캐릭터 기본 값 타입(복도·텃밭 공용): 한 프레임 입력(MoveIntent), 바라보는 방향, 상태 Id.
using UnityEngine;

namespace VoxelMagic.Core
{
    /// <summary>입력 장치와 상태 머신 사이의 약속. 복도는 좌우(Direction -1/0/1), 텃밭은 바닥 평면 방향(Planar, 월드 x·z), 달리기는 Shift 누름 여부.</summary>
    public readonly struct MoveIntent
    {
        /// <summary>복도(좌우)용: Planar = (방향, 0).</summary>
        public MoveIntent(int direction, bool run)
        {
            Direction = direction < 0 ? -1 : direction > 0 ? 1 : 0;
            Planar = new Vector2(Direction, 0f);
            Run = run;
        }

        /// <summary>바닥 평면용: 길이 1 이하 방향 (월드 x, 월드 z).</summary>
        public MoveIntent(Vector2 planar, bool run)
        {
            Planar = planar.sqrMagnitude > 1f ? planar.normalized : planar;
            Direction = Planar.x < 0f ? -1 : Planar.x > 0f ? 1 : 0;
            Run = run;
        }

        public int Direction { get; }
        public Vector2 Planar { get; }
        public bool Run { get; }
        public bool HasMove => Planar.sqrMagnitude > 0.0001f;

        public MoveIntent WithDirection(int direction) => new MoveIntent(direction, Run);
    }

    public enum Facing
    {
        Left = -1,
        Right = 1
    }

    /// <summary>새 상태(Jump, Interact, CastSpell 등)는 여기에 값을 추가하고 상태 클래스를 등록한다.</summary>
    public enum CharacterStateId
    {
        Idle,
        Walk,
        Run
    }
}
