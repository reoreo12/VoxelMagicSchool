// 대사창 안 선택지 한 줄(UI_SPEC 6-4): 배경 없는 글자 버튼. 고른 상태(올려놓음·키보드)면 강조색 + 왼쪽 ▶, 누르는 동안 1px 내려가며 진해진다.
// 클릭·올려놓음은 이벤트로만 알리고, 무엇을 할지는 DialogueWindow가 정한다. 투명한 판(Image)이 줄 전체에서 마우스를 받는다.
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VoxelMagic.UI
{
    public sealed class DialogueChoiceRow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        [SerializeField, Tooltip("줄 전체에서 마우스를 받는 투명한 판")] private Image _hitArea;
        [SerializeField] private ThemedText _label;
        [SerializeField, Tooltip("오른쪽 작은 글자 (보상 +n)")] private ThemedText _detail;
        [SerializeField] private Image _cursor;

        private bool _selected;
        private bool _pressed;
        private bool _dimmed;
        private DialogueValues _values;
        private RectInt _rect; // 패널 기준 1배 px
        private int _indent;

        public event Action<DialogueChoiceRow> Hovered;
        public event Action<DialogueChoiceRow> Clicked;

        public int Index { get; set; }

        /// <summary>글자·위치를 넣는다. rect = 줄 칸(패널 기준 1배 px, 글자는 indent만큼 들여씀).</summary>
        public void Setup(DialogueValues values, string label, string detail, bool dimmed, RectInt rect)
        {
            _values = values;
            _rect = rect;
            _indent = values.ChoiceIndent;
            _dimmed = dimmed;
            _pressed = false;
            _label.SetStyle(values.ChoiceStyle);
            _label.SetText(label ?? string.Empty);
            bool hasDetail = !string.IsNullOrEmpty(detail);
            _detail.gameObject.SetActive(hasDetail);
            if (hasDetail)
            {
                _detail.SetStyle(values.ChoiceDetailStyle);
                _detail.SetText(detail);
            }
            _hitArea.color = new Color(0f, 0f, 0f, 0f);
            _hitArea.raycastTarget = true;
            _cursor.sprite = values.ChoiceCursor;
            _cursor.raycastTarget = false;
            PixelLayout.PlaceTopLeft(_hitArea.rectTransform, rect.x, rect.y, rect.width, rect.height);
            Refresh();
        }

        public void SetSelected(bool selected)
        {
            if (_selected == selected) return;
            _selected = selected;
            if (!selected) _pressed = false;
            Refresh();
        }

        public void OnPointerEnter(PointerEventData eventData) => Hovered?.Invoke(this);

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_pressed) return;
            _pressed = false;
            Refresh();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            _pressed = true;
            Refresh();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_pressed) return;
            _pressed = false;
            Refresh();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) Clicked?.Invoke(this);
        }

        private void Refresh()
        {
            if (_values == null) return;
            int drop = _pressed ? _values.ChoicePressDrop : 0;
            int textX = _rect.x + _indent;
            PixelLayout.PlaceTopLeft((RectTransform)_label.transform, textX, _rect.y + drop, _rect.width - _indent, _rect.height);
            PixelLayout.PlaceTopLeft((RectTransform)_detail.transform, textX, _rect.y + drop, _rect.width - _indent, _rect.height);

            bool useColor = _pressed || _selected || _dimmed;
            Color color = _pressed ? _values.ChoicePressedColor : _dimmed ? _values.ChoiceDimmedColor : _values.ChoiceSelectedColor;
            _label.SetColorOverride(useColor, color);
            _detail.SetColorOverride(useColor, color);

            _cursor.enabled = _selected && _cursor.sprite != null;
            if (_cursor.enabled)
            {
                var size = new Vector2Int(Mathf.RoundToInt(_cursor.sprite.rect.width), Mathf.RoundToInt(_cursor.sprite.rect.height));
                var offset = _values.ChoiceCursorOffset;
                PixelLayout.PlaceTopLeft(_cursor.rectTransform, _rect.x + offset.x, _rect.y + offset.y + drop, size.x, size.y);
                _cursor.color = _pressed ? _values.ChoicePressedColor : _dimmed ? _values.ChoiceDimmedColor : _values.ChoiceSelectedColor;
            }
        }
    }
}
