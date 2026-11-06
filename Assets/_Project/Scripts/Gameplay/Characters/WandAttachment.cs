// 지팡이 장착물: 끝(Tip) 위치를 제공한다. 메시는 프리팹에서 바꾸면 되고 코드는 그대로다.
using UnityEngine;

namespace VoxelMagic.Gameplay
{
    public sealed class WandAttachment : AttachmentBase, ITipProvider
    {
        [SerializeField] private Transform _tip;

        public Transform Tip => _tip;
    }
}
