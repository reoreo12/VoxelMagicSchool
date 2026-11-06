// 바닥 종류(풀·흙·진흙·짚·돌·나무판)와 farm 타일 → 바닥 종류 표를 만든다. 이미 있으면 빠진 것만 더한다 (사용자가 고친 줄은 그대로).
// Rule Tile은 칠한 재료(RT_<바닥>), 경계 낱장은 세트 이름의 to_ 뒤 재료로 정한다.
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using VoxelMagic.Core;

namespace VoxelMagic.EditorTools
{
    public static class GroundSurfaceBuilder
    {
        private const string SurfaceFolder = "Assets/_Project/Data/Tiles/Surfaces";
        private const string TablePath = "Assets/_Project/Data/Tiles/GroundSurfaceTable.asset";
        private const string TileFolder = "Assets/_Project/Art/Map/TileAssets/farm";
        private const string TransitionMarker = "_to_";

        // 기본 바닥 종류: 에셋 이름, 표시 이름, 이 종류인 farm 바닥 타일
        private static readonly (string asset, string display, string[] floors)[] Defaults =
        {
            ("Grass", "풀", new[] { "meadow_grass", "farm_grass", "flower_meadow" }),
            ("Soil", "흙", new[] { "dark_garden_soil", "tilled_soil_rows", "dry_dirt_path" }),
            ("Mud", "진흙", new[] { "muddy_farm_path" }),
            ("Straw", "짚", new[] { "straw_covered_ground", "hay_floor" }),
            ("Stone", "돌", new[] { "barnyard_stones", "stepping_stone_path" }),
            ("Wood", "나무판", new[] { "barn_floor_planks" }),
        };

        [MenuItem("VoxelMagic/타일 맵/바닥 종류 표 만들기·채우기 (farm)")]
        private static void BuildFromMenu()
        {
            var (table, added) = Build(FarmTileSetBuilder.LoadOrCreateConfig());
            EditorUtility.DisplayDialog("바닥 종류 표", $"{TablePath}\n새로 더한 줄 {added}개 (전체 {table.Count}줄). 이미 있던 줄은 그대로입니다.", "확인");
            Selection.activeObject = table;
        }

        public static (GroundSurfaceTable table, int added) Build(TileMapConfig config)
        {
            EnsureFolder(SurfaceFolder);
            var byFloor = new Dictionary<string, GroundSurface>();
            foreach (var (asset, display, floors) in Defaults)
            {
                var surface = LoadOrCreateSurface(asset, display);
                foreach (var floor in floors) byFloor[floor] = surface;
            }

            var table = AssetDatabase.LoadAssetAtPath<GroundSurfaceTable>(TablePath);
            if (table == null)
            {
                table = ScriptableObject.CreateInstance<GroundSurfaceTable>();
                AssetDatabase.CreateAsset(table, TablePath);
            }

            int added = 0, unknown = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:TileBase", new[] { TileFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var tile = AssetDatabase.LoadAssetAtPath<TileBase>(path);
                string floor = FloorNameOf(path);
                if (floor == null || !byFloor.TryGetValue(floor, out var surface)) { unknown++; continue; }
                if (table.AddIfMissing(tile, surface)) added++;
            }
            EditorUtility.SetDirty(table);

            if (config.SurfaceTable != table)
            {
                Undo.RecordObject(config, "바닥 종류 표 연결");
                config.SetSurfaceTable(table);
                EditorUtility.SetDirty(config);
            }
            AssetDatabase.SaveAssets();
            if (unknown > 0) Debug.LogWarning($"[타일 맵] 바닥 종류를 정하지 못한 타일 {unknown}개 — {TablePath}에서 직접 넣으세요");
            Debug.Log($"[타일 맵] 바닥 종류 표: 새 줄 {added}, 전체 {table.Count}");
            return (table, added);
        }

        /// <summary>타일 에셋 경로 → 그 타일이 나타내는 바닥 이름 (floors/x → x, rules/RT_x → x, transitions/a_to_b/… → b)</summary>
        private static string FloorNameOf(string path)
        {
            string name = Path.GetFileNameWithoutExtension(path);
            string parent = Path.GetFileName(Path.GetDirectoryName(path));
            if (parent == "floors") return name;
            if (parent == "rules") return name.StartsWith("RT_") ? name.Substring(3) : null;
            int marker = parent.IndexOf(TransitionMarker, System.StringComparison.Ordinal);
            return marker >= 0 ? parent.Substring(marker + TransitionMarker.Length) : null;
        }

        private static GroundSurface LoadOrCreateSurface(string asset, string display)
        {
            string path = $"{SurfaceFolder}/Surface_{asset}.asset";
            var surface = AssetDatabase.LoadAssetAtPath<GroundSurface>(path);
            if (surface != null) return surface;
            surface = ScriptableObject.CreateInstance<GroundSurface>();
            surface.SetDisplayName(display);
            AssetDatabase.CreateAsset(surface, path);
            return surface;
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            int slash = folder.LastIndexOf('/');
            EnsureFolder(folder.Substring(0, slash));
            AssetDatabase.CreateFolder(folder.Substring(0, slash), folder.Substring(slash + 1));
        }
    }
}
