// 세로 상태 바 하나(UI_SPEC 7-2, bar_ui README): 프레임(9-slice, 폭 16 고정) + 아래에서 차오르는 채움 + 윗면 하이라이트 + 위쪽 아이콘.
// 채움 높이는 정수 px로 맞추고(fillAmount = round(값 × 안쪽 높이) / 안쪽 높이), 값이 바뀔 때만 갱신한다.
using UnityEngine;
using UnityEngine.UI;

namespace VoxelMagic.UI
{
    public sealed class StatusBarView : MonoBehaviour
    {
        [SerializeField] private ThemedPanel _frame;
        [SerializeField] private Image _fill;
        [SerializeField] private Image _cap;
        [SerializeField] private Image _icon;

        private int _innerHeight;
        private int _shownPixels = -1;

        public RectTransform Rect => (RectTransform)transform;

        /// <summary>프레임 높이와 스프라이트를 정하고 요소를 배치한다. 자신의 크기는 아이콘을 포함한 폭·높이.</summary>
        public void Layout(StatusBarValues values, Sprite fill, Sprite cap, Sprite icon)
        {
            int frameWidth = _frame != null && _frameSprite(_frame) != null ? Mathf.RoundToInt(_frameSprite(_frame).rect.width) : 16;
            int height = values.BarHeight;
            // 아이콘은 바와 따로 키운다: 묶음 전체가 Scale배라 여기서는 아이콘배수 ÷ Scale 만큼만 곱한다.
            float iconFactor = values.IconScale / values.Scale;
            int iconWidth = icon != null ? Mathf.RoundToInt(icon.rect.width * iconFactor) : frameWidth;
            int iconHeight = icon != null ? Mathf.RoundToInt(icon.rect.height * iconFactor) : 0;
            // 자기 크기는 프레임 폭만. 아이콘이 프레임보다 넓으면 양옆으로 삐져나와도 된다(여백은 프레임 기준).
            int width = frameWidth;
            Rect.sizeDelta = new Vector2(width, height + values.IconGap + iconHeight);

            int frameX = 0;
            PixelLayout.PlaceBottomLeft((RectTransform)_frame.transform, frameX, 0, frameWidth, height);

            int inset = values.FillInset;
            _innerHeight = Mathf.Max(1, height - inset * 2);
            _fill.sprite = fill;
            _fill.type = Image.Type.Filled;
            _fill.fillMethod = Image.FillMethod.Vertical;
            _fill.fillOrigin = (int)Image.OriginVertical.Bottom;
            _fill.raycastTarget = false;
            int fillWidth = fill != null ? Mathf.RoundToInt(fill.rect.width) : frameWidth - inset * 2;
            PixelLayout.PlaceBottomLeft(_fill.rectTransform, frameX + inset, inset, fillWidth, _innerHeight);

            _cap.sprite = cap;
            _cap.raycastTarget = false;
            if (cap != null) _cap.rectTransform.sizeDelta = cap.rect.size;

            _icon.sprite = icon;
            _icon.enabled = icon != null;
            _icon.raycastTarget = false;
            PixelLayout.PlaceBottomLeft(_icon.rectTransform, (width - iconWidth) / 2, height + values.IconGap, iconWidth, iconHeight);

            _shownPixels = -1;
        }

        /// <summary>값(0~1)을 정수 px 채움으로 보인다. 같은 px이면 아무것도 바꾸지 않는다.</summary>
        public void SetValue(float ratio, int inset)
        {
            int pixels = Mathf.RoundToInt(Mathf.Clamp01(ratio) * _innerHeight);
            if (pixels == _shownPixels) return;
            _shownPixels = pixels;
            _fill.fillAmount = (float)pixels / _innerHeight;

            // 윗면 하이라이트는 채움 맨 위 2행과 겹치게, 채움이 하이라이트보다 낮으면 숨긴다(bar_ui README 5).
            var capRect = _cap.rectTransform;
            int capHeight = Mathf.RoundToInt(capRect.sizeDelta.y);
            bool showCap = _cap.sprite != null && pixels >= capHeight;
            _cap.enabled = showCap;
            if (showCap)
            {
                var fillRect = _fill.rectTransform;
                PixelLayout.PlaceBottomLeft(capRect, Mathf.RoundToInt(fillRect.anchoredPosition.x), inset + pixels - capHeight,
                    Mathf.RoundToInt(capRect.sizeDelta.x), capHeight);
            }
        }

        private static Sprite _frameSprite(ThemedPanel frame) => frame.GetComponent<Image>().sprite;
    }
}
