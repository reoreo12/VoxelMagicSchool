// UI 규칙 검사기(UI_SPEC 4장, 4-A-5). 선택한 UI 아래를 돌며 도트·성능 규칙 위반을 목록으로 돌려준다.
// 정수 위치·크기, 홀수 크기 + 가운데 기준점(0.5px 어긋남), 불필요한 Raycast Target, LayoutGroup·ContentSizeFitter·Mask, Point가 아닌 스프라이트, 설계 크기의 정수 배가 아닌 글자.
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VoxelMagic.UI;

namespace VoxelMagic.Tools.EditorTools.UiWorkbench
{
    public static class UiLayoutValidator
    {
        public readonly struct Issue
        {
            public Issue(Object target, string message)
            {
                Target = target;
                Message = message;
            }

            public Object Target { get; }
            public string Message { get; }
        }

        private const float Epsilon = 0.001f;

        public static List<Issue> Validate(GameObject root)
        {
            var issues = new List<Issue>();
            if (root == null) return issues;
            foreach (var rect in root.GetComponentsInChildren<RectTransform>(true))
            {
                if (rect.GetComponent<Canvas>() != null && rect.parent == null) continue; // 루트 캔버스 크기는 화면이 정함
                CheckRect(rect, issues);
            }
            foreach (var graphic in root.GetComponentsInChildren<Graphic>(true)) CheckGraphic(graphic, issues);
            foreach (var layout in root.GetComponentsInChildren<LayoutGroup>(true))
                issues.Add(new Issue(layout, "LayoutGroup 사용 (크기는 데이터에서 정수 px로 직접 계산)"));
            foreach (var fitter in root.GetComponentsInChildren<ContentSizeFitter>(true))
                issues.Add(new Issue(fitter, "ContentSizeFitter 사용"));
            foreach (var mask in root.GetComponentsInChildren<Mask>(true))
                issues.Add(new Issue(mask, "Mask 대신 RectMask2D를 쓴다"));
            return issues;
        }

        private static void CheckRect(RectTransform rect, List<Issue> issues)
        {
            bool stretchX = !Mathf.Approximately(rect.anchorMin.x, rect.anchorMax.x);
            bool stretchY = !Mathf.Approximately(rect.anchorMin.y, rect.anchorMax.y);
            if (!IsInt(rect.anchoredPosition.x) || !IsInt(rect.anchoredPosition.y))
                issues.Add(new Issue(rect, $"위치가 정수가 아님 {rect.anchoredPosition}"));
            if (!IsInt(rect.sizeDelta.x) || !IsInt(rect.sizeDelta.y))
                issues.Add(new Issue(rect, $"크기가 정수가 아님 {rect.sizeDelta}"));
            if (!stretchX && IsOdd(rect.sizeDelta.x) && Mathf.Approximately(rect.pivot.x, 0.5f))
                issues.Add(new Issue(rect, $"가로 {rect.sizeDelta.x} 홀수 + 가운데 기준점 → 0.5px 어긋남"));
            if (!stretchY && IsOdd(rect.sizeDelta.y) && Mathf.Approximately(rect.pivot.y, 0.5f))
                issues.Add(new Issue(rect, $"세로 {rect.sizeDelta.y} 홀수 + 가운데 기준점 → 0.5px 어긋남"));
            if (!IsAllowedScale(rect.localScale))
                issues.Add(new Issue(rect, $"Scale이 가로세로 같은 0.5 단위 배수가 아님 {rect.localScale}"));
        }

        private static void CheckGraphic(Graphic graphic, List<Issue> issues)
        {
            if (graphic.raycastTarget && !WantsRaycast(graphic))
                issues.Add(new Issue(graphic, "클릭을 받지 않는데 Raycast Target이 켜져 있음"));

            if (graphic is Image image && image.sprite != null)
            {
                var texture = image.sprite.texture;
                if (texture != null && texture.filterMode != FilterMode.Point)
                    issues.Add(new Issue(image, $"스프라이트 {image.sprite.name}의 필터가 Point가 아님"));
            }

            if (graphic is TMP_Text text && text.font != null)
            {
                var design = Mathf.RoundToInt(text.font.faceInfo.pointSize);
                var size = text.fontSize;
                if (design > 0 && (!IsInt(size) || Mathf.RoundToInt(size) % design != 0))
                    issues.Add(new Issue(text, $"글자 크기 {size}가 폰트 설계 크기 {design}의 정수 배가 아님"));
                if (text.enableAutoSizing) issues.Add(new Issue(text, "Auto Size 사용 (정수 배 크기가 깨짐)"));
            }
        }

        // 같은 오브젝트나 부모의 Selectable·스크롤·이벤트 처리기가 이 그래픽을 클릭 대상으로 쓰면 정상.
        private static bool WantsRaycast(Graphic graphic)
        {
            if (graphic.GetComponent<UiRaycastBlocker>() != null) return true;
            foreach (var handler in graphic.GetComponents<IEventSystemHandler>())
                if (!(handler is Graphic)) return true;
            var selectable = graphic.GetComponentInParent<Selectable>(true);
            if (selectable != null && (selectable.targetGraphic == graphic || selectable is Scrollbar)) return true;
            // 부모가 클릭·드래그를 받는 경우(예: 퀵슬롯 칸) 이 그래픽이 클릭 대상이 된다.
            for (var parent = graphic.transform.parent; parent != null; parent = parent.parent)
                if (parent.GetComponent<IPointerClickHandler>() != null || parent.GetComponent<IBeginDragHandler>() != null
                    || parent.GetComponent<IScrollHandler>() != null) return true;
            return false;
        }

        // 가로세로 같은 배수만 허용한다. 정수 배가 가장 선명하고, 0.5 단위(1.5 등)는 캔버스가 짝수 배일 때만 고르다(테마에서 사용자가 고른 값).
        private static bool IsAllowedScale(Vector3 scale)
        {
            float x = Mathf.Abs(scale.x);
            return IsInt(x * 2f) && x >= 0.5f && Mathf.Approximately(x, scale.y) && Mathf.Approximately(scale.z, 1f);
        }

        private static bool IsInt(float value) => Mathf.Abs(value - Mathf.Round(value)) < Epsilon;
        private static bool IsOdd(float value) => IsInt(value) && Mathf.RoundToInt(value) % 2 != 0;

        public static void SelectTarget(Issue issue)
        {
            if (issue.Target == null) return;
            Selection.activeObject = issue.Target is Component c ? c.gameObject : issue.Target;
            EditorGUIUtility.PingObject(Selection.activeObject);
        }
    }
}
