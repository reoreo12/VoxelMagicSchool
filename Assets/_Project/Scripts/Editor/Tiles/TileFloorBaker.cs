// 칠한 Tilemap을 굽는다: 사용한 타일만 모은 아틀라스(여백 + 가장자리 늘이기) + 조각 평면 메시(위 방향, 칸 단위) + URP Lit 머티리얼 + 바닥 프리팹.
// 결과: Art/Map/Baked/<맵 이름>/ (+ 칸 데이터 <맵>_Cells.asset, 바닥 종류 <맵>_Surfaces.asset, 선택하면 칠한 칸 모양대로 BoxCollider 여러 개). 이전 결과가 있으면 덮어쓰기 전에 묻는다. 프리팹 루트 배율 = 구울 때의 칸 크기 (게임 씬에 그대로 놓음).
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;
using VoxelMagic.Core;
using VoxelMagic.Gameplay;

namespace VoxelMagic.EditorTools
{
    public static class TileFloorBaker
    {
        public const string BakedRoot = "Assets/_Project/Art/Map/Baked";
        private const string LitShader = "Universal Render Pipeline/Lit";
        private const int MaxAtlasSize = 8192;

        public static string OutputFolder(string mapName) => $"{BakedRoot}/{mapName}";
        public static string PrefabPath(string mapName) => $"{OutputFolder(mapName)}/{mapName}_Floor.prefab";
        public static string CellsPath(string mapName) => $"{OutputFolder(mapName)}/{mapName}_Cells.asset";
        public static string SurfacesPath(string mapName) => $"{OutputFolder(mapName)}/{mapName}_Surfaces.asset";

        public sealed class Result
        {
            public int Cells, UniqueTiles, Chunks, RotatedCells, SurfaceKinds, UnmappedCells;
            public bool HasCollider;
            public int ColliderCount;
            public Vector2Int AtlasSize;
            public string PrefabPath;
        }

