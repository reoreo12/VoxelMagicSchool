// 화면 아래 가운데 퀵슬롯 바(UI_SPEC 7-1): 어두운 칸 9-slice 바 안에 밝은 칸 5개, 선택 칸은 선택 프레임이 보간으로 따라간다.
// 칸은 미리 만들어 두고 내용만 바꾼다(4-A-5). 인벤토리는 이벤트로만 읽고, 클릭은 "이 칸 선택" 요청만 보낸다.
// 칸 수는 테마 값이라 나중에 확장 인벤토리(7-4)에서 같은 칸 프리팹을 더 늘려 쓸 수 있다.
// 치트(에디터·개발 빌드만): 칸 우클릭 = 개수와 상관없이 그 칸 통째로 버리기(안내는 조작법 맨 아래 한 줄, ControlsHintView).
// 마우스가 바 위에 있을 때 휠을 돌리면 칸을 바꾼다(아래로 = 다음 칸, 끝 칸에서는 멈춤). 그동안 게임의 휠 줌은 쉰다(HudState).
// 아이템이 든 칸에 마우스를 올리면 공용 툴팁(시간대 HUD와 같은 것)으로 아이템 이름을 그 칸 바로 위 가운데에 띄운다(칸 아이콘을 가리지 않게).
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class QuickSlotBarView : ThemedWidget, IGameContextBinder, IScrollHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private RectTransform _bar;
        [SerializeField] private ThemedPanel _background;
        [SerializeField] private List<QuickSlotView> _slots = new List<QuickSlotView>();
        [SerializeField] private SelectionFrameView _selection;
        [SerializeField, Tooltip("UI_Root의 Overlay 층 툴팁 (비우면 같은 UI_Root에서 찾는다)")] private UiTooltipView _tooltip;

        private Inventory _inventory;
        private HudState _hud;
        private GameMessages _messages;
        private int _selectedShown = -1;
        private int _hovered = -1;

        public void Bind(GameContext context)
        {
            _inventory = context.Inventory;
            _hud = context.Hud;
            _messages = context.Messages;
            for (int i = 0; i < _slots.Count; i++)
            {
                _slots[i].Setup(i, _inventory.Select, CheatDiscard);
                _slots[i].SetHover(OnSlotHover, OnSlotExit);
            }
            if (_tooltip == null)
            {
                var canvas = GetComponentInParent<Canvas>();
                if (canvas != null) _tooltip = canvas.rootCanvas.GetComponentInChildren<UiTooltipView>(true);
            }
            _inventory.SlotChanged += OnSlotChanged;
            _inventory.SelectionChanged += OnSelectionChanged;
            for (int i = 0; i < _slots.Count; i++) OnSlotChanged(i);
            MoveSelection(_inventory.SelectedIndex, animate: false);
        }

        // 우클릭 버리기는 치트라 정식 빌드에서는 연결하지 않는다.
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private System.Action<int> CheatDiscard => Discard;
#else
        private System.Action<int> CheatDiscard => null;
