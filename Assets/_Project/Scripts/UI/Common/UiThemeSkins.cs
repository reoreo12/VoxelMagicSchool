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
        ScrollHandle,
        /// <summary>퀵슬롯 바 배경 (Sprout 어두운 칸 9-slice, 7-1)</summary>
        QuickSlotBar,
        /// <summary>퀵슬롯 칸 (가장 밝은 칸)</summary>
        QuickSlot,
        /// <summary>숨겨진 칸 등 다른 밝기의 칸 (확장 인벤토리용 예비)</summary>
        QuickSlotAlt,
        /// <summary>세로 상태 바 프레임 (7-2)</summary>
        StatusBarFrame
    }

    public enum UiTextStyle
    {
        Body,
        BodyBold,
        Title,
        /// <summary>작은 글씨 (갈무리9 10px). HUD 바 제목 등.</summary>
        Small,
        /// <summary>더 작은 글씨 (갈무리7 8px).</summary>
        Tiny,
        /// <summary>HUD 바 제목 (굵게, 띄어쓰기 조금 넓게).</summary>
        HudTitle,
        /// <summary>게임 화면 위에 바로 놓이는 HUD 글자 (점수 이름 등). 배경 없이 읽히는 색.</summary>
        HudLabel
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
        [SerializeField, Tooltip("비우면 테마 기본 폰트(굵게면 Bold). 채우면 이 폰트를 쓴다 (예: 작은 글씨용 갈무리9)")] private TMP_FontAsset _fontOverride;
        [SerializeField, Min(1), Tooltip("글자 크기 = 폰트 설계 크기 × 이 값 (정수 배만)")] private int _sizeMultiplier = 1;
        [SerializeField] private Color _color = Color.black;
        [SerializeField, Tooltip("띄어쓰기를 이만큼(px, 정수) 더 넓힌다")] private int _extraWordSpacingPx;

        public TextStyleValues(UiTextStyle id, bool bold, int sizeMultiplier, Color color, TMP_FontAsset fontOverride = null, int extraWordSpacingPx = 0)
        {
            _extraWordSpacingPx = extraWordSpacingPx;
            _id = id;
            _bold = bold;
            _sizeMultiplier = sizeMultiplier;
            _color = color;
            _fontOverride = fontOverride;
        }

        public UiTextStyle Id => _id;
        public bool Bold => _bold;
        public int SizeMultiplier => _sizeMultiplier;
        public Color Color => _color;
        public int ExtraWordSpacingPx => _extraWordSpacingPx;

        /// <summary>쓰는 폰트의 줄 높이(px, 올림).</summary>
        public int LineHeightFrom(UiTheme.FontValues fonts)
        {
            var font = FontFrom(fonts);
            if (font == null) return SizeFrom(fonts);
            return Mathf.CeilToInt(font.faceInfo.lineHeight / font.faceInfo.pointSize * SizeFrom(fonts));
        }

        public TMP_FontAsset FontFrom(UiTheme.FontValues fonts)
        {
            if (_fontOverride != null) return _fontOverride;
            return _bold && fonts.Bold != null ? fonts.Bold : fonts.Regular;
        }

        /// <summary>글자 크기 = 쓰는 폰트의 설계 크기(폰트 에셋의 Sampling 크기) × 배수. 정수 배만 나온다.</summary>
        public int SizeFrom(UiTheme.FontValues fonts)
        {
            var font = FontFrom(fonts);
            int design = font != null ? Mathf.Max(1, Mathf.RoundToInt(font.faceInfo.pointSize)) : 12;
            return design * Math.Max(1, _sizeMultiplier);
        }
    }
}
