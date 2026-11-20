// 테마 안의 위젯 모양 묶음: 버튼(3상태 스프라이트), 패널(스프라이트 + Sliced/Tiled), 글자 스타일(폰트·배수·색).
// 위젯은 이 이름(enum)만 가지고 모양은 테마에서 가져온다 → 스프라이트를 바꿔도 화면 코드는 그대로(UI_SPEC 1장 교체 대비).
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VoxelMagic.UI
{
    public enum UiButtonSkin
    {
        MenuBook,
        MenuBag,
        ArrowDown,
        ArrowUp,
        Close
    }

    public enum UiPanelSkin
    {
        QuestBar,
        QuestPanel,
        QuestDetail,
        QuestToast,
        ScrollTrack,
        ScrollHandle
    }

    public enum UiTextStyle
    {
        Body,
        BodyBold,
        Title
    }

    [Serializable]
    public sealed class ButtonSkinValues
    {
        [SerializeField] private UiButtonSkin _id;
        [SerializeField] private Sprite _normal;
        [SerializeField] private Sprite _hover;
        [SerializeField] private Sprite _pressed;

        public ButtonSkinValues(UiButtonSkin id, Sprite normal, Sprite hover, Sprite pressed)
        {
            _id = id;
            _normal = normal;
            _hover = hover;
            _pressed = pressed;
        }

        public UiButtonSkin Id => _id;
        public Sprite Normal => _normal;
        public Sprite Hover => _hover;
        public Sprite Pressed => _pressed;
    }

    [Serializable]
    public sealed class PanelSkinValues
    {
        [SerializeField] private UiPanelSkin _id;
        [SerializeField] private Sprite _sprite;
        [SerializeField, Tooltip("9-slice는 Sliced, 찢어진 종이 패널은 Tiled (UI_SPEC 4장, 10-7)")] private Image.Type _imageType = Image.Type.Sliced;

        public PanelSkinValues(UiPanelSkin id, Sprite sprite, Image.Type imageType)
        {
            _id = id;
            _sprite = sprite;
            _imageType = imageType;
        }

        public UiPanelSkin Id => _id;
        public Sprite Sprite => _sprite;
        public Image.Type ImageType => _imageType;
    }

    [Serializable]
    public sealed class TextStyleValues
    {
        [SerializeField] private UiTextStyle _id;
        [SerializeField] private bool _bold;
        [SerializeField, Min(1), Tooltip("글자 크기 = 폰트 설계 크기 × 이 값 (정수 배만)")] private int _sizeMultiplier = 1;
        [SerializeField] private Color _color = Color.black;

        public TextStyleValues(UiTextStyle id, bool bold, int sizeMultiplier, Color color)
        {
            _id = id;
            _bold = bold;
            _sizeMultiplier = sizeMultiplier;
            _color = color;
        }

        public UiTextStyle Id => _id;
        public bool Bold => _bold;
        public int SizeMultiplier => _sizeMultiplier;
        public Color Color => _color;

        public TMP_FontAsset FontFrom(UiTheme.FontValues fonts) => _bold && fonts.Bold != null ? fonts.Bold : fonts.Regular;
        public int SizeFrom(UiTheme.FontValues fonts) => fonts.DesignSize * Math.Max(1, _sizeMultiplier);
    }
}
