// 퀵슬롯 선택 프레임(UI_SPEC 7-1): Sprout 시트의 중간 색 모서리 4조각으로 칸을 감싼다.
// 다른 칸으로 바뀌면 보간으로 미끄러지듯 옮겨 가고(13장), 매 프레임 위치를 정수 px로 반올림한다. 시간 정지 중에도 unscaled time으로 움직인다.
// 움직이는 동안만 위치가 바뀌므로 자체 캔버스를 두어 퀵슬롯 바 전체가 다시 그려지지 않게 한다(4-A-5).
using UnityEngine;
using UnityEngine.UI;

namespace VoxelMagic.UI
{
    public sealed class SelectionFrameView : MonoBehaviour
    {
        [SerializeField] private Image _topLeft;
        [SerializeField] private Image _topRight;
        [SerializeField] private Image _bottomLeft;
        [SerializeField] private Image _bottomRight;

        private Vector2 _from;
        private Vector2 _to;
        private float _elapsed;
        private float _duration;
        private bool _moving;

        private RectTransform Rect => (RectTransform)transform;

        /// <summary>모서리 스프라이트와 프레임 크기(칸 + 바깥 여백)를 정한다.</summary>
        public void Setup(UiTheme theme, Vector2Int slotSize)
        {
            var values = theme.QuickSlots;
            int outset = values.SelectionOutset;
            var size = new Vector2Int(slotSize.x + outset * 2, slotSize.y + outset * 2);
            Rect.anchorMin = Rect.anchorMax = Rect.pivot = new Vector2(0f, 1f);
            Rect.sizeDelta = size;
            SetCorner(_topLeft, values.SelectionTopLeft, new Vector2(0f, 1f));
            SetCorner(_topRight, values.SelectionTopRight, new Vector2(1f, 1f));
            SetCorner(_bottomLeft, values.SelectionBottomLeft, new Vector2(0f, 0f));
            SetCorner(_bottomRight, values.SelectionBottomRight, new Vector2(1f, 0f));
        }

        /// <summary>칸의 왼쪽 위 위치(바 기준)로 옮긴다. animate가 false면 바로 옮긴다.</summary>
        public void MoveTo(Vector2 slotTopLeft, int outset, float duration, bool animate)
        {
            var target = new Vector2(slotTopLeft.x - outset, slotTopLeft.y + outset);
            if (!animate || duration <= 0f || !Application.isPlaying)
            {
                _moving = false;
                Rect.anchoredPosition = target;
                return;
            }
            _from = Rect.anchoredPosition;
            _to = target;
            _elapsed = 0f;
            _duration = duration;
            _moving = true;
        }

        private void Update()
        {
            if (!_moving) return;
            _elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(_elapsed / _duration);
            t = t * t * (3f - 2f * t); // 부드럽게 출발·도착
            var p = Vector2.Lerp(_from, _to, t);
            Rect.anchoredPosition = new Vector2(Mathf.Round(p.x), Mathf.Round(p.y));
            if (t >= 1f) _moving = false;
        }

        private static void SetCorner(Image image, Sprite sprite, Vector2 corner)
        {
            if (image == null) return;
            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.raycastTarget = false;
            var rect = image.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = corner;
            rect.anchoredPosition = Vector2.zero;
            if (sprite != null) rect.sizeDelta = sprite.rect.size;
        }
    }
}
