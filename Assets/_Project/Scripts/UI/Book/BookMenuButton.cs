// 책 페이지 안의 글자 버튼: 깔끔한 9-slice 버튼 그림(평상시·마우스 올림·누름) + 가운데 글자. 누르면 Clicked로 알린다.
// 글자는 실제로 그려진 글자 모양 기준으로 왼쪽·오른쪽 여백이 같고 위·아래 여백이 같도록 정수 px로 맞춘다(사용자 요청).
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VoxelMagic.UI
{
    public sealed class BookMenuButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Image _background;
        [SerializeField, Tooltip("가운데 글자 (보통 / 마우스 올림 = 굵게)")] private ThemedText _label;

        private BookValues _values;
        private Vector2Int _baseSize;
        private bool _hover;
        private bool _pressed;

        public RectTransform Rect => (RectTransform)transform;
        public event Action Clicked;

        /// <summary>부모 왼쪽 위 기준 정수 px에 놓고 글자를 넣는다.</summary>
        public void Show(string text, BookValues values, int x, int y, int width, int height)
        {
            _values = values;
            PixelLayout.PlaceTopLeft(Rect, x, y, width, height);
            _baseSize = new Vector2Int(width, height);
            _label.SetText(text);
            Refresh();
        }

        private void Refresh()
        {
            if (_values == null) return;
            _background.sprite = _values.PageButton(_hover, _pressed);
            _background.type = Image.Type.Sliced;
            _background.enabled = _background.sprite != null;
            _background.raycastTarget = true;
            PixelLayout.Stretch(_background.rectTransform);
            _label.SetStyle(_hover ? UiTextStyle.BodyBold : UiTextStyle.Body);
            CenterLabel();
        }

        // 글자 칸을 버튼 전체로 둔 뒤, 실제 글자 모양(textBounds)이 버튼 정가운데에 오도록 옮긴다.
        // 남는 여백이 홀수면 양쪽을 똑같이 맞출 수 없어서, 버튼 폭·높이를 1px 줄여 짝수로 만든다(왼쪽 위 기준이라 오른쪽·아래 끝만 1px 들어감).
        private void CenterLabel()
        {
            var size = _baseSize.x > 0 ? _baseSize : Vector2Int.RoundToInt(Rect.sizeDelta);
            Rect.sizeDelta = size;
            var rect = (RectTransform)_label.transform;
            PixelLayout.PlaceTopLeft(rect, 0, 0, size.x, size.y);
            var tmp = _label.GetComponent<TMP_Text>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.ForceMeshUpdate();
            var bounds = tmp.textBounds;
            if (bounds.size.x <= 0f) return;
            // 글자 칸 왼쪽 위(피벗) 기준: 왼쪽 = min.x, 위 = −max.y. 글자 모양의 왼쪽·위 끝이 정수 px에 오도록 칸을 옮긴다
            // (칸 위치는 소수가 될 수 있지만 도트 글자는 화면 격자에 맞는다).
            int glyphWidth = Mathf.RoundToInt(bounds.size.x), glyphHeight = Mathf.RoundToInt(bounds.size.y);
            if ((size.x - glyphWidth) % 2 != 0) size.x -= 1;
            if ((size.y - glyphHeight) % 2 != 0) size.y -= 1;
            Rect.sizeDelta = size;
            PixelLayout.Stretch(_background.rectTransform);
            int wantLeft = (size.x - glyphWidth) / 2;
            int wantTop = (size.y - glyphHeight) / 2;
            rect.anchoredPosition = new Vector2(wantLeft - bounds.min.x, -(wantTop - (-bounds.max.y)));
        }

        public void OnPointerEnter(PointerEventData eventData) { _hover = true; Refresh(); }
        public void OnPointerExit(PointerEventData eventData) { _hover = false; _pressed = false; Refresh(); }
        public void OnPointerDown(PointerEventData eventData) { if (eventData.button == PointerEventData.InputButton.Left) { _pressed = true; Refresh(); } }
        public void OnPointerUp(PointerEventData eventData) { _pressed = false; Refresh(); }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) Clicked?.Invoke();
        }
    }
}
