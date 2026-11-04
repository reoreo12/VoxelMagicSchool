// 씬의 상호작용 대상 목록. 레이어 설정 없이 반경 안에서 가장 가까운 대상 하나를 찾는다.
using System.Collections.Generic;
using UnityEngine;

namespace VoxelMagic.Gameplay
{
    public sealed class InteractableRegistry
    {
        private readonly List<IInteractable> _items = new List<IInteractable>();

        public int Count => _items.Count;

        public void Add(IInteractable item)
        {
            if (item != null && !_items.Contains(item)) _items.Add(item);
        }

        public void Remove(IInteractable item) => _items.Remove(item);

        public IInteractable FindNearest(Vector3 position, float radius)
        {
            IInteractable best = null;
            float bestSqr = radius * radius;
            foreach (var item in _items)
            {
                if (!item.CanBeTargeted) continue;
                var offset = item.ClosestPoint(position) - position;
                offset.y = 0f;
                float sqr = offset.sqrMagnitude;
                if (sqr > bestSqr) continue;
                bestSqr = sqr;
                best = item;
            }
            return best;
        }
    }
}
