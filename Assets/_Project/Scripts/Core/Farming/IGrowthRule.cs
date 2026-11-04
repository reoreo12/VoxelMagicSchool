// 작물 성장 규칙. 지금은 "물 주면 즉시 성장"이고, 나중에 날짜 기반 규칙으로 바꿔 끼운다.
namespace VoxelMagic.Core
{
    public interface IGrowthRule
    {
        GrowthStage StageAfterWatering(FarmPlot plot);
    }
}
