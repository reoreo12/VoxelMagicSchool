// 크리처에게 전달해 본 재료 종류 기록. 처음 전달하는 재료에 추가 점수를 주는 기준이 된다.
using System.Collections.Generic;

namespace VoxelMagic.Core
{
    public sealed class MaterialJournal
    {
        private readonly HashSet<string> _deliveredIds = new HashSet<string>();

        public int Count => _deliveredIds.Count;

        public bool HasDelivered(ItemDefinition item) => item != null && _deliveredIds.Contains(item.Id);

        public IEnumerable<string> DeliveredIds => _deliveredIds;

        /// <summary>기록하고, 처음 기록한 종류면 true.</summary>
        public bool Record(ItemDefinition item) => item != null && _deliveredIds.Add(item.Id);

        /// <summary>저장 파일에서 불러온 ID 목록으로 바꾼다.</summary>
        internal void Restore(IEnumerable<string> ids)
        {
            _deliveredIds.Clear();
            if (ids == null) return;
            foreach (var id in ids)
                if (!string.IsNullOrEmpty(id)) _deliveredIds.Add(id);
        }
    }
}
