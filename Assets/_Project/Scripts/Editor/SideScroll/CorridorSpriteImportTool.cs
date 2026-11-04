// 복도·창밖 배경 이미지에 스프라이트 임포트 설정을 일괄 적용한다. PPU·필터·최대 크기는 CorridorConfig에서 읽는다.
using UnityEditor;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.EditorTools
{
    public static class CorridorSpriteImportTool
    {
        public const string CorridorArtFolder = "Assets/_Project/Art/Corridor";
        public const string BackdropArtFolder = "Assets/_Project/Art/Backdrop";

        [MenuItem("VoxelMagic/복도/이미지 임포트 설정 적용")]
        private static void ApplyFromMenu() => Apply(CorridorSceneBuilder.LoadOrCreateConfig());

        public static void Apply(CorridorConfig config)
        {
            ApplyFolder(CorridorArtFolder, config.CorridorFilter, config);
            ApplyFolder(BackdropArtFolder, config.BackdropFilter, config);
        }

        private static void ApplyFolder(string folder, FilterMode filter, CorridorConfig config)
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { folder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!(AssetImporter.GetAtPath(path) is TextureImporter importer)) continue;

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = config.PixelsPerUnit;
                importer.filterMode = filter;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.maxTextureSize = config.MaxTextureSize;

                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect;
                settings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
                settings.spritePivot = new Vector2(0.5f, 0f);
                importer.SetTextureSettings(settings);
                importer.SaveAndReimport();
            }
        }
    }
}
