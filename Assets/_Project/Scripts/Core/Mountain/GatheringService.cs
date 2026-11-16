// 뒷산 채집 규칙: 재료 다시 배치(시작·새 하루·시간대 전환), 줍기(가방에 넣고 획득 정보 띄우기). 바뀌면 Changed로 알린다.
using System;
using System.Collections.Generic;

namespace VoxelMagic.Core
{
    public sealed class GatheringService
    {
        private readonly GatherSpawner _spawner;
        private readonly IReadOnlyList<GatherMaterialDefinition> _pool;
        private readonly GatheringConfig _config;
        private readonly TimeOfDayState _time;
        private readonly Inventory _inventory;
        private readonly GameMessages _messages;
        private readonly RequestTracker _requests;
        private readonly IRandom _random;

        public GatheringService(GatherSpawner spawner, IReadOnlyList<GatherMaterialDefinition> pool, GatheringConfig config,
            TimeOfDayState time, Inventory inventory, GameMessages messages, RequestTracker requests, IRandom random)
        {
            _spawner = spawner;
            _pool = pool;
            _config = config;
            _time = time;
            _inventory = inventory;
            _messages = messages;
            _requests = requests;
            _random = random;
            _time.Changed += Respawn;
            if (_requests != null) _requests.ActiveChanged += OnRequestChanged;
        }

        /// <summary>씬이 내려갈 때 부른다 (의뢰 추적은 씬을 넘어 유지되므로 구독을 푼다).</summary>
        public void Dispose()
        {
            _time.Changed -= Respawn;
            if (_requests != null) _requests.ActiveChanged -= OnRequestChanged;
        }

        // 새 의뢰를 받으면 필요한 재료가 지금 맵에 최소 1개 있도록 채운다 (다른 재료는 그대로).
        private void OnRequestChanged(RequestDefinition active)
        {
            if (active == null) return;
            _spawner.EnsureRequired(RequiredItems(), _time.Time, _random);
            Changed?.Invoke();
        }

        public event Action Changed;

        public GatherMaterialDefinition Get(int habitat, int slot) => _spawner.Get(habitat, slot);
        public int CountOf(GatherMaterialDefinition material) => _spawner.CountOf(material);

        public bool CanStore(int habitat, int slot)
        {
            var material = _spawner.Get(habitat, slot);
            return material != null && _inventory.CanAdd(material, 1);
        }

        /// <summary>새 하루 시작·시간대 전환 때 부른다. 지금은 디버그 키로도 부른다.</summary>
        public void Respawn()
        {
            _spawner.Respawn(_pool, _time.Time, RequiredItems(), _config, _random);
            Changed?.Invoke();
        }

        public bool TryPick(int habitat, int slot)
        {
            var material = _spawner.Get(habitat, slot);
            if (material == null) return false;
            if (!_inventory.TryAdd(material, 1))
            {
                _messages.ShowToast(GameTexts.InventoryFull);
                return false;
            }
            _spawner.Take(habitat, slot);
            _messages.ShowAcquired(new AcquiredInfo(material, material.SizeRangeCm.Roll(_random)));
            Changed?.Invoke();
            return true;
        }

        private IEnumerable<ItemDefinition> RequiredItems()
        {
            if (_requests == null || !_requests.HasActive) yield break;
            foreach (var need in _requests.Active.Requirements)
                yield return need.Item;
        }
    }
}
