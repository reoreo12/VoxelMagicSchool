// 테마 체크 상자(12×12, 켜짐/꺼짐 × 평상시/마우스 올림). 누르면 값을 뒤집고 Changed로 알린다. 값은 바깥에서 SetValue로 맞춘다.
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VoxelMagic.UI
{
    [RequireComponent(typeof(Image))]
    public sealed class ThemedCheckbox : ThemedWidget, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private bool _value = true;
        private bool _hover;

        public bool Value => _value;
        public event Action<bool> Changed;

        public void SetValue(bool value)
        {
            if (_value == value) return;
            _value = value;
            ApplyTheme();
        }

        protected override void Apply(UiTheme theme)
        {
            var image = GetComponent<Image>();
            var sprite = theme.Book.Checkbox(_value, _hover);
            image.sprite = sprite;
            image.enabled = sprite != null;
            image.raycastTarget = true;
            if (sprite != null) ((RectTransform)transform).sizeDelta = new Vector2(Mathf.RoundToInt(sprite.rect.width), Mathf.RoundToInt(sprite.rect.height));
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            _value = !_value;
            ApplyTheme();
            Changed?.Invoke(_value);
        }

        public void OnPointerEnter(PointerEventData eventData) { _hover = true; ApplyTheme(); }
        public void OnPointerExit(PointerEventData eventData) { _hover = false; ApplyTheme(); }
    }
}
