// UiImportRules를 UI 폴더의 PNG에 일괄 적용한다(Sprite·Point·압축 없음·Full Rect·PPU·9-slice 경계·시트 자르기).
// 새 PNG를 넣거나 규칙을 바꾼 뒤 메뉴로 다시 실행한다. 규칙 에셋이 없으면 UI_SPEC 값으로 처음 한 번 만든다.
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace VoxelMagic.Tools.EditorTools.UiWorkbench
{
    public static class UiImportRuleApplier
    {
        public const string RulesAssetPath = "Assets/_Project/Data/Tools/UI/UiImportRules.asset";

        [MenuItem("VoxelMagic/UI/임포트 규칙 적용")]
        private static void ApplyFromMenu() => Debug.Log(ApplyAll());

        public static UiImportRules GetOrCreateRules()
        {
            var rules = AssetDatabase.LoadAssetAtPath<UiImportRules>(RulesAssetPath);
            if (rules != null)
            {
                // 새로 생긴 기본 규칙(예: 다음 화면에서 쓰는 시트)만 덧붙인다.
                if (rules.AddMissing(DefaultRules()))
                {
                    EditorUtility.SetDirty(rules);
                    AssetDatabase.SaveAssets();
                }
                return rules;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(RulesAssetPath));
            rules = ScriptableObject.CreateInstance<UiImportRules>();
            rules.ResetRules(DefaultRules()); // 처음 값만 넣고, 이후 조절은 인스펙터에서
            AssetDatabase.CreateAsset(rules, RulesAssetPath);
            AssetDatabase.SaveAssets();
            return rules;
        }

        // UI_SPEC.md 4장·5-1·7-2·9-2·10-7 값. 경계는 (왼쪽, 위, 오른쪽, 아래).
        private static IEnumerable<UiImportRule> DefaultRules()
        {
            yield return UiImportRule.Create("book_ui/book_page_*.png", 8, 8, 8, 10);
            yield return UiImportRule.Create("book_ui/book_spine.png", 0, 8, 0, 10);
            yield return UiImportRule.Create("book_ui/tabs/*.png", 4, 4, 4, 4);
            yield return UiImportRule.Create("book_ui/page_turn/leaf_edge.png", 0, 6, 0, 6);
            yield return UiImportRule.Create("book_ui/page_turn/leaf_*.png", 2, 6, 6, 6);
            yield return UiImportRule.Create("bar_ui/bar_frame_vertical.png", 5, 5, 5, 5);
            yield return UiImportRule.Create("quest_ui/quest_*_paper.png", 6, 6, 6, 6);
            yield return UiImportRule.Create("quest_ui/scrollbar/scroll_track.png", 0, 4, 0, 4);
            yield return UiImportRule.Create("quest_ui/scrollbar/scroll_handle.png", 0, 3, 0, 3);
            yield return UiImportRule.Grid("score_ui/v2/score_digits_sheet.png", new Vector2Int(11, 12));
            // Sprout Lands 인벤토리 시트(UI_SPEC 7-1): 중간 크기 칸 30×32(경계 4/5/4/7)과 선택 프레임 중간 색 모서리 11×12.
            yield return UiImportRule.Named("Sprout Lands - UI Pack - Basic packemojis-free/Inventory/Inventory_Spritesheet.png",
                new UiSliceRect("inv_slot_light", 233, 121, 30, 32, 4, 5, 4, 7),
                new UiSliceRect("inv_slot_mid", 281, 121, 30, 32, 4, 5, 4, 7),
                new UiSliceRect("inv_bar_dark", 329, 121, 30, 32, 4, 5, 4, 7),
                new UiSliceRect("inv_select_tl", 273, 0, 11, 12),
                new UiSliceRect("inv_select_tr", 308, 0, 11, 12),
                new UiSliceRect("inv_select_bl", 273, 36, 11, 12),
                new UiSliceRect("inv_select_br", 308, 36, 11, 12));
            // 시간대 UI(UI_SPEC 8-1): 틀(날짜 패널 포함)은 v2 시트(날짜 판 가운데, 잎 없음), 룰렛(방과후 주황·밤 파랑)·가로 화살표는 Noon 수정본,
            // 큰 아이콘은 원본 작은 날씨 아이콘의 왼쪽 열(노을 = 방과후, 달 = 밤).
            yield return UiImportRule.Named("Sprout Lands (수정본)/weather/Weather_UI_Free_v2.png",
                new UiSliceRect("time_frame", 7, 7, 56, 54));
            yield return UiImportRule.Named("Sprout Lands (수정본)/weather/Weather_UI_Free_Noon_ver.png",
                new UiSliceRect("time_roulette", 17, 71, 13, 17),
                new UiSliceRect("time_arrow", 4, 85, 10, 8));
            yield return UiImportRule.Named("Sprout Lands - UI Pack - Basic packemojis-free/weather/Weather_Icons_smal_freel.png",
                new UiSliceRect("time_icon_night", 7, 39, 18, 17),
                new UiSliceRect("time_icon_after_school", 7, 71, 18, 17));
        }

        public static string ApplyAll()
        {
            var rules = GetOrCreateRules();
            var root = rules.RootFolder.TrimEnd('/');
            var paths = AssetDatabase.FindAssets("t:Texture2D", new[] { root })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => p.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
                .Where(p => !rules.SkipFolderNames.Any(s => p.Split('/').Contains(s)))
                .Distinct()
                .ToList();

            int changed = 0, sliced = 0;
            foreach (var path in paths)
            {
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;
                var rule = rules.Find(path.Substring(root.Length + 1));
                if (Apply(importer, rules, rule))
                {
                    importer.SaveAndReimport();
                    changed++;
                    if (rule != null && rule.SliceGrid) sliced++;
                }
            }
            return $"[UI 임포트] 대상 {paths.Count}개 중 {changed}개 갱신 (시트 자르기 {sliced}개)";
        }

        // 바뀐 것이 있으면 true.
        private static bool Apply(TextureImporter importer, UiImportRules rules, UiImportRule rule)
        {
            var before = EditorJsonUtility.ToJson(importer);
            var multiple = rule != null && (rule.SliceGrid || rule.SliceNamed);

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = multiple ? SpriteImportMode.Multiple : SpriteImportMode.Single;
            importer.spritePixelsPerUnit = rules.PixelsPerUnit;
            importer.filterMode = rules.FilterMode;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteBorder = !multiple && rule != null ? rule.ToUnityBorder() : Vector4.zero;
            importer.SetTextureSettings(settings);

            var changed = before != EditorJsonUtility.ToJson(importer);
            if (multiple && rule.SliceNamed) changed |= SliceNamed(importer, rule.Slices);
            else if (multiple) changed |= SliceGrid(importer, rule.CellSize);
            return changed;
        }

        // 이름 붙인 조각만 자른다. 이름이 같은 조각은 기존 ID를 유지한다.
        private static bool SliceNamed(TextureImporter importer, IReadOnlyList<UiSliceRect> slices)
        {
            var factory = new SpriteDataProviderFactories();
            factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();
            importer.GetSourceTextureWidthAndHeight(out _, out var height);
            var existing = provider.GetSpriteRects().ToDictionary(r => r.name, r => r);

            var rects = new List<SpriteRect>();
            foreach (var slice in slices)
            {
                var rect = existing.TryGetValue(slice.Name, out var old) ? old : new SpriteRect { name = slice.Name, spriteID = GUID.Generate() };
                rect.rect = slice.ToUnityRect(height);
                rect.border = slice.ToUnityBorder();
                rect.alignment = SpriteAlignment.Center;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rects.Add(rect);
            }

            var same = existing.Count == rects.Count && rects.All(r => existing.ContainsKey(r.name)
                && existing[r.name].rect == r.rect && existing[r.name].border == r.border);
            if (same) return false;

            provider.SetSpriteRects(rects.ToArray());
            var nameIds = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            nameIds?.SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)).ToList());
            provider.Apply();
            return true;
        }

        // 위쪽 줄부터 왼쪽→오른쪽으로 칸 크기대로 자른다. 이름이 같은 칸은 기존 ID를 유지해 참조가 끊기지 않게 한다.
        private static bool SliceGrid(TextureImporter importer, Vector2Int cell)
        {
            var factory = new SpriteDataProviderFactories();
            factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();

            importer.GetSourceTextureWidthAndHeight(out var width, out var height);
            var baseName = Path.GetFileNameWithoutExtension(importer.assetPath);
            var existing = provider.GetSpriteRects().ToDictionary(r => r.name, r => r);

            var rects = new List<SpriteRect>();
            var index = 0;
            for (var y = height - cell.y; y >= 0; y -= cell.y)
            for (var x = 0; x + cell.x <= width; x += cell.x)
            {
                var name = $"{baseName}_{index++}";
                var rect = existing.TryGetValue(name, out var old) ? old : new SpriteRect { name = name, spriteID = GUID.Generate() };
                rect.rect = new Rect(x, y, cell.x, cell.y);
                rect.alignment = SpriteAlignment.Center;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.border = Vector4.zero;
                rects.Add(rect);
            }

            var same = existing.Count == rects.Count && rects.All(r => existing.ContainsKey(r.name) && existing[r.name].rect == r.rect);
            if (same) return false;

            provider.SetSpriteRects(rects.ToArray());
            var nameIds = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            nameIds?.SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)).ToList());
            provider.Apply();
            return true;
        }
    }
}
