// Humanoid 아바타의 본을 Animator.GetBoneTransform으로 찾는다. 모델(FBX)이 달라도 같은 본 이름(오른손 등)으로 찾을 수 있다.
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    [RequireComponent(typeof(Animator))]
    public sealed class HumanoidSocketSource : MonoBehaviour, ISocketSource
    {
        private Animator _animator;

        public bool TryGetAnchor(SocketBinding binding, out Transform anchor)
        {
            anchor = null;
            if (binding == null || !binding.UsesHumanBone) return false;
            if (_animator == null) _animator = GetComponent<Animator>();
            if (_animator == null || !_animator.isHuman) return false;
            anchor = _animator.GetBoneTransform(binding.HumanBone);
            return anchor != null;
        }
    }
}
