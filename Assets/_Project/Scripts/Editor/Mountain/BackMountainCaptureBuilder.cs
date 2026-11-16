// 뒷산 포획 프로토타입 도구: 동물 데이터 3종·포획 설정·풀숲 프리팹을 만들고(있으면 그대로 둠),
// BackMountain 씬의 [Prototype]/Capture에 B 북쪽 평지 바닥(윗면 4.2 m)·트리거·풀숲 12개·팻말·투명 벽을 다시 배치하고 연결한다.
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VoxelMagic.Core;
using VoxelMagic.Gameplay;

namespace VoxelMagic.EditorTools
{
    public static class BackMountainCaptureBuilder
    {
        private const string AnimalFolder = "Assets/_Project/Data/Mountain/Animals";
        private const string MaterialFolder = "Assets/_Project/Materials/Mountain";
        private const string PrefabPath = "Assets/_Project/Prefabs/Mountain/GrassTuft.prefab";
        private const string ConfigPath = "Assets/_Project/Data/Config/CaptureConfig.asset";
        private const string FontPath = "Assets/_Project/Fonts/Pretendard SDF.asset";

        // 구역: x 0~16, z 64~80, 바닥 윗면 4.2 m (B 북쪽 변 실측 4.13~4.27)
        private static readonly Vector3 ZoneMin = new Vector3(0f, 4.2f, 64f);
        private static readonly Vector2 ZoneSize = new Vector2(16f, 16f);
        private const float FloorThickness = 0.5f;
        private const float TriggerInset = 0.5f;
        private const float TriggerHeight = 3f;
        private const float WallHeight = 1.2f;
        // 구역 근처 산길 (B 꺾임 중간 ~ C1 앞, 길 높이 4.1~4.8 m). 이 안에서는 기본 카메라에 맨 앞 풀숲이 보인다
        private static readonly Vector3 NearPathCenter = new Vector3(14.25f, 5.5f, 56.75f);
        private static readonly Vector3 NearPathSize = new Vector3(11.5f, 4f, 6.5f);
        private const int FrontTuftIndex = 1;

        private static readonly Vector2[] TuftPositions =
        {
            new Vector2(2.5f, 66.5f), new Vector2(12.5f, 65.3f), // 두 번째가 길에서 보이는 맨 앞 풀숲 new Vector2(5.5f, 69f), new Vector2(9.5f, 68.5f),
            new Vector2(14f, 69.5f), new Vector2(2f, 72f), new Vector2(7.5f, 72.5f), new Vector2(11.5f, 72f),
            new Vector2(4.5f, 76f), new Vector2(9f, 76.5f), new Vector2(13.5f, 75.5f), new Vector2(7f, 78.5f),
        };

        private static readonly (string id, string name, string desc, float accuracy, Color color, float min, float max)[] Animals =
        {
            ("Hare", "산토끼", "귀가 긴 뒷산 토끼. 겁이 많아 풀숲에 숨는다.", 0.5f, new Color(0.85f, 0.82f, 0.75f), 30f, 45f),
            ("Squirrel", "다람쥐", "볼에 도토리를 가득 문 작은 다람쥐.", 0.6f, new Color(0.6f, 0.4f, 0.22f), 15f, 22f),
            ("Fox", "여우", "꼬리가 탐스러운 영리한 여우. 잡기 어렵다.", 0.8f, new Color(0.9f, 0.45f, 0.15f), 50f, 70f),
        };

        [MenuItem("VoxelMagic/뒷산/포획 구역 만들기·배치")]
        public static void BuildAll()
        {
            if (EditorSceneManager.GetActiveScene().name != "BackMountain")
            {
                EditorUtility.DisplayDialog("포획 구역", "BackMountain 씬을 연 상태에서 실행하세요.", "확인");
                return;
            }
            EnsureFolder(AnimalFolder);
            EnsureFolder(MaterialFolder);
            var animals = CreateAnimals();
            var config = AssetDatabase.LoadAssetAtPath<CaptureConfig>(ConfigPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<CaptureConfig>();
                AssetDatabase.CreateAsset(config, ConfigPath);
            }
            var tuftPrefab = BuildTuftPrefab();
            PlaceInScene(tuftPrefab, animals, config);
            AssetDatabase.SaveAssets();
            Debug.Log($"[뒷산 포획] 동물 {animals.Count}종, 풀숲 {TuftPositions.Length}개 배치 (씬 저장은 직접)");
        }

