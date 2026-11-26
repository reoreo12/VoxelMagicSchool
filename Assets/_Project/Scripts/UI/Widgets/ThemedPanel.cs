// 테마 패널 모양(9-slice Sliced 또는 찢어진 종이 Tiled)을 Image에 입힌다(UI_SPEC 4장, 10-7). 크기는 배치하는 쪽이 정수 px로 정한다.
// 패널은 기본으로 클릭을 받지 않는다. 바깥 클릭 막기 등 입력이 필요한 곳만 켠다.
using UnityEngine;
using UnityEngine.UI;

namespace VoxelMagic.UI
{
    [RequireComponent(typeof(Image))]
    public sealed class ThemedPanel : ThemedWidget
    {
        [SerializeField] private UiPanelSkin _skin;
        [SerializeField, Tooltip("입력을 받아야 하는 패널만 켠다(4-A-5)")] private bool _raycastTarget;

        public void SetSkin(UiPanelSkin skin)
        {
            _skin = skin;
            ApplyTheme();
        }

        /// <summary>입력을 받을지 바꾼다(예: 스크롤바 트랙·손잡이는 끌기·누르기를 받아야 한다).</summary>
        public void SetRaycastTarget(bool on)
        {
            _raycastTarget = on;
            ApplyTheme();
        }

        /// <summary>스크롤바 트랙·손잡이가 마우스를 받게 한다. 패널은 기본으로 입력을 받지 않아 손잡이를 끌 수 없었다.</summary>
        public static void EnableScrollbarInput(Scrollbar scrollbar)
        {
            if (scrollbar == null) return;
            foreach (var panel in scrollbar.GetComponentsInChildren<ThemedPanel>(true))
                if (!panel._raycastTarget) panel.SetRaycastTarget(true);
        }

        protected override void Apply(UiTheme theme)
        {
            var values = theme.Panel(_skin);
            if (values == null || values.Sprite == null) return;
            var image = GetComponent<Image>();
            image.sprite = values.Sprite;
            image.type = values.ImageType;
            image.fillCenter = true;
            image.pixelsPerUnitMultiplier = 1f;
            image.raycastTarget = _raycastTarget;
        }
    }
}
