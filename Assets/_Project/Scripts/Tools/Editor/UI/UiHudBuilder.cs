// 왼쪽 위 HUD 프리팹(책·가방 버튼, 의뢰 드롭다운 바)을 위젯 프리셋으로 조합해 만들고 UI_Root 프리팹 안에 넣는다(UI_SPEC 5-4, 10-1).
// 다시 실행하면 HUD 프리팹을 새로 만들고 UI_Root 안의 같은 이름 오브젝트를 바꾼다. 위치·크기는 테마 값으로 각 화면이 직접 계산한다.
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using VoxelMagic.Core;
using VoxelMagic.UI;

namespace VoxelMagic.Tools.EditorTools.UiWorkbench
{
    public static class UiHudBuilder
    {
        public const string HudFolder = "Assets/_Project/Prefabs/UI/HUD";
        private const string TopLeftName = "HUD_TopLeftMenu";
        private const string QuestBarName = "HUD_QuestBar";

        [MenuItem("VoxelMagic/UI/HUD 만들기/왼쪽 위 메뉴·의뢰 바")]
        public static void BuildTopLeft()
        {
            var theme = UiWidgetPresetBuilder.FillThemeDefaults();
            SeedTexts(theme);
            Directory.CreateDirectory(HudFolder);

            var topLeft = SavePrefab(BuildTopLeftMenu(theme));
            var questBar = SavePrefab(BuildQuestBar(theme));
            InsertIntoRoot(topLeft, questBar);
            Debug.Log("[UI] 왼쪽 위 메뉴·의뢰 바를 UI_Root에 넣었습니다.");
        }

        private static void SeedTexts(UiTheme theme)
        {
            var table = AssetDatabase.LoadAssetAtPath<TextTable>(UiRootPrefabBuilder.TextTablePath);
            if (table == null || table.TryGet(UiTextKeys.QuestBarTitle, out _)) return;
            table.Set(UiTextKeys.QuestBarTitle, "수락한 의뢰({0})"); // "의뢰"와 개수 사이 띄어쓰기 없음 (사용자 결정 2026-11-20)
            EditorUtility.SetDirty(table);
            AssetDatabase.SaveAssets();
        }

        private static GameObject BuildTopLeftMenu(UiTheme theme)
        {
            var root = new GameObject(TopLeftName, typeof(RectTransform));
            var book = AddButton(root.transform, "Book", UiButtonSkin.MenuBook, theme);
            var bag = AddButton(root.transform, "Bag", UiButtonSkin.MenuBag, theme);
            var view = root.AddComponent<TopLeftMenuView>();
            Assign(view, "_book", book);
            Assign(view, "_bag", bag);
            view.SetTheme(theme);
            return root;
        }

        private static GameObject BuildQuestBar(UiTheme theme)
        {
            var root = new GameObject(QuestBarName, typeof(RectTransform));
            var background = Instantiate("W_PaperPanel", root.transform, "Background").GetComponent<ThemedPanel>();
            background.SetSkin(UiPanelSkin.QuestBar);
            background.SetTheme(theme);
            var arrow = AddButton(root.transform, "Arrow", UiButtonSkin.ArrowDown, theme);

            var title = Instantiate("W_Text", root.transform, "Title");
            var label = title.GetComponent<TMP_Text>();
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.overflowMode = TextOverflowModes.Ellipsis;
            var themedTitle = title.GetComponent<ThemedText>();
            themedTitle.SetTheme(theme);
            themedTitle.SetStyle(UiTextStyle.HudTitle); // 바 제목: 굵게 + 띄어쓰기 넓게 (사용자 피드백 2026-11-20)

            var close = AddButton(root.transform, "Close", UiButtonSkin.Close, theme);

            var view = root.AddComponent<QuestBarView>();
            Assign(view, "_background", background);
            Assign(view, "_arrow", arrow);
            Assign(view, "_close", close);
            Assign(view, "_title", themedTitle);
            view.SetTheme(theme);
            return root;
        }

        private static void InsertIntoRoot(GameObject topLeft, GameObject questBar)
        {
            var path = UiRootPrefabBuilder.PrefabPath;
            UiRootPrefabBuilder.GetOrCreatePrefab();
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var hudStatic = root.transform.Find("HudStatic");
                var hudDynamic = root.transform.Find("HudDynamic");
                Replace(hudStatic, topLeft);
                var menu = hudStatic.Find(TopLeftName).GetComponent<TopLeftMenuView>();
                Assign(menu, "_router", root.GetComponentInChildren<UiScreenRouter>(true));
                Replace(hudDynamic, questBar);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        internal static GameObject Replace(Transform parent, GameObject prefab)
        {
            var old = parent.Find(prefab.name);
            if (old != null) Object.DestroyImmediate(old.gameObject);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.name = prefab.name;
            return instance;
        }

        internal static ThemedButton AddButton(Transform parent, string name, UiButtonSkin skin, UiTheme theme)
        {
            var button = Instantiate("W_Button", parent, name).GetComponent<ThemedButton>();
            button.SetSkin(skin);
            button.SetTheme(theme);
            return button;
        }

        internal static GameObject Instantiate(string preset, Transform parent, string name)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{UiWidgetPresetBuilder.WidgetFolder}/{preset}.prefab");
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.name = name;
            return go;
        }

        internal static void Assign(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        internal static GameObject SavePrefab(GameObject go)
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, $"{HudFolder}/{go.name}.prefab");
            Object.DestroyImmediate(go);
            return prefab;
        }
    }
}
