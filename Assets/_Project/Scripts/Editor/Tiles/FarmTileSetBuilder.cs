// 타일 팩 임포트 설정(팩 전체)과 farm 타일·Rule Tile·Tile Palette를 만든다. 이미 있으면 덮어쓰기 전에 묻는다.
// 경계 이름 = 흙(풀이 아닌 쪽)이 있는 방향 (DESIGN_TILEMAP.md 1-3, 6-1).
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;
using VoxelMagic.Core;

namespace VoxelMagic.EditorTools
{
    public static class FarmTileSetBuilder
    {
        public const string ConfigPath = "Assets/_Project/Data/Tiles/TileMapConfig.asset";
        public const string TilePackFolder = "Assets/_Project/Art/Map/Tile";
        private const string FarmFolder = TilePackFolder + "/farm";
        private const string TileAssetFolder = "Assets/_Project/Art/Map/TileAssets/farm";
        private const string PaletteFolder = "Assets/_Project/Art/Map/TilePalettes";
        private const string PaletteName = "Farm";
        public const string PalettePath = PaletteFolder + "/" + PaletteName + ".prefab";
        private const string BaseFloor = "meadow_grass";
        private const string TransitionPrefix = BaseFloor + "_to_";
        private const int SetColumnStride = 6;   // 팔레트에서 경계 세트 사이 간격 (5칸 + 빈칸 1)
        private const int TransitionTopRow = -3;

        private static readonly Vector3Int N = new Vector3Int(0, 1, 0), S = new Vector3Int(0, -1, 0),
            E = new Vector3Int(1, 0, 0), W = new Vector3Int(-1, 0, 0),
            NE = new Vector3Int(1, 1, 0), NW = new Vector3Int(-1, 1, 0),
            SE = new Vector3Int(1, -1, 0), SW = new Vector3Int(-1, -1, 0);

        // Rule Tile 규칙 (위에서부터 먼저 맞는 것). notThis = 흙이 아닌 이웃, this = 같은 흙 이웃
        private static readonly (string suffix, Vector3Int[] notThis, Vector3Int[] same)[] Rules =
        {
            ("se", new[] { N, W }, new[] { S, E }),
            ("sw", new[] { N, E }, new[] { S, W }),
            ("ne", new[] { S, W }, new[] { N, E }),
            ("nw", new[] { S, E }, new[] { N, W }),
            ("s", new[] { N }, new[] { S, E, W }),
            ("n", new[] { S }, new[] { N, E, W }),
            ("w", new[] { E }, new[] { N, S, W }),
            ("e", new[] { W }, new[] { N, S, E }),
            ("isw", new[] { NE }, new[] { N, S, E, W }),
            ("ise", new[] { NW }, new[] { N, S, E, W }),
            ("inw", new[] { SE }, new[] { N, S, E, W }),
            ("ine", new[] { SW }, new[] { N, S, E, W }),
        };

        // 팔레트 안 경계 세트 배치 (접미사 → 세트 블록 안의 열·행). "" = 가운데 바닥 타일
        private static readonly (string suffix, int x, int y)[] TransitionLayout =
        {
            ("nw", 0, 0), ("n", 1, 0), ("ne", 2, 0), ("inw", 3, 0), ("ine", 4, 0),
            ("w", 0, -1), ("", 1, -1), ("e", 2, -1), ("isw", 3, -1), ("ise", 4, -1),
            ("sw", 0, -2), ("s", 1, -2), ("se", 2, -2),
        };

        [MenuItem("VoxelMagic/타일 맵/타일 팩 임포트 설정 적용 (전체)")]
        private static void ApplyImportFromMenu() => ApplyImportSettings(LoadOrCreateConfig());

        [MenuItem("VoxelMagic/타일 맵/farm 타일·Rule Tile·팔레트 만들기")]
        private static void BuildFromMenu() => BuildFarm(LoadOrCreateConfig(), askBeforeOverwrite: true);

        public static TileMapConfig LoadOrCreateConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<TileMapConfig>(ConfigPath);
            if (config != null) return config;
            EnsureFolder(Path.GetDirectoryName(ConfigPath));
            config = ScriptableObject.CreateInstance<TileMapConfig>();
            AssetDatabase.CreateAsset(config, ConfigPath);
            AssetDatabase.SaveAssets();
            return config;
        }

