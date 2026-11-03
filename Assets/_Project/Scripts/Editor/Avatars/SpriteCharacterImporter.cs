// 2D 스프라이트 준비: json(칸 크기·프레임·fps·기준점)을 읽어 PNG에 도트용 임포트 설정을 하고, 칸마다 스프라이트를 만들어
// SpriteCharacterSet(Data/Avatars/<캐릭터>_2D) 안에 함께 저장한다. PPU는 Idle 그림 높이가 목표 키가 되게 계산하고, 같은 캐릭터의 아바타들에 연결한다.
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.EditorTools
{
    public static class SpriteCharacterImporter
    {
        private const string MenuPath = "VoxelMagic/아바타/선택한 2D 스프라이트 준비";
        private static readonly string[] States = { "Idle", "Walk", "Run" };

        private sealed class Animation
        {
            public string Name;
            public string File;
            public float Fps;
            public bool Loop;
            public List<RectInt> Rects = new List<RectInt>();
        }

        [MenuItem(MenuPath)]
        private static void PrepareSelected()
        {
            foreach (var path in SelectedJsonFiles()) Selection.activeObject = Prepare(path);
        }

        [MenuItem(MenuPath, true)]
        private static bool CanPrepare() => SelectedJsonFiles().Any();

        /// <summary>Art/Characters/&lt;캐릭터&gt;/Sprites/*.json 하나로 2D 캐릭터 데이터를 만들거나 갱신한다.</summary>
        public static SpriteCharacterSet Prepare(string jsonPath)
        {
            string folder = Path.GetDirectoryName(jsonPath).Replace('\\', '/');
            string text = File.ReadAllText(jsonPath);
            Vector2 pivotPixels = ParsePivot(text);
            var animations = ParseAnimations(text);
            if (!animations.TryGetValue("Idle", out var idle))
            {
                Debug.LogError($"[SpriteCharacterImporter] {jsonPath}: Idle 애니메이션이 없습니다.");
                return null;
            }

            string character = CharacterOf(jsonPath);
            var set = LoadOrCreateSet($"{character}_2D");

            // 키 맞추기: Idle 프레임에서 발(기준점)부터 머리 끝까지의 픽셀 높이 → 목표 키가 되는 PPU
            float heightPixels = MeasureHeightAbovePivot($"{folder}/{idle.File}", idle.Rects, pivotPixels.y);
            float pixelsPerUnit = heightPixels / set.TargetHeight;

            foreach (var animation in animations.Values)
                ApplyTextureSettings($"{folder}/{animation.File}", pixelsPerUnit);

            // 이전에 만든 스프라이트를 지우고 새로 만든다.
            foreach (var old in AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(set)).OfType<Sprite>())
                Object.DestroyImmediate(old, true);

            var so = new SerializedObject(set);
            foreach (var state in States)
            {
                if (!animations.TryGetValue(state, out var animation)) continue;
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>($"{folder}/{animation.File}");
                var frames = new List<Sprite>();
                for (int i = 0; i < animation.Rects.Count; i++)
                {
                    var r = animation.Rects[i];
                    // json 좌표는 위에서부터, Unity 텍스처 좌표는 아래에서부터
                    var rect = new Rect(r.x, texture.height - r.y - r.height, r.width, r.height);
                    var pivot = new Vector2(pivotPixels.x / r.width, pivotPixels.y / r.height);
                    var sprite = Sprite.Create(texture, rect, pivot, pixelsPerUnit, 0, SpriteMeshType.FullRect);
                    sprite.name = $"{character}_{state}_{i}";
                    AssetDatabase.AddObjectToAsset(sprite, set);
                    frames.Add(sprite);
                }

                var clip = so.FindProperty("_" + state.ToLowerInvariant());
                var list = clip.FindPropertyRelative("_frames");
                list.arraySize = frames.Count;
                for (int i = 0; i < frames.Count; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
                clip.FindPropertyRelative("_fps").floatValue = animation.Fps;
                clip.FindPropertyRelative("_loop").boolValue = animation.Loop;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(set);

            string characterFolder = $"{AvatarPipeline.CharactersRoot}/{character}/";
            var linked = AvatarPipeline.ScanAvatars()
                .Where(a => a.Model != null && AssetDatabase.GetAssetPath(a.Model).StartsWith(characterFolder)).ToList();
            foreach (var avatar in linked) avatar.SetRef("_sprites", set);
            AssetDatabase.SaveAssets();

            Debug.Log($"[SpriteCharacterImporter] {set.name}: PPU {pixelsPerUnit:0.##} (Idle 높이 {heightPixels}px → 키 {set.TargetHeight}), " +
                      $"애니메이션 {string.Join(", ", animations.Values.Select(a => $"{a.Name} {a.Rects.Count}프레임 {a.Fps}fps"))}, " +
                      $"연결한 아바타 {string.Join(", ", linked.Select(a => a.name))}");
            return set;
        }

        // ── json 읽기 (이 형식만: cellWidth/cellHeight/pivotPixels/animations{이름:{file,frames,fps,loop,rects}}) ──

        private static Vector2 ParsePivot(string text)
        {
            var match = Regex.Match(text, @"""pivotPixels""\s*:\s*\[\s*([\d.]+)\s*,\s*([\d.]+)");
            return match.Success ? new Vector2(Number(match.Groups[1].Value), Number(match.Groups[2].Value)) : Vector2.zero;
        }

        private static Dictionary<string, Animation> ParseAnimations(string text)
        {
            int cellWidth = (int)Number(Regex.Match(text, @"""cellWidth""\s*:\s*(\d+)").Groups[1].Value);
            int cellHeight = (int)Number(Regex.Match(text, @"""cellHeight""\s*:\s*(\d+)").Groups[1].Value);
            var headers = Regex.Matches(text,
                @"""(\w+)""\s*:\s*\{\s*""file""\s*:\s*""([^""]+)""\s*,\s*""frames""\s*:\s*(\d+)\s*,\s*""fps""\s*:\s*([\d.]+)\s*,\s*""loop""\s*:\s*(true|false)");
            var result = new Dictionary<string, Animation>();
            for (int i = 0; i < headers.Count; i++)
            {
                var header = headers[i];
                var animation = new Animation
                {
                    Name = header.Groups[1].Value,
                    File = header.Groups[2].Value,
                    Fps = Number(header.Groups[4].Value),
                    Loop = header.Groups[5].Value == "true"
                };
                int end = i + 1 < headers.Count ? headers[i + 1].Index : text.Length;
                string block = text.Substring(header.Index + header.Length, end - header.Index - header.Length);
                foreach (Match rect in Regex.Matches(block, @"\[\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*\]"))
                    animation.Rects.Add(new RectInt((int)Number(rect.Groups[1].Value), (int)Number(rect.Groups[2].Value),
                        (int)Number(rect.Groups[3].Value), (int)Number(rect.Groups[4].Value)));
                // 칸 좌표가 없으면 가로 한 줄로 본다.
                int frames = (int)Number(header.Groups[3].Value);
                if (animation.Rects.Count == 0)
                    for (int f = 0; f < frames; f++) animation.Rects.Add(new RectInt(f * cellWidth, 0, cellWidth, cellHeight));
                result[animation.Name] = animation;
            }
            return result;
        }

        private static float Number(string value) => float.Parse(value, CultureInfo.InvariantCulture);

        // ── 도우미 ───────────────────────────────────────────

        private static float MeasureHeightAbovePivot(string pngPath, List<RectInt> rects, float pivotY)
        {
            var texture = new Texture2D(2, 2);
            texture.LoadImage(File.ReadAllBytes(pngPath));
            var pixels = texture.GetPixels32();
            int height = 0;
            foreach (var r in rects)
            {
                int bottom = texture.height - r.y - r.height;
                for (int y = r.height - 1; y >= 0; y--)
                {
                    bool any = false;
                    for (int x = 0; x < r.width && !any; x++)
                        any = pixels[(bottom + y) * texture.width + r.x + x].a > 0;
                    if (!any) continue;
                    height = Mathf.Max(height, y + 1 - Mathf.RoundToInt(pivotY));
                    break;
                }
            }
            Object.DestroyImmediate(texture);
            return Mathf.Max(1, height);
        }

        private static void ApplyTextureSettings(string path, float pixelsPerUnit)
        {
            if (!(AssetImporter.GetAtPath(path) is TextureImporter importer)) return;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();
        }

        private static SpriteCharacterSet LoadOrCreateSet(string name)
        {
            string path = $"{AvatarPipeline.AvatarDataFolder}/{name}.asset";
            var set = AssetDatabase.LoadAssetAtPath<SpriteCharacterSet>(path);
            if (set != null) return set;
            set = ScriptableObject.CreateInstance<SpriteCharacterSet>();
            AssetDatabase.CreateAsset(set, path);
            return set;
        }

        private static string CharacterOf(string path) =>
            path.Substring(AvatarPipeline.CharactersRoot.Length + 1).Split('/')[0];

        // json 파일 또는 그 파일이 든 폴더를 선택해도 된다.
        private static IEnumerable<string> SelectedJsonFiles()
        {
            foreach (var path in Selection.objects.Select(AssetDatabase.GetAssetPath))
            {
                if (string.IsNullOrEmpty(path) || !path.StartsWith(AvatarPipeline.CharactersRoot)) continue;
                if (AssetDatabase.IsValidFolder(path))
                {
                    foreach (var file in Directory.GetFiles(path, "*.json")) yield return file.Replace('\\', '/');
                }
                else if (path.EndsWith(".json")) yield return path;
            }
        }
    }
}
