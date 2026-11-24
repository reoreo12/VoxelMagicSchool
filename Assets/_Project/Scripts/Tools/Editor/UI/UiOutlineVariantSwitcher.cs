// 책·가방 버튼과 점수 숫자·밑줄을 원본 ↔ 외곽선 v2 그림으로 바꿔 끼운다(배경과 구분되는지 화면 비교용). 테마의 스프라이트만 바꾼다.
// 외곽선 v2: 버튼 20×20, 숫자 칸 13×14(피치 9 그대로, 왼쪽부터 그림), 밑줄 가로세로 +2. 밑줄은 외곽선 두께만큼 올린다(Scores › Outline Px).
using System.Linq;
using UnityEditor;
using UnityEngine;
using VoxelMagic.UI;

namespace VoxelMagic.Tools.EditorTools.UiWorkbench
{
    public static class UiOutlineVariantSwitcher
    {
        private const string Art = "Assets/_Project/Art/UI/";

        [MenuItem("VoxelMagic/UI/외곽선 버전/외곽선 v2 쓰기")]
        private static void UseOutline() => Apply(true);

        [MenuItem("VoxelMagic/UI/외곽선 버전/원본 쓰기")]
        private static void UseOriginal() => Apply(false);

        public static void Apply(bool outline)
        {
            Debug.Log(UiImportRuleApplier.ApplyAll()); // 새 PNG도 UI 임포트 규칙(Point·PPU 16)으로
            var theme = AssetDatabase.LoadAssetAtPath<UiTheme>(UiRootPrefabBuilder.ThemePath);
            string suffix = outline ? "_v2" : "";

            Sprite[] digits;
            if (outline)
                digits = Enumerable.Range(0, 10).Select(i => Load($"score_ui/v2/digits/score_digit_{i}_v2.png")).ToArray();
            else
            {
                var sheet = AssetDatabase.LoadAllAssetsAtPath(Art + "score_ui/v2/score_digits_sheet.png").OfType<Sprite>().ToDictionary(s => s.name);
                digits = Enumerable.Range(0, 10).Select(i => sheet.TryGetValue($"score_digits_sheet_{i}", out var s) ? s : null).ToArray();
            }
            var written = Enumerable.Range(1, 4).Select(i => Load($"score_ui/underline/score_underline_{i}{suffix}.png")).ToArray();
            var practical = Enumerable.Range(1, 4).Select(i => Load($"score_ui/v2/underline/score_underline_{i}{suffix}.png")).ToArray();
            theme.Scores.SetSprites(digits, written, practical);

            var so = new SerializedObject(theme);
            so.FindProperty("_scores._outlinePx").intValue = outline ? 1 : 0;
            var buttons = so.FindProperty("_buttons");
            for (int i = 0; i < buttons.arraySize; i++)
            {
                var entry = buttons.GetArrayElementAtIndex(i);
                var id = (UiButtonSkin)entry.FindPropertyRelative("_id").enumValueIndex;
                string name = id == UiButtonSkin.MenuBook ? "book" : id == UiButtonSkin.MenuBag ? "bag" : null;
                if (name == null) continue;
                entry.FindPropertyRelative("_normal").objectReferenceValue = Load($"menu_icons/buttons/btn_{name}_normal{suffix}.png");
                entry.FindPropertyRelative("_hover").objectReferenceValue = Load($"menu_icons/buttons/btn_{name}_hover{suffix}.png");
                entry.FindPropertyRelative("_pressed").objectReferenceValue = Load($"menu_icons/buttons/btn_{name}_pressed{suffix}.png");
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(theme);
            AssetDatabase.SaveAssets();

            // 열린 화면이 바로 따라오게 테마를 쓰는 위젯을 다시 적용한다.
            foreach (var widget in Object.FindObjectsByType<ThemedWidget>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                widget.ApplyTheme();
            Debug.Log($"[UI] 책·가방·점수 그림을 {(outline ? "외곽선 v2" : "원본")}로 바꿨습니다.");
        }

        private static Sprite Load(string relative)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Art + relative);
            if (sprite == null) Debug.LogWarning($"[UI] 스프라이트가 없습니다: {relative}");
            return sprite;
        }
    }
}
