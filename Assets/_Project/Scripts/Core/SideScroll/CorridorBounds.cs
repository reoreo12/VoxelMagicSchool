// 복도가 차지하는 x 범위와 높이(유닛). 카메라 제한과 캐릭터 이동 범위의 기준.
namespace VoxelMagic.Core
{
    public readonly struct CorridorBounds
    {
        public CorridorBounds(float minX, float maxX, float height)
        {
            MinX = minX;
            MaxX = maxX;
            Height = height;
        }

        public float MinX { get; }
        public float MaxX { get; }
        public float Height { get; }
        public float Width => MaxX - MinX;
        public float CenterX => (MinX + MaxX) * 0.5f;
    }
}
