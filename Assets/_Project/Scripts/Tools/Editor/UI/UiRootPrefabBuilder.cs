// UI 루트 프리팹(정수 배율 캔버스 + 층별 캔버스 + 화면 라우터 + 입력 막기 판)과 테마·문구 표 에셋을 만든다.
// 프리팹이 이미 있으면 덮어쓰지 않는다(화면을 붙인 뒤에도 안전). 다시 만들려면 프리팹을 지우고 실행한다.
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using VoxelMagic.Core;
using VoxelMagic.UI;

namespace VoxelMagic.Tools.EditorTools.UiWorkbench
{
    public static class UiRootPrefabBuilder
    {
        public const string ThemePath = "Assets/_Project/Data/UI/UiTheme.asset";
        public const string TextTablePath = "Assets/_Project/Data/UI/Texts_ko.asset";
        public const string PrefabPath = "Assets/_Project/Prefabs/UI/UI_Root.prefab";

        [MenuItem("VoxelMagic/UI/UI 루트 프리팹 만들기")]
        private static void BuildFromMenu() => Selection.activeObject = GetOrCreatePrefab();

        public static UiTheme GetOrCreateTheme() => GetOrCreateAsset<UiTheme>(ThemePath);
        public static TextTable GetOrCreateTextTable() => GetOrCreateAsset<TextTable>(TextTablePath);

        public static GameObject GetOrCreatePrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (existing != null) return existing;

            var theme = GetOrCreateTheme();
            GetOrCreateTextTable();

            var root = new GameObject("UI_Root", typeof(RectTransform));
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.referencePixelsPerUnit = theme.Canvas.ReferencePixelsPerUnit;
            root.AddComponent<GraphicRaycaster>();
            SetTheme(root.AddComponent<PixelCanvasScaler>(), theme);

            CreateLayer(root.transform, UiLayer.HudStatic, theme, raycast: true);
            CreateLayer(root.transform, UiLayer.HudDynamic, theme, raycast: true);
            // 완료 알림의 X 버튼이 있어서 클릭을 받는다(입력 막기 판은 Popup 층이라 영향 없음).
            CreateLayer(root.transform, UiLayer.HudAnimated, theme, raycast: true);
            var popup = CreateLayer(root.transform, UiLayer.Popup, theme, raycast: true);
            CreateLayer(root.transform, UiLayer.Overlay, theme, raycast: true);

            var blocker = new GameObject("InputBlocker", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(UiRaycastBlocker));
            blocker.transform.SetParent(popup, false);
            Stretch((RectTransform)blocker.transform);
            blocker.SetActive(false);

            var router = popup.gameObject.AddComponent<UiScreenRouter>();
            var so = new SerializedObject(router);
            so.FindProperty("_theme").objectReferenceValue = theme;
            so.FindProperty("_screenParent").objectReferenceValue = popup;
            so.FindProperty("_inputBlocker").objectReferenceValue = blocker.GetComponent<Image>();
            so.ApplyModifiedPropertiesWithoutUndo();

            Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath));
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static RectTransform CreateLayer(Transform parent, UiLayer layer, UiTheme theme, bool raycast)
        {
            var go = new GameObject(layer.ToString(), typeof(RectTransform));
            go.transform.SetParent(parent, false);
            Stretch((RectTransform)go.transform);
            go.AddComponent<Canvas>();
            if (raycast) go.AddComponent<GraphicRaycaster>();
            var layerCanvas = go.AddComponent<UiLayerCanvas>();
            var so = new SerializedObject(layerCanvas);
            so.FindProperty("_theme").objectReferenceValue = theme;
            so.FindProperty("_layer").enumValueIndex = (int)layer;
            so.ApplyModifiedPropertiesWithoutUndo();
            layerCanvas.Apply();
            return (RectTransform)go.transform;
        }

        private static void SetTheme(Object target, UiTheme theme)
        {
            var so = new SerializedObject(target);
            so.FindProperty("_theme").objectReferenceValue = theme;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static T GetOrCreateAsset<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            return asset;
        }
    }
}
