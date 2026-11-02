// 복도 씬의 Core 객체 묶음. 부트스트랩이 한 번 만들고 ISideScrollBinder에게 나눠 준다.
namespace VoxelMagic.Core
{
    public sealed class SideScrollContext
    {
        private SideScrollContext() { }

        public CorridorConfig Config { get; private set; }
        public CorridorBounds Bounds { get; private set; }

        public static SideScrollContext Create(CorridorConfig config, CorridorBounds bounds) =>
            new SideScrollContext { Config = config, Bounds = bounds };
    }

    public interface ISideScrollBinder
    {
        void Bind(SideScrollContext context);
    }
}
