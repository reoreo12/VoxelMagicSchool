// 서식지 슬롯마다 어떤 재료가 놓였는지 관리한다. 다시 배치할 때 자기 서식지·현재 시간대 재료만 서식지 종류별 개수(GatheringConfig)만큼 고르고,
// 직전에 비어 있던 슬롯을 먼저 써서 위치가 바뀌게 하며, 의뢰에 필요한 재료는 맵 전체에 최소 1개를 보장한다.
using System.Collections.Generic;

namespace VoxelMagic.Core
{
    public sealed class GatherSpawner
    {
        private readonly GatherHabitatKind[] _kinds;
        private readonly GatherMaterialDefinition[][] _slots;
        private readonly List<GatherMaterialDefinition> _candidates = new List<GatherMaterialDefinition>();
        private readonly List<int> _order = new List<int>();

        public GatherSpawner(IReadOnlyList<GatherHabitatKind> kinds, IReadOnlyList<int> slotCounts)
        {
            _kinds = new GatherHabitatKind[kinds.Count];
            _slots = new GatherMaterialDefinition[kinds.Count][];
            for (int h = 0; h < kinds.Count; h++)
            {
                _kinds[h] = kinds[h];
                _slots[h] = new GatherMaterialDefinition[slotCounts[h]];
            }
        }

        public int HabitatCount => _kinds.Length;
        public GatherHabitatKind KindOf(int habitat) => _kinds[habitat];
        public int SlotCount(int habitat) => _slots[habitat].Length;
        public GatherMaterialDefinition Get(int habitat, int slot) => _slots[habitat][slot];

        public int CountOf(GatherMaterialDefinition material)
        {
            int count = 0;
            foreach (var habitat in _slots)
                foreach (var placed in habitat)
                    if (placed == material) count++;
            return count;
        }

        /// <summary>슬롯에서 재료를 꺼낸다 (빈 슬롯이면 null).</summary>
        public GatherMaterialDefinition Take(int habitat, int slot)
        {
            var material = _slots[habitat][slot];
            _slots[habitat][slot] = null;
            return material;
        }

        public void Respawn(IReadOnlyList<GatherMaterialDefinition> pool, TimeOfDay time,
            IEnumerable<ItemDefinition> required, GatheringConfig balance, IRandom random)
        {
            for (int h = 0; h < _slots.Length; h++)
                RespawnHabitat(h, pool, time, balance.For(_kinds[h]), random);
            EnsureRequired(required, time, random);
        }

        /// <summary>지금 시간대에 나올 수 있는 의뢰 재료가 맵에 하나도 없으면 하나 놓는다 (나머지는 그대로).</summary>
        public void EnsureRequired(IEnumerable<ItemDefinition> required, TimeOfDay time, IRandom random)
        {
            if (required == null) return;
            foreach (var item in required)
                if (item is GatherMaterialDefinition material && material.AppearsAt(time) && CountOf(material) == 0)
                    Guarantee(material, random);
        }

        private void RespawnHabitat(int h, IReadOnlyList<GatherMaterialDefinition> pool, TimeOfDay time,
            HabitatBalance balance, IRandom random)
        {
            var slots = _slots[h];
            _candidates.Clear();
            foreach (var material in pool)
                if (material != null && material.Habitat == _kinds[h] && material.AppearsAt(time))
                    _candidates.Add(material);

            // 직전에 비어 있던 슬롯을 앞에, 차 있던 슬롯을 뒤에 (각각 섞음) → 위치가 바뀐다.
            _order.Clear();
            AddShuffled(slots, empty: true, random);
            AddShuffled(slots, empty: false, random);
            for (int s = 0; s < slots.Length; s++) slots[s] = null;
            if (_candidates.Count == 0 || slots.Length == 0) return;

            // 서식지 종류별 밸런스: 최소 + 확률로 더한 개수 (0개도 가능)
            int target = balance.RollCount(slots.Length, random);
            for (int i = 0; i < target; i++)
                slots[_order[i]] = _candidates[random.Range(0, _candidates.Count)];
        }

        private void AddShuffled(GatherMaterialDefinition[] slots, bool empty, IRandom random)
        {
            int start = _order.Count;
            for (int s = 0; s < slots.Length; s++)
                if ((slots[s] == null) == empty) _order.Add(s);
            for (int i = _order.Count - 1; i > start; i--)
            {
                int j = random.Range(start, i + 1);
                (_order[i], _order[j]) = (_order[j], _order[i]);
            }
        }

        private void Guarantee(GatherMaterialDefinition material, IRandom random)
        {
            var habitats = new List<int>();
            for (int h = 0; h < _kinds.Length; h++)
                if (_kinds[h] == material.Habitat && _slots[h].Length > 0) habitats.Add(h);
            if (habitats.Count == 0) return;

            var slots = _slots[habitats[random.Range(0, habitats.Count)]];
            int slot = System.Array.IndexOf(slots, null);
            if (slot < 0) slot = random.Range(0, slots.Length);
            slots[slot] = material;
        }
    }
}
