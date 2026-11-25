// 씨앗 다시 받기 규칙(QUEST_TIMER 7-6): 작물마다 더 줄 수 있는 씨앗 = 받아 둔 의뢰가 요구하는 그 작물 합 − 가진 작물 − 밭에 심긴 그 씨앗(안 거둔 것) − 가진 그 씨앗.
// 몇 번을 받아도 "의뢰를 끝내는 데 필요한 만큼"을 넘지 않는다.
using System.Collections.Generic;

namespace VoxelMagic.Core
{
    public static class SeedRefillRule
    {
        /// <summary>지금 더 줄 수 있는 씨앗 목록 (0개인 씨앗은 빠짐).</summary>
        public static List<ItemAmount> Compute(IEnumerable<RequestDefinition> accepted, Inventory inventory, FarmGrid farm)
        {
            var result = new List<ItemAmount>();
            if (accepted == null || inventory == null) return result;

            // 받아 둔 의뢰들이 주는 씨앗 종류와, 그 작물이 모두 몇 개 필요한지
            var needed = new Dictionary<SeedDefinition, int>();
            foreach (var request in accepted)
            {
                if (request == null) continue;
                foreach (var given in request.GivenItems)
                {
                    if (!(given.Item is SeedDefinition seed) || seed.Produce == null) continue;
                    if (!needed.ContainsKey(seed)) needed[seed] = 0;
                }
            }
            if (needed.Count == 0) return result;

            var seeds = new List<SeedDefinition>(needed.Keys);
            foreach (var seed in seeds)
            {
                int total = 0;
                foreach (var request in accepted)
                {
                    if (request == null) continue;
                    foreach (var need in request.Requirements)
                        if (need.Item == seed.Produce) total += need.Amount;
                }
                int planted = 0;
                if (farm != null)
                    foreach (var plot in farm.Plots)
                        if (plot.Seed == seed) planted++;
                int give = total - inventory.Count(seed.Produce) - planted - inventory.Count(seed);
                if (give > 0) result.Add(new ItemAmount(seed, give));
            }
            return result;
        }
    }
}
