// 에디터 씬의 맵 다루기: 맵 루트 만들기(Map_<이름> > Grid > Paint), 새 맵(겹치지 않게 배치), 보이기/숨기기, 이름 검사·바꾸기, 옛 구조 씬 옮기기.
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using VoxelMagic.Core;
using VoxelMagic.Gameplay;

namespace VoxelMagic.EditorTools
{
    public static class TileMapLibrary
    {
        public const string MapsRootName = "Maps";
        public const string CompareRootName = "Compare";

        /// <summary>맵 루트 하나를 만든다. fill이 있으면 (0,0)부터 size만큼 채운다. undoName이 있으면 Ctrl+Z로 취소 가능</summary>
        public static TileMapDocument CreateMap(Transform mapsRoot, string mapName, Vector2Int size, TileBase fill,
            Vector3 position, TileMapConfig config, string undoName = null)
        {
            var root = new GameObject(TileMapDocument.RootPrefix + mapName);
            if (undoName != null) Undo.RegisterCreatedObjectUndo(root, undoName);
            root.transform.SetParent(mapsRoot, false);
            root.transform.position = position;

            var grid = new GameObject("Grid", typeof(Grid));
            grid.transform.SetParent(root.transform, false);
            var paintGo = new GameObject("Paint", typeof(Tilemap), typeof(TilemapRenderer), typeof(PaintOnlyTilemap));
            paintGo.transform.SetParent(grid.transform, false);
            var paint = paintGo.GetComponent<Tilemap>();

            var document = root.AddComponent<TileMapDocument>();
            document.Setup(mapName, grid.GetComponent<Grid>(), paint, size);
            root.AddComponent<TileGridScaler>().SetConfig(config);

            if (fill != null)
            {
                var positions = new Vector3Int[size.x * size.y];
                var tiles = new TileBase[positions.Length];
                for (int y = 0; y < size.y; y++)
                for (int x = 0; x < size.x; x++)
                {
                    positions[y * size.x + x] = new Vector3Int(x, y, 0);
                    tiles[y * size.x + x] = fill;
                }
                paint.SetTiles(positions, tiles);
            }
            return document;
        }

        /// <summary>씬의 모든 맵 (꺼진 맵 포함), Hierarchy 순서</summary>
        public static List<TileMapDocument> AllMaps()
        {
            var maps = new List<TileMapDocument>(Object.FindObjectsByType<TileMapDocument>(FindObjectsInactive.Include, FindObjectsSortMode.None));
            maps.Sort((a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));
            return maps;
        }

        /// <summary>새 맵: 기존 맵들 동쪽에 겹치지 않게 놓고, 바탕 타일로 채우고, 원하면 다른 맵을 숨긴다. Ctrl+Z 한 번으로 취소</summary>
        public static TileMapDocument NewMap(string mapName, Vector2Int size, TileBase fill, bool hideOthers, TileMapConfig config)
        {
            string problem = ValidateNewName(mapName);
            if (problem != null)
            {
                EditorUtility.DisplayDialog("새 맵", problem, "확인");
                return null;
            }
            size = new Vector2Int(Mathf.Max(1, size.x), Mathf.Max(1, size.y));

            var existing = AllMaps();
            var position = NextMapPosition(existing, 0, config);

            Undo.SetCurrentGroupName($"새 맵 {mapName}");
            int group = Undo.GetCurrentGroup();
            var maps = FindOrCreateRoot(MapsRootName, "Maps");
            var document = CreateMap(maps, mapName, size, fill, position, config, "새 맵");
            if (hideOthers)
                foreach (var other in existing) SetVisible(other, false);
            Undo.CollapseUndoOperations(group);
            EditorSceneManager.MarkSceneDirty(document.gameObject.scene);
            return document;
        }