        public static int ApplyImportSettings(TileMapConfig config)
        {
            int count = 0;
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { TilePackFolder }))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (!(AssetImporter.GetAtPath(path) is TextureImporter importer)) continue;

                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.spritePixelsPerUnit = config.TilePixels;
                    importer.filterMode = FilterMode.Point;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.mipmapEnabled = false;
                    importer.alphaIsTransparency = true;
                    importer.wrapMode = TextureWrapMode.Clamp;

                    var settings = new TextureImporterSettings();
                    importer.ReadTextureSettings(settings);
                    settings.spriteMeshType = SpriteMeshType.FullRect;
                    settings.spriteAlignment = (int)SpriteAlignment.Center;
                    settings.spritePivot = new Vector2(0.5f, 0.5f);
                    importer.SetTextureSettings(settings);
                    importer.SaveAndReimport();
                    count++;
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
            Debug.Log($"[타일 맵] 임포트 설정 적용: {count}장");
            return count;
        }

        /// <returns>만들었으면 true, 사용자가 취소했으면 false</returns>
        public static bool BuildFarm(TileMapConfig config, bool askBeforeOverwrite)
        {
            string palettePath = $"{PaletteFolder}/{PaletteName}.prefab";
            bool anyExisting = AssetDatabase.IsValidFolder(TileAssetFolder) ||
                               AssetDatabase.LoadAssetAtPath<GameObject>(palettePath) != null;
            if (anyExisting && askBeforeOverwrite &&
                !EditorUtility.DisplayDialog("farm 타일 다시 만들기",
                    $"{TileAssetFolder} 또는 팔레트 {PaletteName}가 이미 있습니다.\n기존 에셋을 같은 이름으로 덮어쓸까요? (GUID는 유지)",
                    "덮어쓰기", "취소"))
                return false;

            var floors = new Dictionary<string, Tile>();
            foreach (var path in PngsIn($"{FarmFolder}/floors"))
                floors[Path.GetFileNameWithoutExtension(path)] = SaveTile(path, $"{TileAssetFolder}/floors");

            var sets = new List<(string floor, Dictionary<string, Tile> tiles, RuleTile rule)>();
            foreach (var setFolder in AssetDatabase.GetSubFolders($"{FarmFolder}/transitions"))
            {
                string setName = Path.GetFileName(setFolder);
                if (!setName.StartsWith(TransitionPrefix)) { Debug.LogWarning($"[타일 맵] 이름 규칙 밖 세트 건너뜀: {setName}"); continue; }
                string floor = setName.Substring(TransitionPrefix.Length);
                if (!floors.ContainsKey(floor)) { Debug.LogWarning($"[타일 맵] 가운데 바닥 없음: {floor}"); continue; }

                var tiles = new Dictionary<string, Tile>();
                foreach (var path in PngsIn(setFolder))
                    tiles[Path.GetFileNameWithoutExtension(path).Substring(setName.Length + 1)] =
                        SaveTile(path, $"{TileAssetFolder}/transitions/{setName}");
                sets.Add((floor, tiles, SaveRuleTile(floor, floors[floor].sprite, tiles)));
            }

            BuildPalette(palettePath, config, floors, sets);
            AssetDatabase.SaveAssets();
            Debug.Log($"[타일 맵] farm: 바닥 {floors.Count}, 경계 세트 {sets.Count}, Rule Tile {sets.Count}, 팔레트 {palettePath}");
            return true;
        }

        private static Tile SaveTile(string spritePath, string folder)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (sprite == null) throw new System.InvalidOperationException($"스프라이트가 아님 (임포트 설정 먼저): {spritePath}");
            EnsureFolder(folder);
            string path = $"{folder}/{Path.GetFileNameWithoutExtension(spritePath)}.asset";
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                AssetDatabase.CreateAsset(tile, path);
            }
            tile.sprite = sprite;
            tile.colliderType = Tile.ColliderType.None;
            EditorUtility.SetDirty(tile);
            return tile;
        }

        private static RuleTile SaveRuleTile(string floor, Sprite center, Dictionary<string, Tile> edges)
        {
            string folder = $"{TileAssetFolder}/rules";
            EnsureFolder(folder);
            string path = $"{folder}/RT_{floor}.asset";
            var rule = AssetDatabase.LoadAssetAtPath<RuleTile>(path);
            if (rule == null)
            {
                rule = ScriptableObject.CreateInstance<RuleTile>();
                AssetDatabase.CreateAsset(rule, path);
            }
            rule.m_DefaultSprite = center;
            rule.m_DefaultColliderType = Tile.ColliderType.None;
            rule.m_TilingRules = new List<RuleTile.TilingRule>();
            foreach (var (suffix, notThis, same) in Rules)
            {
                if (!edges.TryGetValue(suffix, out var tile)) { Debug.LogWarning($"[타일 맵] {floor}: 경계 그림 없음 _{suffix}"); continue; }
                var tilingRule = new RuleTile.TilingRule
                {
                    m_Id = rule.m_TilingRules.Count,
                    m_Sprites = new[] { tile.sprite },
                    m_Output = RuleTile.TilingRuleOutput.OutputSprite.Single,
                    m_ColliderType = Tile.ColliderType.None,
                    m_RuleTransform = RuleTile.TilingRuleOutput.Transform.Fixed,
                    m_Neighbors = new List<int>(),
                    m_NeighborPositions = new List<Vector3Int>(),
                };
                foreach (var p in notThis) { tilingRule.m_NeighborPositions.Add(p); tilingRule.m_Neighbors.Add(RuleTile.TilingRuleOutput.Neighbor.NotThis); }
                foreach (var p in same) { tilingRule.m_NeighborPositions.Add(p); tilingRule.m_Neighbors.Add(RuleTile.TilingRuleOutput.Neighbor.This); }
                rule.m_TilingRules.Add(tilingRule);
            }
            EditorUtility.SetDirty(rule);
            return rule;
        }

        // 1줄: 바닥 (meadow_grass 먼저) · 2줄 아래: Rule Tile · 그 아래: 경계 세트를 그림 모양대로 (5×3 블록)
        private static void BuildPalette(string palettePath, TileMapConfig config, Dictionary<string, Tile> floors,
            List<(string floor, Dictionary<string, Tile> tiles, RuleTile rule)> sets)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(palettePath) == null)
            {
                EnsureFolder(PaletteFolder);
                GridPaletteUtility.CreateNewPalette(PaletteFolder, PaletteName, GridLayout.CellLayout.Rectangle,
                    GridPalette.CellSizing.Automatic, Vector3.one, GridLayout.CellSwizzle.XYZ);
            }

            var root = PrefabUtility.LoadPrefabContents(palettePath);
            try
            {
                var tilemap = root.GetComponentInChildren<Tilemap>();
                tilemap.ClearAllTiles();

                var floorNames = new List<string>(floors.Keys);
                floorNames.Sort((a, b) => a == BaseFloor ? -1 : b == BaseFloor ? 1 : string.CompareOrdinal(a, b));
                for (int i = 0; i < floorNames.Count; i++)
                    tilemap.SetTile(new Vector3Int(i, 0, 0), floors[floorNames[i]]);

                for (int s = 0; s < sets.Count; s++)
                {
                    tilemap.SetTile(new Vector3Int(s, -1, 0), sets[s].rule);
                    int originX = s * SetColumnStride;
                    foreach (var (suffix, x, y) in TransitionLayout)
                    {
                        TileBase tile = suffix.Length == 0 ? floors[sets[s].floor] : sets[s].tiles.TryGetValue(suffix, out var t) ? t : null;
                        if (tile != null) tilemap.SetTile(new Vector3Int(originX + x, TransitionTopRow + y, 0), tile);
                    }
                }
                PrefabUtility.SaveAsPrefabAsset(root, palettePath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static IEnumerable<string> PngsIn(string folder)
        {
            var paths = new List<string>();
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { folder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetDirectoryName(path).Replace('\\', '/') == folder) paths.Add(path);
            }
            paths.Sort(System.StringComparer.Ordinal);
            return paths;
        }

        private static void EnsureFolder(string folder)
        {
            folder = folder.Replace('\\', '/');
            if (AssetDatabase.IsValidFolder(folder)) return;
            EnsureFolder(Path.GetDirectoryName(folder));
            AssetDatabase.CreateFolder(Path.GetDirectoryName(folder).Replace('\\', '/'), Path.GetFileName(folder));
        }
    }
}