        /// <returns>구웠으면 결과, 취소·실패면 null</returns>
        public static Result Bake(TileMapDocument document, TileMapConfig config, bool askBeforeOverwrite)
        {
            var paint = document.Paint;
            var scaler = document.GetComponent<TileGridScaler>();
            if (paint == null || scaler == null)
            {
                Debug.LogError($"[타일 맵] {document.name}: Paint 또는 TileGridScaler가 없음");
                return null;
            }
            string mapName = document.MapName;
            if (string.IsNullOrWhiteSpace(mapName) || mapName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                Debug.LogError($"[타일 맵] 맵 이름이 파일 이름으로 쓸 수 없음: '{mapName}'");
                return null;
            }

            string folder = OutputFolder(mapName);
            if (askBeforeOverwrite && AssetDatabase.IsValidFolder(folder) &&
                AssetDatabase.FindAssets("", new[] { folder }).Length > 0 &&
                !EditorUtility.DisplayDialog("굽기 결과 덮어쓰기",
                    $"{folder}에 이전 굽기 결과가 있습니다.\n아틀라스·머티리얼·메시·프리팹·칸 데이터·바닥 종류를 같은 이름으로 덮어쓰고, 이번에 없는 조각 메시는 지웁니다. 계속할까요?",
                    "덮어쓰기", "취소"))
                return null;

            // 1. 칠한 칸 읽기 (Rule Tile이 고른 그림 그대로)
            paint.CompressBounds();
            var cells = new List<(Vector3Int pos, Sprite sprite)>();
            var paintedPositions = new List<Vector2Int>();
            var paintedTiles = new List<TileBase>();
            var result = new Result();
            foreach (var pos in paint.cellBounds.allPositionsWithin)
            {
                var sprite = paint.GetSprite(pos);
                if (sprite == null) continue;
                if (paint.GetTransformMatrix(pos) != Matrix4x4.identity) result.RotatedCells++;
                cells.Add((pos, sprite));
                paintedPositions.Add(new Vector2Int(pos.x, pos.y));
                paintedTiles.Add(paint.GetTile(pos)); // 칠한 타일 그대로 (Rule Tile이면 Rule Tile) → 다시 불러오기용
            }
            if (cells.Count == 0) { Debug.LogWarning("[타일 맵] 칠한 칸이 없어 굽지 않음"); return null; }

            // 2. 아틀라스
            var sprites = cells.Select(c => c.sprite).Distinct().ToList();
            var index = new Dictionary<Sprite, int>();
            for (int i = 0; i < sprites.Count; i++) index[sprites[i]] = i;
            var layout = new TileAtlasLayout(sprites.Count, config.TilePixels, config.AtlasPadding);
            if (layout.Width > MaxAtlasSize || layout.Height > MaxAtlasSize)
            {
                Debug.LogError($"[타일 맵] 아틀라스가 너무 큼 {layout.Width}×{layout.Height}");
                return null;
            }

            EnsureFolder(folder);
            string atlasPath = $"{folder}/{mapName}_Atlas.png";
            if (!WriteAtlas(atlasPath, sprites, layout)) return null;
            var atlas = ImportAtlas(atlasPath, layout);

            // 3. 머티리얼
            var material = SaveMaterial($"{folder}/{mapName}_Floor.mat", atlas);

            // 3-1. 칸 데이터 (다시 불러와 고칠 때)
            SaveCells(CellsPath(mapName), mapName, config.CellSize, paintedPositions, paintedTiles);

            // 3-2. 바닥 종류 맵 (플레이 중 발밑 종류 찾기)
            var surfaceMap = SaveSurfaces(SurfacesPath(mapName), config.SurfaceTable, paintedPositions, paintedTiles, result);

            // 4. 조각 메시 (칸 단위, 조각 원점 기준)
            string meshFolder = $"{folder}/Meshes";
            EnsureFolder(meshFolder);
            int chunk = config.ChunkSize;
            var chunks = cells.GroupBy(c => new Vector2Int(FloorDiv(c.pos.x, chunk), FloorDiv(c.pos.y, chunk)))
                .OrderBy(g => g.Key.y).ThenBy(g => g.Key.x).ToList();
            var keptMeshes = new HashSet<string>();
            var chunkMeshes = new List<(Vector2Int key, Mesh mesh)>();
            foreach (var group in chunks)
            {
                string meshPath = $"{meshFolder}/{mapName}_Chunk_{group.Key.x}_{group.Key.y}.asset";
                keptMeshes.Add(meshPath);
                var origin = new Vector2Int(group.Key.x * chunk, group.Key.y * chunk);
                chunkMeshes.Add((group.Key, SaveChunkMesh(meshPath, group.ToList(), origin, layout, index)));
            }
            foreach (var guid in AssetDatabase.FindAssets("t:Mesh", new[] { meshFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!keptMeshes.Contains(path)) AssetDatabase.DeleteAsset(path);
            }

            // 5. 프리팹
            string prefabPath = PrefabPath(mapName);
            var root = new GameObject($"{mapName}_Floor");
            try
            {
                root.transform.localScale = Vector3.one * config.CellSize;
                root.AddComponent<GroundSurfaceSource>().SetMap(surfaceMap);
                if (document.BakeCollider) AddFloorCollider(root, paintedPositions, config, result);
                foreach (var (key, mesh) in chunkMeshes)
                {
                    var go = new GameObject($"Chunk_{key.x}_{key.y}", typeof(MeshFilter), typeof(MeshRenderer));
                    go.transform.SetParent(root.transform, false);
                    go.transform.localPosition = new Vector3(key.x * chunk, 0f, key.y * chunk);
                    go.GetComponent<MeshFilter>().sharedMesh = mesh;
                    var renderer = go.GetComponent<MeshRenderer>();
                    renderer.sharedMaterial = material;
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
                    renderer.receiveShadows = true;
                }
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
            AssetDatabase.SaveAssets();

            // 6. 에디터 씬의 맵 루트 아래에 놓기 (이미 놓여 있으면 프리팹이 갱신되어 그대로 따라옴)
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            var instance = document.BakedFloor;
            if (instance == null || PrefabUtility.GetCorrespondingObjectFromSource(instance) != prefab)
            {
                if (instance != null) Undo.DestroyObjectImmediate(instance);
                instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, document.transform);
                Undo.RegisterCreatedObjectUndo(instance, "구운 바닥 놓기");
            }
            else if (instance.transform.parent != document.transform)
            {
                Undo.SetTransformParent(instance.transform, document.transform, "구운 바닥 옮기기");
            }
            Undo.RecordObject(document, "굽기 기록");
            document.RecordBake(config.CellSize, instance);
            Undo.RecordObject(instance.transform, "구운 바닥 배율");
            scaler.Apply();
            EditorSceneManager.MarkSceneDirty(document.gameObject.scene);

            result.Cells = cells.Count;
            result.UniqueTiles = sprites.Count;
            result.Chunks = chunkMeshes.Count;
            result.AtlasSize = new Vector2Int(layout.Width, layout.Height);
            result.PrefabPath = prefabPath;
            Debug.Log($"[타일 맵] 굽기: 칸 {result.Cells}, 타일 {result.UniqueTiles}종, 조각 {result.Chunks}, 아틀라스 {layout.Width}×{layout.Height} (여백 {config.AtlasPadding}px), 칸 크기 {config.CellSize} m → {prefabPath}");
            if (result.RotatedCells > 0) Debug.LogWarning($"[타일 맵] 회전·뒤집힌 칸 {result.RotatedCells}개는 회전 없이 구웠음");
            return result;
        }

        /// <summary>보기 전환: 칠하기 층(Tilemap) 또는 구운 바닥 중 하나만 보이게 (겹쳐 깜빡임 방지)</summary>
        public static void SetView(TileMapDocument document, bool showBaked)
        {
            var paint = document.Paint;
            var renderer = paint.GetComponent<TilemapRenderer>();
            Undo.RecordObject(renderer, "보기 전환");
            renderer.enabled = !showBaked || document.BakedFloor == null;
            if (document.BakedFloor != null)
            {
                Undo.RecordObject(document.BakedFloor, "보기 전환");
                document.BakedFloor.SetActive(showBaked);
            }
            EditorSceneManager.MarkSceneDirty(paint.gameObject.scene);
        }

        private static bool WriteAtlas(string path, List<Sprite> sprites, TileAtlasLayout layout)
        {
            var pixels = new Color32[layout.Width * layout.Height];
            for (int i = 0; i < sprites.Count; i++)
            {
                var sprite = sprites[i];
                string src = AssetDatabase.GetAssetPath(sprite.texture);
                var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                try
                {
                    // 원본 PNG를 직접 읽는다 (임포트 설정의 Read/Write를 바꾸지 않음)
                    if (!tex.LoadImage(File.ReadAllBytes(src)))
                    {
                        Debug.LogError($"[타일 맵] 그림을 읽지 못함: {src}");
                        return false;
                    }
                    var r = sprite.rect;
                    if ((int)r.width != layout.TilePixels || (int)r.height != layout.TilePixels || tex.width != (int)sprite.texture.width)
                    {
                        Debug.LogError($"[타일 맵] 타일 크기가 설정({layout.TilePixels}px)과 다름: {src} {r.width}×{r.height}");
                        return false;
                    }
                    var source = tex.GetPixels32();
                    var dest = layout.TileRect(i);
                    int pad = layout.Padding, size = layout.TilePixels;
                    for (int y = -pad; y < size + pad; y++)
                    for (int x = -pad; x < size + pad; x++)
                    {
                        int sx = (int)r.x + Mathf.Clamp(x, 0, size - 1);
                        int sy = (int)r.y + Mathf.Clamp(y, 0, size - 1);
                        pixels[(dest.y + y) * layout.Width + dest.x + x] = source[sy * tex.width + sx];
                    }
                }
                finally
                {
                    Object.DestroyImmediate(tex);
                }
            }

            var atlas = new Texture2D(layout.Width, layout.Height, TextureFormat.RGBA32, false);
            atlas.SetPixels32(pixels);
            File.WriteAllBytes(path, atlas.EncodeToPNG());
            Object.DestroyImmediate(atlas);
            return true;
        }

        private static Texture2D ImportAtlas(string path, TileAtlasLayout layout)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Default;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.sRGBTexture = true;
            importer.alphaSource = TextureImporterAlphaSource.None;
            importer.isReadable = false;
            importer.maxTextureSize = Mathf.Max(32, Mathf.NextPowerOfTwo(Mathf.Max(layout.Width, layout.Height)));
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        /// <summary>칠한 칸 모양대로 얇은 BoxCollider 여러 개 (빈 칸은 덮지 않음). 칸을 가로로 길게 묶은 뒤 같은 폭의 아래 줄과 합쳐 직사각형을 만든다.
        /// 루트 배율 = 칸 크기라 로컬 단위 = 칸, 두께만 m → 칸으로 바꾼다. 윗면은 y = 0</summary>
        private static void AddFloorCollider(GameObject root, List<Vector2Int> positions, TileMapConfig config, Result result)
        {
            var remaining = new HashSet<Vector2Int>(positions);
            float thickness = config.ColliderThickness / config.CellSize;
            foreach (var start in positions.OrderBy(p => p.y).ThenBy(p => p.x))
            {
                if (!remaining.Contains(start)) continue;
                int width = 1;
                while (remaining.Contains(new Vector2Int(start.x + width, start.y))) width++;
                int depth = 1;
                while (RowFree(remaining, start.x, start.y + depth, width)) depth++;
                for (int y = 0; y < depth; y++)
                    for (int x = 0; x < width; x++) remaining.Remove(new Vector2Int(start.x + x, start.y + y));

                var box = root.AddComponent<BoxCollider>();
                box.size = new Vector3(width, thickness, depth);
                box.center = new Vector3(start.x + width * 0.5f, -thickness * 0.5f, start.y + depth * 0.5f);
                result.ColliderCount++;
            }
            result.HasCollider = result.ColliderCount > 0;
        }

        private static bool RowFree(HashSet<Vector2Int> cells, int x0, int y, int width)
        {
            for (int x = 0; x < width; x++) if (!cells.Contains(new Vector2Int(x0 + x, y))) return false;
            return true;
        }

        private static GroundSurfaceMap SaveSurfaces(string path, GroundSurfaceTable table, List<Vector2Int> positions, List<TileBase> tiles, Result result)
        {
            if (table == null) Debug.LogWarning("[타일 맵] 바닥 종류 표가 없음 (TileMapConfig) — 모든 칸이 '없음'으로 구워짐. 메뉴 VoxelMagic/타일 맵/바닥 종류 표 만들기·채우기");

            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            foreach (var p in positions)
            {
                minX = Mathf.Min(minX, p.x); minY = Mathf.Min(minY, p.y);
                maxX = Mathf.Max(maxX, p.x); maxY = Mathf.Max(maxY, p.y);
            }
            int width = maxX - minX + 1, height = maxY - minY + 1;
            var codes = new byte[width * height];
            var surfaces = new List<GroundSurface>();
            var unmapped = new HashSet<string>();
            for (int i = 0; i < positions.Count; i++)
            {
                var surface = table != null ? table.Find(tiles[i]) : null;
                if (surface == null) { result.UnmappedCells++; if (tiles[i] != null) unmapped.Add(tiles[i].name); continue; }
                int code = surfaces.IndexOf(surface);
                if (code < 0)
                {
                    if (surfaces.Count >= GroundSurfaceMap.MaxSurfaces) { result.UnmappedCells++; continue; }
                    surfaces.Add(surface);
                    code = surfaces.Count - 1;
                }
                codes[(positions[i].y - minY) * width + (positions[i].x - minX)] = (byte)(code + 1);
            }
            result.SurfaceKinds = surfaces.Count;
            if (unmapped.Count > 0) Debug.LogWarning($"[타일 맵] 바닥 종류 표에 없는 타일 (칸 {result.UnmappedCells}개는 '없음'): {string.Join(", ", unmapped)}");

            var map = AssetDatabase.LoadAssetAtPath<GroundSurfaceMap>(path);
            if (map == null)
            {
                map = ScriptableObject.CreateInstance<GroundSurfaceMap>();
                AssetDatabase.CreateAsset(map, path);
            }
            map.Store(new Vector2Int(minX, minY), width, height, codes, surfaces);
            EditorUtility.SetDirty(map);
            return map;
        }

        private static TileMapCells SaveCells(string path, string mapName, float cellSize, List<Vector2Int> positions, List<TileBase> tiles)
        {
            var cells = AssetDatabase.LoadAssetAtPath<TileMapCells>(path);
            if (cells == null)
            {
                cells = ScriptableObject.CreateInstance<TileMapCells>();
                AssetDatabase.CreateAsset(cells, path);
            }
            cells.Store(mapName, cellSize, positions, tiles);
            EditorUtility.SetDirty(cells);
            return cells;
        }

        private static Material SaveMaterial(string path, Texture2D atlas)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find(LitShader));
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetTexture("_BaseMap", atlas);
            material.mainTexture = atlas;
            material.SetColor("_BaseColor", Color.white);
            material.SetFloat("_Smoothness", 0f);
            material.SetFloat("_Metallic", 0f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Mesh SaveChunkMesh(string path, List<(Vector3Int pos, Sprite sprite)> cells, Vector2Int origin,
            TileAtlasLayout layout, Dictionary<Sprite, int> index)
        {
            int n = cells.Count;
            var vertices = new Vector3[n * 4];
            var normals = new Vector3[n * 4];
            var uvs = new Vector2[n * 4];
            var triangles = new int[n * 6];
            for (int i = 0; i < n; i++)
            {
                float x = cells[i].pos.x - origin.x, z = cells[i].pos.y - origin.y;
                var uv = layout.TileUv(index[cells[i].sprite]);
                int v = i * 4;
                // 0 남서, 1 남동, 2 북서, 3 북동 (그림 위쪽 = 북 = +Z)
                vertices[v] = new Vector3(x, 0f, z);
                vertices[v + 1] = new Vector3(x + 1f, 0f, z);
                vertices[v + 2] = new Vector3(x, 0f, z + 1f);
                vertices[v + 3] = new Vector3(x + 1f, 0f, z + 1f);
                uvs[v] = new Vector2(uv.xMin, uv.yMin);
                uvs[v + 1] = new Vector2(uv.xMax, uv.yMin);
                uvs[v + 2] = new Vector2(uv.xMin, uv.yMax);
                uvs[v + 3] = new Vector2(uv.xMax, uv.yMax);
                for (int k = 0; k < 4; k++) normals[v + k] = Vector3.up;
                int t = i * 6;
                triangles[t] = v; triangles[t + 1] = v + 2; triangles[t + 2] = v + 3;
                triangles[t + 3] = v; triangles[t + 4] = v + 3; triangles[t + 5] = v + 1;
            }

            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            bool isNew = mesh == null;
            if (isNew) mesh = new Mesh();
            mesh.Clear();
            mesh.name = Path.GetFileNameWithoutExtension(path);
            mesh.indexFormat = n * 4 > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            if (isNew) AssetDatabase.CreateAsset(mesh, path);
            else EditorUtility.SetDirty(mesh);
            return mesh;
        }

        private static int FloorDiv(int a, int b) => a >= 0 ? a / b : -((-a + b - 1) / b);

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            int slash = folder.LastIndexOf('/');
            EnsureFolder(folder.Substring(0, slash));
            AssetDatabase.CreateFolder(folder.Substring(0, slash), folder.Substring(slash + 1));
        }
    }
}
