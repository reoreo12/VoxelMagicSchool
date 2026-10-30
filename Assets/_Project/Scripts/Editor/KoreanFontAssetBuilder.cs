// OS에 설치된 맑은 고딕을 참조하는 TMP 동적 폰트 에셋을 만든다 (폰트 파일 복사·다운로드 없음).
using TMPro;
using UnityEditor;
using UnityEngine;

namespace VoxelMagic.EditorTools
{
    public static class KoreanFontAssetBuilder
    {
        public const string AssetPath = "Assets/_Project/Fonts/MalgunGothic SDF.asset";
        private const string FamilyName = "Malgun Gothic";
        private const string StyleName = "Regular";
        private const int SamplingPointSize = 90;

        [MenuItem("VoxelMagic/한글 폰트 에셋 만들기")]
        private static void CreateFromMenu() => Selection.activeObject = GetOrCreate();

        public static TMP_FontAsset GetOrCreate()
        {
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetPath);
            if (existing != null) return existing;

            var font = TMP_FontAsset.CreateFontAsset(FamilyName, StyleName, SamplingPointSize);
            if (font == null)
            {
                Debug.LogError($"[KoreanFont] OS 폰트 '{FamilyName}'를 찾지 못했습니다.");
                return null;
            }

            font.name = "MalgunGothic SDF";
            AssetDatabase.CreateAsset(font, AssetPath);
            foreach (var texture in font.atlasTextures)
            {
                texture.name = font.name + " Atlas";
                AssetDatabase.AddObjectToAsset(texture, font);
            }
            font.material.name = font.name + " Material";
            AssetDatabase.AddObjectToAsset(font.material, font);
            AssetDatabase.SaveAssets();
            return font;
        }
    }
}