        /// <summary>새 맵 루트 위치: 기존 맵들 동쪽 끝 + 간격. 새 맵 칸이 음수 X부터 시작하면(newMinCellX) 그만큼 더 띄운다</summary>
        private static Vector3 NextMapPosition(List<TileMapDocument> existing, int newMinCellX, TileMapConfig config)
        {
            var spans = new TileMapPlacement.MapSpan[existing.Count];
            for (int i = 0; i < existing.Count; i++)
            {
                var rect = existing[i].PaintedCellRect();
                spans[i] = TileMapPlacement.MapSpan.From(existing[i].transform.position.x, rect.xMin, rect.xMax, config.CellSize);
            }
            float z = existing.Count > 0 ? existing[0].transform.position.z : 0f;
            float x = TileMapPlacement.NextOriginX(spans, config.NewMapGapCells, config.CellSize) - newMinCellX * config.CellSize;
            return new Vector3(x, 0f, z);
        }

        /// <summary>구운 맵의 칸 데이터 (없으면 null). 프리팹·칸 데이터·폴더 어느 것을 골라도 같은 맵을 찾는다</summary>
        public static TileMapCells FindCells(Object picked)
        {
            if (picked == null) return null;
            if (picked is TileMapCells cells) return cells;
            string path = AssetDatabase.GetAssetPath(picked);
            if (string.IsNullOrEmpty(path)) return null;
            string folder = AssetDatabase.IsValidFolder(path) ? path : Path.GetDirectoryName(path).Replace('\\', '/');
            string mapName = Path.GetFileName(folder);
            return AssetDatabase.LoadAssetAtPath<TileMapCells>(TileFloorBaker.CellsPath(mapName));
        }

        /// <summary>굽기 결과 폴더 목록: (맵 이름, 칸 데이터 — 없으면 null)</summary>
        public static List<(string mapName, TileMapCells cells)> BakedMaps()
        {
            var list = new List<(string, TileMapCells)>();
            if (!AssetDatabase.IsValidFolder(TileFloorBaker.BakedRoot)) return list;
            foreach (var folder in AssetDatabase.GetSubFolders(TileFloorBaker.BakedRoot))
            {
                string mapName = Path.GetFileName(folder);
                list.Add((mapName, AssetDatabase.LoadAssetAtPath<TileMapCells>(TileFloorBaker.CellsPath(mapName))));
            }
            return list;
        }

        public static TileMapDocument FindMap(string mapName)
        {
            foreach (var map in AllMaps())
                if (map.MapName == mapName) return map;
            return null;
        }

