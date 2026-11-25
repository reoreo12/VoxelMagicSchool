// 텃밭에 심어진 작물 하나의 상태(씨앗, 차지한 칸, 심은 방향, 물 준 게임 시각, 성장 단계).
using UnityEngine;

namespace VoxelMagic.Core
{
    public enum GrowthStage
    {
        Sprout,
        Grown
    }

    public sealed class FarmPlot
    {
        internal FarmPlot(int id, SeedDefinition seed, Vector2Int origin, FarmFacing facing)
        {
            Id = id;
            Seed = seed;
            Origin = origin;
            Facing = facing;
            Size = FarmPlacement.RotatedSize(seed.Footprint, facing);
            Stage = GrowthStage.Sprout;
        }

        public int Id { get; }
        public SeedDefinition Seed { get; }
        /// <summary>차지한 칸 중 x, y가 가장 작은 칸.</summary>
        public Vector2Int Origin { get; }
        /// <summary>돌린 뒤 차지한 칸 크기.</summary>
        public Vector2Int Size { get; }
        /// <summary>심을 때 바라본 방향 (모양도 이 방향으로 돌아감).</summary>
        public FarmFacing Facing { get; }
        public GrowthStage Stage { get; private set; }
        public bool IsWatered { get; private set; }
        /// <summary>물을 준 게임 시각 (1일 0시부터의 누적 분, GameClock.AbsoluteMinute).</summary>
        public float WateredAt { get; private set; }
        public bool IsGrown => Stage == GrowthStage.Grown;

        public bool Contains(Vector2Int cell)
            => cell.x >= Origin.x && cell.x < Origin.x + Size.x
               && cell.y >= Origin.y && cell.y < Origin.y + Size.y;

        internal void MarkWatered(float at)
        {
            IsWatered = true;
            WateredAt = at;
        }
        internal void SetStage(GrowthStage stage) => Stage = stage;
    }
}
