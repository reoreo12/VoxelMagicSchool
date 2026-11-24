// 공용 툴팁: 맨 위 Overlay 층에 하나만 두고, HUD가 마우스를 올렸을 때 글자 한 줄을 마우스 옆 정수 px 위치에 띄운다.
// 화면 밖으로 나가면 반대쪽으로 붙이고, 같은 글자면 다시 계산하지 않는다(4-A-5).
using TMPro;
using UnityEngine;

namespace VoxelMagic.UI
{
    public sealed class UiTooltipView : ThemedWidget
    {
        [SerializeField] private RectTransform _body;
        [SerializeField] private ThemedPanel _background;
        [SerializeField] private ThemedText _text;

        private Vector2Int _size;
        private string _shown;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (_body != null && Application.isPlaying) _body.gameObject.SetActive(false);
        }

        /// <summary>screenPosition(화면 px) 옆에 글자를 띄운다. offset은 확대 뒤 화면 기준(x +오른쪽, y +아래).</summary>
        public void Show(string text, Vector2 screenPosition, Vector2Int offset)
        {
            if (Theme == null || _body == null) return;
            _body.gameObject.SetActive(true);
            if (_shown != text)
            {
                _shown = text;
                _text.SetText(text);
                Resize();
            }
            Place(screenPosition, offset);
        }

        public void Hide()
        {
            if (_body != null) _body.gameObject.SetActive(false);
        }

        protected override void Apply(UiTheme theme)
        {
            var root = (RectTransform)transform;
            PixelLayout.Stretch(root);
            if (_text != null) _text.SetStyle(theme.Tooltip.TextStyle);
            if (_shown != null) Resize();
        }

        private void Resize()
        {
            var values = Theme.Tooltip;
            var label = _text.GetComponent<TMP_Text>();
            var preferred = label.GetPreferredValues(_shown);
            int lineHeight = Mathf.CeilToInt(label.fontSize * (label.font != null ? label.font.faceInfo.lineHeight / label.font.faceInfo.pointSize : 1.4f));
            int textWidth = Mathf.CeilToInt(preferred.x);
            _size = new Vector2Int(textWidth + values.Padding.x * 2, lineHeight + values.Padding.y * 2);
            _body.sizeDelta = _size;
            PixelLayout.Stretch((RectTransform)_background.transform);
            PixelLayout.PlaceTopLeft((RectTransform)_text.transform, values.Padding.x, values.Padding.y, textWidth, lineHeight);
        }

        // 화면 좌표 → 부모(전체 화면) 왼쪽 아래 기준 px. 오른쪽·아래로 넘치면 마우스 반대쪽으로 붙인다.
        private void Place(Vector2 screenPosition, Vector2Int offset)
        {
            var parent = (RectTransform)transform;
            var canvas = GetComponentInParent<Canvas>();
            var camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPosition, camera, out var local)) return;

            var rect = parent.rect;
            float mouseX = local.x - rect.xMin;
            float mouseY = local.y - rect.yMin;
            int margin = Theme.Tooltip.ScreenMargin;

            float left = mouseX + offset.x;
            if (offset.x < 0) left = mouseX + offset.x - _size.x; // 왼쪽으로 띄우기
            if (left + _size.x > rect.width - margin) left = mouseX - Mathf.Abs(offset.x) - _size.x;
            if (left < margin) left = mouseX + Mathf.Abs(offset.x);

            float top = mouseY - offset.y;
            if (top - _size.y < margin) top = mouseY + Mathf.Abs(offset.y) + _size.y;

            int x = Mathf.Clamp(Mathf.FloorToInt(left), margin, Mathf.Max(margin, Mathf.FloorToInt(rect.width) - margin - _size.x));
            int y = Mathf.Clamp(Mathf.FloorToInt(top), _size.y + margin, Mathf.Max(_size.y + margin, Mathf.FloorToInt(rect.height) - margin));
            // 부모 왼쪽 아래 기준, 툴팁의 왼쪽 위 모서리를 (x, y)에 둔다.
            _body.anchorMin = _body.anchorMax = Vector2.zero;
            _body.pivot = new Vector2(0f, 1f);
            _body.anchoredPosition = new Vector2(x, y);
        }
    }
}
