// 정수 px 배치 도우미. LayoutGroup 대신 코드로 직접 위치·크기를 계산해 넣는다(UI_SPEC 4-A-5). 기준은 부모의 왼쪽 위.
using UnityEngine;

namespace VoxelMagic.UI
{
    public static class PixelLayout
    {
        private static readonly Vector2 TopLeft = new Vector2(0f, 1f);
        private static readonly Vector2 TopRight = new Vector2(1f, 1f);

        /// <summary>부모 왼쪽 위에서 오른쪽 x, 아래쪽 y만큼 떨어진 곳에 둔다. 크기가 0이면 크기는 건드리지 않는다.</summary>
        public static void PlaceTopLeft(RectTransform rect, int x, int y, int width, int height)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = TopLeft;
            rect.anchoredPosition = new Vector2(x, -y);
            if (width > 0 && height > 0) rect.sizeDelta = new Vector2(width, height);
        }

        /// <summary>부모 오른쪽 위에서 왼쪽 x, 아래쪽 y만큼 떨어진 곳에 둔다.</summary>
        public static void PlaceTopRight(RectTransform rect, int x, int y, int width, int height)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = TopRight;
            rect.anchoredPosition = new Vector2(-x, -y);
            if (width > 0 && height > 0) rect.sizeDelta = new Vector2(width, height);
        }

        /// <summary>부모 오른쪽 아래에서 왼쪽 x, 위쪽 y만큼 떨어진 곳에 둔다. 화면 오른쪽 끝은 항상 정수 px라 어긋나지 않는다.</summary>
        public static void PlaceBottomRight(RectTransform rect, int x, int y, int width, int height)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = new Vector2(-x, y);
            if (width > 0 && height > 0) rect.sizeDelta = new Vector2(width, height);
        }

        /// <summary>부모 왼쪽 아래에서 오른쪽 x, 위쪽 y만큼 떨어진 곳에 둔다.</summary>
        public static void PlaceBottomLeft(RectTransform rect, int x, int y, int width, int height)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero;
            rect.anchoredPosition = new Vector2(x, y);
            if (width > 0 && height > 0) rect.sizeDelta = new Vector2(width, height);
        }

        /// <summary>
        /// 부모 아래쪽 가운데에 둔다. 가운데 앵커(0.5)는 화면 폭이 홀수면 0.5px 어긋나므로, 왼쪽 끝에서 정수 px로 계산한다.
        /// 부모 폭이 바뀌면 다시 불러야 한다.
        /// </summary>
        public static void PlaceBottomCenter(RectTransform rect, float parentWidth, int y, int width, int height)
        {
            PlaceBottomLeft(rect, Mathf.FloorToInt((parentWidth - width) / 2f), y, width, height);
        }

        /// <summary>부모를 꽉 채운다.</summary>
        public static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        /// <summary>안쪽 칸 높이 inner를 바깥 높이 outer 가운데에 둘 때 위쪽 여백(내림, 정수).</summary>
        public static int CenterOffset(int outer, int inner) => (outer - inner) / 2;
    }
}