        private static List<AnimalDefinition> CreateAnimals()
        {
            var list = new List<AnimalDefinition>();
            foreach (var a in Animals)
            {
                string path = $"{AnimalFolder}/Animal_{a.id}.asset";
                var asset = AssetDatabase.LoadAssetAtPath<AnimalDefinition>(path);
                if (asset == null)
                {
                    asset = ScriptableObject.CreateInstance<AnimalDefinition>();
                    AssetDatabase.CreateAsset(asset, path);
                    var so = new SerializedObject(asset);
                    so.FindProperty("_id").stringValue = "animal_" + a.id;
                    so.FindProperty("_displayName").stringValue = a.name;
                    so.FindProperty("_description").stringValue = a.desc;
                    so.FindProperty("_iconColor").colorValue = a.color;
                    so.FindProperty("_maxStack").intValue = 99;
                    so.FindProperty("_requiredAccuracy").floatValue = a.accuracy;
                    so.FindProperty("_sizeRangeCm").FindPropertyRelative("_min").floatValue = a.min;
                    so.FindProperty("_sizeRangeCm").FindPropertyRelative("_max").floatValue = a.max;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
                list.Add(asset);
            }
            return list;
        }

        private static GameObject BuildTuftPrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (existing != null) return existing;

            var grass = MakeMaterial("M_Mtn_Tuft", new Color(0.62f, 0.66f, 0.25f));
            var root = new GameObject("GrassTuft");
            var visual = new GameObject("Visual").transform;
            visual.SetParent(root.transform, false);
            Blob(visual, new Vector3(0f, 0.35f, 0f), new Vector3(0.75f, 0.7f, 0.75f), grass);
            Blob(visual, new Vector3(0.3f, 0.25f, 0.15f), new Vector3(0.5f, 0.5f, 0.5f), grass);
            Blob(visual, new Vector3(-0.25f, 0.22f, -0.2f), new Vector3(0.55f, 0.45f, 0.55f), grass);

            var tuft = root.AddComponent<GrassTuft>();
            var so = new SerializedObject(tuft);
            so.FindProperty("_visual").objectReferenceValue = visual;
            so.FindProperty("_defaultAnchorHeight").floatValue = 1.3f;
            so.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static void PlaceInScene(GameObject tuftPrefab, List<AnimalDefinition> animals, CaptureConfig config)
        {
            var proto = GameObject.Find("[Prototype]");
            var old = proto.transform.Find("Capture");
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            var group = new GameObject("Capture");
            Undo.RegisterCreatedObjectUndo(group, "포획 구역 배치");
            group.transform.SetParent(proto.transform, false);

            var center = new Vector3(ZoneMin.x + ZoneSize.x * 0.5f, ZoneMin.y, ZoneMin.z + ZoneSize.y * 0.5f);

            // 바닥 (윗면 = 4.2 m)
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.SetParent(group.transform, false);
            floor.transform.position = center - Vector3.up * FloorThickness * 0.5f;
            floor.transform.localScale = new Vector3(ZoneSize.x, FloorThickness, ZoneSize.y);
            floor.GetComponent<MeshRenderer>().sharedMaterial = MakeMaterial("M_Mtn_GoldenGrass", new Color(0.83f, 0.7f, 0.3f));

            // 트리거 영역 + 포획 구역 로직
            var zoneGo = new GameObject("Zone");
            zoneGo.transform.SetParent(group.transform, false);
            zoneGo.transform.position = center + Vector3.up * TriggerHeight * 0.5f;
            var box = zoneGo.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(ZoneSize.x - TriggerInset * 2f, TriggerHeight, ZoneSize.y - TriggerInset * 2f);
            var zone = zoneGo.AddComponent<CaptureZone>();
            var zso = new SerializedObject(zone);
            zso.FindProperty("_area").objectReferenceValue = box;
            zso.FindProperty("_player").objectReferenceValue = GameObject.Find("Player")?.transform;
            zso.ApplyModifiedPropertiesWithoutUndo();

            // 풀숲
            GrassTuft front = null;
            for (int i = 0; i < TuftPositions.Length; i++)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(tuftPrefab, group.scene);
                go.name = $"GrassTuft_{i + 1:00}";
                go.transform.SetParent(zoneGo.transform, true);
                go.transform.position = new Vector3(TuftPositions[i].x, ZoneMin.y, TuftPositions[i].y);
                go.transform.rotation = Quaternion.Euler(0f, i * 47f, 0f);
                if (i == FrontTuftIndex) front = go.GetComponent<GrassTuft>();
            }

            // 근처 산길 영역: 여기를 걸어 지나가면 맨 앞 풀숲이 한 번 흔들린다
            var near = new GameObject("NearPath");
            near.transform.SetParent(group.transform, false);
            near.transform.position = NearPathCenter;
            var nearBox = near.AddComponent<BoxCollider>();
            nearBox.isTrigger = true;
            nearBox.size = NearPathSize;
            zso.Update();
            zso.FindProperty("_nearPath").objectReferenceValue = nearBox;
            zso.FindProperty("_frontTuft").objectReferenceValue = front;
            zso.ApplyModifiedPropertiesWithoutUndo();

            BuildSign(group.transform, new Vector3(6f, ZoneMin.y, 64.6f));
            BuildWalls(group.transform);
            ConnectBootstrap(proto, animals, config);
            EnsureBanner();
            EditorSceneManager.MarkSceneDirty(group.scene);
        }

