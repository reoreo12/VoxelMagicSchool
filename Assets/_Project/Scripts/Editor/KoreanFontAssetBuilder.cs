// Pretendard 폰트 파일로 TMP 고정(Static) 폰트 에셋을 굽는다. 기본 문자 목록 + 게임 문구·데이터에 쓰인 글자를 넣는다.
// 고정 아틀라스라 플레이 중 글자가 추가되지 않아 에셋 파일이 바뀌지 않는다. 다시 구워도 GUID는 유지된다.
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace VoxelMagic.EditorTools
{
    public static class KoreanFontAssetBuilder
    {
        public const string AssetPath = "Assets/_Project/Fonts/Pretendard SDF.asset";
        private const string TempAssetPath = "Assets/_Project/Fonts/Pretendard SDF (Baking).asset";
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
            font.name = Path.GetFileNameWithoutExtension(AssetPath);
            font.TryAddCharacters(CollectCharacters(), out var missing);
            font.atlasPopulationMode = AtlasPopulationMode.Static;
            if (!string.IsNullOrEmpty(missing))
                Debug.LogWarning($"[KoreanFont] 폰트에 없는 글자 {missing.Length}개: {missing}");

            // 새 파일로 저장한 뒤 기존 파일 내용만 교체해 .meta(GUID)를 지킨다. 씬·프리팹 참조가 끊기지 않는다.
            var replace = File.Exists(AssetPath);
            var savePath = replace ? TempAssetPath : AssetPath;
            AssetDatabase.CreateAsset(font, savePath);
            for (var i = 0; i < font.atlasTextures.Length; i++)
            {
                var texture = font.atlasTextures[i];
                if (texture == null) continue;
                texture.name = font.name + " Atlas" + (i == 0 ? "" : " " + i);
                if (!AssetDatabase.Contains(texture)) AssetDatabase.AddObjectToAsset(texture, font);
            }
            font.material.name = font.name + " Material";
            AssetDatabase.AddObjectToAsset(font.material, font);
            AssetDatabase.SaveAssets();

            if (replace)
            {
                File.Copy(savePath, AssetPath, true);
                AssetDatabase.DeleteAsset(savePath);
                AssetDatabase.ImportAsset(AssetPath, ImportAssetOptions.ForceUpdate);
            }

            var result = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetPath);
            Debug.Log($"[KoreanFont] 굽기 완료: 글자 {result.characterTable.Count}개, 아틀라스 {result.atlasTextures.Count(t => t != null)}장");
            return result;
        }

        // 기본 문자 목록 + Core 스크립트와 데이터 에셋에 쓰인 비ASCII 글자 (YAML의 \uXXXX 표기도 풀어서 읽는다).
        private static string CollectCharacters()
        {
            var chars = new SortedSet<char>(File.ReadAllText(CharsetPath).Where(c => !char.IsControl(c)));
            var escaped = new Regex(@"\\u([0-9A-Fa-f]{4})");
            foreach (var folder in TextSourceFolders)
            foreach (var path in Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories)
                         .Where(p => p.EndsWith(".cs") || p.EndsWith(".asset")))
            {
                var text = escaped.Replace(File.ReadAllText(path),
                    m => ((char)System.Convert.ToInt32(m.Groups[1].Value, 16)).ToString());
                foreach (var c in text)
                    if (c > 0x7F && !char.IsControl(c) && !char.IsSurrogate(c)) chars.Add(c);
            }
            return new StringBuilder().Append(chars.ToArray()).ToString();
        }
    }
}