#endif

        private void Discard(int index)
        {
            var removed = _inventory.DiscardSlot(index);
            if (!removed.IsEmpty)
                _messages?.ShowToast(string.Format(GameTexts.DiscardedFormat, removed.Item.DisplayName, removed.Count));
        }

        public void OnPointerEnter(PointerEventData eventData) => _hud?.SetPointerOverScrollHud(true);
        public void OnPointerExit(PointerEventData eventData) => _hud?.SetPointerOverScrollHud(false);

        public void OnScroll(PointerEventData eventData)
        {
            if (_inventory == null || Mathf.Approximately(eventData.scrollDelta.y, 0f)) return;
            int count = Mathf.Min(_slots.Count, _inventory.SlotCount);
            if (count <= 0) return;
            int step = eventData.scrollDelta.y < 0f ? 1 : -1;
            // 끝 칸에서는 넘어가지 않고 멈춘다(첫 칸에서 위로, 마지막 칸에서 아래로). 돌아 넘어가면 선택 틀이 바를 가로질러 되돌아가 어지럽다.
            int next = Mathf.Clamp(_inventory.SelectedIndex + step, 0, count - 1);
            if (next != _inventory.SelectedIndex) _inventory.Select(next);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _hud?.SetPointerOverScrollHud(false); // 꺼지면서 나감 이벤트를 못 받는 경우
        }

        // 인벤토리는 씬을 넘어 유지되므로 꼭 구독을 푼다.
        private void OnDestroy()
        {
            if (_inventory == null) return;
            _inventory.SlotChanged -= OnSlotChanged;
            _inventory.SelectionChanged -= OnSelectionChanged;
        }

        private void OnSlotChanged(int index)
        {
            if (this == null) { OnDestroy(); return; } // 꺼진 채 씬이 내려가 OnDestroy가 불리지 않은 구독자: 여기서 구독을 푼다
            if (index < 0 || index >= _slots.Count || index >= _inventory.SlotCount) return;
            _slots[index].Show(_inventory.GetSlot(index));
            if (index == _hovered) ShowSlotTooltip(); // 올려 둔 칸 내용이 바뀌면 이름도 바꾼다
        }

        private void OnSlotHover(int index, Vector2 screenPosition)
        {
            _hovered = index;
            ShowSlotTooltip();
        }

        private void OnSlotExit(int index)
        {
            if (index != _hovered) return;
            _hovered = -1;
            if (_tooltip != null) _tooltip.Hide();
        }

        // 빈 칸이면 숨긴다.
        private void ShowSlotTooltip()
        {
            if (_tooltip == null || Theme == null || _inventory == null || _hovered < 0 || _hovered >= _inventory.SlotCount) return;
            var stack = _inventory.GetSlot(_hovered);
            if (stack.IsEmpty) { _tooltip.Hide(); return; }
            // 칸 위 끝 가운데(화면 좌표) 기준으로 위쪽 가운데에 띄운다.
            var rect = _slots[_hovered].Rect;
            var canvas = rect.GetComponentInParent<Canvas>();
            var camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            var corner = RectTransformUtility.WorldToScreenPoint(camera, rect.TransformPoint(new Vector3(rect.rect.center.x, rect.rect.yMax, 0f)));
            _tooltip.Show(stack.Item.DisplayName, corner, Theme.QuickSlots.TooltipOffset, above: true, centered: true);
        }

        private void OnSelectionChanged(int index)
        {
            if (this == null) { OnDestroy(); return; } // 꺼진 채 씬이 내려가 OnDestroy가 불리지 않은 구독자: 여기서 구독을 푼다
            MoveSelection(index, animate: true);
        }

        private void MoveSelection(int index, bool animate)
        {
            if (index < 0 || index >= _slots.Count || Theme == null) return;
            _selection.gameObject.SetActive(true);
            var values = Theme.QuickSlots;
            animate &= _selectedShown >= 0 && _selectedShown != index;
            _selectedShown = index;
            _selection.MoveTo(_slots[index].Rect.anchoredPosition, values.SelectionOutset, values.SelectionMoveSeconds, animate);
        }

        // 화면 폭이 바뀌면(창 크기 변경) 가운데 위치를 다시 계산한다.
        private void OnRectTransformDimensionsChange()
        {
            if (Theme != null && _bar != null) PlaceBar(Theme.QuickSlots);
        }

        protected override void Apply(UiTheme theme)
        {
            var values = theme.QuickSlots;
            var root = (RectTransform)transform;
            // 뿌리는 화면 아래쪽 띠(가로로 늘림). 바는 그 안에서 왼쪽 끝 기준 정수 px로 가운데에 둔다.
            root.anchorMin = new Vector2(0f, 0f);
            root.anchorMax = new Vector2(1f, 0f);
            root.pivot = new Vector2(0.5f, 0f);
            root.anchoredPosition = Vector2.zero;
            root.sizeDelta = new Vector2(0f, values.BarSize.y + values.BottomMargin);
            if (_bar == null || _background == null || _selection == null) return;

            PlaceBar(values);
            PixelLayout.Stretch((RectTransform)_background.transform);
            for (int i = 0; i < _slots.Count; i++)
            {
                var slot = _slots[i];
                bool used = i < values.SlotCount;
                slot.gameObject.SetActive(used);
                if (!used) continue;
                var offset = values.SlotOffset(i);
                PixelLayout.PlaceTopLeft(slot.Rect, offset.x, offset.y, values.SlotSize.x, values.SlotSize.y);
                var border = SlotBorder(theme);
                slot.Layout(values.SlotSize, values.IconSize, Mathf.RoundToInt(border.x), Mathf.RoundToInt(border.w), Mathf.RoundToInt(border.y));
            }
            _selection.Setup(theme, values.SlotSize);
            int selected = _selectedShown >= 0 ? _selectedShown : 0;
            if (selected < _slots.Count) _selection.MoveTo(_slots[selected].Rect.anchoredPosition, values.SelectionOutset, 0f, false);
        }

        private void PlaceBar(QuickSlotValues values)
        {
            var size = values.BarSize;
            PixelLayout.PlaceBottomCenter(_bar, ((RectTransform)transform).rect.width, values.BottomMargin, size.x, size.y);
        }

        // 칸 스프라이트의 9-slice 경계(왼쪽, 아래, 오른쪽, 위). 아이콘을 평평한 안쪽 가운데에 두는 데 쓴다.
        private static Vector4 SlotBorder(UiTheme theme)
        {
            var skin = theme.Panel(UiPanelSkin.QuickSlot);
            return skin != null && skin.Sprite != null ? skin.Sprite.border : new Vector4(4, 7, 4, 5);
        }
    }
}
