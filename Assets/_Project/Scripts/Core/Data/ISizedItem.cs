// 크기 범위가 있는 아이템(작물, 물고기)의 공통 계약. 얻을 때 이 범위에서 크기를 굴린다.
namespace VoxelMagic.Core
{
    public interface ISizedItem
    {
        FloatRange SizeRangeCm { get; }
    }
}
