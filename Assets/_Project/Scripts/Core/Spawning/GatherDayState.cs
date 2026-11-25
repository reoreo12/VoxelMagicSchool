// 오늘의 채집 자리: 서식지 종류마다 자리 N개 중 켜진 자리에 놓인 재료. 하루 시작 때 GatherDayRoller가 채우고, 씬을 넘어 하루 동안 유지된다(주운 자리는 그날 다시 안 생김).
using System;
using System.Collections.Generic;

namespace VoxelMagic.Core
{
    public sealed class GatherDayState
    {
        private readonly Dictionary<GatherHabitatKind, GatherMaterialDefinition[]> _spots = new Dictionary<GatherHabitatKind, GatherMaterialDefinition[]>();

        public GatherDayState(int spotsPerKind)
        {
            SpotsPerKind = Math.Max(1, spotsPerKind);
            foreach (GatherHabitatKind kind in Enum.GetValues(typeof(GatherHabitatKind)))
                _spots[kind] = new GatherMaterialDefinition[SpotsPerKind];
        }

        /// <summary>재료가 새로 놓이거나 주워졌을 때.</summary>
        public event Action Changed;

        public int SpotsPerKind { get; }
        /// <summary>마지막으로 뽑은 날 (0 = 아직 없음).</summary>
        public int Day { get; private set; }

        public GatherMaterialDefinition Get(GatherHabitatKind kind, int spot)
            => spot >= 0 && spot < SpotsPerKind ? _spots[kind][spot] : null;

        public int CountOf(GatherMaterialDefinition material)
        {
            int count = 0;
            foreach (var spots in _spots.Values)
                foreach (var placed in spots)
                    if (placed != null && placed == material) count++;
            return count;
        }

        /// <summary>자리에서 재료를 꺼낸다 (빈 자리면 null).</summary>
        public GatherMaterialDefinition Take(GatherHabitatKind kind, int spot)
        {
            var material = Get(kind, spot);
            if (material == null) return null;
            _spots[kind][spot] = null;
            Changed?.Invoke();
            return material;
        }

        /// <summary>하루치를 통째로 바꾼다 (뽑기·불러오기).</summary>
        public void Set(int day, IReadOnlyDictionary<GatherHabitatKind, GatherMaterialDefinition[]> spots)
        {
            Day = day;
            foreach (var pair in _spots)
            {
                Array.Clear(pair.Value, 0, pair.Value.Length);
                if (spots == null || !spots.TryGetValue(pair.Key, out var source) || source == null) continue;
                Array.Copy(source, pair.Value, Math.Min(source.Length, pair.Value.Length));
            }
            Changed?.Invoke();
        }
    }
}
