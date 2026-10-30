// 텃밭에 심어진 작물 하나의 상태(씨앗, 차지한 칸, 성장 단계).
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
        internal FarmPlot(int id, SeedDefinition seed, Vector2Int origin)
        {
            Id = id;
            Seed = seed;
            Origin = origin;
            Size = seed.Footprint;
            Stage = GrowthStage.Sprout;
        }

        public int Id { get; }
        public SeedDefinition Seed { get; }
        /// <summary>차지한 칸 중 x, y가 가장 작은 칸.</summary>
        public Vector2Int Origin { get; }
        public Vector2Int Size { get; }
        public GrowthStage Stage { get; private set; }
        public bool IsWatered { get; private set; }
        public bool IsGrown => Stage == GrowthStage.Grown;

        public bool Contains(Vector2Int cell)
            => cell.x >= Origin.x && cell.x < Origin.x + Size.x
               && cell.y >= Origin.y && cell.y < Origin.y + Size.y;

        internal void MarkWatered() => IsWatered = true;
        internal void SetStage(GrowthStage stage) => Stage = stage;
    }
}
