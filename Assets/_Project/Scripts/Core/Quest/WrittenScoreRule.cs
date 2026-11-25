// 의뢰 필기 점수 규칙(QUEST_TIMER 2-7): 필요 재료(개수 포함)의 희귀도 점수 합. 처음 가져온 재료 보너스는 없다.
namespace VoxelMagic.Core
{
    public static class WrittenScoreRule
    {
        public static int Score(RequestDefinition request, QuestConfig config)
        {
            if (request == null || config == null) return 0;
            int total = 0;
            foreach (var need in request.Requirements)
                if (need.Item != null) total += config.ScoreOf(need.Item.Rarity) * System.Math.Max(0, need.Amount);
            return total;
        }
    }
}
