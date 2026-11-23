// 테마 안의 HUD·화면 값 묶음: 퀵슬롯·상태 바·점수·시간대·의뢰 목록·알림·툴팁·책. 숫자는 임시값이며 사용자 피드백으로 조정한다.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelMagic.UI
{
    [Serializable]
    public sealed class QuickSlotValues
    {
        [SerializeField, Min(1), Tooltip("퀵슬롯 칸 수 (7-1, 5칸)")] private int _slotCount = 5;
        [SerializeField, Tooltip("슬롯 칸 크기 (밝은 칸 9-slice, 원본 30×32)")] private Vector2Int _slotSize = new Vector2Int(30, 32);
        [SerializeField, Tooltip("슬롯 사이 간격")] private int _slotGap = 2;
        [SerializeField, Tooltip("바 안쪽 평평한 곳에서 슬롯까지 여백(위아래 같게, 7-1 공식의 m)")] private int _innerMargin = 2;
        [SerializeField, Tooltip("바 테두리 두께 (어두운 칸: 위·좌·우 4, 아래 6)")] private int _borderSide = 4;
        [SerializeField] private int _borderBottom = 6;
        [SerializeField, Tooltip("화면 아래에서 바까지")] private int _bottomMargin = 8;
        [SerializeField, Tooltip("아이템 아이콘 크기(px, 16×16 도트)")] private int _iconSize = 16;
        [SerializeField, Tooltip("선택 프레임을 슬롯 바깥으로 내미는 정도")] private int _selectionOutset = 2;
        [SerializeField, Tooltip("선택 프레임이 다음 칸으로 옮겨 가는 시간(초, 시간 정지와 무관)")] private float _selectionMoveSeconds = 0.08f;
        [SerializeField] private Sprite _selectionTopLeft;
        [SerializeField] private Sprite _selectionTopRight;
        [SerializeField] private Sprite _selectionBottomLeft;
        [SerializeField] private Sprite _selectionBottomRight;

        public int SlotCount => Mathf.Max(1, _slotCount);
        public Vector2Int SlotSize => _slotSize;
        public int SlotGap => _slotGap;
        public int InnerMargin => _innerMargin;
        public int BorderSide => _borderSide;
        public int BorderBottom => _borderBottom;
        public int BottomMargin => _bottomMargin;
        public int IconSize => _iconSize;
        public int SelectionOutset => _selectionOutset;
        public float SelectionMoveSeconds => _selectionMoveSeconds;
        public Sprite SelectionTopLeft => _selectionTopLeft;
        public Sprite SelectionTopRight => _selectionTopRight;
        public Sprite SelectionBottomLeft => _selectionBottomLeft;
        public Sprite SelectionBottomRight => _selectionBottomRight;

        /// <summary>바 크기 = 7-1 공식: 가로 n×w + (n−1)×g + 2×(테두리+m), 세로 위 테두리 + m + h + m + 아래 테두리.</summary>
        public Vector2Int BarSize => new Vector2Int(
            SlotCount * _slotSize.x + (SlotCount - 1) * _slotGap + 2 * (_borderSide + _innerMargin),
            _borderSide + _innerMargin * 2 + _slotSize.y + _borderBottom);

        /// <summary>i번째 슬롯의 바 왼쪽 위 기준 위치.</summary>
        public Vector2Int SlotOffset(int index) => new Vector2Int(
            _borderSide + _innerMargin + index * (_slotSize.x + _slotGap),
            _borderSide + _innerMargin);

        public void SetSelectionSprites(Sprite topLeft, Sprite topRight, Sprite bottomLeft, Sprite bottomRight)
        {
            if (_selectionTopLeft == null) _selectionTopLeft = topLeft;
            if (_selectionTopRight == null) _selectionTopRight = topRight;
            if (_selectionBottomLeft == null) _selectionBottomLeft = bottomLeft;
            if (_selectionBottomRight == null) _selectionBottomRight = bottomRight;
        }
    }

    [Serializable]
    public sealed class StatusBarValues
    {
        [SerializeField, Tooltip("화면 오른쪽·아래 끝에서 바 프레임까지 (같은 값, 13장). 바 사이 간격보다 커야 한다")] private int _margin = 8;
        [SerializeField, Min(0.5f), Tooltip("바 묶음 확대 배수 (0.5 단위). 정수 배가 가장 선명하고, x.5는 1080p·4K에서만 고르게 보인다")] private float _scale = 1.5f;
        [SerializeField, Min(0.5f), Tooltip("아이콘 확대 배수 (바와 따로). 정수·0.5 단위가 아니면 도트 폭이 조금 고르지 않다. 아이콘 크기는 정수 px로 반올림")]
        private float _iconScale = 1.125f;
        [SerializeField, Tooltip("바 프레임 높이 (7-2 예시 80, 가로 16 고정)")] private int _barHeight = 80;
        [SerializeField, Tooltip("기력 바와 마력 바 프레임 사이 (확대 전 px). 아이콘이 겹치지 않을 만큼은 둔다")] private int _barGap = 4;
        [SerializeField, Tooltip("바와 위 아이콘 사이 (7-2)")] private int _iconGap = 2;
        [SerializeField, Tooltip("프레임 안쪽 여백 (채움은 사방 5px 안쪽)")] private int _fillInset = 5;
        [SerializeField] private Sprite _manaFill;
        [SerializeField] private Sprite _staminaFill;
        [SerializeField] private Sprite _manaCap;
        [SerializeField] private Sprite _staminaCap;
        [SerializeField] private Sprite _manaIcon;
        [SerializeField] private Sprite _staminaIcon;

        public int Margin => _margin;
        public float Scale => Mathf.Max(0.5f, Mathf.Round(_scale * 2f) / 2f);
        public float IconScale => Mathf.Max(0.5f, _iconScale);
        public int BarHeight => Mathf.Max(20, _barHeight);
        public int BarGap => _barGap;
        public int IconGap => _iconGap;
        public int FillInset => _fillInset;
        public Sprite ManaFill => _manaFill;
        public Sprite StaminaFill => _staminaFill;
        public Sprite ManaCap => _manaCap;
        public Sprite StaminaCap => _staminaCap;
        public Sprite ManaIcon => _manaIcon;
        public Sprite StaminaIcon => _staminaIcon;

        public void SetSprites(Sprite manaFill, Sprite staminaFill, Sprite manaCap, Sprite staminaCap, Sprite manaIcon, Sprite staminaIcon)
        {
            if (_manaFill == null) _manaFill = manaFill;
            if (_staminaFill == null) _staminaFill = staminaFill;
            if (_manaCap == null) _manaCap = manaCap;
            if (_staminaCap == null) _staminaCap = staminaCap;
            if (_manaIcon == null) _manaIcon = manaIcon;
            if (_staminaIcon == null) _staminaIcon = staminaIcon;
        }
    }

    [Serializable]
    public sealed class ScoreValues
    {
        [SerializeField, Tooltip("화면 위에서 점수 줄까지")] private int _topMargin = 8;
        [SerializeField, Tooltip("필기 묶음과 실기 묶음 사이")] private int _groupGap = 24;
        [SerializeField, Tooltip("글자(필기 예상 점수)와 숫자 사이 (가로)")] private int _labelGap = 4;
        [SerializeField, Tooltip("필기 묶음 미세 조정 (x +오른쪽, y +아래, px). 자동 배치 위에 더한다")] private ScoreGroupOffsets _writtenOffsets = new ScoreGroupOffsets();
        [SerializeField, Tooltip("실기 묶음 미세 조정 (x +오른쪽, y +아래, px). 자동 배치 위에 더한다")] private ScoreGroupOffsets _practicalOffsets = new ScoreGroupOffsets();
        [SerializeField, Min(1), Tooltip("숫자·밑줄 확대 배수 (정수 배)")] private int _numberScale = 2;
        [SerializeField, Tooltip("숫자 칸 간격(피치). 칸 폭 11, 간격 −2 → 9 (9-2)")] private int _digitPitch = 9;
        [SerializeField, Tooltip("미리 만들어 두는 자릿수 (9-2, 최대 4자리)")] private int _maxDigits = 4;
        [SerializeField, Min(0), Tooltip("숫자·밑줄 그림의 외곽선 두께(px). 외곽선 v2 = 1. 밑줄을 그만큼 위로 올려 원래 자리와 맞춘다")] private int _outlinePx;
        [SerializeField, Tooltip("v2 숫자 0~9 (9-2: 숫자는 항상 v2)")] private Sprite[] _digits = new Sprite[10];
        [SerializeField, Tooltip("필기 밑줄 v1: score_underline_1~4")] private Sprite[] _writtenUnderlines = new Sprite[4];
        [SerializeField, Tooltip("실기 밑줄 v2: score_underline_1~4")] private Sprite[] _practicalUnderlines = new Sprite[4];

        public int TopMargin => _topMargin;
        public int GroupGap => _groupGap;
        public int LabelGap => _labelGap;
        public ScoreGroupOffsets Offsets(bool written) => (written ? _writtenOffsets : _practicalOffsets) ?? ScoreGroupOffsets.None;
        public int NumberScale => Mathf.Max(1, _numberScale);
        public int DigitPitch => _digitPitch;
        public int MaxDigits => Mathf.Max(1, _maxDigits);
        public int OutlinePx => Mathf.Max(0, _outlinePx);
        public Sprite Digit(int value) => _digits != null && value >= 0 && value < _digits.Length ? _digits[value] : null;

        /// <summary>자릿수에 맞는 밑줄(9-2): 1~2자리 → 2번, 3자리 → 3번, 4자리 → 4번(1번은 쓰지 않음).</summary>
        public Sprite Underline(bool written, int digitCount)
        {
            var set = written ? _writtenUnderlines : _practicalUnderlines;
            int index = Mathf.Clamp(Mathf.Max(2, digitCount), 2, 4) - 1;
            return set != null && index < set.Length ? set[index] : null;
        }

        public void SetSprites(Sprite[] digits, Sprite[] written, Sprite[] practical)
        {
            _digits = digits;
            _writtenUnderlines = written;
            _practicalUnderlines = practical;
        }
    }

    /// <summary>의뢰 드롭다운 목록 패널(UI_SPEC 10-2~10-4, 13장). 가로는 의뢰 바와 같고 세로만 항목에 맞춰 바뀐다.</summary>
    [System.Serializable]
    public sealed class QuestListValues
    {
        [SerializeField, Tooltip("의뢰 바 아래 끝에서 패널까지 (0 = 바로 아래에 붙임)")] private int _barGap = 0;
        [SerializeField, Tooltip("패널 안쪽 왼쪽·오른쪽 여백 (종이 가장자리 그늘에 글자가 묻히지 않게)")] private int _paddingX = 9;
        [SerializeField, Tooltip("맨 위 항목 위와 맨 아래 항목 아래 여백 (항상 같음, 10-2)")] private int _paddingY = 10;
        [SerializeField, Tooltip("항목 사이 간격 (임시값 하나로 통일, 13장)")] private int _itemGap = 4;
        [SerializeField, Tooltip("아이템 줄 들여쓰기 (제목의 항목 기호 뒤 글자에 맞춤)")] private int _requirementIndent = 8;
        [SerializeField, Min(1), Tooltip("이 개수(제목 + 아이템 1줄 기준)만큼 보이면 더 늘리지 않고 스크롤 (10-2: 대략 4개)")] private int _maxVisibleItems = 4;
        [SerializeField, Tooltip("스크롤바 폭 (트랙 8)")] private int _scrollbarWidth = 8;
        [SerializeField, Tooltip("목록과 스크롤바 사이")] private int _scrollbarGap = 2;
        [SerializeField, Min(1), Tooltip("휠 한 칸에 움직이는 px")] private int _wheelStep = 16;
        [SerializeField, Tooltip("의뢰 제목 글자 (갈무리9 10px)")] private UiTextStyle _titleStyle = UiTextStyle.Small;
        [SerializeField, Tooltip("갈무리9에는 굵은체가 없어 제목을 1px 오른쪽에 한 번 더 그려 굵게 보이게 한다")] private bool _titlePixelBold = true;
        [SerializeField, Tooltip("재료 줄 글자 (갈무리9 10px)")] private UiTextStyle _requirementStyle = UiTextStyle.Small;
        [SerializeField, Tooltip("완료(납품 가능)한 의뢰 글자색 (13장 임시 #4A90C8)")] private Color _completedColor = new Color32(0x4A, 0x90, 0xC8, 0xFF);

        public int BarGap => _barGap;
        public int PaddingX => _paddingX;
        public int PaddingY => _paddingY;
        public int ItemGap => _itemGap;
        public int RequirementIndent => _requirementIndent;
        public int MaxVisibleItems => Mathf.Max(1, _maxVisibleItems);
        public int ScrollbarWidth => _scrollbarWidth;
        public int ScrollbarGap => _scrollbarGap;
        public int WheelStep => Mathf.Max(1, _wheelStep);
        public UiTextStyle TitleStyle => _titleStyle;
        public bool TitlePixelBold => _titlePixelBold;
        public UiTextStyle RequirementStyle => _requirementStyle;
        public Color CompletedColor => _completedColor;
    }

    /// <summary>의뢰 완료 알림 창(UI_SPEC 10-6, 13장 임시: 3초, 페이드, 여러 건은 하나씩).</summary>
    [System.Serializable]
    public sealed class QuestToastValues
    {
        [SerializeField, Tooltip("화면 왼쪽·아래 모서리 여백")] private int _margin = 8;
        [SerializeField, Tooltip("글자 둘레 여백 (x 좌우, y 위아래)")] private Vector2Int _padding = new Vector2Int(7, 5);
        [SerializeField, Tooltip("글자와 X 버튼 사이")] private int _closeGap = 4;
        [SerializeField, Min(0f), Tooltip("완전히 보이는 시간(초, 시간 정지와 무관)")] private float _showSeconds = 3f;
        [SerializeField, Min(0f), Tooltip("사라지는 페이드 시간(초)")] private float _fadeSeconds = 0.4f;
        [SerializeField] private UiTextStyle _textStyle = UiTextStyle.Body;

        public int Margin => _margin;
        public Vector2Int Padding => _padding;
        public int CloseGap => _closeGap;
        public float ShowSeconds => _showSeconds;
        public float FadeSeconds => _fadeSeconds;
        public UiTextStyle TextStyle => _textStyle;
    }

    /// <summary>책(메뉴) 화면(UI_SPEC 5장): 왼쪽 페이지 + 책등 + 오른쪽 페이지, 오른쪽 가장자리에 사이드 탭. 크기는 화면 960×540 기준 px.</summary>
    [System.Serializable]
    public sealed class BookValues
    {
        [SerializeField, Tooltip("페이지 한 장 크기 (두 장 같음). 책 폭 = 페이지 × 2 + 책등 (임시, 프리뷰 비율)")] private Vector2Int _pageSize = new Vector2Int(220, 300);
        [SerializeField, Tooltip("페이지 가장자리에서 내용까지")] private int _contentInset = 16;
        [SerializeField, Tooltip("책 위 끝에서 첫 탭까지")] private int _firstTabY = 12;
        [SerializeField, Tooltip("탭 사이 간격 (탭 높이 + 2)")] private int _tabStep = 34;
        [SerializeField, Tooltip("탭 왼쪽이 표지 아래로 들어가는 폭 (5-1)")] private int _tabTuck = 4;
        [SerializeField, Tooltip("탭 크기: 평상시 (9-slice로 늘려 외곽선은 1px 그대로, 원본 20×20)")] private Vector2Int _tabSize = new Vector2Int(37, 32);
        [SerializeField, Tooltip("탭 크기: 선택 (평상시보다 4 더 튀어나옴, 원본 24×20)")] private Vector2Int _tabSelectedSize = new Vector2Int(41, 32);
        [SerializeField, Min(1), Tooltip("탭 아이콘 배수 (12×12 × 정수 배)")] private int _tabIconScale = 2;
        [SerializeField, Tooltip("탭 그림 외곽선 두께. 아이콘은 표지 밖으로 보이는 안쪽 칸의 가운데에 둔다")] private int _tabOutline = 1;
        [SerializeField, Tooltip("구분선 색")] private Color _lineColor = new Color32(191, 160, 112, 255);
        [SerializeField] private Sprite _pageLeft;
        [SerializeField] private Sprite _pageRight;
        [SerializeField] private Sprite _spine;
        [SerializeField, Tooltip("탭마다 색 스프라이트와 아이콘 (위에서 아래 순서는 BookTab 순서)")] private List<BookTabValues> _tabs = new List<BookTabValues>();

        [Header("페이지 넘김 (5-3)")]
        [SerializeField, Min(0.001f), Tooltip("한 프레임 시간(초, 시간 정지와 무관). 9프레임 × 0.045 ≈ 0.4초")] private float _turnFrameSeconds = 0.045f;
        [SerializeField, Min(1), Tooltip("프레임 수 k = 0 ~ n−1, p = (k+1)/(n+1)")] private int _turnFrames = 9;
        [SerializeField, Tooltip("페이지 표지 테두리 두께: 넘어가는 종이는 이 안쪽을 덮는다")] private int _paperInset = 4;
        [SerializeField, Tooltip("가장 많이 들어 올리는 높이(px) = round(이 값 × sin(πp))")] private int _turnLift = 4;
        [SerializeField, Tooltip("폭 비율이 이 값 이상이면 leaf_0")] private float _leafFlatRatio = 0.8f;
        [SerializeField, Tooltip("폭 비율이 이 값 이상이면 leaf_1, 아래면 leaf_2")] private float _leafCurveRatio = 0.45f;
        [SerializeField] private Sprite[] _leaves = new Sprite[3];
        [SerializeField] private Sprite _leafEdge;
        [SerializeField] private Sprite _turnShadow;

        [Header("의뢰 탭")]
        [SerializeField, Tooltip("목록 한 칸 높이")] private int _rowHeight = 22;
        [SerializeField, Tooltip("칸 안 글자 왼쪽 여백")] private int _rowTextInset = 5;
        [SerializeField, Tooltip("머리(수락한 의뢰(n)) 아래 구분선까지")] private int _headerGap = 4;
        [SerializeField, Tooltip("체크 상자와 글자 사이")] private int _checkboxGap = 4;
        [SerializeField, Tooltip("본문 제목 → 대사 사이")] private int _titleGap = 6;
        [SerializeField, Tooltip("본문 묶음 사이 (대사 → 재료 → 보상)")] private int _sectionGap = 10;
        [SerializeField, Tooltip("재료 줄 들여쓰기")] private int _requirementIndent = 8;
        [SerializeField, Tooltip("'보상'과 값 사이")] private int _rewardGap = 8;
        [SerializeField] private Sprite _checkboxOn;
        [SerializeField] private Sprite _checkboxOnHover;
        [SerializeField] private Sprite _checkboxOff;
        [SerializeField] private Sprite _checkboxOffHover;

        public Vector2Int PageSize => _pageSize;
        public int ContentInset => _contentInset;
        public int FirstTabY => _firstTabY;
        public int TabStep => _tabStep;
        public int TabTuck => _tabTuck;
        public Vector2Int TabSize(bool selected) => selected ? _tabSelectedSize : _tabSize;
        public int TabIconScale => Mathf.Max(1, _tabIconScale);
        public int TabOutline => Mathf.Max(0, _tabOutline);
        public Color LineColor => _lineColor;
        public Sprite PageLeft => _pageLeft;
        public Sprite PageRight => _pageRight;
        public Sprite Spine => _spine;
        public int SpineWidth => _spine != null ? Mathf.RoundToInt(_spine.rect.width) : 8;
        public BookTabValues Tab(VoxelMagic.Core.BookTab id) => _tabs.Find(t => t.Id == id);
        public float TurnFrameSeconds => Mathf.Max(0.001f, _turnFrameSeconds);
        public int TurnFrames => Mathf.Max(1, _turnFrames);
        public int PaperInset => Mathf.Max(0, _paperInset);
        public int TurnLift => _turnLift;
        public Sprite LeafEdge => _leafEdge;
        public Sprite TurnShadow => _turnShadow;
        /// <summary>폭 비율에 맞는 넘어가는 종이 모양 (평평 → 휘어짐 → 많이 휘어짐).</summary>
        public Sprite Leaf(float ratio)
        {
            int index = ratio >= _leafFlatRatio ? 0 : ratio >= _leafCurveRatio ? 1 : 2;
            return _leaves != null && index < _leaves.Length ? _leaves[index] : null;
        }

        public void SetTurnSprites(Sprite[] leaves, Sprite edge, Sprite shadow)
        {
            _leaves = leaves;
            _leafEdge = edge;
            _turnShadow = shadow;
        }
        public int RowHeight => _rowHeight;
        public int RowTextInset => _rowTextInset;
        public int HeaderGap => _headerGap;
        public int CheckboxGap => _checkboxGap;
        public int TitleGap => _titleGap;
        public int SectionGap => _sectionGap;
        public int RequirementIndent => _requirementIndent;
        public int RewardGap => _rewardGap;
        public Sprite Checkbox(bool on, bool hover) => on ? (hover && _checkboxOnHover != null ? _checkboxOnHover : _checkboxOn) : (hover && _checkboxOffHover != null ? _checkboxOffHover : _checkboxOff);

        public void SetSprites(Sprite left, Sprite right, Sprite spine, IEnumerable<BookTabValues> tabs, Sprite[] checkbox)
        {
            _pageLeft = left;
            _pageRight = right;
            _spine = spine;
            _tabs = new List<BookTabValues>(tabs);
            _checkboxOn = checkbox[0];
            _checkboxOnHover = checkbox[1];
            _checkboxOff = checkbox[2];
            _checkboxOffHover = checkbox[3];
        }
    }

    [System.Serializable]
    public sealed class BookTabValues
    {
        [SerializeField] private VoxelMagic.Core.BookTab _id;
        [SerializeField] private Sprite _normal;
        [SerializeField] private Sprite _selected;
        [SerializeField] private Sprite _icon;

        public BookTabValues(VoxelMagic.Core.BookTab id, Sprite normal, Sprite selected, Sprite icon)
        {
            _id = id;
            _normal = normal;
            _selected = selected;
            _icon = icon;
        }

        public VoxelMagic.Core.BookTab Id => _id;
        public Sprite Normal => _normal;
        public Sprite Selected => _selected;
        public Sprite Icon => _icon;
    }

    /// <summary>공용 툴팁(종이 바탕 + 글자 한 줄). 맨 위 Overlay 층에 하나만 둔다.</summary>
    [System.Serializable]
    public sealed class TooltipValues
    {
        [SerializeField, Tooltip("글자 둘레 여백 (x 좌우, y 위아래)")] private Vector2Int _padding = new Vector2Int(5, 3);
        [SerializeField, Tooltip("화면 가장자리에서 최소 거리")] private int _screenMargin = 4;
        [SerializeField] private UiTextStyle _textStyle = UiTextStyle.Body;

        public Vector2Int Padding => _padding;
        public int ScreenMargin => _screenMargin;
        public UiTextStyle TextStyle => _textStyle;
    }

    /// <summary>화면 오른쪽 위 시간대 HUD(UI_SPEC 8장, 13장). 위치는 틀 그림 왼쪽 위 기준 1배 px, 아래가 +y.</summary>
    [System.Serializable]
    public sealed class TimeValues
    {
        [SerializeField, Tooltip("화면 오른쪽·위 모서리 여백 (확대 뒤 화면 px)")] private int _margin = 8;
        [SerializeField, Min(0.5f), Tooltip("틀·아이콘·룰렛 확대 배수 (0.5 단위). 그 밖의 값은 도트 폭이 고르지 않고 조각 사이에 틈이 생긴다")] private float _scale = 1.5f;

        [Header("조각 위치 (틀 왼쪽 위 기준, 1배 px)")]
        [SerializeField, Tooltip("틀 뒤 받침 칸(틀 구멍 뒤를 채워 조각 사이 틈이 비치지 않게). x, y, 폭, 높이")] private RectInt _backing = new RectInt(8, 8, 41, 18);
        [SerializeField, Tooltip("받침 색 (틀 안쪽 테두리 색)")] private Color _backingColor = new Color32(196, 154, 108, 255);
        [SerializeField, Tooltip("왼쪽 칸 시간대 그림 (틀보다 앞에 그린다)")] private Vector2Int _iconPosition = new Vector2Int(8, 8);
        [SerializeField, Tooltip("오른쪽 칸 룰렛")] private Vector2Int _roulettePosition = new Vector2Int(35, 8);
        [SerializeField, Tooltip("화살표 회전 중심 = 룰렛 반원의 중심 (0.5 단위 가능)")] private Vector2 _arrowPivot = new Vector2(34f, 16.5f);
        [SerializeField, Tooltip("화살표 그림 안의 회전 중심 (그림 왼쪽 위 기준)")] private Vector2Int _arrowPivotInSprite = new Vector2Int(0, 4);
        [SerializeField, Tooltip("날짜 글자 칸 위치 (아래 패널 안)")] private Vector2Int _datePosition = new Vector2Int(3, 36);
        [SerializeField, Tooltip("날짜 글자 칸 크기")] private Vector2Int _dateSize = new Vector2Int(48, 16);
        [SerializeField, Tooltip("개발용 '멈춰 있음' 글자 위치 (8-2)")] private Vector2Int _pausedPosition = new Vector2Int(0, 56);
        [SerializeField, Tooltip("날짜 글자 스타일 (수락한 의뢰 제목처럼 굵게)")] private UiTextStyle _dateStyle = UiTextStyle.HudTitle;

        [Header("시각 → 화살표 각도 (임시값, 8-1 [미정])")]
        [SerializeField, Range(0, 24), Tooltip("방과후 시작 시각")] private int _afterSchoolStartHour = 16;
        [SerializeField, Range(0, 24), Tooltip("밤 시작 시각 (방과후 끝)")] private int _nightStartHour = 19;
        [SerializeField, Range(0, 24), Tooltip("밤 끝 시각")] private int _nightEndHour = 24;
        [SerializeField, Tooltip("방과후 시작 때 화살표 각도 (0 = 오른쪽, +는 위로)")] private int _startAngle = 75;
        [SerializeField, Tooltip("방과후→밤 경계 각도 (룰렛 색 경계)")] private int _boundaryAngle = 0;
        [SerializeField, Tooltip("밤 끝 때 화살표 각도")] private int _endAngle = -75;

        [Header("시계 연결 전 미리보기")]
        [SerializeField, Min(1)] private int _previewDay = 1;
        [SerializeField, Min(1), Tooltip("플레이하는 날 수. 날짜 판 = D-(이 값 − 며칠째 + 1): 1일째 D-8 … 마지막 날 D-1. 게임 시계가 생기면 규칙 쪽 값으로 옮길 것")] private int _playDays = 8;
        [SerializeField, Range(0, 24)] private int _previewHour = 16;

        [Header("툴팁")]
        [SerializeField, Tooltip("마우스 끝에서 툴팁까지 (확대 뒤 화면 px, x +오른쪽 y +아래)")] private Vector2Int _tooltipOffset = new Vector2Int(-8, 12);

        [Header("스프라이트")]
        [SerializeField, Tooltip("나무 틀 + 날짜 패널 (수정본 v2 시트: 날짜 판 가운데, 잎 없음)")] private Sprite _frame;
        [SerializeField, Tooltip("룰렛 반원 (위 주황 = 방과후, 아래 파랑 = 밤)")] private Sprite _roulette;
        [SerializeField, Tooltip("가로 화살표 한 개 (회전해서 씀)")] private Sprite _arrow;
        [SerializeField, Tooltip("방과후 큰 아이콘 (노을 해)")] private Sprite _afterSchoolIcon;
        [SerializeField, Tooltip("밤 큰 아이콘 (달)")] private Sprite _nightIcon;

        public int Margin => _margin;
        public float Scale => Mathf.Max(0.5f, Mathf.Round(_scale * 2f) / 2f);
        public RectInt Backing => _backing;
        public Color BackingColor => _backingColor;
        public Vector2Int IconPosition => _iconPosition;
        public Vector2Int RoulettePosition => _roulettePosition;
        public Vector2 ArrowPivot => _arrowPivot;
        public Vector2Int ArrowPivotInSprite => _arrowPivotInSprite;
        public Vector2Int DatePosition => _datePosition;
        public Vector2Int DateSize => _dateSize;
        public Vector2Int PausedPosition => _pausedPosition;
        public UiTextStyle DateStyle => _dateStyle;
        public int PreviewDay => Mathf.Max(1, _previewDay);
        /// <summary>며칠째 → 남은 날 n (D-n). 마지막 날이 1.</summary>
        public int DaysLeft(int day) => Mathf.Max(0, Mathf.Max(1, _playDays) - day + 1);
        public int PreviewHour => _previewHour;
        public int NightStartHour => _nightStartHour;
        public Vector2Int TooltipOffset => _tooltipOffset;
        public Sprite Frame => _frame;
        public Sprite Roulette => _roulette;
        public Sprite Arrow => _arrow;
        public Sprite Icon(bool night) => night ? _nightIcon : _afterSchoolIcon;

        /// <summary>정시 → 화살표 각도(정수 도). 방과후는 시작~경계, 밤은 경계~끝 각도에 나눠 놓아 구간 길이가 달라도 색 경계에서 바뀐다(스텝, 13장).</summary>
        public int ArrowAngle(int hour)
        {
            if (hour < _nightStartHour)
                return Lerp(_startAngle, _boundaryAngle, _afterSchoolStartHour, _nightStartHour, hour);
            return Lerp(_boundaryAngle, _endAngle, _nightStartHour, _nightEndHour, hour);
        }

        private static int Lerp(int from, int to, int startHour, int endHour, int hour)
        {
            if (endHour <= startHour) return from;
            float t = Mathf.Clamp01((hour - startHour) / (float)(endHour - startHour));
            return Mathf.RoundToInt(Mathf.Lerp(from, to, t));
        }

        public void SetSprites(Sprite frame, Sprite roulette, Sprite arrow, Sprite afterSchool, Sprite night)
        {
            _frame = frame;
            _roulette = roulette;
            _arrow = arrow;
            _afterSchoolIcon = afterSchool;
            _nightIcon = night;
        }
    }

    /// <summary>점수 한 묶음(글자 + 숫자)의 손 조정 값. 묶음 전체, 글자만, 숫자만 따로 옮긴다. 가운데 정렬 계산에는 들어가지 않는다.</summary>
    [System.Serializable]
    public sealed class ScoreGroupOffsets
    {
        public static readonly ScoreGroupOffsets None = new ScoreGroupOffsets();

        [SerializeField, Tooltip("글자와 숫자를 함께 옮김")] private Vector2Int _group;
        [SerializeField, Tooltip("글자(예상 점수)만 옮김. y 0 = 숫자 줄 가운데")] private Vector2Int _label;
        [SerializeField, Tooltip("숫자(밑줄 포함)만 옮김")] private Vector2Int _number;

        public Vector2Int Group => _group;
        public Vector2Int Label => _label;
        public Vector2Int Number => _number;
    }
}
