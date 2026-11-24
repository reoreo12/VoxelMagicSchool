// 책 페이지 넘김 연출 한 번(UI_SPEC 5-3, book_ui/README): 9프레임 × 45ms, 폭 = round(W × |cos(πp)|), 위로 round(4 × sin(πp)) 들어 올림.
// k ≤ 3은 출발한 쪽, k = 4는 책등 위 얇은 단면, k ≥ 5는 반대편(좌우 반전). 그림자는 종이 바깥 가장자리에 붙이고 페이지 안쪽에서 자른다.
// 시간 정지 중에도 돌도록 unscaled time을 쓴다. 정수 px만 쓰고 소수 배율은 쓰지 않는다.
using System;
using UnityEngine;
using UnityEngine.UI;

namespace VoxelMagic.UI
{
    public sealed class BookPageTurner : MonoBehaviour
    {
        [SerializeField] private Image _leaf;
        [SerializeField, Tooltip("RectMask2D: 페이지 종이 안쪽만 보이게 그림자를 자른다")] private RectTransform _shadowClip;
        [SerializeField] private Image _shadowA;
        [SerializeField] private Image _shadowB;

        private BookValues _values;
        private bool _forward;
        private int _frame = -1;
        private float _elapsed;
        private Action<int> _onFrame;
        private Action _onDone;

        public bool IsPlaying => _frame >= 0;

        /// <summary>한 장 넘긴다. forward = 오른쪽에서 왼쪽으로(다음 탭). onFrame(k)는 각 프레임이 보일 때, onDone은 끝난 뒤.</summary>
        public void Play(BookValues values, bool forward, Action<int> onFrame, Action onDone)
        {
            _values = values;
            _forward = forward;
            _onFrame = onFrame;
            _onDone = onDone;
            _elapsed = 0f;
            _frame = 0;
            gameObject.SetActive(true);
            ShowFrame(0);
        }

        /// <summary>연출을 바로 끝낸다(책을 닫을 때). onDone은 부르지 않는다.</summary>
        public void Stop()
        {
            _frame = -1;
            _onFrame = null;
            _onDone = null;
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (_frame < 0) return;
            _elapsed += Time.unscaledDeltaTime;
            float step = _values.TurnFrameSeconds;
            while (_frame >= 0 && _elapsed >= step)
            {
                _elapsed -= step;
                int next = _frame + 1;
                if (next >= _values.TurnFrames)
                {
                    var done = _onDone;
                    Stop();
                    done?.Invoke();
                    return;
                }
                _frame = next;
                ShowFrame(next);
            }
        }

        private void ShowFrame(int k)
        {
            var v = _values;
            int frames = v.TurnFrames;
            float p = (k + 1) / (float)(frames + 1);
            var page = v.PageSize;
            int spine = v.SpineWidth;
            int inset = v.PaperInset;
            int hinge = page.x + spine / 2;
            int fullWidth = page.x + spine / 2 - inset; // 책등 가운데에서 반대편 표지 안쪽까지 (W)
            int height = page.y - inset * 2;
            int lift = Mathf.RoundToInt(v.TurnLift * Mathf.Sin(Mathf.PI * p));
            int y = inset - lift;
            int middle = (frames - 1) / 2;

            PixelLayout.PlaceTopLeft(_shadowClip, inset, inset, page.x * 2 + spine - inset * 2, height);
            var shadowSprite = v.TurnShadow;
            int shadowWidth = shadowSprite != null ? Mathf.RoundToInt(shadowSprite.rect.width) : 8;

            if (k == middle)
            {
                // 가장 얇은 순간: 책등 위에 단면을 세우고 그림자는 양쪽에.
                var edge = v.LeafEdge;
                int edgeWidth = edge != null ? Mathf.RoundToInt(edge.rect.width) : 6;
                SetLeaf(edge, hinge - edgeWidth / 2, y, edgeWidth, height, false);
                SetShadow(_shadowA, shadowSprite, hinge + edgeWidth / 2 - inset, shadowWidth, height, false);
                SetShadow(_shadowB, shadowSprite, hinge - edgeWidth / 2 - inset, shadowWidth, height, true);
            }
            else
            {
                float ratio = Mathf.Abs(Mathf.Cos(Mathf.PI * p));
                int width = Mathf.Max(1, Mathf.RoundToInt(fullWidth * ratio));
                // 앞으로 넘김은 오른쪽에서 출발, 뒤로 넘김은 왼쪽에서 출발. 가운데 지나면 반대편.
                bool onRight = (k < middle) == _forward;
                var sprite = v.Leaf(ratio);
                if (onRight)
                {
                    SetLeaf(sprite, hinge, y, width, height, false);
                    SetShadow(_shadowA, shadowSprite, hinge + width - inset, shadowWidth, height, false);
                }
                else
                {
                    SetLeaf(sprite, hinge, y, width, height, true);
                    SetShadow(_shadowA, shadowSprite, hinge - width - inset, shadowWidth, height, true);
                }
                _shadowB.enabled = false;
            }
            _onFrame?.Invoke(k);
        }

        // 왼쪽 위 기준. flip이면 x를 오른쪽 끝(경첩)으로 보고 왼쪽으로 펼친다(Scale X = −1, 정수 반전).
        private void SetLeaf(Sprite sprite, int x, int y, int width, int height, bool flip)
        {
            _leaf.sprite = sprite;
            _leaf.type = Image.Type.Sliced;
            _leaf.enabled = sprite != null;
            _leaf.raycastTarget = false;
            PixelLayout.PlaceTopLeft(_leaf.rectTransform, x, y, width, height);
            _leaf.rectTransform.localScale = new Vector3(flip ? -1f : 1f, 1f, 1f);
        }

        // 그림자 칸 안(종이 안쪽) 좌표. flip이면 x에서 왼쪽으로 펼친다.
        private static void SetShadow(Image image, Sprite sprite, int x, int width, int height, bool flip)
        {
            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.enabled = sprite != null;
            image.raycastTarget = false;
            PixelLayout.PlaceTopLeft(image.rectTransform, x, 0, width, height);
            image.rectTransform.localScale = new Vector3(flip ? -1f : 1f, 1f, 1f);
        }
    }
}
