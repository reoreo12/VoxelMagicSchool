// 타일 맵 에디터 씬(Scenes/Tools/TileMapEditor)을 만든다: Maps > Map_FarmSample(10×10 샘플), Compare(비교용 카메라·치비·1 m 상자·연못).
// 씬이 이미 있으면 덮어쓰기 전에 묻는다. 칸 크기는 TileMapConfig에서 바꾸면 TileGridScaler가 바로 반영한다.
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;
using System.Linq;
using VoxelMagic.Core;
using VoxelMagic.Gameplay;

namespace VoxelMagic.EditorTools
{
    public static class TileMapEditorSceneBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/Tools/TileMapEditor.unity";
        private const string PlayerConfigPath = "Assets/_Project/Data/Config/PlayerConfig.asset";
        private const string TileFolder = "Assets/_Project/Art/Map/TileAssets/farm";
        private const string PondFolder = "Assets/_Project/Art/Map/Pond";

        // 농장 씬과 같은 해·연못 값 (2026-11-05 Prototype_FarmFishing에서 읽음)
        private static readonly Vector3 SunEuler = new Vector3(50f, 330f, 0f);
        private static readonly Color SunColor = new Color(1f, 0.957f, 0.839f);
        private static readonly Vector3 PondScale = new Vector3(2f, 2f, 1.5f);
        private static readonly Vector3 PondEuler = new Vector3(270f, 0f, 0f);
        private static readonly Vector3 PondMeshOffset = new Vector3(0.125f, 0f, -0.202f);

        // 비교 오브젝트 배치 (기준점 기준, m)
        private static readonly Vector3 RefCubeOffset = new Vector3(1.5f, 0.5f, 0f);
        private const float PondGapFromEastEdge = 4.5f;

        // 샘플 무늬: 위 줄 = 북. G 풀, D 흙, P 진흙 길, S 짚 (흙·길·짚은 Rule Tile)
        // 흙 4×4, 길 2칸 폭, 짚 ㄱ자, 4-3 한계용 1칸 폭 줄(x9)과 대각선만 닿는 칸(5,4)
        private static readonly string[] SamplePattern =
        {
            "GGGGGGPPGG",
            "GDDDDGPPGD",
            "GDDDDGPPGD",
            "GDDDDGPPGD",
            "GDDDDGPPGD",
            "GGGGGDPPGD",
            "GSSGGGPPGD",
            "GSSSSGPPGG",
            "GSSSSGPPGG",
            "GGGGGGPPGG",
        };

