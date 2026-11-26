// 의뢰 게시판 프리팹 UI_RequestBoard를 만들어 UI_Root의 Popup 층에 넣는다: 낡은 종이(QuestDetail Tiled) 창 + 머리·X + 의뢰 칸 원본 + 씨앗 버튼·안내.
// 씬의 옛 임시 게시판(RequestBoardWindow)은 UI_Root가 있는 씬에서 지운다(따로 메뉴).
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using VoxelMagic.Core;
using VoxelMagic.UI;

namespace VoxelMagic.Tools.EditorTools.UiWorkbench
{
    public static class UiRequestBoardBuilder
    {
        private const string BoardName = "UI_RequestBoard";

        [MenuItem("VoxelMagic/UI/화면 만들기/의뢰 게시판")]
        public static void Build()
        {
            var theme = UiWidgetPresetBuilder.FillThemeDefaults();
            var prefab = UiHudBuilder.SavePrefab(BuildBoard(theme));
            InsertIntoRoot(prefab);
            Debug.Log("[UI] 의뢰 게시판을 UI_Root에 넣었습니다.");
        }

        private static GameObject BuildBoard(UiTheme theme)
        {
            var root = new GameObject(BoardName, typeof(RectTransform), typeof(CanvasGroup));
            PixelLayout.Stretch((RectTransform)root.transform);
            var window = UiHudBuilder.Instantiate("W_PaperPanel", root.transform, "Window").GetComponent<ThemedPanel>();
            window.SetSkin(UiPanelSkin.QuestDetail);
            window.SetTheme(theme);
            window.SetRaycastTarget(true); // 창 안 클릭이 바깥(닫기)으로 새지 않게
            var w = window.transform;

            var title = Text(w, "Title", UiTextStyle.BodyBold, theme, TextAlignmentOptions.Top);
            var close = UiHudBuilder.AddButton(w, "Close", UiButtonSkin.Close, theme);
            var headerLine = Line(w, "HeaderLine");
            var list = new GameObject("List", typeof(RectTransform));
            list.transform.SetParent(w, false);
            var entry = BuildEntry(list.transform, theme);
            entry.gameObject.SetActive(false);
            var footerLine = Line(w, "FooterLine");
            var refill = BuildButton(w, "RefillButton", theme);
            var info = Text(w, "Info", UiTextStyle.Small, theme, TextAlignmentOptions.TopRight);

            var screen = root.AddComponent<RequestBoardScreen>();
            var so = new SerializedObject(screen);
            so.FindProperty("_id").enumValueIndex = (int)GameScreen.RequestBoard;
            so.FindProperty("_theme").objectReferenceValue = theme;
            so.FindProperty("_window").objectReferenceValue = w;
            so.FindProperty("_title").objectReferenceValue = title;
            so.FindProperty("_close").objectReferenceValue = close;
            so.FindProperty("_headerLine").objectReferenceValue = headerLine;
            so.FindProperty("_list").objectReferenceValue = list.transform;
            so.FindProperty("_entryTemplate").objectReferenceValue = entry;
            so.FindProperty("_footerLine").objectReferenceValue = footerLine;
            so.FindProperty("_refill").objectReferenceValue = refill;
            so.FindProperty("_info").objectReferenceValue = info;
            so.ApplyModifiedPropertiesWithoutUndo();
            return root;
        }

        private static RequestBoardEntryView BuildEntry(Transform parent, UiTheme theme)
        {
            var go = new GameObject("EntryTemplate", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var view = go.AddComponent<RequestBoardEntryView>();
            UiHudBuilder.Assign(view, "_title", Text(go.transform, "Title", UiTextStyle.BodyBold, theme, TextAlignmentOptions.TopLeft, TextOverflowModes.Ellipsis));
            UiHudBuilder.Assign(view, "_line", Text(go.transform, "Line", UiTextStyle.Small, theme, TextAlignmentOptions.TopLeft));
            UiHudBuilder.Assign(view, "_needsLabel", Text(go.transform, "NeedsLabel", UiTextStyle.Small, theme, TextAlignmentOptions.TopLeft));
            UiHudBuilder.Assign(view, "_needs", Text(go.transform, "Needs", UiTextStyle.Small, theme, TextAlignmentOptions.TopLeft));
            UiHudBuilder.Assign(view, "_hint", Text(go.transform, "Hint", UiTextStyle.Tiny, theme, TextAlignmentOptions.TopLeft));
            UiHudBuilder.Assign(view, "_accept", BuildButton(go.transform, "AcceptButton", theme));
            UiHudBuilder.Assign(view, "_reward", Text(go.transform, "Reward", UiTextStyle.Small, theme, TextAlignmentOptions.Top));
            UiHudBuilder.Assign(view, "_reason", Text(go.transform, "Reason", UiTextStyle.Tiny, theme, TextAlignmentOptions.TopRight));
            return view;
        }

        private static BookMenuButton BuildButton(Transform parent, string name, UiTheme theme)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var background = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            background.transform.SetParent(go.transform, false);
            var image = background.GetComponent<Image>();
            image.raycastTarget = true; // 버튼 누르기 영역
            var label = Text(go.transform, "Label", UiTextStyle.Body, theme, TextAlignmentOptions.Center);
            var button = go.AddComponent<BookMenuButton>();
            UiHudBuilder.Assign(button, "_background", image);
            UiHudBuilder.Assign(button, "_label", label);
            return button;
        }

        private static ThemedText Text(Transform parent, string name, UiTextStyle style, UiTheme theme, TextAlignmentOptions alignment,
            TextOverflowModes overflow = TextOverflowModes.Overflow)
        {
            var go = UiHudBuilder.Instantiate("W_Text", parent, name);
            var label = go.GetComponent<TMP_Text>();
            label.alignment = alignment;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.overflowMode = overflow;
            label.raycastTarget = false;
            var themed = go.GetComponent<ThemedText>();
            themed.SetTheme(theme);
            themed.SetStyle(style);
            return themed;
        }

        private static Image Line(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.raycastTarget = false;
            return image;
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

        // UI_Root(새 게시판)가 있는 열린 씬에서 옛 임시 게시판을 지운다. 둘 다 있으면 게시판이 두 장 뜬다.
        [MenuItem("VoxelMagic/UI/화면 만들기/옛 의뢰 게시판 지우기 (열린 씬)")]
        public static void RemoveOldBoardInOpenScene()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (Object.FindFirstObjectByType<UiScreenRouter>(FindObjectsInactive.Include) == null)
            {
                Debug.Log($"[UI] {scene.name}: UI_Root가 없어 옛 게시판을 남깁니다.");
                return;
            }
            int removed = 0;
            foreach (var old in Object.FindObjectsByType<RequestBoardWindow>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Object.DestroyImmediate(old.gameObject);
                removed++;
            }
            if (removed > 0) EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log($"[UI] {scene.name}: 옛 게시판 {removed}개를 지웠습니다.");
        }
    }
}
