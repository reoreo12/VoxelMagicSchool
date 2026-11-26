// 확인 창(예/아니요) 프리팹 UI_Confirm을 만들어 UI_Root의 Popup 층에 넣는다: 종이 9-slice 창 + 메시지 + 책 페이지 버튼 두 개.
// 테마에 창 바탕 그림을 채운다. 씬의 옛 임시 확인 창(ConfirmDialog)은 UI_Root가 있는 씬에서 지운다(따로 메뉴).
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using VoxelMagic.Core;
using VoxelMagic.UI;

namespace VoxelMagic.Tools.EditorTools.UiWorkbench
{
    public static class UiConfirmBuilder
    {
        private const string PanelPath = "Assets/_Project/Art/UI/confirm_ui/confirm_panel.png";
        private const string ConfirmName = "UI_Confirm";

        [MenuItem("VoxelMagic/UI/화면 만들기/확인 창")]
        public static void Build()
        {
            Debug.Log(UiImportRuleApplier.ApplyAll());
            var theme = UiWidgetPresetBuilder.FillThemeDefaults();
            theme.Confirm.SetPanel(AssetDatabase.LoadAssetAtPath<Sprite>(PanelPath));
            EditorUtility.SetDirty(theme);
            AssetDatabase.SaveAssets();
            var prefab = UiHudBuilder.SavePrefab(BuildConfirm(theme));
            InsertIntoRoot(prefab);
            Debug.Log("[UI] 확인 창을 UI_Root에 넣었습니다.");
        }

        private static GameObject BuildConfirm(UiTheme theme)
        {
            var root = new GameObject(ConfirmName, typeof(RectTransform), typeof(CanvasGroup));
            PixelLayout.Stretch((RectTransform)root.transform);
            var window = new GameObject("Window", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            window.transform.SetParent(root.transform, false);
            var windowImage = window.GetComponent<Image>();
            windowImage.sprite = theme.Confirm.Panel;
            windowImage.type = Image.Type.Sliced;
            windowImage.raycastTarget = true;

            var message = UiHudBuilder.Instantiate("W_Text", window.transform, "Message");
            var label = message.GetComponent<TMP_Text>();
            label.alignment = TextAlignmentOptions.Top;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.overflowMode = TextOverflowModes.Overflow;
            label.raycastTarget = false;
            var themed = message.GetComponent<ThemedText>();
            themed.SetTheme(theme);
            themed.SetStyle(theme.Confirm.MessageStyle);

            var screen = root.AddComponent<ConfirmScreen>();
            var so = new SerializedObject(screen);
            so.FindProperty("_id").enumValueIndex = (int)GameScreen.Confirm;
            so.FindProperty("_theme").objectReferenceValue = theme;
            so.FindProperty("_window").objectReferenceValue = windowImage;
            so.FindProperty("_message").objectReferenceValue = themed;
            so.FindProperty("_yes").objectReferenceValue = BuildButton(window.transform, "YesButton", theme);
            so.FindProperty("_no").objectReferenceValue = BuildButton(window.transform, "NoButton", theme);
            so.ApplyModifiedPropertiesWithoutUndo();
            return root;
        }

        private static BookMenuButton BuildButton(Transform parent, string name, UiTheme theme)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var background = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            background.transform.SetParent(go.transform, false);
            var image = background.GetComponent<Image>();
            image.raycastTarget = true; // 버튼 누르기 영역
            var text = UiHudBuilder.Instantiate("W_Text", go.transform, "Label");
            var label = text.GetComponent<TMP_Text>();
            label.alignment = TextAlignmentOptions.Center;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.raycastTarget = false;
            var themed = text.GetComponent<ThemedText>();
            themed.SetTheme(theme);
            themed.SetStyle(UiTextStyle.Body);
            var button = go.AddComponent<BookMenuButton>();
            UiHudBuilder.Assign(button, "_background", image);
            UiHudBuilder.Assign(button, "_label", themed);
            return button;
        }

        private static void InsertIntoRoot(GameObject prefab)
        {
            var path = UiRootPrefabBuilder.PrefabPath;
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var instance = UiHudBuilder.Replace(root.transform.Find("Popup"), prefab);
                PixelLayout.Stretch((RectTransform)instance.transform);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        // UI_Root(새 확인 창)가 있는 열린 씬에서 옛 임시 확인 창을 지운다. 둘 다 있으면 한 번 물을 때 창이 두 개 뜬다.
        [MenuItem("VoxelMagic/UI/화면 만들기/옛 확인 창 지우기 (열린 씬)")]
        public static void RemoveOldDialogInOpenScene()
        {
            var scene = EditorSceneManager.GetActiveScene();
            bool hasRoot = Object.FindFirstObjectByType<UiScreenRouter>(FindObjectsInactive.Include) != null;
            if (!hasRoot)
            {
                Debug.Log($"[UI] {scene.name}: UI_Root가 없어 옛 확인 창을 남깁니다.");
                return;
            }
            int removed = 0;
            foreach (var old in Object.FindObjectsByType<ConfirmDialog>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Object.DestroyImmediate(old.gameObject);
                removed++;
            }
            if (removed > 0) EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log($"[UI] {scene.name}: 옛 확인 창 {removed}개를 지웠습니다.");
        }
    }
}
