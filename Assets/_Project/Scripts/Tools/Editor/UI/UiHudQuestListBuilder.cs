// 의뢰 드롭다운 목록 패널(가상 스크롤)과 완료 알림 창 프리팹을 만들어 UI_Root에 넣는다(UI_SPEC 10-2~10-4, 10-6).
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using VoxelMagic.Core;
using VoxelMagic.UI;

namespace VoxelMagic.Tools.EditorTools.UiWorkbench
{
    public static class UiHudQuestListBuilder
    {
        private const string ListName = "HUD_QuestList";
        private const string ToastName = "HUD_QuestToast";
        private const string QuestBarName = "HUD_QuestBar";

        [MenuItem("VoxelMagic/UI/HUD 만들기/의뢰 목록·완료 알림")]
        public static void Build()
        {
            var theme = UiWidgetPresetBuilder.FillThemeDefaults();
            SeedTexts();
            var list = UiHudBuilder.SavePrefab(BuildList(theme));
            var toast = UiHudBuilder.SavePrefab(BuildToast(theme));
            InsertIntoRoot(list, toast);
            Debug.Log("[UI] 의뢰 목록·완료 알림을 UI_Root에 넣었습니다.");
        }

        private static void SeedTexts()
        {
            var table = AssetDatabase.LoadAssetAtPath<TextTable>(UiRootPrefabBuilder.TextTablePath);
            if (table == null) return;
            if (!table.TryGet(UiTextKeys.QuestToast, out _)) table.Set(UiTextKeys.QuestToast, "의뢰 완료: {0}");
            if (!table.TryGet(UiTextKeys.QuestRequirement, out _)) table.Set(UiTextKeys.QuestRequirement, "{0} {1}/{2}");
            if (!table.TryGet(UiTextKeys.QuestItemTitle, out _)) table.Set(UiTextKeys.QuestItemTitle, "· {0}");
            EditorUtility.SetDirty(table);
            AssetDatabase.SaveAssets();
        }

        private static GameObject BuildList(UiTheme theme)
        {
            var root = new GameObject(ListName, typeof(RectTransform));
            var background = UiHudBuilder.Instantiate("W_PaperPanel", root.transform, "Background").GetComponent<ThemedPanel>();
            background.SetSkin(UiPanelSkin.QuestPanel);
            background.SetTheme(theme);
            SetRaycast(background, true); // 패널 위 휠 스크롤

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
            viewport.transform.SetParent(root.transform, false);

            var item = BuildItemTemplate(viewport.transform, theme);
            item.gameObject.SetActive(false);

            var scrollbar = UiHudBuilder.Instantiate("W_Scrollbar", root.transform, "Scrollbar").GetComponent<Scrollbar>();
            foreach (var w in scrollbar.GetComponentsInChildren<ThemedWidget>(true)) w.SetTheme(theme);
            scrollbar.gameObject.SetActive(false);

            var view = root.AddComponent<QuestListView>();
            UiHudBuilder.Assign(view, "_background", background);
            UiHudBuilder.Assign(view, "_viewport", viewport.transform);
            UiHudBuilder.Assign(view, "_itemTemplate", item);
            UiHudBuilder.Assign(view, "_scrollbar", scrollbar);
            view.SetTheme(theme);
            return root;
        }

        private static QuestListItemView BuildItemTemplate(Transform parent, UiTheme theme)
        {
            var go = new GameObject("ItemTemplate", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var hit = go.GetComponent<Image>();
            hit.color = new Color(1f, 1f, 1f, 0f); // 누르기 영역만(보이지 않음)
            hit.raycastTarget = true;

            var title = Text(go.transform, "Title", theme.QuestList.TitleStyle, theme, TextOverflowModes.Ellipsis);
            var titleBold = Text(go.transform, "TitleBold", theme.QuestList.TitleStyle, theme, TextOverflowModes.Ellipsis);
            var lines = Text(go.transform, "Requirements", theme.QuestList.RequirementStyle, theme, TextOverflowModes.Overflow);
            var view = go.AddComponent<QuestListItemView>();
            UiHudBuilder.Assign(view, "_title", title);
            UiHudBuilder.Assign(view, "_titleBold", titleBold);
            UiHudBuilder.Assign(view, "_requirements", lines);
            return view;
        }

        private static GameObject BuildToast(UiTheme theme)
        {
            var root = new GameObject(ToastName, typeof(RectTransform), typeof(CanvasGroup));
            var background = UiHudBuilder.Instantiate("W_PaperPanel", root.transform, "Background").GetComponent<ThemedPanel>();
            background.SetSkin(UiPanelSkin.QuestToast);
            background.SetTheme(theme);
            var text = Text(root.transform, "Text", theme.QuestToast.TextStyle, theme, TextOverflowModes.Overflow);
            var close = UiHudBuilder.AddButton(root.transform, "Close", UiButtonSkin.Close, theme);

            var view = root.AddComponent<QuestToastView>();
            UiHudBuilder.Assign(view, "_background", background);
            UiHudBuilder.Assign(view, "_text", text);
            UiHudBuilder.Assign(view, "_close", close);
            view.SetTheme(theme);
            return root;
        }

        private static void InsertIntoRoot(GameObject list, GameObject toast)
        {
            var path = UiRootPrefabBuilder.PrefabPath;
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var hudDynamic = root.transform.Find("HudDynamic");
                var listInstance = UiHudBuilder.Replace(hudDynamic, list);
                var bar = hudDynamic.Find(QuestBarName);
                if (bar == null) Debug.LogWarning("[UI] UI_Root에 의뢰 바가 없습니다. 왼쪽 위 메뉴·의뢰 바를 먼저 만드세요.");
                UiHudBuilder.Assign(listInstance.GetComponent<QuestListView>(), "_bar", bar);
                UiHudBuilder.Replace(root.transform.Find("HudAnimated"), toast);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static ThemedText Text(Transform parent, string name, UiTextStyle style, UiTheme theme, TextOverflowModes overflow)
        {
            var go = UiHudBuilder.Instantiate("W_Text", parent, name);
            var label = go.GetComponent<TMP_Text>();
            label.alignment = TextAlignmentOptions.TopLeft;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.overflowMode = overflow;
            var themed = go.GetComponent<ThemedText>();
            themed.SetTheme(theme);
            themed.SetStyle(style);
            return themed;
        }

        private static void SetRaycast(ThemedPanel panel, bool on)
        {
            var so = new SerializedObject(panel);
            so.FindProperty("_raycastTarget").boolValue = on;
            so.ApplyModifiedPropertiesWithoutUndo();
            panel.ApplyTheme();
        }
    }
}
