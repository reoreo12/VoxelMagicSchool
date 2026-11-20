// 하단 HUD 프리팹(퀵슬롯 바, 오른쪽 아래 마력·기력 바)을 만들어 UI_Root에 넣는다(UI_SPEC 7-1, 7-2).
// 테마에 Sprout 칸·선택 프레임·바 스프라이트를 채우고, 지팡이 아이템에 아이콘(item_staff, 방식 1 임시)을 연결한다.
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using VoxelMagic.Core;
using VoxelMagic.UI;

namespace VoxelMagic.Tools.EditorTools.UiWorkbench
{
    public static class UiHudBottomBuilder
    {
        private const string Art = "Assets/_Project/Art/UI/";
        private const string SproutSheet = Art + "Sprout Lands - UI Pack - Basic packemojis-free/Inventory/Inventory_Spritesheet.png";
        private const string QuickSlotName = "HUD_QuickSlot";
        private const string SlotCellName = "HUD_QuickSlotCell";
        private const string StatusBarsName = "HUD_StatusBars";

        [MenuItem("VoxelMagic/UI/HUD 만들기/퀵슬롯·상태 바")]
        public static void Build()
        {
            Debug.Log(UiImportRuleApplier.ApplyAll()); // Sprout 시트 조각을 먼저 자른다
            var theme = UiWidgetPresetBuilder.FillThemeDefaults();
            FillTheme(theme);
            AssignStaffIcon();

            var cell = UiHudBuilder.SavePrefab(BuildSlotCell(theme));
            var quickSlot = UiHudBuilder.SavePrefab(BuildQuickSlotBar(theme, cell));
            var statusBars = UiHudBuilder.SavePrefab(BuildStatusBars(theme));
            InsertIntoRoot(quickSlot, statusBars);
            Debug.Log("[UI] 퀵슬롯·상태 바를 UI_Root에 넣었습니다.");
        }

        private static void FillTheme(UiTheme theme)
        {
            var sprout = AssetDatabase.LoadAllAssetsAtPath(SproutSheet).OfType<Sprite>().ToDictionary(s => s.name);
            Sprite Sheet(string name) => sprout.TryGetValue(name, out var s) ? s : null;

            theme.AddMissing(
                System.Array.Empty<TextStyleValues>(),
                System.Array.Empty<ButtonSkinValues>(),
                new[]
                {
                    new PanelSkinValues(UiPanelSkin.QuickSlotBar, Sheet("inv_bar_dark"), Image.Type.Sliced),
                    new PanelSkinValues(UiPanelSkin.QuickSlot, Sheet("inv_slot_light"), Image.Type.Sliced),
                    new PanelSkinValues(UiPanelSkin.QuickSlotAlt, Sheet("inv_slot_mid"), Image.Type.Sliced),
                    new PanelSkinValues(UiPanelSkin.StatusBarFrame, Load("bar_ui/bar_frame_vertical.png"), Image.Type.Sliced),
                });
            theme.QuickSlots.SetSelectionSprites(Sheet("inv_select_tl"), Sheet("inv_select_tr"), Sheet("inv_select_bl"), Sheet("inv_select_br"));
            theme.StatusBars.SetSprites(Load("bar_ui/bar_fill_mana.png"), Load("bar_ui/bar_fill_stamina.png"),
                Load("bar_ui/bar_cap_mana.png"), Load("bar_ui/bar_cap_stamina.png"),
                Load("Ninja Adventure - Asset Pack/Skill/Alchemy.png"), Load("bar_ui/icon/icon_stamina.png"));
            EditorUtility.SetDirty(theme);
            AssetDatabase.SaveAssets();
        }

        // 지팡이 아이템에 아이콘이 비어 있으면 방식 1 샘플(item_staff.png)을 임시로 넣는다(7-3 비교 전).
        private static void AssignStaffIcon()
        {
            var icon = Load("item_icons/item_staff.png");
            foreach (var guid in AssetDatabase.FindAssets("t:ItemDefinition", new[] { "Assets/_Project/Data" }))
            {
                var item = AssetDatabase.LoadAssetAtPath<ItemDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (item == null || item.Icon != null || item.DisplayName != "지팡이") continue;
                var so = new SerializedObject(item);
                so.FindProperty("_icon").objectReferenceValue = icon;
                so.ApplyModifiedPropertiesWithoutUndo();
                Debug.Log($"[UI] {item.name}에 임시 아이콘 item_staff 연결");
            }
        }

        private static GameObject BuildSlotCell(UiTheme theme)
        {
            var root = new GameObject(SlotCellName, typeof(RectTransform));
            var background = UiHudBuilder.Instantiate("W_PaperPanel", root.transform, "Background").GetComponent<ThemedPanel>();
            background.SetSkin(UiPanelSkin.QuickSlot);
            background.SetTheme(theme);
            SetRaycast(background, true); // 칸을 눌러 선택

            var fallback = NewImage(root.transform, "Fallback");
            var icon = NewImage(root.transform, "Icon");
            var letter = Text(root.transform, "Letter", UiTextStyle.Small, TextAlignmentOptions.Center, theme);
            var count = Text(root.transform, "Count", UiTextStyle.Tiny, TextAlignmentOptions.BottomRight, theme);

            // 빈 칸 상태로 시작한다(편집 화면·연결 전에 흰 사각형이 보이지 않게).
            fallback.enabled = false;
            icon.enabled = false;
            letter.gameObject.SetActive(false);
            count.gameObject.SetActive(false);

            var view = root.AddComponent<QuickSlotView>();
            UiHudBuilder.Assign(view, "_background", background);
            UiHudBuilder.Assign(view, "_icon", icon);
            UiHudBuilder.Assign(view, "_fallback", fallback);
            UiHudBuilder.Assign(view, "_letter", letter);
            UiHudBuilder.Assign(view, "_count", count);
            return root;
        }

