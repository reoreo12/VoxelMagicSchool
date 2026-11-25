// 기본 성장 규칙: 물을 준 뒤 씨앗의 GrowHours(게임 시간)가 지나면 다 자란다. GrowHours가 0인 작물(희귀)은 다음 날 하루 시작에 자란다.
// 게임 시계가 멈추면 성장도 멈춘다(게임 시각 기준). 물은 한 번만 주면 된다.
namespace VoxelMagic.Core
{
    public sealed class TimedGrowthRule : IGrowthRule
    {
        public GrowthStage StageAfterWatering(FarmPlot plot) => plot.Stage;

        public GrowthStage StageAt(FarmPlot plot, float now, bool dayStart)
        {
            if (!plot.IsWatered || plot.IsGrown) return plot.Stage;
            if (plot.Seed.GrowsNextDay) return dayStart ? GrowthStage.Grown : plot.Stage;
            return now >= plot.WateredAt + plot.Seed.GrowHours * 60f ? GrowthStage.Grown : plot.Stage;
        }
    }
}
