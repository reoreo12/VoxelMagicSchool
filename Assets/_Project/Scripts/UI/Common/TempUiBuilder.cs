// 임시 창(전달 고르기·확인 창)을 프리팹 없이 코드로 만드는 도우미. 정식 디자인이 나오면 테마 위젯으로 바꾼다.
// 글꼴은 씬에 이미 있는 TMP 글자의 것을 빌려 한글이 깨지지 않게 한다.
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VoxelMagic.UI
{
    internal static class TempUiBuilder
    {
        public static readonly Color Dim = new Color(0f, 0f, 0f, 0.5f);
        public static readonly Color Window = new Color(0.27f, 0.19f, 0.12f, 0.97f);
        public static readonly Color ButtonColor = new Color(0.45f, 0.33f, 0.2f, 1f);
        public static readonly Color Text = new Color(0.98f, 0.94f, 0.86f, 1f);

        /// <summary>부모 아래 전체를 덮는 자체 캔버스(정렬 순서 지정)와 어두운 배경을 만든다.</summary>
        public static RectTransform Root(Transform host, int sortingOrder)
        {
            var go = host.gameObject;
            var canvas = go.GetComponent<Canvas>();
            if (canvas == null) canvas = go.AddComponent<Canvas>(); // ??는 Unity의 가짜 null을 못 걸러서 쓰지 않는다
            canvas.overrideSorting = true;
            canvas.sortingOrder = sortingOrder;
            if (go.GetComponent<GraphicRaycaster>() == null) go.AddComponent<GraphicRaycaster>();
            var rt = (RectTransform)host;
            Stretch(rt);
            var image = go.GetComponent<Image>();
            if (image == null) image = go.AddComponent<Image>();
            image.color = Dim;
            return rt;
        }

        public static RectTransform Panel(string name, Transform parent, Color color, Vector2 size)
        {
            var rt = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.GetComponent<Image>().color = color;
            return rt;
        }

        public static TMP_Text Label(string name, Transform parent, TMP_FontAsset font, float size, TextAlignmentOptions align)
        {
            var text = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            text.transform.SetParent(parent, false);
            if (font != null) text.font = font;
            text.fontSize = size;
            text.alignment = align;
            text.color = Text;
            text.enableWordWrapping = true;
            text.raycastTarget = false;
            return text;
        }

        public static Button Button(string name, Transform parent, TMP_FontAsset font, float fontSize, out TMP_Text label)
        {
            var rt = Panel(name, parent, ButtonColor, new Vector2(160f, 52f));
            var button = rt.gameObject.AddComponent<Button>();
            button.targetGraphic = rt.GetComponent<Image>();
            label = Label("Label", rt, font, fontSize, TextAlignmentOptions.Center);
            Stretch(label.rectTransform);
            return button;
        }

        public static void Place(RectTransform rt, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
        }

        public static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        /// <summary>씬에 있는 TMP 글자의 글꼴(한글 포함)을 빌린다. 없으면 TMP 기본 글꼴.</summary>
        public static TMP_FontAsset FindFont(Transform near)
        {
            var root = near.root;
            foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
                if (text.font != null) return text.font;
            return TMP_Settings.defaultFontAsset;
        }
    }
}
