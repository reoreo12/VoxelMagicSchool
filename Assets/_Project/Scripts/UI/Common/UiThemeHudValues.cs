// 테마 안의 하단 HUD 값: 퀵슬롯(UI_SPEC 7-1)과 세로 상태 바(7-2). 숫자는 임시값이며 사용자 피드백으로 조정한다.
using System;
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
}
