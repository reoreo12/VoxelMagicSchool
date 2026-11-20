// Pretendard 폰트 파일로 TMP 고정(Static) 폰트 에셋을 굽는다. 기본 문자 목록 + 게임 문구·데이터에 쓰인 글자를 넣는다.
// 고정 아틀라스라 플레이 중 글자가 추가되지 않아 에셋 파일이 바뀌지 않는다. 다시 구워도 GUID는 유지된다.
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace VoxelMagic.EditorTools
{
    public static class KoreanFontAssetBuilder
    {
        public const string AssetPath = "Assets/_Project/Fonts/Pretendard SDF.asset";
        private const string SourceFontPath = "Assets/_Project/Fonts/Pretendard/Pretendard-Regular.otf";
        private const string CharsetPath = "Assets/_Project/Fonts/Pretendard/KoreanCharset.txt";
        private static readonly string[] TextSourceFolders = { "Assets/_Project/Scripts/Core", "Assets/_Project/Data" };

        private const int SamplingPointSize = 32;
        private const int AtlasPadding = 4;
        private const int AtlasSize = 2048;

        [MenuItem("VoxelMagic/한글 폰트 다시 굽기")]
        private static void BakeFromMenu() => Selection.activeObject = Bake();

        public static TMP_FontAsset GetOrCreate()
        {
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetPath);
            return existing != null ? existing : Bake();
        }

        public static TMP_FontAsset Bake()
        {
            var source = AssetDatabase.LoadAssetAtPath<Font>(SourceFontPath);
            if (source == null)
            {
                Debug.LogError($"[KoreanFont] 폰트 파일이 없습니다: {SourceFontPath}");
                return null;
            }

            var font = TMP_FontAsset.CreateFontAsset(source, SamplingPointSize, AtlasPadding,
                GlyphRenderMode.SDFAA, AtlasSize, AtlasSize, AtlasPopulationMode.Dynamic, true);
            font.TryAddCharacters(CollectCharacters(), out var missing);
            font.atlasPopulationMode = AtlasPopulationMode.Static;
            if (!string.IsNullOrEmpty(missing))
                Debug.LogWarning($"[KoreanFont] 폰트에 없는 글자 {missing.Length}개: {missing}");

            return FontAssetFileWriter.Save(font, AssetPath);
        }

        private static string CollectCharacters() => FontCharsetCollector.Collect(CharsetPath, TextSourceFolders);
    }
}
