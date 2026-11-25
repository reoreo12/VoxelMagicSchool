// 하루 시작 때 채집 자리를 뽑는다(QUEST_TIMER 3-2): 서식지 종류마다 자리 N곳 중 M곳을 무작위로 켜고, 켜진 자리마다 희귀도 확률로 그 서식지의 재료를 고른다.
// 의뢰 재료가 나온다는 보장은 없다. 튜토리얼처럼 꼭 나와야 하는 재료는 guaranteed로 넘기면 그 서식지의 켜진 자리 하나를 그 재료로 바꾼다.
using System.Collections.Generic;

namespace VoxelMagic.Core
{
    public static class GatherDayRoller
    {
        public static Dictionary<GatherHabitatKind, GatherMaterialDefinition[]> Roll(GatheringConfig config, IRandom random,
            IEnumerable<GatherMaterialDefinition> guaranteed = null)
        {
            int spots = config.SpotsPerKind;
            int active = config.ActiveSpotsPerKind;
            var result = new Dictionary<GatherHabitatKind, GatherMaterialDefinition[]>();
            var order = new List<int>();

            foreach (GatherHabitatKind kind in System.Enum.GetValues(typeof(GatherHabitatKind)))
            {
                var slots = new GatherMaterialDefinition[spots];
                result[kind] = slots;
                var pool = new List<GatherMaterialDefinition>();
                foreach (var material in config.Materials)
                    if (material != null && material.Habitat == kind) pool.Add(material);
                if (pool.Count == 0) continue;

                // 자리 번호를 섞어 앞에서 M개를 켠다.
                order.Clear();
                for (int i = 0; i < spots; i++) order.Add(i);
                for (int i = order.Count - 1; i > 0; i--)
                {
                    int j = random.Range(0, i + 1);
                    (order[i], order[j]) = (order[j], order[i]);
                }
                for (int i = 0; i < active; i++)
                {
                    var rarity = config.Rarity.Roll(random);
                    var candidates = RarityTable.CandidatesOf(pool, rarity);
                    slots[order[i]] = candidates[random.Range(0, candidates.Count)];
                }
            }

            if (guaranteed != null)
                foreach (var material in guaranteed)
                    Guarantee(result[material.Habitat], material, random);
            return result;
        }

        // 이미 있으면 그대로, 없으면 켜진 자리 하나(없으면 아무 자리)를 그 재료로.
        private static void Guarantee(GatherMaterialDefinition[] slots, GatherMaterialDefinition material, IRandom random)
        {
            if (material == null || System.Array.IndexOf(slots, material) >= 0) return;
            var filled = new List<int>();
            for (int i = 0; i < slots.Length; i++)
                if (slots[i] != null) filled.Add(i);
            int spot = filled.Count > 0 ? filled[random.Range(0, filled.Count)] : random.Range(0, slots.Length);
            slots[spot] = material;
        }
    }
}
