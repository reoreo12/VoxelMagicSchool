// 시간대 HUD 프리팹(화면 오른쪽 위, UI_SPEC 8장)과 공용 툴팁을 만들어 UI_Root에 넣고, 테마에 조각 스프라이트와 문구를 채운다.
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using VoxelMagic.Core;
using VoxelMagic.UI;

namespace VoxelMagic.Tools.EditorTools.UiWorkbench
{
    public static class UiHudTimeBuilder
    {
        private const string Art = "Assets/_Project/Art/UI/";
        private const string FrameSheet = Art + "Sprout Lands (수정본)/weather/Weather_UI_Free_v2.png";
        private const string Sheet = Art + "Sprout Lands (수정본)/weather/Weather_UI_Free_Noon_ver.png";
        private const string Icons = Art + "Sprout Lands - UI Pack - Basic packemojis-free/weather/Weather_Icons_smal_freel.png";
        private const string TimeName = "HUD_Time";
        private const string TooltipName = "UI_Tooltip";

        [MenuItem("VoxelMagic/UI/HUD 만들기/시간대·툴팁")]
        public static void Build()
        {
            Debug.Log(UiImportRuleApplier.ApplyAll()); // 날씨 시트 조각을 먼저 자른다
            var theme = UiWidgetPresetBuilder.FillThemeDefaults();
            FillTheme(theme);
            SeedTexts();
            var tooltip = UiHudBuilder.SavePrefab(BuildTooltip(theme));
            var time = UiHudBuilder.SavePrefab(BuildTime(theme));
            InsertIntoRoot(time, tooltip);
            Debug.Log("[UI] 시간대 HUD와 툴팁을 UI_Root에 넣었습니다.");
        }

        private static void FillTheme(UiTheme theme)
        {
            var frameSheet = Sprites(FrameSheet);
            var sheet = Sprites(Sheet);
            var icons = Sprites(Icons);
            Sprite Get(System.Collections.Generic.Dictionary<string, Sprite> set, string name)
            {
                if (set.TryGetValue(name, out var s)) return s;
                Debug.LogWarning($"[UI] 스프라이트가 없습니다: {name} (Sprout 폴더는 git에서 빠져 있음)");
                return null;
            }

            // 틀 시트를 바꿨을 수 있으므로 시트 조각은 항상 다시 넣는다(위치 값은 그대로).
                theme.Time.SetSprites(Get(frameSheet, "time_frame"), Get(sheet, "time_roulette"), Get(sheet, "time_arrow"),
                    Get(icons, "time_icon_after_school"), Get(icons, "time_icon_night"));
            theme.AddMissing(System.Array.Empty<TextStyleValues>(), System.Array.Empty<ButtonSkinValues>(),
                new[] { new PanelSkinValues(UiPanelSkin.Tooltip, AssetDatabase.LoadAssetAtPath<Sprite>(Art + "quest_ui/quest_bar_paper.png"), Image.Type.Tiled) });
            EditorUtility.SetDirty(theme);
            AssetDatabase.SaveAssets();
        }

        private static System.Collections.Generic.Dictionary<string, Sprite> Sprites(string path) =>
            AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().GroupBy(s => s.name).ToDictionary(g => g.Key, g => g.First());

        private static void SeedTexts()
        {
            var table = AssetDatabase.LoadAssetAtPath<TextTable>(UiRootPrefabBuilder.TextTablePath);
            if (table == null) return;
            void Seed(string key, string value)
            {
                if (!table.TryGet(key, out _)) table.Set(key, value);
            }
            Seed(UiTextKeys.TimeAfterSchool, "방과후");
            Seed(UiTextKeys.TimeNight, "밤");
            Seed(UiTextKeys.TimeTooltip, "{0} {1}:00");
            Seed(UiTextKeys.TimeDay, "D-{0}");
            Seed(UiTextKeys.TimePaused, "멈춰 있음");
            EditorUtility.SetDirty(table);
            AssetDatabase.SaveAssets();
        }

        private static GameObject BuildTime(UiTheme theme)
        {
            var root = new GameObject(TimeName, typeof(RectTransform));
            // 그리는 순서 = 자식 순서: 받침 → 틀 → 룰렛 → 시간대 그림(틀보다 앞) → 화살표 → 글자.
            var backing = NewImage(root.transform, "Backing");
            var frame = NewImage(root.transform, "Frame");
            var roulette = NewImage(root.transform, "Roulette");
            var icon = NewImage(root.transform, "Icon");
            var arrow = NewImage(root.transform, "Arrow");
            var date = Text(root.transform, "Date", theme.Time.DateStyle, theme);
            var paused = Text(root.transform, "Paused", UiTextStyle.HudLabel, theme);
            paused.SetKey(UiTextKeys.TimePaused);
            paused.gameObject.SetActive(false);

            var view = root.AddComponent<TimeHudView>();
            UiHudBuilder.Assign(view, "_backing", backing);
            UiHudBuilder.Assign(view, "_frame", frame);
            UiHudBuilder.Assign(view, "_roulette", roulette);
            UiHudBuilder.Assign(view, "_icon", icon);
            UiHudBuilder.Assign(view, "_arrow", arrow);
            UiHudBuilder.Assign(view, "_date", date);
            UiHudBuilder.Assign(view, "_paused", paused);
            view.SetTheme(theme);
            return root;
        }

        private static GameObject BuildTooltip(UiTheme theme)
        {
            var root = new GameObject(TooltipName, typeof(RectTransform));
            var body = new GameObject("Body", typeof(RectTransform));
            body.transform.SetParent(root.transform, false);
            var background = UiHudBuilder.Instantiate("W_PaperPanel", body.transform, "Background").GetComponent<ThemedPanel>();
            background.SetSkin(UiPanelSkin.Tooltip);
            background.SetTheme(theme);
            var text = Text(body.transform, "Text", theme.Tooltip.TextStyle, theme);

            var view = root.AddComponent<UiTooltipView>();
            UiHudBuilder.Assign(view, "_body", body.transform);
            UiHudBuilder.Assign(view, "_background", background);
            UiHudBuilder.Assign(view, "_text", text);
            view.SetTheme(theme);
            body.SetActive(false);
            return root;
        }

        private static void InsertIntoRoot(GameObject time, GameObject tooltip)
        {
            var path = UiRootPrefabBuilder.PrefabPath;
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var tooltipInstance = UiHudBuilder.Replace(root.transform.Find("Overlay"), tooltip);
                var timeInstance = UiHudBuilder.Replace(root.transform.Find("HudDynamic"), time);
                UiHudBuilder.Assign(timeInstance.GetComponent<TimeHudView>(), "_tooltip", tooltipInstance.GetComponent<UiTooltipView>());
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static ThemedText Text(Transform parent, string name, UiTextStyle style, UiTheme theme)
        {
            var go = UiHudBuilder.Instantiate("W_Text", parent, name);
            var label = go.GetComponent<TMP_Text>();
            label.alignment = TextAlignmentOptions.Center;
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
            image.enabled = false;
            return image;
        }
    }
}
