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
