// 이번 프로토타입 규칙: 물을 주면 바로 다 자란다.
namespace VoxelMagic.Core
{
    public sealed class InstantGrowthRule : IGrowthRule
    {
        public GrowthStage StageAfterWatering(FarmPlot plot) => GrowthStage.Grown;
    }
}