        private static GameObject BuildQuickSlotBar(UiTheme theme, GameObject cellPrefab)
        {
            var root = new GameObject(QuickSlotName, typeof(RectTransform));
            var bar = new GameObject("Bar", typeof(RectTransform));
            bar.transform.SetParent(root.transform, false);
            var background = UiHudBuilder.Instantiate("W_PaperPanel", bar.transform, "Background").GetComponent<ThemedPanel>();
            background.SetSkin(UiPanelSkin.QuickSlotBar);
            background.SetTheme(theme);
            SetRaycast(background, true); // 바 위 어디서든 휠로 칸 바꾸기

            var view = root.AddComponent<QuickSlotBarView>();
            var slots = new SerializedObject(view);
            var list = slots.FindProperty("_slots");
            for (int i = 0; i < theme.QuickSlots.SlotCount; i++)
            {
                var cell = (GameObject)PrefabUtility.InstantiatePrefab(cellPrefab, bar.transform);
                cell.name = $"Slot{i + 1}";
                list.InsertArrayElementAtIndex(list.arraySize);
                list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = cell.GetComponent<QuickSlotView>();
            }
            slots.ApplyModifiedPropertiesWithoutUndo();

            // 선택 프레임: 움직이는 동안만 바뀌므로 자체 캔버스(4-A-5).
            var selection = new GameObject("Selection", typeof(RectTransform), typeof(Canvas));
            selection.transform.SetParent(bar.transform, false);
            var frame = selection.AddComponent<SelectionFrameView>();
            UiHudBuilder.Assign(frame, "_topLeft", NewImage(selection.transform, "TopLeft"));
            UiHudBuilder.Assign(frame, "_topRight", NewImage(selection.transform, "TopRight"));
            UiHudBuilder.Assign(frame, "_bottomLeft", NewImage(selection.transform, "BottomLeft"));
            UiHudBuilder.Assign(frame, "_bottomRight", NewImage(selection.transform, "BottomRight"));

            UiHudBuilder.Assign(view, "_bar", bar.transform);
            UiHudBuilder.Assign(view, "_background", background);
            UiHudBuilder.Assign(view, "_selection", frame);
            view.SetTheme(theme);
            return root;
        }

        private static GameObject BuildStatusBars(UiTheme theme)
        {
            var root = new GameObject(StatusBarsName, typeof(RectTransform));
            var mana = BuildStatusBar(root.transform, "Mana", theme);
            var stamina = BuildStatusBar(root.transform, "Stamina", theme);
            var view = root.AddComponent<StatusBarsView>();
            UiHudBuilder.Assign(view, "_mana", mana);
            UiHudBuilder.Assign(view, "_stamina", stamina);
            view.SetTheme(theme);
            return root;
        }

        private static StatusBarView BuildStatusBar(Transform parent, string name, UiTheme theme)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var frame = UiHudBuilder.Instantiate("W_PaperPanel", go.transform, "Frame").GetComponent<ThemedPanel>();
            frame.SetSkin(UiPanelSkin.StatusBarFrame);
            frame.SetTheme(theme);
            var view = go.AddComponent<StatusBarView>();
            UiHudBuilder.Assign(view, "_frame", frame);
            UiHudBuilder.Assign(view, "_fill", NewImage(go.transform, "Fill"));
            UiHudBuilder.Assign(view, "_cap", NewImage(go.transform, "Cap"));
            UiHudBuilder.Assign(view, "_icon", NewImage(go.transform, "Icon"));
            return view;
        }

        private static void InsertIntoRoot(GameObject quickSlot, GameObject statusBars)
        {
            var path = UiRootPrefabBuilder.PrefabPath;
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                UiHudBuilder.Replace(root.transform.Find("HudStatic"), quickSlot);
                UiHudBuilder.Replace(root.transform.Find("HudDynamic"), statusBars);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static ThemedText Text(Transform parent, string name, UiTextStyle style, TextAlignmentOptions alignment, UiTheme theme)
        {
            var go = UiHudBuilder.Instantiate("W_Text", parent, name);
            var label = go.GetComponent<TMP_Text>();
            label.alignment = alignment;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.overflowMode = TextOverflowModes.Overflow;
            var themed = go.GetComponent<ThemedText>();
            themed.SetTheme(theme);
            themed.SetStyle(style);
            return themed;
        }

        private static Image NewImage(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.raycastTarget = false;
            return image;
        }

        private static void SetRaycast(ThemedPanel panel, bool on)
        {
            var so = new SerializedObject(panel);
            so.FindProperty("_raycastTarget").boolValue = on;
            so.ApplyModifiedPropertiesWithoutUndo();
            panel.ApplyTheme();
        }

        private static Sprite Load(string relative)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Art + relative);
            if (sprite == null) Debug.LogWarning($"[UI] 스프라이트가 없습니다: {relative}");
            return sprite;
        }
    }
}
