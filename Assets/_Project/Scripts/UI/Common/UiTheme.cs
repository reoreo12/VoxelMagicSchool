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

            public FontValues() { }

            public FontValues(TMP_FontAsset regular, TMP_FontAsset bold)
            {
                _regular = regular;
                _bold = bold;
            }

            public TMP_FontAsset Regular => _regular;
            public TMP_FontAsset Bold => _bold;
        }

        [Serializable]
        public sealed class TopLeftValues
        {
            [SerializeField, Tooltip("화면 왼쪽·위 모서리 여백 (임시값, UI_SPEC 5-4)")] private int _margin = 8;
            [SerializeField, Min(1), Tooltip("책·가방 아이콘 확대 배수 (정수 배만, 16px 아이콘 × 이 값)")] private int _iconScale = 2;
            [SerializeField, Tooltip("책 아이콘과 가방 아이콘 사이")] private int _iconGap = 4;
            [SerializeField, Tooltip("아이콘 줄과 의뢰 바 사이")] private int _barGap = 4;
            [SerializeField, Tooltip("의뢰 바 가로 길이 (고정, 10-2)")] private int _barWidth = 144;
            [SerializeField, Tooltip("의뢰 바 제목 글자 위아래 여백. 바 높이 = 글자 줄 높이 + 여백 × 2")] private int _barPaddingY = 3;
            [SerializeField, Tooltip("바 왼쪽·오른쪽 끝에서 화살표·X 버튼까지")] private int _barButtonInset = 5;
            [SerializeField, Tooltip("버튼과 제목 글자 사이")] private int _barTitleGap = 4;

            public int Margin => _margin;
            public int IconScale => Mathf.Max(1, _iconScale);
            public int IconGap => _iconGap;
            public int BarGap => _barGap;
            public int BarWidth => _barWidth;
            public int BarPaddingY => _barPaddingY;
            public int BarButtonInset => _barButtonInset;
            public int BarTitleGap => _barTitleGap;
        }

        [SerializeField] private CanvasValues _canvas = new CanvasValues();
        [SerializeField] private LayerValues _layers = new LayerValues();
        [SerializeField] private ModalValues _modal = new ModalValues();
        [SerializeField] private FontValues _fonts = new FontValues();
        [SerializeField] private TopLeftValues _topLeft = new TopLeftValues();
        [SerializeField] private QuickSlotValues _quickSlots = new QuickSlotValues();
        [SerializeField] private StatusBarValues _statusBars = new StatusBarValues();
        [SerializeField] private ScoreValues _scores = new ScoreValues();
        [SerializeField] private TimeValues _time = new TimeValues();
        [SerializeField] private TooltipValues _tooltip = new TooltipValues();
        [SerializeField] private QuestListValues _questList = new QuestListValues();
        [SerializeField] private QuestToastValues _questToast = new QuestToastValues();
        [SerializeField] private BookValues _book = new BookValues();
        [SerializeField] private ConfirmValues _confirm = new ConfirmValues();
        [SerializeField] private RequestBoardValues _requestBoard = new RequestBoardValues();
        [SerializeField] private DialogueValues _dialogue = new DialogueValues();
        [SerializeField, Tooltip("화면 문구 표(키 → 문장)")] private TextTable _texts;
        [SerializeField] private List<TextStyleValues> _textStyles = new List<TextStyleValues>();
        [SerializeField] private List<ButtonSkinValues> _buttons = new List<ButtonSkinValues>();
        [SerializeField] private List<PanelSkinValues> _panels = new List<PanelSkinValues>();

        public CanvasValues Canvas => _canvas;
        public LayerValues Layers => _layers;
        public ModalValues Modal => _modal;
        public FontValues Fonts => _fonts;
        public TopLeftValues TopLeft => _topLeft;
        public QuickSlotValues QuickSlots => _quickSlots;
        public StatusBarValues StatusBars => _statusBars;
        public ScoreValues Scores => _scores;
        public TimeValues Time => _time;
        public TooltipValues Tooltip => _tooltip;
        public QuestListValues QuestList => _questList;
        public QuestToastValues QuestToast => _questToast;
        public BookValues Book => _book;
        public ConfirmValues Confirm => _confirm;
        public RequestBoardValues RequestBoard => _requestBoard;
        public DialogueValues Dialogue => _dialogue;
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

        public void SetFonts(TMP_FontAsset regular, TMP_FontAsset bold) => _fonts = new FontValues(regular, bold);
        public void SetTexts(TextTable texts) => _texts = texts;

        /// <summary>인스펙터에서 값을 바꾸면 이 테마를 쓰는 화면이 다시 적용한다(에디터 미리보기용).</summary>
        public event Action Changed;

        // 문구 표를 고쳐도 화면이 바로 다시 그려지도록 테마 변경으로 넘겨 준다.
        private void OnEnable()
        {
            if (_texts != null) _texts.Changed += RaiseChanged;
        }

        private void OnDisable()
        {
            if (_texts != null) _texts.Changed -= RaiseChanged;
        }

        private void OnValidate()
        {
            OnDisable();
            OnEnable();
            RaiseChanged();
        }

        private void RaiseChanged() => Changed?.Invoke();
    }
}
