// 픽셀 글꼴은 줄 높이 안에서 글자 모양이 위쪽에 붙어 있어(갈무리11: 위 1px, 아래 4px) 줄 높이로 가운데 맞추면 위로 치우친다.
// 실제 글자 모양(글리프)의 위·아래를 재서 칸 안 세로 가운데에 놓는 도우미.
using TMPro;
using UnityEngine;

namespace VoxelMagic.UI
{
    public static class UiTextInk
    {
        /// <summary>보이는 글자들의 글리프 모양 맨 위·맨 아래 (글자 칸 기준 y, 위가 +).</summary>
        public static bool TryInkRows(TMP_Text label, out float top, out float bottom)
        {
            top = float.MinValue;
            bottom = float.MaxValue;
            var info = label.textInfo;
            for (int i = 0; i < info.characterCount; i++)
            {
                var ch = info.characterInfo[i];
                if (!ch.isVisible || ch.textElement == null || ch.textElement.glyph == null) continue;
                var metrics = ch.textElement.glyph.metrics;
                float glyphTop = ch.baseLine + metrics.horizontalBearingY * ch.scale;
                top = Mathf.Max(top, glyphTop);
                bottom = Mathf.Min(bottom, glyphTop - metrics.height * ch.scale);
            }
            return top > bottom;
        }

        /// <summary>
        /// 글자 칸(PlaceTopLeft로 놓인, 위 끝 = 0)을 세로로 옮겨 글자 모양이 부모 기준 [areaTop, areaTop + areaHeight) 가운데에 오게 한다.
        /// 글자가 없으면 그대로 둔다. 정수 px.
        /// </summary>
        public static void CenterVertically(TMP_Text label, int areaTop, int areaHeight)
        {
            label.ForceMeshUpdate();
            if (!TryInkRows(label, out float inkTop, out float inkBottom)) return;
            var rect = label.rectTransform;
            int offset = Mathf.RoundToInt(rect.rect.yMax - inkTop); // 칸 위 끝 → 글자 모양 위 끝
            int height = Mathf.RoundToInt(inkTop - inkBottom);
            int y = areaTop + (areaHeight - height) / 2 - offset;
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, -y);
        }
    }
}
