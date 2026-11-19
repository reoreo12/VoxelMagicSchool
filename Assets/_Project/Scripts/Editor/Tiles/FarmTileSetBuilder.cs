// 타일 팩 임포트 설정(팩 전체)과 farm 타일·Rule Tile·Tile Palette를 만든다. 이미 있으면 덮어쓰기 전에 묻는다.
// 경계 세트 = transitions/<바탕 풀>_to_<바닥>/, 경계 이름 = 바닥(풀이 아닌 쪽)이 있는 방향 (DESIGN_TILEMAP.md 1-3, 6-1). 바탕 풀마다 Rule Tile 하나.
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
        public const string TileAssetRoot = "Assets/_Project/Art/Map/TileAssets";
        private const string TileAssetFolder = TileAssetRoot + "/farm";
        // 팔레트 바닥 줄 끝에 낱장으로 붙이는 다른 팩 (Tile/<팩>/*.png)
        public static readonly string[] ExtraFloorPacks = { "cave_floor_tiles_v3" };
        private const string PaletteFolder = "Assets/_Project/Art/Map/TilePalettes";
        private const string PaletteName = "Farm";
        public const string PalettePath = PaletteFolder + "/" + PaletteName + ".prefab";
        private const string BaseFloor = "meadow_grass";
        private const string TransitionMarker = "_to_";
        private const int SetColumnStride = 6;   // 팔레트에서 경계 세트 사이 간격 (5칸 + 빈칸 1)
        private const int TransitionTopRow = -3;
        private const int ExtraTopRow = -7;      // 다른 팩 바닥 3×3 블록 윗줄 (경계 세트 3줄 아래 + 빈 줄 1)
        private const int ExtraBlockStride = 4;  // 3×3 블록 사이 간격 (3칸 + 빈칸 1)

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

            // 다른 팩의 낱장 바닥 (floors 폴더 없이 팩 폴더에 바로 있는 PNG). farm 바닥 뒤에 붙여 기존 팔레트 배치를 안 바꾼다
            var extraFloors = new List<string>();
            foreach (var pack in ExtraFloorPacks)
                foreach (var path in PngsIn($"{TilePackFolder}/{pack}"))
                {
                    string name = Path.GetFileNameWithoutExtension(path);
                    floors[name] = SaveTile(path, $"{TileAssetRoot}/{pack}/floors");
                    extraFloors.Add(name);
                }

            // 경계 세트 = <바탕 풀>_to_<바닥>. 기본 바탕(meadow_grass) 세트를 먼저, 나머지는 이름순 (팔레트 배치가 바뀌지 않게)
            var setFolders = new List<string>(AssetDatabase.GetSubFolders($"{FarmFolder}/transitions"));
            setFolders.Sort((a, b) =>
            {
                bool aBase = Path.GetFileName(a).StartsWith(BaseFloor + TransitionMarker), bBase = Path.GetFileName(b).StartsWith(BaseFloor + TransitionMarker);
                return aBase != bBase ? (aBase ? -1 : 1) : string.CompareOrdinal(a, b);
            });
            var sets = new List<(string floor, Dictionary<string, Tile> tiles, RuleTile rule)>();
            foreach (var setFolder in setFolders)
            {
                string setName = Path.GetFileName(setFolder);
                int marker = setName.IndexOf(TransitionMarker, System.StringComparison.Ordinal);
                if (marker <= 0) { Debug.LogWarning($"[타일 맵] 이름 규칙(<바탕>_to_<바닥>) 밖 세트 건너뜀: {setName}"); continue; }
                string baseFloor = setName.Substring(0, marker);
                string floor = setName.Substring(marker + TransitionMarker.Length);
                if (!floors.ContainsKey(floor)) { Debug.LogWarning($"[타일 맵] {setName}: 가운데 바닥 floors/{floor} 없음"); continue; }
                if (!floors.ContainsKey(baseFloor)) Debug.LogWarning($"[타일 맵] {setName}: 바탕 풀 floors/{baseFloor} 없음 (경계는 만들지만 바탕으로 칠할 타일이 없음)");

                var tiles = new Dictionary<string, Tile>();
                foreach (var path in PngsIn(setFolder))
                    tiles[Path.GetFileNameWithoutExtension(path).Substring(setName.Length + 1)] =
                        SaveTile(path, $"{TileAssetFolder}/transitions/{setName}");
                sets.Add((floor, tiles, SaveRuleTile(RuleTileName(baseFloor, floor), floors[floor].sprite, tiles)));
            }

            BuildPalette(palettePath, config, floors, sets, extraFloors);
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

        /// <summary>Rule Tile 이름: 기본 바탕(meadow_grass)이면 RT_<바닥> (기존 이름 유지), 다른 바탕이면 RT_<바탕>_to_<바닥></summary>
        public static string RuleTileName(string baseFloor, string floor) =>
            baseFloor == BaseFloor ? $"RT_{floor}" : $"RT_{baseFloor}{TransitionMarker}{floor}";

        private static RuleTile SaveRuleTile(string ruleName, Sprite center, Dictionary<string, Tile> edges)
        {
            string folder = $"{TileAssetFolder}/rules";
            EnsureFolder(folder);
            string path = $"{folder}/{ruleName}.asset";
            string floor = ruleName;
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

        // 다른 팩 바닥: 경계 세트 아래에 <바닥>_moss_<방향>_NN을 변형 번호(NN)마다 3×3 블록으로 (가운데 = <바닥>_NN),
        // 블록 가운데에 못 들어간 낱장(<바닥>_NN 나머지 등)은 블록 아래 한 줄로
        private static void PlaceExtraFloors(Tilemap tilemap, Dictionary<string, Tile> floors, List<string> extraFloors)
        {
            var edgeRegex = new System.Text.RegularExpressions.Regex(@"^(.+)_moss_(top_left|top_right|bottom_left|bottom_right|top|bottom|left|right)_(\d+)$");
            var placed = new HashSet<string>();
            var blockBases = new Dictionary<string, int>(); // <바닥> → 블록 가로 시작 (변형 1번 기준)
            int nextBaseX = 0, blockCount = 0;
            foreach (var name in extraFloors)
            {
                var match = edgeRegex.Match(name);
                if (!match.Success) continue;
                string floor = match.Groups[1].Value;
                int variant = int.Parse(match.Groups[3].Value);
                if (!blockBases.TryGetValue(floor, out int baseX))
                {
                    baseX = nextBaseX;
                    blockBases[floor] = baseX;
                }
                var (dx, dy) = EdgeCell(match.Groups[2].Value);
                int x = baseX + (variant - 1) * ExtraBlockStride + dx;
                tilemap.SetTile(new Vector3Int(x, ExtraTopRow + dy, 0), floors[name]);
                placed.Add(name);
                blockCount = Mathf.Max(blockCount, variant);
                nextBaseX = Mathf.Max(nextBaseX, baseX + variant * ExtraBlockStride);
            }

            // 블록 가운데: <바닥>_NN
            foreach (var pair in blockBases)
                for (int v = 1; v <= blockCount; v++)
                {
                    string center = $"{pair.Key}_{v:00}";
                    if (!floors.ContainsKey(center) || !extraFloors.Contains(center)) continue;
                    tilemap.SetTile(new Vector3Int(pair.Value + (v - 1) * ExtraBlockStride + 1, ExtraTopRow - 1, 0), floors[center]);
                    placed.Add(center);
                }

            // 나머지 낱장: 블록 아래 한 줄
            int rest = 0;
            foreach (var name in extraFloors)
                if (!placed.Contains(name))
                    tilemap.SetTile(new Vector3Int(rest++, ExtraTopRow - 4, 0), floors[name]);
        }

        private static (int dx, int dy) EdgeCell(string direction)
        {
            switch (direction)
            {
                case "top_left": return (0, 0);
                case "top": return (1, 0);
                case "top_right": return (2, 0);
                case "left": return (0, -1);
                case "right": return (2, -1);
                case "bottom_left": return (0, -2);
                case "bottom": return (1, -2);
                default: return (2, -2); // bottom_right
            }
        }

        // 1줄: 바닥 (meadow_grass 먼저) · 2줄 아래: Rule Tile · 그 아래: 경계 세트를 그림 모양대로 (5×3 블록) · 그 아래: 다른 팩 바닥 3×3 블록
        private static void BuildPalette(string palettePath, TileMapConfig config, Dictionary<string, Tile> floors,
            List<(string floor, Dictionary<string, Tile> tiles, RuleTile rule)> sets, List<string> extraFloors)
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
                floorNames.RemoveAll(extraFloors.Contains);
                floorNames.Sort((a, b) => a == BaseFloor ? -1 : b == BaseFloor ? 1 : string.CompareOrdinal(a, b));
                for (int i = 0; i < floorNames.Count; i++)
                    tilemap.SetTile(new Vector3Int(i, 0, 0), floors[floorNames[i]]);
                PlaceExtraFloors(tilemap, floors, extraFloors);

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
