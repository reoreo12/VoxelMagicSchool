// 무작위 값 공급 인터페이스. 테스트에서 고정 값으로 바꿔 끼울 수 있게 분리한다.
namespace VoxelMagic.Core
{
    public interface IRandom
    {
        /// <summary>0 이상 1 미만의 값.</summary>
        float Value { get; }
        float Range(float min, float max);
        int Range(int minInclusive, int maxExclusive);
    }
}
