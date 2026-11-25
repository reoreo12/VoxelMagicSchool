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
            // 덮어쓸 때는 옆의 임시 폴더에 같은 파일 이름으로 먼저 저장한다(이름이 달라 생기는 경고 방지).
            var folder = Path.GetDirectoryName(assetPath).Replace('\\', '/');
            var bakingFolder = folder + "/_Baking";
            var savePath = replace ? $"{bakingFolder}/{Path.GetFileName(assetPath)}" : assetPath;
            if (replace && !AssetDatabase.IsValidFolder(bakingFolder)) AssetDatabase.CreateFolder(folder, "_Baking");

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
            // 서브 에셋을 붙이며 이름이 바뀌지 않도록 최종 이름을 한 번 더 넣고 저장한다.
            font.name = Path.GetFileNameWithoutExtension(assetPath);
            EditorUtility.SetDirty(font);
            AssetDatabase.SaveAssets();

            if (replace)
            {
                File.Copy(savePath, assetPath, true);
                AssetDatabase.DeleteAsset(bakingFolder);
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            }

            var result = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
            Debug.Log($"[Font] {result.name}: 글자 {result.characterTable.Count}개, 아틀라스 {result.atlasTextures.Count(t => t != null)}장");
            return result;
        }
    }
}