        private static void BuildSign(Transform parent, Vector3 position)
        {
            var sign = new GameObject("Sign_AnimalWarning").transform;
            sign.SetParent(parent, false);
            sign.position = position;
            sign.rotation = Quaternion.Euler(0f, 180f, 0f); // 글씨가 남쪽(길 쪽)을 보게
            var wood = MakeMaterial("M_Mtn_SignWood", new Color(0.55f, 0.38f, 0.22f));
            var post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            post.name = "Post";
            post.transform.SetParent(sign, false);
            post.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            post.transform.localScale = new Vector3(0.12f, 0.6f, 0.12f);
            post.GetComponent<MeshRenderer>().sharedMaterial = wood;
            var board = GameObject.CreatePrimitive(PrimitiveType.Cube);
            board.name = "Board";
            board.transform.SetParent(sign, false);
            board.transform.localPosition = new Vector3(0f, 1.35f, 0f);
            board.transform.localScale = new Vector3(1.3f, 0.5f, 0.06f);
            board.GetComponent<MeshRenderer>().sharedMaterial = wood;
            Object.DestroyImmediate(board.GetComponent<Collider>());

            var label = new GameObject("Text");
            label.transform.SetParent(sign, false);
            label.transform.localPosition = new Vector3(0f, 1.35f, 0.035f);
            label.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            var text = label.AddComponent<TextMeshPro>();
            text.text = "산 동물 조심";
            text.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            text.fontSize = 2.4f;
            text.color = new Color(1f, 0.95f, 0.85f);
            text.alignment = TextAlignmentOptions.Center;
            ((RectTransform)label.transform).sizeDelta = new Vector2(1.25f, 0.45f);
        }

        private static void BuildWalls(Transform parent)
        {
            var walls = new GameObject("Walls (invisible)").transform;
            walls.SetParent(parent, false);
            float y = ZoneMin.y + WallHeight * 0.5f;
            float t = 0.2f;
            Wall(walls, "West", new Vector3(ZoneMin.x - t * 0.5f, y, ZoneMin.z + ZoneSize.y * 0.5f), new Vector3(t, WallHeight, ZoneSize.y));
            Wall(walls, "East", new Vector3(ZoneMin.x + ZoneSize.x + t * 0.5f, y, ZoneMin.z + ZoneSize.y * 0.5f), new Vector3(t, WallHeight, ZoneSize.y));
            Wall(walls, "North", new Vector3(ZoneMin.x + ZoneSize.x * 0.5f, y, ZoneMin.z + ZoneSize.y + t * 0.5f), new Vector3(ZoneSize.x + t * 2f, WallHeight, t));
        }

        private static void Wall(Transform parent, string name, Vector3 position, Vector3 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.AddComponent<BoxCollider>().size = size;
        }

        private static void ConnectBootstrap(GameObject proto, List<AnimalDefinition> animals, CaptureConfig config)
        {
            var bootstrap = Object.FindFirstObjectByType<MountainBootstrap>(FindObjectsInactive.Include);
            if (bootstrap == null)
            {
                Debug.LogWarning("[뒷산 포획] MountainBootstrap이 없습니다. 먼저 '채집 서식지 만들기·배치'를 실행하세요.");
                return;
            }
            if (bootstrap.GetComponent<MagicCircleStub>() == null) Undo.AddComponent<MagicCircleStub>(bootstrap.gameObject);
            var so = new SerializedObject(bootstrap);
            so.FindProperty("_capture").objectReferenceValue = config;
            var list = so.FindProperty("_animals");
            list.arraySize = animals.Count;
            for (int i = 0; i < animals.Count; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = animals[i];
            so.ApplyModifiedProperties();
        }

        /// <summary>토스트 UI를 복제해 같은 글꼴로 화면 정중앙 큰 문구를 만든다.</summary>
        private static void EnsureBanner()
        {
            var toast = Object.FindFirstObjectByType<UI.ToastView>(FindObjectsInactive.Include);
            if (toast == null || toast.transform.parent.Find("CenterBanner (Prototype)") != null) return;
            var copy = Object.Instantiate(toast.gameObject, toast.transform.parent);
            copy.name = "CenterBanner (Prototype)";
            Undo.RegisterCreatedObjectUndo(copy, "중앙 문구");
            Object.DestroyImmediate(copy.GetComponent<UI.ToastView>());
            var text = copy.GetComponentInChildren<TMP_Text>(true);
            text.fontSize *= 1.8f;
            var view = copy.AddComponent<UI.CenterBannerView>();
            var so = new SerializedObject(view);
            so.FindProperty("_text").objectReferenceValue = text;
            so.ApplyModifiedPropertiesWithoutUndo();
            var rect = (RectTransform)copy.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, rect.sizeDelta.y * 1.6f);
        }

        private static void Blob(Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Blob";
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            go.GetComponent<MeshRenderer>().sharedMaterial = material;
            Object.DestroyImmediate(go.GetComponent<Collider>());
        }

        private static Material MakeMaterial(string name, Color color)
        {
            string path = $"{MaterialFolder}/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null) return mat;
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.SetColor("_BaseColor", color);
            mat.SetFloat("_Smoothness", 0.1f);
            AssetDatabase.CreateAsset(mat, path);
            return mat;
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