        [MenuItem("VoxelMagic/타일 맵/타일 맵 에디터 씬 만들기")]
        private static void BuildFromMenu()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null &&
                !EditorUtility.DisplayDialog("타일 맵 에디터 씬", $"{ScenePath}가 이미 있습니다.\n칠한 내용까지 모두 새로 만들어집니다. 덮어쓸까요?", "덮어쓰기", "취소"))
                return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Build();
        }

        public static void Build()
        {
            // 새 씬을 연 뒤에 에셋을 읽는다 (씬을 바꿀 때 쓰지 않는 에셋이 내려가 먼저 읽은 참조가 끊김)
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var config = FarmTileSetBuilder.LoadOrCreateConfig();

            CreateSun();

            // 비교 오브젝트: Compare > Center(카메라·치비·상자), East(연못) — 작업 대상 맵을 따라감
            var compare = new GameObject(TileMapLibrary.CompareRootName);
            var center = new GameObject("Center").transform;
            var east = new GameObject("East").transform;
            center.SetParent(compare.transform, false);
            east.SetParent(compare.transform, false);
            CreateCamera(center);
            CreateChibi(center);
            CreateRefCube(center);
            CreatePond(east);

            // 맵: Maps > Map_FarmSample > Grid > Paint
            var maps = new GameObject(TileMapLibrary.MapsRootName).transform;
            int size = config.SampleSize;
            var document = TileMapLibrary.CreateMap(maps, "FarmSample", new Vector2Int(size, size), null, Vector3.zero, config);
            PaintSample(document.Paint, size);

            compare.AddComponent<TileMapCompareRig>().Setup(config, center, east);
            compare.GetComponent<TileMapCompareRig>().SetTarget(document);

            EnsureFolder("Assets/_Project/Scenes/Tools");
            EditorSceneManager.SaveScene(scene, ScenePath);
            Selection.activeGameObject = document.gameObject;
            Debug.Log($"[타일 맵] 에디터 씬 만듦: {ScenePath} (칸 {config.CellSize} m, 샘플 {config.SampleSize}×{config.SampleSize})");
        }

        private static void CreateSun()
        {
            var light = new GameObject("Directional Light", typeof(Light)).GetComponent<Light>();
            light.type = LightType.Directional;
            light.color = SunColor;
            light.shadows = LightShadows.Soft;
            light.transform.rotation = Quaternion.Euler(SunEuler);
            VoxelShadowSettings.Apply(light);
        }

        private static void CreateCamera(Transform center)
        {
            var player = AssetDatabase.LoadAssetAtPath<PlayerConfig>(PlayerConfigPath);
            var cam = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cam.tag = "MainCamera";
            cam.transform.SetParent(center, false);
            var rotation = Quaternion.Euler(player.CameraPitch, player.CameraYaw, 0f);
            cam.transform.localRotation = rotation;
            cam.transform.localPosition = rotation * new Vector3(0f, 0f, -player.CameraDistance);
        }

        private static void CreateChibi(Transform center)
        {
            var player = AssetDatabase.LoadAssetAtPath<PlayerConfig>(PlayerConfigPath);
            var avatar = player.Avatar;
            var model = (GameObject)PrefabUtility.InstantiatePrefab(avatar.Model);
            model.name = "Chibi";
            model.transform.SetParent(center, false);
            var animator = model.GetComponent<Animator>();
            if (animator == null) animator = model.AddComponent<Animator>();
            animator.avatar = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(avatar.Model)).OfType<Avatar>().FirstOrDefault();
            animator.runtimeAnimatorController = avatar.Controller;
            animator.applyRootMotion = false;
            model.AddComponent<GroundSurfaceProbe>(); // 플레이 중 발밑 바닥 종류 표시 (확인용)
        }

        private static void CreateRefCube(Transform center)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "RefCube_1m";
            Object.DestroyImmediate(cube.GetComponent<Collider>());
            cube.transform.SetParent(center, false);
            cube.transform.localPosition = RefCubeOffset;
        }

        private static void CreatePond(Transform east)
        {
            var pond = new GameObject("Pond").transform;
            pond.SetParent(east, false);
            pond.localPosition = new Vector3(PondGapFromEastEdge, 0f, 0f);
            AddPondMesh(pond, "Pond_Bank", null, ShadowCastingMode.On);
            AddPondMesh(pond, "Pond_Water", AssetDatabase.LoadAssetAtPath<Material>($"{PondFolder}/Pond_Water_Mat.mat"), ShadowCastingMode.Off);
        }

        private static void AddPondMesh(Transform parent, string name, Material material, ShadowCastingMode cast)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>($"{PondFolder}/{name}.fbx"));
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = PondMeshOffset;
            go.transform.localRotation = Quaternion.Euler(PondEuler);
            go.transform.localScale = PondScale;
            var renderer = go.GetComponentInChildren<Renderer>();
            if (material != null) renderer.sharedMaterial = material;
            renderer.shadowCastingMode = cast;
        }

        private static void PaintSample(Tilemap tilemap, int size)
        {
            TileBase Load(string path) => AssetDatabase.LoadAssetAtPath<TileBase>($"{TileFolder}/{path}.asset");
            var grass = Load("floors/meadow_grass");
            TileBase For(char c)
            {
                switch (c)
                {
                    case 'D': return Load("rules/RT_dark_garden_soil");
                    case 'P': return Load("rules/RT_muddy_farm_path");
                    case 'S': return Load("rules/RT_straw_covered_ground");
                    default: return grass;
                }
            }

            int rows = SamplePattern.Length;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                int row = rows - 1 - y; // 무늬 위 줄 = 북 = 큰 y
                char c = row >= 0 && row < rows && x < SamplePattern[row].Length ? SamplePattern[row][x] : 'G';
                tilemap.SetTile(new Vector3Int(x, y, 0), For(c));
            }
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
