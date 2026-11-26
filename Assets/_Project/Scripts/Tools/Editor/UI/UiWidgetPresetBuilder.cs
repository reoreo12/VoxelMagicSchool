// 테마 기본값(폰트·문구 표·글자 스타일·버튼·패널 모양)을 채우고, 기본 위젯 프리셋 프리팹을 만든다(UI_SPEC 3장).
// 테마에 이미 있는 값은 덮어쓰지 않는다. 프리셋은 Prefabs/UI/Widgets/에 두고 화면을 만들 때 가져다 쓴다.
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using VoxelMagic.EditorTools;
using VoxelMagic.UI;

namespace VoxelMagic.Tools.EditorTools.UiWorkbench
{
    public static class UiWidgetPresetBuilder
    {
        public const string WidgetFolder = "Assets/_Project/Prefabs/UI/Widgets";
        private const string Art = "Assets/_Project/Art/UI/";
        // 임시 글자색: 종이 패널 외곽선과 같은 갈색(quest_ui README). 사용자 피드백으로 조정.
        private static readonly Color TextBrown = new Color32(0x5C, 0x41, 0x28, 0xFF);

        [MenuItem("VoxelMagic/UI/테마 기본값·위젯 프리셋 만들기")]
        private static void BuildFromMenu()
        {
            var theme = FillThemeDefaults();
            BuildPresets(theme);
            Selection.activeObject = theme;
        }

        public static UiTheme FillThemeDefaults()
        {
            var theme = UiRootPrefabBuilder.GetOrCreateTheme();
            if (theme.Fonts.Regular == null)
            {
                // 비교 전 임시 폰트: 갈무리11 (13장). 비교 후 테마에서 한 번에 바꾼다.
                PixelFontAssetBuilder.GetOrCreateSet();
                var regular = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/Pixel/Galmuri11.asset");
                var bold = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/Pixel/Galmuri11 Bold.asset");
                theme.SetFonts(regular, bold);
            }
            if (theme.Texts == null) theme.SetTexts(UiRootPrefabBuilder.GetOrCreateTextTable());

            theme.AddMissing(
                new[]
                {
                    new TextStyleValues(UiTextStyle.Body, false, 1, TextBrown),
                    new TextStyleValues(UiTextStyle.BodyBold, true, 1, TextBrown),
                    new TextStyleValues(UiTextStyle.Title, true, 1, TextBrown),
                    new TextStyleValues(UiTextStyle.Small, false, 1, TextBrown, PixelFont("Galmuri9")),
                    new TextStyleValues(UiTextStyle.Tiny, false, 1, TextBrown, PixelFont("Galmuri7")),
                    // 바 제목: 굵게 + 띄어쓰기 2px 더 (사용자 피드백 2026-11-20, "의뢰"와 "(0)" 사이가 붙어 보임)
                    new TextStyleValues(UiTextStyle.HudTitle, true, 1, TextBrown, null, 2),
                },
                new[]
                {
                    ButtonSkin(UiButtonSkin.MenuBook, "menu_icons/buttons/btn_book_"),
                    ButtonSkin(UiButtonSkin.MenuBag, "menu_icons/buttons/btn_bag_"),
                    ButtonSkin(UiButtonSkin.ArrowDown, "quest_ui/buttons/arrow_down_"),
                    ButtonSkin(UiButtonSkin.ArrowUp, "quest_ui/buttons/arrow_up_"),
                    ButtonSkin(UiButtonSkin.Close, "quest_ui/buttons/close_"),
                },
                new[]
                {
                    new PanelSkinValues(UiPanelSkin.QuestBar, Sprite("quest_ui/quest_bar_paper.png"), Image.Type.Tiled),
                    new PanelSkinValues(UiPanelSkin.QuestPanel, Sprite("quest_ui/quest_panel_paper.png"), Image.Type.Tiled),
                    new PanelSkinValues(UiPanelSkin.QuestDetail, Sprite("quest_ui/quest_detail_paper.png"), Image.Type.Tiled),
                    new PanelSkinValues(UiPanelSkin.QuestToast, Sprite("quest_ui/quest_toast_paper.png"), Image.Type.Tiled),
                    new PanelSkinValues(UiPanelSkin.ScrollTrack, Sprite("quest_ui/scrollbar/scroll_track.png"), Image.Type.Sliced),
                    new PanelSkinValues(UiPanelSkin.ScrollHandle, Sprite("quest_ui/scrollbar/scroll_handle.png"), Image.Type.Sliced),
                });
            EditorUtility.SetDirty(theme);
            AssetDatabase.SaveAssets();
            return theme;
        }

