// 채집 서식지 하나 (큰 나무 밑 그늘·덤불·바위틈·초입 평지). 재료가 놓일 슬롯(줍기 대상)들을 갖고, 채집 상태가 바뀌면 슬롯 모양을 갱신한다.
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class GatherHabitat : MonoBehaviour, IMountainContextBinder
    {
        [SerializeField] private GatherHabitatKind _kind;
        [SerializeField] private GatherPickup[] _slots = new GatherPickup[0];

        private GatheringService _gathering;

        public GatherHabitatKind Kind => _kind;
        public int SlotCount => _slots.Length;
        public int Index { get; private set; } = -1;

        public void AssignIndex(int index)
        {
            Index = index;
            for (int s = 0; s < _slots.Length; s++) _slots[s].Setup(this, s);
        }

        public void Bind(MountainContext context)
        {
            _gathering = context.Gathering;
            _gathering.Changed += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            if (_gathering != null) _gathering.Changed -= Refresh;
        }

        public GatherMaterialDefinition Get(int slot) => _gathering?.Get(Index, slot);
        public bool CanStore(int slot) => _gathering != null && _gathering.CanStore(Index, slot);
        public bool TryPick(int slot) => _gathering != null && _gathering.TryPick(Index, slot);

        private void Refresh()
        {
            for (int s = 0; s < _slots.Length; s++) _slots[s].Show(_gathering.Get(Index, s));
        }
    }
}
