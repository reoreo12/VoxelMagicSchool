// 획득 창 프리팹 UI_Acquire를 만들어 UI_Root의 HudAnimated 층에 넣는다: 밝은 종이 창 + 모델 그림(RawImage) + 아이콘 자리 + 제목·설명.
// 테마에 툰·외곽선 셰이더를 채운다. 씬의 옛 임시 획득 창(AcquisitionPopup)은 UI_Root가 있는 씬에서 지운다(따로 메뉴).
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using VoxelMagic.UI;

namespace VoxelMagic.Tools.EditorTools.UiWorkbench
{
    public static class UiAcquireBuilder
    {
        private const string AcquireName = "UI_Acquire";
        private const string ToonShaderPath = "Assets/_Project/Shaders/ToonPreview.shader";
        private const string OutlineShaderPath = "Assets/_Project/Shaders/UIPixelOutline.shader";

        [MenuItem("VoxelMagic/UI/화면 만들기/획득 창")]
        public static void Build()
        {
            var theme = UiWidgetPresetBuilder.FillThemeDefaults();
            theme.Acquire.SetShaders(AssetDatabase.LoadAssetAtPath<Shader>(ToonShaderPath), AssetDatabase.LoadAssetAtPath<Shader>(OutlineShaderPath));
            EditorUtility.SetDirty(theme);
            AssetDatabase.SaveAssets();
            var prefab = UiHudBuilder.SavePrefab(BuildAcquire(theme));
            InsertIntoRoot(prefab);
            Debug.Log("[UI] 획득 창을 UI_Root에 넣었습니다.");
        }

        private static GameObject BuildAcquire(UiTheme theme)
        {
            var root = new GameObject(AcquireName, typeof(RectTransform), typeof(CanvasGroup));
            var panel = UiHudBuilder.Instantiate("W_PaperPanel", root.transform, "Panel").GetComponent<ThemedPanel>();
            panel.SetSkin(UiPanelSkin.QuestToast);
            panel.SetTheme(theme);

            var model = new GameObject("Model", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
            model.transform.SetParent(root.transform, false);
            model.GetComponent<RawImage>().raycastTarget = false;
            var icon = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            icon.transform.SetParent(root.transform, false);
            icon.GetComponent<Image>().raycastTarget = false;

            var view = root.AddComponent<AcquireView>();
            var so = new SerializedObject(view);
            so.FindProperty("_theme").objectReferenceValue = theme;
            so.FindProperty("_panel").objectReferenceValue = panel;
            so.FindProperty("_model").objectReferenceValue = model.GetComponent<RawImage>();
            so.FindProperty("_icon").objectReferenceValue = icon.GetComponent<Image>();
            so.FindProperty("_title").objectReferenceValue = Text(root.transform, "Title", theme.Acquire.TitleStyle, theme);
            so.FindProperty("_description").objectReferenceValue = Text(root.transform, "Description", theme.Acquire.DescriptionStyle, theme);
            so.ApplyModifiedPropertiesWithoutUndo();
            return root;
        }

        private static ThemedText Text(Transform parent, string name, UiTextStyle style, UiTheme theme)
        {
            var go = UiHudBuilder.Instantiate("W_Text", parent, name);
            var label = go.GetComponent<TMP_Text>();
            label.alignment = TextAlignmentOptions.TopLeft;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.overflowMode = TextOverflowModes.Overflow;
            label.raycastTarget = false;
            var themed = go.GetComponent<ThemedText>();
            themed.SetTheme(theme);
            themed.SetStyle(style);
            return themed;
        }

        private static void InsertIntoRoot(GameObject prefab)
        {
            var path = UiRootPrefabBuilder.PrefabPath;
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                UiHudBuilder.Replace(root.transform.Find("HudAnimated"), prefab);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        // UI_Root(새 획득 창)가 있는 열린 씬에서 옛 임시 획득 창을 지운다. 둘 다 있으면 획득할 때 창이 두 개 뜬다.
        [MenuItem("VoxelMagic/UI/화면 만들기/옛 획득 창 지우기 (열린 씬)")]
        public static void RemoveOldPopupInOpenScene()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (Object.FindFirstObjectByType<UiScreenRouter>(FindObjectsInactive.Include) == null)
            {
                Debug.Log($"[UI] {scene.name}: UI_Root가 없어 옛 획득 창을 남깁니다.");
                return;
            }
            int removed = 0;
            foreach (var old in Object.FindObjectsByType<AcquisitionPopup>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Object.DestroyImmediate(old.gameObject);
                removed++;
            }
            if (removed > 0) EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log($"[UI] {scene.name}: 옛 획득 창 {removed}개를 지웠습니다.");
        }
    }
}