        public static void BuildPresets(UiTheme theme)
        {
            Directory.CreateDirectory(WidgetFolder);

            var text = NewUi("W_Text", new Vector2(96, 16));
            text.AddComponent<TextMeshProUGUI>();
            Init(text.AddComponent<ThemedText>(), theme);
            Save(text);

            var button = NewUi("W_Button", new Vector2(16, 16));
            button.AddComponent<Image>();
            button.AddComponent<Button>();
            Init(button.AddComponent<ThemedButton>(), theme);
            Save(button);

            var panel = NewUi("W_PaperPanel", new Vector2(144, 76));
            panel.AddComponent<Image>();
            var themedPanel = panel.AddComponent<ThemedPanel>();
            themedPanel.SetSkin(UiPanelSkin.QuestPanel);
            Init(themedPanel, theme);
            Save(panel);

            Save(BuildScrollbar(theme));
        }

        // 세로 스크롤바: 트랙(폭 8) 안쪽에 핸들(폭 6, 좌우 1px 여백) (10-7).
        private static GameObject BuildScrollbar(UiTheme theme)
        {
            var root = NewUi("W_Scrollbar", new Vector2(8, 48));
            root.AddComponent<Image>();
            var track = root.AddComponent<ThemedPanel>();
            track.SetSkin(UiPanelSkin.ScrollTrack);
            Init(track, theme);

            var area = NewUi("Sliding Area", Vector2.zero);
            area.transform.SetParent(root.transform, false);
            var areaRect = (RectTransform)area.transform;
            areaRect.anchorMin = Vector2.zero;
            areaRect.anchorMax = Vector2.one;
            areaRect.offsetMin = new Vector2(1, 0);
            areaRect.offsetMax = new Vector2(-1, 0);

            var handle = NewUi("Handle", Vector2.zero);
            handle.transform.SetParent(area.transform, false);
            var handleImage = handle.AddComponent<Image>();
            var handlePanel = handle.AddComponent<ThemedPanel>();
            handlePanel.SetSkin(UiPanelSkin.ScrollHandle);
            Init(handlePanel, theme);
            var handleRect = (RectTransform)handle.transform;
            handleRect.offsetMin = Vector2.zero;
            handleRect.offsetMax = Vector2.zero;

            var scrollbar = root.AddComponent<Scrollbar>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            scrollbar.handleRect = handleRect;
            scrollbar.targetGraphic = handleImage;
            scrollbar.transition = Selectable.Transition.None;
            handleImage.raycastTarget = true; // 핸들은 끌 수 있어야 한다
            track.SetRaycastTarget(true); // 트랙을 누르면 그쪽으로 넘어간다
            handlePanel.SetRaycastTarget(true); // 패널은 기본으로 입력을 받지 않아 손잡이를 끌 수 없었다
            return root;
        }

        private static TMP_FontAsset PixelFont(string name) =>
            AssetDatabase.LoadAssetAtPath<TMP_FontAsset>($"Assets/_Project/Fonts/Pixel/{name}.asset");

        private static ButtonSkinValues ButtonSkin(UiButtonSkin id, string prefix) =>
            new ButtonSkinValues(id, Sprite(prefix + "normal.png"), Sprite(prefix + "hover.png"), Sprite(prefix + "pressed.png"));

        private static Sprite Sprite(string relative)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Art + relative);
            if (sprite == null) Debug.LogWarning($"[UI 테마] 스프라이트가 없습니다: {relative}");
            return sprite;
        }

        private static GameObject NewUi(string name, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            ((RectTransform)go.transform).sizeDelta = size;
            return go;
        }

        private static void Init(ThemedWidget widget, UiTheme theme) => widget.SetTheme(theme);

        private static void Save(GameObject go)
        {
            PrefabUtility.SaveAsPrefabAsset(go, $"{WidgetFolder}/{go.name}.prefab");
            Object.DestroyImmediate(go);
        }
    }
}
