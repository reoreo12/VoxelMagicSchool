// 새로 구운 TMP 폰트 에셋을 저장한다. 같은 경로에 이미 있으면 내용만 바꿔 .meta(GUID)를 지킨다 → 씬·프리팹 참조가 끊기지 않는다.
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace VoxelMagic.EditorTools
{
    public static class FontAssetFileWriter
    {
        public static TMP_FontAsset Save(TMP_FontAsset font, string assetPath)
        {
            font.name = Path.GetFileNameWithoutExtension(assetPath);
            var replace = File.Exists(assetPath);
            var savePath = replace ? Path.ChangeExtension(assetPath, null) + " (Baking).asset" : assetPath;

            Directory.CreateDirectory(Path.GetDirectoryName(assetPath));
            AssetDatabase.CreateAsset(font, savePath);
            for (var i = 0; i < font.atlasTextures.Length; i++)
            {
                var texture = font.atlasTextures[i];
                if (texture == null) continue;
                texture.name = font.name + " Atlas" + (i == 0 ? "" : " " + i);
                if (!AssetDatabase.Contains(texture)) AssetDatabase.AddObjectToAsset(texture, font);
            }
            font.material.name = font.name + " Material";
            if (!AssetDatabase.Contains(font.material)) AssetDatabase.AddObjectToAsset(font.material, font);
            AssetDatabase.SaveAssets();

            if (replace)
            {
                File.Copy(savePath, assetPath, true);
                AssetDatabase.DeleteAsset(savePath);
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            }

            var result = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
            Debug.Log($"[Font] {result.name}: 글자 {result.characterTable.Count}개, 아틀라스 {result.atlasTextures.Count(t => t != null)}장");
            return result;
        }
    }
}
