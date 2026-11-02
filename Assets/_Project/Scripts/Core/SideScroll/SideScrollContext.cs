// 복도 씬의 Core 객체 묶음. 부트스트랩이 한 번 만들고 ISideScrollBinder에게 나눠 준다.
namespace VoxelMagic.Core
{
    public sealed class SideScrollContext
    {
        private SideScrollContext() { }

        public CorridorConfig Config { get; private set; }
        public CharacterMoveConfig MoveConfig { get; private set; }
        public CorridorBounds Bounds { get; private set; }
        public CharacterStateMachine Character { get; private set; }

        /// <summary>캐릭터 이동 범위는 복도 양 끝에서 끝 여유만큼 안쪽 (문 앞에서 멈춤).</summary>
        public static SideScrollContext Create(CorridorConfig config, CharacterMoveConfig moveConfig, CorridorBounds bounds)
        {
            var motor = new SideScrollMotor(moveConfig.StartX, bounds.MinX + config.EndMargin, bounds.MaxX - config.EndMargin);
            return new SideScrollContext
            {
                Config = config,
                MoveConfig = moveConfig,
                Bounds = bounds,
                Character = new CharacterStateMachine(motor, moveConfig)
            };
        }
    }

    public interface ISideScrollBinder
    {
        void Bind(SideScrollContext context);
    }
}
