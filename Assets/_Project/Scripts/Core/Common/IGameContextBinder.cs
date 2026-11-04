// 부트스트랩이 Core 객체 묶음을 넘겨줄 때 쓰는 계약. Gameplay가 UI 타입을 몰라도 연결할 수 있다.
namespace VoxelMagic.Core
{
    public interface IGameContextBinder
    {
        void Bind(GameContext context);
    }
}