        /// <summary>구운 맵을 칸 데이터대로 다시 칠해 새 맵으로 불러오고 구운 프리팹을 연결한다. 같은 이름의 맵이 씬에 있으면 그 맵을 돌려준다</summary>
        public static TileMapDocument LoadBakedMap(TileMapCells cells, bool hideOthers, TileMapConfig config)
        {
            string mapName = cells.MapName;
            var already = FindMap(mapName);
            if (already != null) return already;

            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            var positions = new Vector3Int[cells.Count];
            var tiles = new TileBase[cells.Count];
            for (int i = 0; i < cells.Count; i++)
            {
                var p = cells.PositionAt(i);
                positions[i] = new Vector3Int(p.x, p.y, 0);
                tiles[i] = cells.TileAt(i);
                minX = Mathf.Min(minX, p.x); minY = Mathf.Min(minY, p.y);
                maxX = Mathf.Max(maxX, p.x + 1); maxY = Mathf.Max(maxY, p.y + 1);
            }
            var size = cells.Count > 0 ? new Vector2Int(maxX - minX, maxY - minY) : config.NewMapSize;

            var existing = AllMaps();
            var position = NextMapPosition(existing, cells.Count > 0 ? minX : 0, config);

            Undo.SetCurrentGroupName($"구운 맵 불러오기 {mapName}");
            int group = Undo.GetCurrentGroup();
            var maps = FindOrCreateRoot(MapsRootName, "Maps");
            var document = CreateMap(maps, mapName, size, null, position, config, "구운 맵 불러오기");
            document.Paint.SetTiles(positions, tiles);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TileFloorBaker.PrefabPath(mapName));
            if (prefab != null)
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, document.transform);
                document.RecordBake(cells.CellSize, instance);
                document.GetComponent<TileGridScaler>().Apply();
                instance.SetActive(false); // 고치러 불러왔으니 칠하기 보기로 시작
            }
            if (hideOthers)
                foreach (var other in existing) SetVisible(other, false);
            Undo.CollapseUndoOperations(group);
            EditorSceneManager.MarkSceneDirty(document.gameObject.scene);
            return document;
        }

        /// <summary>이 맵의 마지막 굽기 칸 데이터 (없으면 null)</summary>
        public static TileMapCells BakedCellsOf(TileMapDocument document) =>
            AssetDatabase.LoadAssetAtPath<TileMapCells>(TileFloorBaker.CellsPath(document.MapName));

        /// <summary>칠한 칸을 마지막으로 구운 칸 데이터로 그 자리에서 바꾼다. Ctrl+Z로 취소 가능. 굽기 결과 파일은 읽기만</summary>
        public static bool RevertToBaked(TileMapDocument document)
        {
            var cells = BakedCellsOf(document);
            if (cells == null || document.Paint == null) return false;

            var positions = new Vector3Int[cells.Count];
            var tiles = new TileBase[cells.Count];
            for (int i = 0; i < cells.Count; i++)
            {
                var p = cells.PositionAt(i);
                positions[i] = new Vector3Int(p.x, p.y, 0);
                tiles[i] = cells.TileAt(i);
            }

            Undo.RegisterCompleteObjectUndo(document.Paint, "구운 상태로 되돌리기");
            document.Paint.ClearAllTiles();
            document.Paint.SetTiles(positions, tiles);
            document.Paint.CompressBounds();
            EditorSceneManager.MarkSceneDirty(document.gameObject.scene);
            return true;
        }

        public static void SetVisible(TileMapDocument document, bool visible)
        {
            if (document.gameObject.activeSelf == visible) return;
            Undo.RecordObject(document.gameObject, visible ? "맵 보이기" : "맵 숨기기");
            document.gameObject.SetActive(visible);
            EditorSceneManager.MarkSceneDirty(document.gameObject.scene);
        }

        /// <summary>제안 이름: NewMap, NewMap2, …</summary>
        public static string SuggestName(string baseName = "NewMap")
        {
            if (ValidateNewName(baseName) == null) return baseName;
            for (int i = 2; i < 1000; i++)
                if (ValidateNewName(baseName + i) == null) return baseName + i;
            return baseName + System.DateTime.Now.Ticks;
        }

        /// <summary>파일 이름으로 쓸 수 있고, 씬의 다른 맵·굽기 결과 폴더와 겹치지 않는지. 문제가 없으면 null</summary>
        public static string ValidateNewName(string mapName, TileMapDocument except = null)
        {
            if (string.IsNullOrWhiteSpace(mapName)) return "맵 이름이 비어 있습니다.";
            if (mapName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) return $"'{mapName}'은 파일 이름으로 쓸 수 없습니다.";
            foreach (var other in Object.FindObjectsByType<TileMapDocument>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (other != except && other.MapName == mapName) return $"씬에 '{mapName}' 맵이 이미 있습니다.";
            if (AssetDatabase.IsValidFolder(TileFloorBaker.OutputFolder(mapName))) return $"'{TileFloorBaker.OutputFolder(mapName)}'에 같은 이름의 굽기 결과가 이미 있습니다.";
            return null;
        }

        public static bool Rename(TileMapDocument document, string mapName)
        {
            if (mapName == document.MapName) return true;
            string problem = ValidateNewName(mapName, document);
            if (problem != null)
            {
                EditorUtility.DisplayDialog("맵 이름", problem, "확인");
                return false;
            }
            Undo.RecordObject(document, "맵 이름");
            Undo.RecordObject(document.gameObject, "맵 이름");
            document.SetMapName(mapName);
            document.gameObject.name = TileMapDocument.RootPrefix + mapName;
            EditorUtility.SetDirty(document);
            return true;
        }

        public static Transform FindOrCreateRoot(string name, string undoName = null)
        {
            var existing = GameObject.Find(name);
            if (existing != null) return existing.transform;
            var created = new GameObject(name);
            if (undoName != null) Undo.RegisterCreatedObjectUndo(created, undoName);
            return created.transform;
        }

        // ── 옛 구조 옮기기 (2026-11-05 씬: FloorGrid에 문서·배율, 구운 바닥은 씬 루트, SampleCenter/SampleEast) ──

        [MenuItem("VoxelMagic/타일 맵/옛 구조 씬을 여러 맵 구조로 옮기기")]
        private static void MigrateFromMenu()
        {
            string message = MigrateLegacyScene();
            EditorUtility.DisplayDialog("타일 맵 구조 옮기기", message, "확인");
        }

        public static string MigrateLegacyScene()
        {
            var floorGrid = GameObject.Find("FloorGrid");
            if (floorGrid == null || floorGrid.GetComponent<TileMapDocument>() == null)
                return "옛 구조(FloorGrid)가 없습니다. 옮길 것이 없습니다.";

            var config = FarmTileSetBuilder.LoadOrCreateConfig();
            var oldDocument = floorGrid.GetComponent<TileMapDocument>();
            string mapName = oldDocument.MapName;
            var baked = oldDocument.BakedFloor;
            float bakedCellSize = oldDocument.BakedCellSize;
            var paint = floorGrid.GetComponentInChildren<Tilemap>(true);
            var origin = floorGrid.transform.position;

            Undo.SetCurrentGroupName("타일 맵 구조 옮기기");
            int group = Undo.GetCurrentGroup();

            var maps = FindOrCreateRoot(MapsRootName, "Maps");
            var root = new GameObject(TileMapDocument.RootPrefix + mapName);
            Undo.RegisterCreatedObjectUndo(root, "맵 루트");
            root.transform.SetParent(maps, false);
            root.transform.position = origin;

            var oldScaler = floorGrid.GetComponent<TileGridScaler>();
            if (oldScaler != null) Undo.DestroyObjectImmediate(oldScaler);
            Undo.DestroyObjectImmediate(oldDocument);
            Undo.SetTransformParent(floorGrid.transform, root.transform, "Grid 옮기기");
            Undo.RecordObject(floorGrid, "Grid 이름");
            floorGrid.name = "Grid";
            if (baked != null) Undo.SetTransformParent(baked.transform, root.transform, "구운 바닥 옮기기");

            var document = Undo.AddComponent<TileMapDocument>(root);
            document.Setup(mapName, floorGrid.GetComponent<Grid>(), paint, new Vector2Int(10, 10));
            document.RecordBake(bakedCellSize, baked);
            Undo.AddComponent<TileGridScaler>(root).SetConfig(config);

            var center = GameObject.Find("SampleCenter");
            var east = GameObject.Find("SampleEast");
            var compare = new GameObject(CompareRootName);
            Undo.RegisterCreatedObjectUndo(compare, "Compare");
            if (center != null) { Undo.SetTransformParent(center.transform, compare.transform, "비교 옮기기"); Undo.RecordObject(center, "이름"); center.name = "Center"; }
            if (east != null) { Undo.SetTransformParent(east.transform, compare.transform, "비교 옮기기"); Undo.RecordObject(east, "이름"); east.name = "East"; }
            var rig = Undo.AddComponent<TileMapCompareRig>(compare);
            rig.Setup(config, center != null ? center.transform : null, east != null ? east.transform : null);
            rig.SetTarget(document);

            Undo.CollapseUndoOperations(group);
            EditorSceneManager.MarkSceneDirty(root.scene);
            Selection.activeGameObject = root;
            return $"옮김: Maps/{root.name} (Grid·Paint{(baked != null ? "·구운 바닥" : "")}), Compare (Center·East). 칠한 칸은 그대로입니다. 씬을 저장하세요.";
        }
    }
}
