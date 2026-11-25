// 점수 HUD 프리팹(화면 위 가운데 필기·실기 예상 점수, UI_SPEC 9장)을 만들어 UI_Root에 넣고, 테마에 숫자·밑줄 스프라이트와 문구를 채운다.
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using VoxelMagic.Core;
using VoxelMagic.UI;

namespace VoxelMagic.Tools.EditorTools.UiWorkbench
{
    public static class UiHudScoreBuilder
    {
        private const string Art = "Assets/_Project/Art/UI/score_ui/";
        private const string ScoreName = "HUD_Score";
        // 임시 글자색: 게임 화면 위에 바로 놓이므로 밝은 종이색(사용자 피드백으로 조정).
        private static readonly Color LabelCream = new Color32(0xF4, 0xE8, 0xCC, 0xFF);

        [MenuItem("VoxelMagic/UI/HUD 만들기/점수")]
        public static void Build()
        {
            Debug.Log(UiImportRuleApplier.ApplyAll());
            var theme = UiWidgetPresetBuilder.FillThemeDefaults();
            FillTheme(theme);
            SeedTexts();
            var prefab = UiHudBuilder.SavePrefab(BuildScore(theme));
            InsertIntoRoot(prefab);
            Debug.Log("[UI] 점수 HUD를 UI_Root에 넣었습니다.");
        }

        private static void FillTheme(UiTheme theme)
        {
            var sheet = AssetDatabase.LoadAllAssetsAtPath(Art + "v2/score_digits_sheet.png").OfType<Sprite>().ToDictionary(s => s.name);
            var digits = Enumerable.Range(0, 10).Select(i => sheet.TryGetValue($"score_digits_sheet_{i}", out var s) ? s : null).ToArray();
            var written = Enumerable.Range(1, 4).Select(i => AssetDatabase.LoadAssetAtPath<Sprite>($"{Art}underline/score_underline_{i}.png")).ToArray();
            var practical = Enumerable.Range(1, 4).Select(i => AssetDatabase.LoadAssetAtPath<Sprite>($"{Art}v2/underline/score_underline_{i}.png")).ToArray();
            if (theme.Scores.Digit(0) == null) theme.Scores.SetSprites(digits, written, practical);
            theme.AddMissing(new[] { new TextStyleValues(UiTextStyle.HudLabel, true, 1, LabelCream) },
                System.Array.Empty<ButtonSkinValues>(), System.Array.Empty<PanelSkinValues>());
            EditorUtility.SetDirty(theme);
            AssetDatabase.SaveAssets();
        }

        private static void SeedTexts()
        {
            var table = AssetDatabase.LoadAssetAtPath<TextTable>(UiRootPrefabBuilder.TextTablePath);
            if (table == null) return;
            if (!table.TryGet(UiTextKeys.ScoreWritten, out _)) table.Set(UiTextKeys.ScoreWritten, "필기 예상 점수");
            if (!table.TryGet(UiTextKeys.ScorePractical, out _)) table.Set(UiTextKeys.ScorePractical, "실기 예상 점수");
            EditorUtility.SetDirty(table);
            AssetDatabase.SaveAssets();
        }

        private static GameObject BuildScore(UiTheme theme)
        {
            var root = new GameObject(ScoreName, typeof(RectTransform));
            var row = new GameObject("Row", typeof(RectTransform));
            row.transform.SetParent(root.transform, false);

            var writtenLabel = Label(row.transform, "WrittenLabel", theme);
            var writtenNumber = Number(row.transform, "WrittenNumber", theme.Scores.MaxDigits);
            var practicalLabel = Label(row.transform, "PracticalLabel", theme);
            var practicalNumber = Number(row.transform, "PracticalNumber", theme.Scores.MaxDigits);

            var view = root.AddComponent<ScoreHudView>();
            UiHudBuilder.Assign(view, "_row", row.transform);
            UiHudBuilder.Assign(view, "_writtenLabel", writtenLabel);
            UiHudBuilder.Assign(view, "_writtenNumber", writtenNumber);
            UiHudBuilder.Assign(view, "_practicalLabel", practicalLabel);
            UiHudBuilder.Assign(view, "_practicalNumber", practicalNumber);
            view.SetTheme(theme);
            return root;
        }

        private static ThemedText Label(Transform parent, string name, UiTheme theme)
        {
            var go = UiHudBuilder.Instantiate("W_Text", parent, name);
            var text = go.GetComponent<TMP_Text>();
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Overflow;
            var themed = go.GetComponent<ThemedText>();
            themed.SetTheme(theme);
            themed.SetStyle(UiTextStyle.HudLabel);
            return themed;
        }

        private static ScoreNumberView Number(Transform parent, string name, int maxDigits)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var view = go.AddComponent<ScoreNumberView>();
            var so = new SerializedObject(view);
            var digits = so.FindProperty("_digits");
            digits.arraySize = maxDigits;
            for (int i = 0; i < maxDigits; i++)
                digits.GetArrayElementAtIndex(i).objectReferenceValue = NewImage(go.transform, $"Digit{i + 1}");
            so.FindProperty("_underline").objectReferenceValue = NewImage(go.transform, "Underline");
            so.ApplyModifiedPropertiesWithoutUndo();
            return view;
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

        private static void InsertIntoRoot(GameObject prefab)
        {
            var path = UiRootPrefabBuilder.PrefabPath;
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                UiHudBuilder.Replace(root.transform.Find("HudDynamic"), prefab);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
    }
}
