// (아이템, 개수) 쌍. 의뢰의 필요 아이템과 지급 씨앗에 쓴다.
using System;
using UnityEngine;

namespace VoxelMagic.Core
{
    [Serializable]
    public struct ItemAmount
    {
        [SerializeField] private ItemDefinition _item;
        [SerializeField, Min(1)] private int _amount;

        public ItemAmount(ItemDefinition item, int amount)
        {
            _item = item;
            _amount = amount;
        }

        public ItemDefinition Item => _item;
        public int Amount => _amount;
    }
}
