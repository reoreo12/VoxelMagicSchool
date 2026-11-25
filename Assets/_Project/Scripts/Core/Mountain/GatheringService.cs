// 뒷산 채집 규칙: 씬의 서식지 슬롯을 오늘의 채집 자리(GatherDayState)에 이어 주고, 줍기(가방에 넣고 획득 정보 띄우기)를 한다.
// 자리 번호 = 같은 종류 서식지들에 번갈아 매긴 순번(각 서식지 0번 슬롯부터). 재료 배치는 하루 시작 때 진행 상태 쪽에서 정해지고 여기서는 바꾸지 않는다.
using System;
using System.Collections.Generic;

namespace VoxelMagic.Core
{
    public sealed class GatheringService
    {
        private readonly GatherDayState _day;
        private readonly Inventory _inventory;
        private readonly GameMessages _messages;
        private readonly IRandom _random;
        private readonly GatherHabitatKind[] _kinds;
        private readonly int[][] _spots; // [서식지][슬롯] → 자리 번호
        private readonly int[] _slotCounts;

        /// <param name="kinds">씬 서식지마다 종류 (서식지 번호 순)</param>
        /// <param name="slotCounts">씬 서식지마다 슬롯 수</param>
        public GatheringService(GatherDayState day, IReadOnlyList<GatherHabitatKind> kinds, IReadOnlyList<int> slotCounts,
            Inventory inventory, GameMessages messages, IRandom random)
        {
            _day = day ?? throw new ArgumentNullException(nameof(day));
            _inventory = inventory;
            _messages = messages;
            _random = random;
            _kinds = new GatherHabitatKind[kinds.Count];
            _slotCounts = new int[kinds.Count];
            _spots = new int[kinds.Count][];
            var totals = new Dictionary<GatherHabitatKind, int>();
            for (int h = 0; h < kinds.Count; h++)
            {
                _kinds[h] = kinds[h];
                _slotCounts[h] = slotCounts[h];
                _spots[h] = new int[slotCounts[h]];
            }

            // 자리 번호는 같은 종류 서식지들에 번갈아 매긴다(각 서식지 0번 슬롯 → 1번 슬롯 …). 서식지가 많아도 자리가 고루 퍼진다.
            int maxSlots = 0;
            foreach (int count in slotCounts) maxSlots = Math.Max(maxSlots, count);
            for (int s = 0; s < maxSlots; s++)
                for (int h = 0; h < kinds.Count; h++)
                {
                    if (s >= _slotCounts[h]) continue;
                    totals.TryGetValue(_kinds[h], out int next);
                    _spots[h][s] = next;
                    totals[_kinds[h]] = next + 1;
                }
            SceneSpotCounts = totals;
            _day.Changed += OnDayChanged;
        }

        public event Action Changed;

        /// <summary>씬에서 센 서식지 종류별 슬롯 합계 (설정의 자리 수보다 적으면 일부 자리는 씬에 없다).</summary>
        public IReadOnlyDictionary<GatherHabitatKind, int> SceneSpotCounts { get; }
        public GatherDayState Day => _day;

        /// <summary>씬이 내려갈 때 부른다 (하루 상태는 씬을 넘어 유지되므로 구독을 푼다).</summary>
        public void Dispose() => _day.Changed -= OnDayChanged;

        private void OnDayChanged() => Changed?.Invoke();

        public GatherHabitatKind KindOf(int habitat) => _kinds[habitat];
        /// <summary>서식지 슬롯의 자리 번호. 설정의 자리 수를 넘으면 -1 (쓰지 않는 슬롯).</summary>
        public int SpotOf(int habitat, int slot)
        {
            if (habitat < 0 || habitat >= _kinds.Length || slot < 0 || slot >= _slotCounts[habitat]) return -1;
            int spot = _spots[habitat][slot];
            return spot < _day.SpotsPerKind ? spot : -1;
        }

        public GatherMaterialDefinition Get(int habitat, int slot)
        {
            int spot = SpotOf(habitat, slot);
            return spot < 0 ? null : _day.Get(_kinds[habitat], spot);
        }

        public int CountOf(GatherMaterialDefinition material) => _day.CountOf(material);

        public bool CanStore(int habitat, int slot)
        {
            var material = Get(habitat, slot);
            return material != null && _inventory.CanAdd(material, 1);
        }

        public bool TryPick(int habitat, int slot)
        {
            var material = Get(habitat, slot);
            if (material == null) return false;
            if (!_inventory.TryAdd(material, 1))
            {
                _messages.ShowToast(GameTexts.InventoryFull);
                return false;
            }
            _day.Take(_kinds[habitat], SpotOf(habitat, slot));
            _messages.ShowAcquired(new AcquiredInfo(material, material.SizeRangeCm.Roll(_random)));
            return true;
        }
    }
}
