// 사람형이 아닌 모델(몬스터 등)용: 모델 안에 둔 Transform을 이름으로 찾는다.
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class TransformSocketSource : MonoBehaviour, ISocketSource
    {
        public bool TryGetAnchor(SocketBinding binding, out Transform anchor)
        {
            anchor = null;
            if (binding == null || binding.UsesHumanBone || string.IsNullOrEmpty(binding.TransformName)) return false;
            anchor = FindDeep(transform, binding.TransformName);
            return anchor != null;
        }

        private static Transform FindDeep(Transform parent, string name)
        {
            if (parent.name == name) return parent;
            for (int i = 0; i < parent.childCount; i++)
            {
                var found = FindDeep(parent.GetChild(i), name);
                if (found != null) return found;
            }
            return null;
        }
    }
}
