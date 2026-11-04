// 횡스크롤 캐릭터의 기본 값 타입: 한 프레임 입력(MoveIntent), 바라보는 방향, 상태 Id.
namespace VoxelMagic.Core
{
    /// <summary>입력 장치와 상태 머신 사이의 약속. 방향은 -1/0/1, 달리기는 Shift 누름 여부.</summary>
    public readonly struct MoveIntent
    {
        public MoveIntent(int direction, bool run)
        {
            Direction = direction < 0 ? -1 : direction > 0 ? 1 : 0;
            Run = run;
        }

        public int Direction { get; }
        public bool Run { get; }
        public bool HasMove => Direction != 0;

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
