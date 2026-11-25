// 튜토리얼 규칙: 물을 주면 바로 다 자란다 (프로토타입 1의 규칙, 튜토리얼 모드에서만 쓴다).
namespace VoxelMagic.Core
{
    public sealed class InstantGrowthRule : IGrowthRule
    {
        public GrowthStage StageAfterWatering(FarmPlot plot) => GrowthStage.Grown;
        public GrowthStage StageAt(FarmPlot plot, float now, bool dayStart) => plot.IsWatered ? GrowthStage.Grown : plot.Stage;
    }
}
