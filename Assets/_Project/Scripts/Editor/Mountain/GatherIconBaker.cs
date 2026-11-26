// 채집 재료 FBX를 촬영해 지팡이 아이콘과 같은 16×16 도트 아이콘(1px 진갈색 외곽선)으로 굽고, 재료 데이터의 아이콘 칸에 연결한다.
// 메시를 코드로 직접 그린 뒤(GPU 미사용), 칸마다 덮인 비율로 칠할지 정하고 색 수를 줄인다. 모양이 바뀌면 메뉴로 다시 굽는다.
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.EditorTools
{
    public static class GatherIconBaker
    {
        private const string SettingsPath = "Assets/_Project/Data/Tools/GatherIconSettings.asset";
        private const string MaterialFolder = "Assets/_Project/Data/Mountain/Gather";

        [MenuItem("VoxelMagic/뒷산/채집 재료 아이콘 굽기")]
        public static void BakeAll()
        {
            var settings = LoadSettings();
            EnsureFolder(settings.OutputFolder + "/_preview");
            // 1) 전부 그린다 (GPU·미리보기 씬을 쓰지 않음, 아래 '촬영' 참고).
            var baked = new List<(GatherMaterialDefinition material, string path, Color32[] pixels)>();
            foreach (var guid in AssetDatabase.FindAssets("t:GatherMaterialDefinition", new[] { MaterialFolder }))
            {
                var material = AssetDatabase.LoadAssetAtPath<GatherMaterialDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (material == null || material.WorldMaterial == null || material.WorldMesh == null) continue; // FBX 없는 임시 재료
                baked.Add((material, $"{settings.OutputFolder}/item_{material.name}.png", Bake(material, settings)));
            }

            // 2) 파일로 쓰고 3) 한꺼번에 임포트·연결한다.
            foreach (var b in baked) Write(b.pixels, settings.IconSize, b.path);
            string preview = WritePreview(baked.ConvertAll(b => b.pixels), settings);
            foreach (var b in baked) ImportIcon(b.path);
            if (preview != null) AssetDatabase.ImportAsset(preview);
            foreach (var b in baked) Assign(b.material, b.path);
            AssetDatabase.SaveAssets();
            Debug.Log($"[아이콘] 채집 재료 {baked.Count}종 아이콘을 {settings.OutputFolder}에 구움");
        }

        // ---------- 촬영 (GPU 없이 삼각형을 직접 그림) ----------
        // 미리보기 렌더러(PreviewRenderUtility)는 이 프로젝트의 URP에서 에디터를 멈추게 해서, 메시를 코드로 직접 그린다.
        // 복셀 모델이라 면마다 팔레트 색 + 해 방향 밝기만으로 충분하다.
        private struct Part
        {
            public Mesh Mesh;
            public Matrix4x4 Matrix;
            public Texture2D Texture; // 읽을 수 있는 팔레트 사본
            public float Emission;
        }

        private static Color32[] Bake(GatherMaterialDefinition material, GatherIconSettings s)
        {
            int res = s.IconSize * s.Supersample;
            var parts = CollectParts(material);
            try
            {
                var alpha = new float[res * res];
                var color = new Color[res * res];
                Rasterize(parts, s, res, alpha, color);
                return Pixelate(alpha, color, res, s);
            }
            finally
            {
                foreach (var p in parts) if (p.Texture != null) Object.DestroyImmediate(p.Texture);
            }
        }

        // 산삼처럼 획득 모양이 따로 있으면 그 FBX를, 없으면 땅에 놓인 메시·머티리얼·회전을 쓴다.
        private static List<Part> CollectParts(GatherMaterialDefinition material)
        {
            var parts = new List<Part>();
            if (material.AcquireModel != null)
            {
                var root = material.AcquireModel.transform;
                foreach (var filter in material.AcquireModel.GetComponentsInChildren<MeshFilter>(true))
                {
                    var renderer = filter.GetComponent<MeshRenderer>();
                    var mat = material.WorldMaterial != null ? material.WorldMaterial : renderer.sharedMaterial;
                    // 루트 위치는 빼고 회전·크기만 (원점 기준으로 그린다)
                    var matrix = Matrix4x4.TRS(Vector3.zero, root.rotation, root.lossyScale) * root.worldToLocalMatrix * filter.transform.localToWorldMatrix;
                    parts.Add(MakePart(filter.sharedMesh, matrix, mat));
                }
            }
            else
            {
                parts.Add(MakePart(material.WorldMesh, Matrix4x4.Rotate(material.WorldRotation), material.WorldMaterial));
            }
            return parts;
        }

        private static Part MakePart(Mesh mesh, Matrix4x4 matrix, Material mat)
        {
            float emission = 0f;
            if (mat != null && mat.IsKeywordEnabled("_EMISSION") && mat.HasProperty("_EmissionColor"))
                emission = mat.GetColor("_EmissionColor").maxColorComponent;
            return new Part { Mesh = mesh, Matrix = matrix, Texture = ReadableCopy(mat != null ? mat.mainTexture as Texture2D : null), Emission = emission };
        }

        // 임포트된 텍스처는 읽기 잠금이라 PNG 파일을 직접 읽어 사본을 만든다.
        private static Texture2D ReadableCopy(Texture2D source)
        {
            if (source == null) return null;
            string path = AssetDatabase.GetAssetPath(source);
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return null;
            var copy = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            copy.LoadImage(File.ReadAllBytes(path));
            copy.filterMode = FilterMode.Point;
            return copy;
        }

        private static void Rasterize(List<Part> parts, GatherIconSettings s, int res, float[] alpha, Color[] color)
        {
            var view = Quaternion.Inverse(Quaternion.Euler(s.Pitch, s.Yaw, 0f)); // 카메라가 +Z를 본다
            var light = -(Quaternion.Euler(s.LightEuler) * Vector3.forward);    // 표면에서 해 쪽
            float ambient = s.Ambient.grayscale;

            // 화면 범위: 모든 꼭짓점을 카메라 공간으로 옮긴 뒤 감싸는 정사각형 (여백 4%)
            var projected = new List<Vector3[]>();
            float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
            foreach (var p in parts)
            {
                var verts = p.Mesh.vertices;
                var outVerts = new Vector3[verts.Length];
                for (int i = 0; i < verts.Length; i++)
                {
                    var v = view * p.Matrix.MultiplyPoint3x4(verts[i]);
                    outVerts[i] = v;
                    minX = Mathf.Min(minX, v.x); maxX = Mathf.Max(maxX, v.x);
                    minY = Mathf.Min(minY, v.y); maxY = Mathf.Max(maxY, v.y);
                }
                projected.Add(outVerts);
            }
            float span = Mathf.Max(maxX - minX, maxY - minY) * 1.04f;
            float cx = (minX + maxX) * 0.5f, cy = (minY + maxY) * 0.5f;
            float scale = res / span;

            var depth = new float[res * res];
            for (int i = 0; i < depth.Length; i++) depth[i] = float.MaxValue;

            for (int pi = 0; pi < parts.Count; pi++)
            {
                var part = parts[pi];
                var verts = projected[pi];
                var uvs = part.Mesh.uv;
                var raw = part.Mesh.vertices;
                for (int sub = 0; sub < part.Mesh.subMeshCount; sub++)
                {
                    var tris = part.Mesh.GetTriangles(sub);
                    for (int t = 0; t < tris.Length; t += 3)
                    {
                        int i0 = tris[t], i1 = tris[t + 1], i2 = tris[t + 2];
                        var a = ToScreen(verts[i0], cx, cy, scale, res);
                        var b = ToScreen(verts[i1], cx, cy, scale, res);
                        var c = ToScreen(verts[i2], cx, cy, scale, res);
                        float area = Edge(a, b, c);
                        if (Mathf.Abs(area) < 1e-6f) continue;

                        // 면 색: 무게중심의 팔레트 색 × (주변광 + 해 밝기), 발광 머티리얼은 그만큼 더 밝게
                        var worldNormal = Vector3.Cross(
                            part.Matrix.MultiplyVector(raw[i1] - raw[i0]),
                            part.Matrix.MultiplyVector(raw[i2] - raw[i0])).normalized; // 유니티 RecalculateNormals와 같은 방향(바깥)
                        var uv = uvs.Length > 0 ? (uvs[i0] + uvs[i1] + uvs[i2]) / 3f : Vector2.zero;
                        var baseColor = part.Texture != null ? PalettePixel(part.Texture, uv) : Color.white;
                        float shade = ambient + s.LightIntensity * Mathf.Max(0f, Vector3.Dot(worldNormal, light));
                        var lit = baseColor * Mathf.Min(shade, 1.25f) + baseColor * part.Emission * 0.5f;
                        lit.r = Mathf.Clamp01(lit.r); lit.g = Mathf.Clamp01(lit.g); lit.b = Mathf.Clamp01(lit.b); lit.a = 1f;

                        int x0 = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(a.x, Mathf.Min(b.x, c.x))));
                        int x1 = Mathf.Min(res - 1, Mathf.CeilToInt(Mathf.Max(a.x, Mathf.Max(b.x, c.x))));
                        int y0 = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(a.y, Mathf.Min(b.y, c.y))));
                        int y1 = Mathf.Min(res - 1, Mathf.CeilToInt(Mathf.Max(a.y, Mathf.Max(b.y, c.y))));
                        for (int y = y0; y <= y1; y++)
                        for (int x = x0; x <= x1; x++)
                        {
                            var p = new Vector3(x + 0.5f, y + 0.5f, 0f);
                            float w0 = Edge(b, c, p) / area, w1 = Edge(c, a, p) / area, w2 = Edge(a, b, p) / area;
                            if (w0 < 0f || w1 < 0f || w2 < 0f) continue;
                            float z = w0 * a.z + w1 * b.z + w2 * c.z;
                            int index = y * res + x;
                            if (z >= depth[index]) continue;
                            depth[index] = z;
                            alpha[index] = 1f;
                            color[index] = lit;
                        }
                    }
                }
            }
        }

        // 팔레트는 칸 하나가 한 색이라 이웃 칸과 섞지 않고 그 칸만 읽는다.
        private static Color PalettePixel(Texture2D texture, Vector2 uv)
        {
            int x = Mathf.Clamp(Mathf.FloorToInt(Mathf.Repeat(uv.x, 1f) * texture.width), 0, texture.width - 1);
            int y = Mathf.Clamp(Mathf.FloorToInt(Mathf.Repeat(uv.y, 1f) * texture.height), 0, texture.height - 1);
            return texture.GetPixel(x, y);
        }

        // 카메라 공간 → 화면 픽셀 (z는 깊이, 작을수록 가까움)
        private static Vector3 ToScreen(Vector3 v, float cx, float cy, float scale, int res)
            => new Vector3((v.x - cx) * scale + res * 0.5f, (v.y - cy) * scale + res * 0.5f, v.z);

        private static float Edge(Vector3 a, Vector3 b, Vector3 p) => (b.x - a.x) * (p.y - a.y) - (b.y - a.y) * (p.x - a.x);

        // ---------- 도트로 ----------
        private static Color32[] Pixelate(float[] alpha, Color[] color, int res, GatherIconSettings s)
        {
            int minX = res, minY = res, maxX = -1, maxY = -1;
            for (int i = 0; i < alpha.Length; i++)
            {
                if (alpha[i] < 0.5f) continue;
                int x = i % res, y = i / res;
                if (x < minX) minX = x; if (x > maxX) maxX = x;
                if (y < minY) minY = y; if (y > maxY) maxY = y;
            }

            int size = s.IconSize;
            int inner = size - s.Margin * 2;
            var cells = new Color[inner * inner];
            var filled = new bool[inner * inner];
            if (maxX >= 0)
            {
                // 모델을 감싸는 정사각형을 안쪽 칸 수로 나눠 칸마다 덮인 비율·평균 색
                float side = Mathf.Max(maxX - minX + 1, maxY - minY + 1);
                float cx = (minX + maxX + 1) * 0.5f, cy = (minY + maxY + 1) * 0.5f;
                float x0 = cx - side * 0.5f, y0 = cy - side * 0.5f, step = side / inner;
                for (int gy = 0; gy < inner; gy++)
                for (int gx = 0; gx < inner; gx++)
                {
                    float sumA = 0f; var sumC = Color.clear; int count = 0;
                    int sx0 = Mathf.FloorToInt(x0 + gx * step), sx1 = Mathf.CeilToInt(x0 + (gx + 1) * step);
                    int sy0 = Mathf.FloorToInt(y0 + gy * step), sy1 = Mathf.CeilToInt(y0 + (gy + 1) * step);
                    for (int sy = sy0; sy < sy1; sy++)
                    for (int sx = sx0; sx < sx1; sx++)
                    {
                        count++;
                        if (sx < 0 || sy < 0 || sx >= res || sy >= res) continue;
                        float a = alpha[sy * res + sx];
                        sumA += a;
                        sumC += color[sy * res + sx] * a;
                    }
                    int index = gy * inner + gx;
                    if (count == 0 || sumA / count < s.Coverage) continue;
                    filled[index] = true;
                    cells[index] = sumC / sumA;
                }
            }

            Quantize(cells, filled, s.Colors);

            // 안쪽 칸을 가운데에 놓고, 비어 있는 칸 중 상하좌우에 칠한 칸이 있으면 외곽선
            var output = new Color32[size * size];
            var solid = new bool[size * size];
            for (int gy = 0; gy < inner; gy++)
            for (int gx = 0; gx < inner; gx++)
            {
                int index = gy * inner + gx;
                if (!filled[index]) continue;
                int o = (gy + s.Margin) * size + gx + s.Margin;
                var c = cells[index];
                output[o] = new Color32((byte)Mathf.RoundToInt(c.r * 255f), (byte)Mathf.RoundToInt(c.g * 255f), (byte)Mathf.RoundToInt(c.b * 255f), 255);
                solid[o] = true;
            }
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                int o = y * size + x;
                if (solid[o]) continue;
                bool edge = (x > 0 && solid[o - 1]) || (x < size - 1 && solid[o + 1]) || (y > 0 && solid[o - size]) || (y < size - 1 && solid[o + size]);
                output[o] = edge ? s.Outline : new Color32(0, 0, 0, 0);
            }
            return output;
        }

        // 칠한 칸 색을 k개로 줄인다 (k-평균, 밝기 순으로 시작점을 고름).
        private static void Quantize(Color[] cells, bool[] filled, int k)
        {
            var list = new List<int>();
            for (int i = 0; i < cells.Length; i++) if (filled[i]) list.Add(i);
            if (list.Count <= k) return;
            list.Sort((a, b) => Luma(cells[a]).CompareTo(Luma(cells[b])));
            var centers = new Color[k];
            for (int c = 0; c < k; c++) centers[c] = cells[list[(int)((c + 0.5f) / k * list.Count)]];
            var assign = new int[cells.Length];
            for (int iteration = 0; iteration < 12; iteration++)
            {
                var sums = new Color[k]; var counts = new int[k];
                foreach (int i in list)
                {
                    int best = 0; float bestD = float.MaxValue;
                    for (int c = 0; c < k; c++)
                    {
                        float d = Distance(cells[i], centers[c]);
                        if (d < bestD) { bestD = d; best = c; }
                    }
                    assign[i] = best; sums[best] += cells[i]; counts[best]++;
                }
                for (int c = 0; c < k; c++) if (counts[c] > 0) centers[c] = sums[c] / counts[c];
            }
            foreach (int i in list) cells[i] = centers[assign[i]];
        }

        private static float Luma(Color c) => 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;
        private static float Distance(Color a, Color b) => (a.r - b.r) * (a.r - b.r) + (a.g - b.g) * (a.g - b.g) + (a.b - b.b) * (a.b - b.b);

        // ---------- 저장 ----------
        private static void Write(Color32[] pixels, int size, string path)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.SetPixels32(pixels);
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        private static void ImportIcon(string path)
        {
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 16f;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }

        private static void Assign(GatherMaterialDefinition material, string path)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            var so = new SerializedObject(material);
            so.FindProperty("_icon").objectReferenceValue = sprite;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // 한눈에 보는 확인용 시트: 크림 바탕에 아이콘을 8배로 늘어놓는다 (게임에서는 쓰지 않음).
        private static string WritePreview(List<Color32[]> baked, GatherIconSettings s)
        {
            if (baked.Count == 0) return null;
            const int zoom = 8, gap = 2;
            int size = s.IconSize, columns = Mathf.Min(7, baked.Count), rows = (baked.Count + columns - 1) / columns;
            int cell = (size + gap * 2) * zoom;
            var sheet = new Texture2D(columns * cell, rows * cell, TextureFormat.RGBA32, false);
            var fill = new Color32[sheet.width * sheet.height];
            for (int i = 0; i < fill.Length; i++) fill[i] = new Color32(222, 190, 145, 255);
            sheet.SetPixels32(fill);
            for (int n = 0; n < baked.Count; n++)
            {
                int ox = (n % columns) * cell + gap * zoom, oy = (rows - 1 - n / columns) * cell + gap * zoom;
                var pixels = baked[n];
                for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    var p = pixels[y * size + x];
                    if (p.a == 0) continue;
                    for (int dy = 0; dy < zoom; dy++)
                    for (int dx = 0; dx < zoom; dx++)
                        sheet.SetPixel(ox + x * zoom + dx, oy + y * zoom + dy, p);
                }
            }
            string path = s.OutputFolder + "/_preview/gather_icons_preview.png";
            File.WriteAllBytes(path, sheet.EncodeToPNG());
            Object.DestroyImmediate(sheet);
            return path;
        }

        private static GatherIconSettings LoadSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<GatherIconSettings>(SettingsPath);
            if (settings != null) return settings;
            EnsureFolder(Path.GetDirectoryName(SettingsPath).Replace('\\', '/'));
            settings = ScriptableObject.CreateInstance<GatherIconSettings>();
            AssetDatabase.CreateAsset(settings, SettingsPath);
            return settings;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
