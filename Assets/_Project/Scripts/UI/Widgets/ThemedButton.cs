// 테마 버튼 모양(평상·마우스 올림·눌림 스프라이트)을 Button의 Sprite Swap으로 입힌다(UI_SPEC 5-4, 10-7).
// Selectable 기반이라 나중에 키보드·게임패드 포커스 이동을 붙일 수 있다(4-A-4). 크기는 스프라이트 원래 크기(정수 px).
using UnityEngine;
using UnityEngine.UI;

namespace VoxelMagic.UI
{
    [RequireComponent(typeof(Button), typeof(Image))]
    public sealed class ThemedButton : ThemedWidget
    {
        [SerializeField] private UiButtonSkin _skin;
        [SerializeField, Min(1), Tooltip("스프라이트 크기 × 이 값 (정수 배만)")] private int _sizeScale = 1;

        public Button Button => GetComponent<Button>();

        public void SetSkin(UiButtonSkin skin)
        {
            _skin = skin;
            ApplyTheme();
        }

        public void SetSizeScale(int scale)
        {
            scale = Mathf.Max(1, scale);
            if (_sizeScale == scale) return;
            _sizeScale = scale;
            ApplyTheme();
        }

        protected override void Apply(UiTheme theme)
        {
            var values = theme.Button(_skin);
            if (values == null || values.Normal == null) return;
            var image = GetComponent<Image>();
            image.sprite = values.Normal;
            image.type = Image.Type.Simple;
            image.raycastTarget = true;
            // 스프라이트 1px = UI 1단위(PPU 16 = 캔버스 기준 PPU 16). SetNativeSize는 캔버스 밖에서 값이 달라 직접 넣는다.
            ((RectTransform)transform).sizeDelta = values.Normal.rect.size * Mathf.Max(1, _sizeScale);

            var button = Button;
            button.transition = Selectable.Transition.SpriteSwap;
            button.targetGraphic = image;
            var state = button.spriteState;
            state.highlightedSprite = values.Hover;
            state.selectedSprite = values.Normal; // 클릭 뒤 선택 상태로 남아 hover처럼 보이지 않게
            state.pressedSprite = values.Pressed;
            state.disabledSprite = values.Normal;
            button.spriteState = state;
        }
    }
}
