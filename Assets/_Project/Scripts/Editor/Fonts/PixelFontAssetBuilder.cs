// PixelFontSet대로 픽셀 폰트 TMP 에셋을 굽는다: 고정(Static, RASTER, Point 필터) + 예비(Dynamic, 빌드 때 글자 비움)를 fallback으로 연결.
// 새 문구를 넣은 뒤 메뉴로 다시 구우면 GUID는 유지된다. 설정 에셋이 없으면 측정한 설계 크기(갈무리11 12px, 스타더스트 13px)로 만든다.
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace VoxelMagic.EditorTools
{
    public static class PixelFontAssetBuilder
    {
        public const string SetPath = "Assets/_Project/Data/UI/PixelFontSet.asset";
        private const string SourceFolder = "Assets/_Project/Art/Font/";

        [MenuItem("VoxelMagic/UI/픽셀 폰트 다시 굽기")]
        private static void BakeFromMenu() => Selection.activeObject = BakeAll().FirstOrDefault();

        public static PixelFontSet GetOrCreateSet()
        {
            var set = AssetDatabase.LoadAssetAtPath<PixelFontSet>(SetPath);
            if (set != null) return set;
            Directory.CreateDirectory(Path.GetDirectoryName(SetPath));
            set = ScriptableObject.CreateInstance<PixelFontSet>();
            // 2026-11-20 측정: 갈무리11은 12px에서 테두리가 완전히 격자에 맞고, 스타더스트 3.0은 13px에서 획이 고르게 1px로 나온다.
            set.SetFonts(new[]
            {
                new PixelFontSet.Entry(Load("Galmuri11.ttf"), "Galmuri11", 12),
                new PixelFontSet.Entry(Load("Galmuri11-Bold.ttf"), "Galmuri11 Bold", 12),
                new PixelFontSet.Entry(Load("PF스타더스트 3.0.ttf"), "Stardust3", 13),
                new PixelFontSet.Entry(Load("PF스타더스트 3.0 Bold.ttf"), "Stardust3 Bold", 13),
                // 2026-11-20 측정: 갈무리9는 10px, 갈무리7은 8px에서 격자에 맞는다(작은 글씨용, Bold 없음).
                new PixelFontSet.Entry(Load("Galmuri9.ttf"), "Galmuri9", 10),
                new PixelFontSet.Entry(Load("Galmuri7.ttf"), "Galmuri7", 8),
            });
            AssetDatabase.CreateAsset(set, SetPath);
            AssetDatabase.SaveAssets();
            return set;
        }

        public static TMP_FontAsset[] BakeAll()
        {
            var set = GetOrCreateSet();
            var characters = FontCharsetCollector.Collect(set.CharsetPath, set.TextSourceFolders);
            return set.Fonts.Where(e => e.Source != null).Select(e => Bake(set, e, characters)).ToArray();
        }

        private static TMP_FontAsset Bake(PixelFontSet set, PixelFontSet.Entry entry, string characters)
        {
            var folder = set.OutputFolder.TrimEnd('/');
            var fallback = GetOrCreateFallback(set, entry, $"{folder}/{entry.AssetName} Fallback.asset");

            var font = TMP_FontAsset.CreateFontAsset(entry.Source, entry.SamplingSize, set.Padding, entry.RenderMode,
                set.StaticAtlasSize, set.StaticAtlasSize, AtlasPopulationMode.Dynamic, true);
            font.TryAddCharacters(characters, out var missing);
            font.atlasPopulationMode = AtlasPopulationMode.Static;
            UsePointFilter(font);
            font.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset> { fallback };
            if (!string.IsNullOrEmpty(missing))
                Debug.LogWarning($"[Font] {entry.AssetName}에 없는 글자 {missing.Length}개: {missing}");

            return FontAssetFileWriter.Save(font, $"{folder}/{entry.AssetName}.asset");
        }

        // 예비 에셋은 처음 한 번만 만든다. 빌드할 때 에디터에서 추가된 글자를 비운다(Clear Dynamic Data On Build).
        private static TMP_FontAsset GetOrCreateFallback(PixelFontSet set, PixelFontSet.Entry entry, string path)
        {
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (existing != null) return existing;
            var font = TMP_FontAsset.CreateFontAsset(entry.Source, entry.SamplingSize, set.Padding, entry.RenderMode,
                set.FallbackAtlasSize, set.FallbackAtlasSize, AtlasPopulationMode.Dynamic, false);
            UsePointFilter(font);
            var saved = FontAssetFileWriter.Save(font, path);
            // 이 옵션은 공개 속성이 없어 직렬화 필드로 켠다.
            var so = new SerializedObject(saved);
            so.FindProperty("m_ClearDynamicDataOnBuild").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            return saved;
        }

        private static void UsePointFilter(TMP_FontAsset font)
        {
            foreach (var texture in font.atlasTextures)
                if (texture != null) texture.filterMode = FilterMode.Point;
        }

        private static Font Load(string file) => AssetDatabase.LoadAssetAtPath<Font>(SourceFolder + file);
    }
}
