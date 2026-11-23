// 책 탭의 "장 표지"(사용자 결정 2026-11-23): 고를 것이 없는 탭의 왼쪽 페이지에 탭 아이콘(정수 배로 크게) + 탭 이름 + 구분선 + 한 줄 문구를 세로 가운데에 둔다.
// 규칙: 모든 탭은 왼쪽 = 고르는 곳 또는 장 표지, 오른쪽 = 내용. 메뉴·환경 설정처럼 내용이 적은 탭이 이 표지를 쓴다.
using UnityEngine;
using UnityEngine.UI;

namespace VoxelMagic.UI
{
    public sealed class BookCoverView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private ThemedText _title;
        [SerializeField] private Image _line;
        [SerializeField] private ThemedText _caption;

        /// <summary>size(페이지 내용 칸) 안 세로 가운데에 놓는다. icon은 탭 아이콘(12×12), 정수 배로 키운다.</summary>
        public void Show(Sprite icon, string title, string caption, UiTheme theme, Vector2Int size)
        {
            var values = theme.Book;
            int scale = values.CoverIconScale;
            int iconW = icon != null ? Mathf.RoundToInt(icon.rect.width) * scale : 0;
            int iconH = icon != null ? Mathf.RoundToInt(icon.rect.height) * scale : 0;
            int titleLine = LineOf(theme, UiTextStyle.BodyBold);
            int captionLine = LineOf(theme, values.CoverCaptionStyle);
            bool hasCaption = !string.IsNullOrEmpty(caption);

            int total = iconH + values.CoverIconGap + titleLine;
            if (hasCaption) total += values.CoverLineGap * 2 + 1 + captionLine;
            int y = Mathf.Max(0, (size.y - total) / 2);

            ((RectTransform)transform).sizeDelta = size;
            PixelLayout.PlaceTopLeft((RectTransform)transform, 0, 0, size.x, size.y);

            _icon.sprite = icon;
            _icon.enabled = icon != null;
            _icon.raycastTarget = false;
            PixelLayout.PlaceTopLeft(_icon.rectTransform, (size.x - iconW) / 2, y, iconW, iconH);
            y += iconH + values.CoverIconGap;

            _title.SetStyle(UiTextStyle.BodyBold);
            _title.SetText(title);
            PixelLayout.PlaceTopLeft((RectTransform)_title.transform, 0, y, size.x, titleLine);
            y += titleLine;

            if (_line.gameObject.activeSelf != hasCaption) _line.gameObject.SetActive(hasCaption);
            if (_caption.gameObject.activeSelf != hasCaption) _caption.gameObject.SetActive(hasCaption);
            if (!hasCaption) return;
            y += values.CoverLineGap;
            _line.sprite = null;
            _line.color = values.LineColor;
            _line.raycastTarget = false;
            int lineWidth = Mathf.Min(size.x, values.CoverLineWidth);
            PixelLayout.PlaceTopLeft(_line.rectTransform, (size.x - lineWidth) / 2, y, lineWidth, 1);
            y += 1 + values.CoverLineGap;

            _caption.SetStyle(values.CoverCaptionStyle);
            _caption.SetText(caption);
            _caption.SetColorOverride(true, values.CoverCaptionColor);
            PixelLayout.PlaceTopLeft((RectTransform)_caption.transform, 0, y, size.x, captionLine);
        }

        private static int LineOf(UiTheme theme, UiTextStyle id)
        {
            var style = theme.TextStyle(id);
            return style != null ? style.LineHeightFrom(theme.Fonts) : 16;
        }
    }
}
