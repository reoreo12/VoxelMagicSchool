// 창밖 배경 세트(낮/밤). 세트만 바꾸면 배경이 바뀌도록 스프라이트와 하늘 대체 색을 묶는다.
using UnityEngine;

namespace VoxelMagic.Core
{
    [CreateAssetMenu(menuName = "VoxelMagic/SideScroll/Backdrop Set", fileName = "BackdropSet")]
    public sealed class BackdropSet : ScriptableObject
    {
        [SerializeField] private Sprite _sprite;
        [Tooltip("배경이 덮지 못한 곳에 보이는 카메라 배경색")]
        [SerializeField] private Color _skyColor = new Color(0.55f, 0.78f, 0.95f);

        public Sprite Sprite => _sprite;
        public Color SkyColor => _skyColor;
    }
}
