// 소켓에 붙는 장착물(지팡이·소품·이펙트)의 공통 기반: 붙을 때·뗄 때 알림과 소유 캐릭터 참조.
using UnityEngine;

namespace VoxelMagic.Gameplay
{
    public abstract class AttachmentBase : MonoBehaviour
    {
        public GameObject Owner { get; private set; }

        internal void NotifyAttached(GameObject owner)
        {
            Owner = owner;
            OnAttached();
        }

        internal void NotifyDetached()
        {
            OnDetached();
            Owner = null;
        }

        protected virtual void OnAttached() { }
        protected virtual void OnDetached() { }
    }
}
