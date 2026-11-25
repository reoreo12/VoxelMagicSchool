// 희귀도 확률표(흔함·보통·희귀 가중치)와 뽑기. 채집 자리·낚시가 같이 쓰고, 고른 희귀도의 후보가 없으면 가까운 희귀도로 내려간다.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelMagic.Core
{
    [Serializable]
    public struct RarityTable
    {
        [SerializeField, Min(0f)] private float _common;
        [SerializeField, Min(0f)] private float _uncommon;
        [SerializeField, Min(0f)] private float _rare;

        public RarityTable(float common, float uncommon, float rare)
        {
            _common = common;
            _uncommon = uncommon;
            _rare = rare;
        }

        /// <summary>지침 기본값 60 / 30 / 10.</summary>
        public static RarityTable Default => new RarityTable(60f, 30f, 10f);

        public float Weight(ItemRarity rarity) => rarity switch
        {
            ItemRarity.Rare => Mathf.Max(0f, _rare),
            ItemRarity.Uncommon => Mathf.Max(0f, _uncommon),
            _ => Mathf.Max(0f, _common)
        };

        public float Total => Weight(ItemRarity.Common) + Weight(ItemRarity.Uncommon) + Weight(ItemRarity.Rare);

        public ItemRarity Roll(IRandom random)
        {
            float total = Total;
            if (total <= 0f) return ItemRarity.Common;
            float roll = random.Value * total;
            if ((roll -= Weight(ItemRarity.Common)) < 0f) return ItemRarity.Common;
            if ((roll -= Weight(ItemRarity.Uncommon)) < 0f) return ItemRarity.Uncommon;
            return ItemRarity.Rare;
        }

        /// <summary>후보 중 그 희귀도의 것. 없으면 한 단계씩 낮은 희귀도, 그래도 없으면 높은 쪽에서 찾는다. 후보가 비면 빈 목록.</summary>
        public static List<T> CandidatesOf<T>(IEnumerable<T> pool, ItemRarity rarity) where T : ItemDefinition
        {
            var all = new List<T>();
            foreach (var item in pool)
                if (item != null) all.Add(item);
            for (int r = (int)rarity; r >= 0; r--)
            {
                var found = all.FindAll(i => (int)i.Rarity == r);
                if (found.Count > 0) return found;
            }
            for (int r = (int)rarity + 1; r <= (int)ItemRarity.Rare; r++)
            {
                var found = all.FindAll(i => (int)i.Rarity == r);
                if (found.Count > 0) return found;
            }
            return all;
        }
    }
}
