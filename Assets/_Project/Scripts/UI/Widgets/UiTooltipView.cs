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

        /// <summary>screenPosition(화면 px) 옆에 글자를 띄운다. offset은 확대 뒤 화면 기준(x +오른쪽, y +아래).
        /// above면 대상 위쪽에 offset.y만큼 띄운다(화면 아래 HUD용, 위로 넘치면 아래로). centered면 가로 가운데를 screenPosition에 맞춘다.</summary>
        public void Show(string text, Vector2 screenPosition, Vector2Int offset, bool above = false, bool centered = false)
        {
            if (Theme == null || _body == null) return;
            _body.gameObject.SetActive(true);
            if (_shown != text)
            {
                _shown = text;
                _text.SetText(text);
                Resize();
            }
            Place(screenPosition, offset, above, centered);
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

        // 상자 폭 = 실제 글자 모양 폭 + 좌우 여백. 글자 간격(12px)으로 재면 마지막 글자 모양이 좁을 때(예: '기' 10px) 오른쪽 여백이 넓어진다.
        // 글자 칸 안에서 글자가 0.5px 밀려 그려지면 도트가 들쭉날쭉 찍히므로, 첫 글자 모양 왼쪽 끝이 정확히 여백 위치(정수 px)에 오도록 칸을 옮긴다.
        private void Resize()
        {
            var values = Theme.Tooltip;
            var label = _text.GetComponent<TMP_Text>();
            label.alignment = TextAlignmentOptions.TopLeft; // 가운데 정렬이면 폭이 홀짝에 따라 글자가 0.5px 밀린다
            var preferred = label.GetPreferredValues(_shown);
            int lineHeight = Mathf.CeilToInt(label.fontSize * (label.font != null ? label.font.faceInfo.lineHeight / label.font.faceInfo.pointSize : 1.4f));
            int textWidth = Mathf.CeilToInt(preferred.x);
            var textRect = (RectTransform)_text.transform;
            PixelLayout.PlaceTopLeft(textRect, values.Padding.x, values.Padding.y, textWidth + 1, lineHeight);

            float inkLeft = 0f, inkWidth = textWidth;
            if (TryInkColumns(label, out float left, out float right))
            {
                inkLeft = left;
                inkWidth = right - left;
            }
            int width = Mathf.RoundToInt(inkWidth) + values.Padding.x * 2;
            _size = new Vector2Int(width, lineHeight + values.Padding.y * 2);
            _body.sizeDelta = _size;
            PixelLayout.Stretch((RectTransform)_background.transform);
            textRect.anchoredPosition = new Vector2(values.Padding.x - inkLeft, textRect.anchoredPosition.y);
        }

        // 보이는 글자들의 실제 도트 왼쪽·오른쪽 끝 (글자 칸 왼쪽 기준 px). 글리프 칸 안에도 빈 열이 있을 수 있어(예: '쑥' 왼쪽 1열)
        // 아틀라스가 읽히면 첫·끝 글자의 칸에서 실제로 칠해진 열을 찾는다. 못 읽으면 글리프 칸 끝을 쓴다.
        private static bool TryInkColumns(TMP_Text label, out float left, out float right)
        {
            left = float.MaxValue;
            right = float.MinValue;
            label.ForceMeshUpdate();
            var info = label.textInfo;
            for (int i = 0; i < info.characterCount; i++)
            {
                var ch = info.characterInfo[i];
                if (!ch.isVisible || ch.textElement == null || ch.textElement.glyph == null) continue;
                var glyph = ch.textElement.glyph;
                float glyphLeft = ch.origin + glyph.metrics.horizontalBearingX * ch.scale;
                InkSpan(ch, glyph, out int first, out int last);
                left = Mathf.Min(left, glyphLeft + first * ch.scale);
                right = Mathf.Max(right, glyphLeft + (last + 1) * ch.scale);
            }
            if (right <= left) return false;
            left -= label.rectTransform.rect.xMin; // 칸 왼쪽 기준
            right -= label.rectTransform.rect.xMin;
            return true;
        }

        // 글리프 칸(아틀라스) 안에서 칠해진 첫·끝 열. 읽을 수 없으면 칸 전체.
        private static void InkSpan(TMP_CharacterInfo ch, UnityEngine.TextCore.Glyph glyph, out int first, out int last)
        {
            var rect = glyph.glyphRect;
            first = 0;
            last = rect.width - 1;
            var font = ch.fontAsset;
            if (font == null || font.atlasTextures == null || glyph.atlasIndex >= font.atlasTextures.Length) return;
            var atlas = font.atlasTextures[glyph.atlasIndex];
            if (atlas == null || !atlas.isReadable || rect.width <= 0 || rect.height <= 0) return;
            var pixels = atlas.GetPixels(rect.x, rect.y, rect.width, rect.height);
            int found = -1, end = -1;
            for (int x = 0; x < rect.width; x++)
            {
                bool ink = false;
                for (int y = 0; y < rect.height && !ink; y++) ink = pixels[y * rect.width + x].a > 0.5f;
                if (!ink) continue;
                if (found < 0) found = x;
                end = x;
            }
            if (found < 0) return;
            first = found;
            last = end;
        }

        // 화면 좌표 → 부모(전체 화면) 왼쪽 아래 기준 px. 오른쪽·아래로 넘치면 마우스 반대쪽으로 붙인다.
        private void Place(Vector2 screenPosition, Vector2Int offset, bool above, bool centered)
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

            float top = above ? mouseY + Mathf.Abs(offset.y) + _size.y : mouseY - offset.y;
            if (above && top > rect.height - margin) top = mouseY - Mathf.Abs(offset.y);
            if (top - _size.y < margin) top = mouseY + Mathf.Abs(offset.y) + _size.y;

            float x = Mathf.Clamp(Mathf.FloorToInt(left), margin, Mathf.Max(margin, Mathf.FloorToInt(rect.width) - margin - _size.x));
            if (centered)
            {
                // 가운데 맞춤: 한글 글자 폭은 홀수라 1배 px로는 짝수 폭 대상(칸)과 중심을 맞출 수 없다.
                // 실제 화면 픽셀 단위(배율 2면 0.5)까지 옮겨 중심을 맞춘다. 배율이 1이면 정수 px로 내림.
                float scale = canvas != null ? Mathf.Max(1f, canvas.rootCanvas.scaleFactor) : 1f;
                float centeredLeft = Mathf.Round((mouseX + offset.x - _size.x * 0.5f) * scale) / scale;
                x = Mathf.Clamp(centeredLeft, margin, Mathf.Max(margin, rect.width - margin - _size.x));
            }
            int y = Mathf.Clamp(Mathf.FloorToInt(top), _size.y + margin, Mathf.Max(_size.y + margin, Mathf.FloorToInt(rect.height) - margin));
            // 부모 왼쪽 아래 기준, 툴팁의 왼쪽 위 모서리를 (x, y)에 둔다.
            _body.anchorMin = _body.anchorMax = Vector2.zero;
            _body.pivot = new Vector2(0f, 1f);
            _body.anchoredPosition = new Vector2(x, y);
        }
    }
}
