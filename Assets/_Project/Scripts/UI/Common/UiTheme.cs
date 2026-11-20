// UI 테마 하나에 위젯·화면의 스프라이트·폰트·여백·색·정렬 순서를 모은다(UI_SPEC 3장, 4-A-3).
// 화면과 위젯은 숫자를 코드에 두지 않고 여기서 읽는다. 화면별 값 묶음은 화면을 만들 때 아래에 추가한다.
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    [CreateAssetMenu(menuName = "VoxelMagic/UI/테마", fileName = "UiTheme")]
    public sealed class UiTheme : ScriptableObject
    {
        [Serializable]
        public sealed class CanvasValues
        {
            [SerializeField, Tooltip("기준 높이(px). 배율 = 화면 높이 ÷ 이 값의 내림")] private int _referenceHeight = 360;
            [SerializeField, Tooltip("스프라이트 PPU와 같게 둔다")] private int _referencePixelsPerUnit = 16;
            [SerializeField, Min(1)] private int _minScale = 1;

            public int ReferenceHeight => _referenceHeight;
            public int ReferencePixelsPerUnit => _referencePixelsPerUnit;
            public int MinScale => _minScale;
        }

        [Serializable]
        public sealed class LayerValues
        {
            [SerializeField, Tooltip("HUD 정적 프레임·버튼")] private int _hudStatic = 0;
            [SerializeField, Tooltip("바 채움·점수·의뢰 목록처럼 이벤트로 자주 바뀌는 HUD")] private int _hudDynamic = 10;
            [SerializeField, Tooltip("시간대 화살표처럼 회전·이동 연출이 있는 HUD")] private int _hudAnimated = 20;
            [SerializeField, Tooltip("책·가방·자세히 패널 같은 팝업")] private int _popup = 100;
            [SerializeField, Tooltip("드래그 중 아이콘·툴팁·완료 알림")] private int _overlay = 200;

            public int HudStatic => _hudStatic;
            public int HudDynamic => _hudDynamic;
            public int HudAnimated => _hudAnimated;
            public int Popup => _popup;
            public int Overlay => _overlay;
        }

        [Serializable]
        public sealed class ModalValues
        {
            [SerializeField, Tooltip("팝업 뒤 입력 막기 판의 색. 지금은 투명(딤 없음). 나중에 딤을 넣으면 알파만 올린다")]
            private Color _blockerColor = new Color(0f, 0f, 0f, 0f);

            public Color BlockerColor => _blockerColor;
        }

        [Serializable]
        public sealed class FontValues
        {
            [SerializeField] private TMP_FontAsset _regular;
            [SerializeField] private TMP_FontAsset _bold;
            [SerializeField, Tooltip("폰트 설계 크기(px). 글자 크기는 이 값의 정수 배만 쓴다")] private int _designSize = 12;

            public FontValues() { }

            public FontValues(TMP_FontAsset regular, TMP_FontAsset bold, int designSize)
            {
                _regular = regular;
                _bold = bold;
                _designSize = designSize;
            }

            public TMP_FontAsset Regular => _regular;
            public TMP_FontAsset Bold => _bold;
            public int DesignSize => _designSize;
        }

        [SerializeField] private CanvasValues _canvas = new CanvasValues();
        [SerializeField] private LayerValues _layers = new LayerValues();
        [SerializeField] private ModalValues _modal = new ModalValues();
        [SerializeField] private FontValues _fonts = new FontValues();
        [SerializeField, Tooltip("화면 문구 표(키 → 문장)")] private TextTable _texts;
        [SerializeField] private List<TextStyleValues> _textStyles = new List<TextStyleValues>();
        [SerializeField] private List<ButtonSkinValues> _buttons = new List<ButtonSkinValues>();
        [SerializeField] private List<PanelSkinValues> _panels = new List<PanelSkinValues>();

        public CanvasValues Canvas => _canvas;
        public LayerValues Layers => _layers;
        public ModalValues Modal => _modal;
        public FontValues Fonts => _fonts;
        public ITextSource Texts => _texts;

        public TextStyleValues TextStyle(UiTextStyle id) => _textStyles.Find(s => s.Id == id);
        public ButtonSkinValues Button(UiButtonSkin id) => _buttons.Find(s => s.Id == id);
        public PanelSkinValues Panel(UiPanelSkin id) => _panels.Find(s => s.Id == id);

        /// <summary>에디터 도구가 기본 모양을 채울 때 쓴다. 이미 있는 이름은 건드리지 않는다.</summary>
        public void AddMissing(IEnumerable<TextStyleValues> styles, IEnumerable<ButtonSkinValues> buttons, IEnumerable<PanelSkinValues> panels)
        {
            foreach (var s in styles) if (TextStyle(s.Id) == null) _textStyles.Add(s);
            foreach (var b in buttons) if (Button(b.Id) == null) _buttons.Add(b);
            foreach (var p in panels) if (Panel(p.Id) == null) _panels.Add(p);
        }

        public void SetFonts(TMP_FontAsset regular, TMP_FontAsset bold, int designSize) => _fonts = new FontValues(regular, bold, designSize);
        public void SetTexts(TextTable texts) => _texts = texts;

        /// <summary>인스펙터에서 값을 바꾸면 이 테마를 쓰는 화면이 다시 적용한다(에디터 미리보기용).</summary>
        public event Action Changed;

        private void OnValidate() => Changed?.Invoke();
    }
}
