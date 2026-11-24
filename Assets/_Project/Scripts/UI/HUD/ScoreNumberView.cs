// 점수 숫자 조합(UI_SPEC 9-2): v2 숫자 스프라이트를 피치 9로 이어 붙이고(전체 폭 9n+2), 자릿수에 맞는 두 줄 밑줄을 가로 가운데로 바로 아래에 붙인다.
// 숫자 오브젝트는 최대 자릿수만큼 미리 만들어 두고 켜고 끈다(만들고 지우지 않음, 4-A-5). 값이 같으면 아무것도 바꾸지 않는다.
// 외곽선 숫자는 이웃 외곽선이 겹치므로 왼쪽 자리부터 그린다(자식 순서 = 왼쪽 → 오른쪽).
using UnityEngine;
using UnityEngine.UI;

namespace VoxelMagic.UI
{
    public sealed class ScoreNumberView : MonoBehaviour
    {
        [SerializeField] private Image[] _digits = new Image[4];
        [SerializeField] private Image _underline;

        private int _shownValue = int.MinValue;

        public RectTransform Rect => (RectTransform)transform;

        /// <summary>숫자 줄(밑줄 제외) 폭·높이와 밑줄을 포함한 전체 높이. 배치 계산용.</summary>
        public Vector2Int DigitsSize { get; private set; }
        public int TotalHeight { get; private set; }

        /// <summary>테마의 숫자·밑줄 그림이 바뀌었을 때 다음 Show에서 다시 그리게 한다.</summary>
        public void Invalidate() => _shownValue = int.MinValue;

        /// <summary>값을 숫자 스프라이트로 보인다. written이면 필기용 밑줄(v1), 아니면 실기용(v2).</summary>
        public void Show(int value, bool written, ScoreValues values)
        {
            int max = Mathf.Min(values.MaxDigits, _digits.Length);
            int limit = Pow10(max) - 1;
            value = Mathf.Clamp(value, 0, limit);
            if (value == _shownValue) return;
            _shownValue = value;

            int count = DigitCount(value);
            int digitHeight = 0, cellWidth = 0;
            // 왼쪽 자리부터 채운다(그리는 순서도 왼쪽 → 오른쪽). 숫자를 문자열로 바꾸지 않아 GC가 없다.
            for (int slot = 0; slot < _digits.Length; slot++)
            {
                var image = _digits[slot];
                bool used = slot < count;
                image.enabled = used;
                if (!used) continue;
                var sprite = values.Digit(value / Pow10(count - 1 - slot) % 10);
                image.sprite = sprite;
                image.raycastTarget = false;
                if (sprite == null) continue;
                cellWidth = Mathf.RoundToInt(sprite.rect.width);
                digitHeight = Mathf.RoundToInt(sprite.rect.height);
                PixelLayout.PlaceTopLeft(image.rectTransform, slot * values.DigitPitch, 0, cellWidth, digitHeight);
            }

            int digitsWidth = (count - 1) * values.DigitPitch + cellWidth;
            DigitsSize = new Vector2Int(digitsWidth, digitHeight);

            var line = values.Underline(written, count);
            _underline.sprite = line;
            _underline.enabled = line != null;
            _underline.raycastTarget = false;
            int lineHeight = 0;
            if (line != null)
            {
                int lineWidth = Mathf.RoundToInt(line.rect.width);
                lineHeight = Mathf.RoundToInt(line.rect.height);
                // 가로 가운데 정렬은 내림으로 정수 px(홀수 폭 0.5px 어긋남 방지).
                // 외곽선이 있으면 숫자 아래 외곽선과 밑줄 위 외곽선만큼(2 × 두께) 올려 원래 밑줄 자리에 둔다.
                int lineY = digitHeight - values.OutlinePx * 2;
                PixelLayout.PlaceTopLeft(_underline.rectTransform, Mathf.FloorToInt((digitsWidth - lineWidth) / 2f), lineY, lineWidth, lineHeight);
            }
            TotalHeight = line != null ? Mathf.Max(digitHeight, digitHeight - values.OutlinePx * 2 + lineHeight) : digitHeight;
            Rect.sizeDelta = new Vector2(digitsWidth, TotalHeight);
        }

        private static int DigitCount(int value)
        {
            int count = 1;
            while (value >= 10) { value /= 10; count++; }
            return count;
        }

        private static int Pow10(int n)
        {
            int result = 1;
            for (int i = 0; i < n; i++) result *= 10;
            return result;
        }
    }
}
