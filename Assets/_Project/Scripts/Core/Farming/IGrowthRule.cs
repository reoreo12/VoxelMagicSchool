// 작물 성장 규칙. 기본은 "물 준 뒤 정한 게임 시간이 지나면(희귀는 다음 날 아침) 자람"(TimedGrowthRule), 튜토리얼은 "물 주면 바로"(InstantGrowthRule).
namespace VoxelMagic.Core
{
    public interface IGrowthRule
    {
        /// <summary>물을 준 직후의 단계.</summary>
        GrowthStage StageAfterWatering(FarmPlot plot);

        /// <summary>지금 시각(누적 분)의 단계. dayStart = 하루가 막 시작된 순간인지.</summary>
        GrowthStage StageAt(FarmPlot plot, float now, bool dayStart);
    }
}
