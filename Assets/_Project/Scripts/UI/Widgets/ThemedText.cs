// 테마 글자 스타일(폰트·정수 배 크기·색)을 입히는 TMP 글자. 문구 키가 있으면 문구 표에서 가져온다(직접 문자열 금지, 4-A-3).
// 바뀌는 글자는 SetText로 넣고, 같은 값이면 다시 그리지 않는다(4-A-5). 클릭을 받지 않으므로 Raycast Target은 끈다.
using TMPro;
using UnityEngine;

namespace VoxelMagic.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public sealed class ThemedText : ThemedWidget
    {
        [SerializeField] private UiTextStyle _style = UiTextStyle.Body;
        [SerializeField, Tooltip("비우면 글자를 코드에서 넣는다")] private string _textKey;

        private TMP_Text _label;
        private string _current;
        private bool _overrideColor;
        private Color _colorOverride;

        private TMP_Text Label => _label != null ? _label : _label = GetComponent<TMP_Text>();

        public void SetText(string text)
        {
            if (_current == text) return;
            _current = text;
            Label.SetText(text);
        }

        public UiTextStyle Style => _style;

        /// <summary>스타일 색 대신 쓸 색(예: 완료한 의뢰 글자). use가 false면 스타일 색으로 돌아간다.</summary>
        public void SetColorOverride(bool use, Color color)
        {
            if (_overrideColor == use && (!use || _colorOverride == color)) return;
            _overrideColor = use;
            _colorOverride = color;
            ApplyTheme();
        }

        public void SetStyle(UiTextStyle style)
        {
            _style = style;
            ApplyTheme();
        }

        public void SetKey(string key)
        {
            _textKey = key;
            ApplyTheme();
        }

        protected override void Apply(UiTheme theme)
        {
            var label = Label;
            label.raycastTarget = false;
            var style = theme.TextStyle(_style);
            if (style != null)
            {
                var font = style.FontFrom(theme.Fonts);
                if (font != null) label.font = font;
                label.fontSize = style.SizeFrom(theme.Fonts);
                label.color = _overrideColor ? _colorOverride : style.Color;
                label.enableAutoSizing = false;
                label.fontStyle = FontStyles.Normal;
                // TMP 띄어쓰기 단위는 글자 크기의 1/100이라 px로 바꿔 넣는다(정수 px가 되도록 테마에서 px로 받음).
                label.wordSpacing = style.ExtraWordSpacingPx * 100f / Mathf.Max(1, label.fontSize);
            }
            if (!string.IsNullOrEmpty(_textKey) && theme.Texts != null) SetText(theme.Texts.Get(_textKey));
        }
    }
}
