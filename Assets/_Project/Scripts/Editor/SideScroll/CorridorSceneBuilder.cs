// Corridor_SideScroll 씬을 다시 만드는 에디터 도구. 이미지 임포트 설정을 적용하고, 데이터 에셋은 없을 때만 만든다.
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VoxelMagic.Core;
using VoxelMagic.Gameplay;

namespace VoxelMagic.EditorTools
{
    public static class CorridorSceneBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/Corridor_SideScroll.unity";
        private const string DataFolder = "Assets/_Project/Data/SideScroll";
        private const string SpriteMaterialPath =
            "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat";

        // 기본 복도 순서: 문 → 창문 → 민벽 → 창문+사물함 → 창문 → 민벽 → 문(반전)
        private static readonly (string file, bool flip)[] DefaultTiles =
        {
            ("tile_door_glass", false), ("tile_window_glass", false), ("tile_wall", false),
            ("tile_locker_glass", false), ("tile_window_glass", false), ("tile_wall", false),
            ("tile_door_glass", true)
        };

        private static readonly Vector3 LightEuler = new Vector3(30f, 20f, 0f);

        [MenuItem("VoxelMagic/복도/복도 씬 다시 만들기")]
        private static void BuildFromMenu()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null &&
                !EditorUtility.DisplayDialog("씬 다시 만들기", "기존 Corridor_SideScroll 씬을 덮어씁니다. 계속할까요?", "덮어쓰기", "취소"))
                return;
            Build();
        }

        public static void Build()
        {
            CorridorSpriteImportTool.Apply(LoadOrCreateConfig());

            // 새 씬을 열면 쓰지 않는 에셋이 메모리에서 내려가 미리 읽은 참조가 풀리므로, 씬을 연 뒤에 읽는다.
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var config = LoadOrCreateConfig();
            var layout = LoadOrCreateLayout();
            var day = LoadOrCreateBackdrop("BackdropSet_Day", "bg_day_loop");
            var spriteMaterial = AssetDatabase.LoadAssetAtPath<Material>(SpriteMaterialPath);

            var camera = Camera.main;
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.gameObject.AddComponent<SideScrollCamera>();

            var light = Object.FindFirstObjectByType<Light>();
            light.transform.rotation = Quaternion.Euler(LightEuler);
            light.shadows = LightShadows.None;

            var corridor = new GameObject("Corridor").AddComponent<CorridorBuilder>();
            corridor.SetRef("_layout", layout).SetRef("_config", config).SetRef("_spriteMaterial", spriteMaterial);
            corridor.Build();

            var backdrop = new GameObject("Backdrop").AddComponent<ParallaxBackdrop>();
            backdrop.SetRef("_config", config).SetRef("_camera", camera).SetRef("_spriteMaterial", spriteMaterial);
            backdrop.SetBackdrop(day);

            var bootstrap = new GameObject("_Bootstrap").AddComponent<CorridorBootstrap>();
            bootstrap.transform.SetAsFirstSibling();
            bootstrap.SetRef("_config", config)
                .SetRef("_corridor", corridor)
                .SetRefList("_bindRoots", new Object[] { camera.gameObject });

            // 에디터에서도 플레이 때와 같은 화면이 보이도록 카메라를 복도 왼쪽 끝에 맞춘다.
            var bounds = corridor.Bounds;
            camera.orthographicSize = SideScrollMath.OrthographicSizeFor(bounds.Height);
            camera.transform.SetPositionAndRotation(
                new Vector3(SideScrollMath.ClampCameraX(bounds.MinX, bounds, camera.orthographicSize * camera.aspect),
                    bounds.Height * 0.5f, config.CameraZ),
                Quaternion.identity);
            camera.backgroundColor = day.SkyColor;

            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[CorridorSceneBuilder] {ScenePath} 생성 완료 (복도 폭 {bounds.Width:0.##}, 높이 {bounds.Height:0.##})");
        }

        internal static CorridorConfig LoadOrCreateConfig() => LoadOrCreate<CorridorConfig>("CorridorConfig");

        private static CorridorLayout LoadOrCreateLayout()
        {
            string path = $"{DataFolder}/CorridorLayout.asset";
            var layout = AssetDatabase.LoadAssetAtPath<CorridorLayout>(path);
            if (layout != null) return layout;

            layout = ScriptableObject.CreateInstance<CorridorLayout>();
            AssetDatabase.CreateAsset(layout, path);
            var so = new SerializedObject(layout);
            var list = so.FindProperty("_tiles");
            list.arraySize = DefaultTiles.Length;
            for (int i = 0; i < DefaultTiles.Length; i++)
            {
                var element = list.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("_sprite").objectReferenceValue =
                    LoadSprite(CorridorSpriteImportTool.CorridorArtFolder, DefaultTiles[i].file);
                element.FindPropertyRelative("_flipX").boolValue = DefaultTiles[i].flip;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            return layout;
        }

        private static BackdropSet LoadOrCreateBackdrop(string assetName, string file)
        {
            string path = $"{DataFolder}/{assetName}.asset";
            var set = AssetDatabase.LoadAssetAtPath<BackdropSet>(path);
            if (set != null) return set;

            set = ScriptableObject.CreateInstance<BackdropSet>();
            AssetDatabase.CreateAsset(set, path);
            set.SetRef("_sprite", LoadSprite(CorridorSpriteImportTool.BackdropArtFolder, file));
            return set;
        }

        private static Sprite LoadSprite(string folder, string file)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{folder}/{file}.png");
            if (sprite == null) Debug.LogError($"[CorridorSceneBuilder] 스프라이트를 찾지 못했습니다: {folder}/{file}.png");
            return sprite;
        }

        private static T LoadOrCreate<T>(string fileName) where T : ScriptableObject
        {
            if (!AssetDatabase.IsValidFolder(DataFolder))
                AssetDatabase.CreateFolder("Assets/_Project/Data", "SideScroll");
            string path = $"{DataFolder}/{fileName}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
    }
}
