// 크리처에게 전달해 본 재료 종류 기록. 처음 전달하는 재료에 추가 점수를 주는 기준이 된다.
using System.Collections.Generic;

namespace VoxelMagic.Core
{
    public sealed class MaterialJournal
    {
        private readonly HashSet<string> _deliveredIds = new HashSet<string>();

        public int Count => _deliveredIds.Count;

        public bool HasDelivered(ItemDefinition item) => item != null && _deliveredIds.Contains(item.Id);

        /// <summary>기록하고, 처음 기록한 종류면 true.</summary>
        public bool Record(ItemDefinition item) => item != null && _deliveredIds.Add(item.Id);
    }
}
