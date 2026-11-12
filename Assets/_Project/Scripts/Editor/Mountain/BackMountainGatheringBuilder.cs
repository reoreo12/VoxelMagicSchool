// 뒷산 채집 프로토타입 도구: 재료 데이터 12종·서식지 기본 도형 프리팹 4종을 만들고(있으면 그대로 둠),
// BackMountain 씬의 [Prototype]/Gathering 아래에 A1~A3 길 양옆으로 12개를 지면에 맞춰 다시 배치한다. 뒷산 의뢰 4종도 만든다.
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VoxelMagic.Core;
using VoxelMagic.Gameplay;

namespace VoxelMagic.EditorTools
{
    public static class BackMountainGatheringBuilder
    {
        private const string DataFolder = "Assets/_Project/Data/Mountain/Gather";
        private const string PrefabFolder = "Assets/_Project/Prefabs/Mountain";
        private const string MaterialFolder = "Assets/_Project/Materials/Mountain";
        private const string ConfigPath = "Assets/_Project/Data/Config/GatheringConfig.asset";

        private struct MaterialSpec
        {
            public string Id, Name, Description, Hint;
            public GatherHabitatKind Habitat;
            public bool AfterSchool, Night;
            public string Mesh;
            public Vector3 Scale;
            public Color Color;
        }

        private struct Placement
        {
            public GatherHabitatKind Kind;
            public float Z;
            public int Side; // -1 = 길 왼쪽(−x), +1 = 오른쪽(+x)
        }

        private static readonly string ShadeHint = "그늘진 큰 나무 아래에서 자란다";
        private static readonly string BushHint = "덤불 사이에 열린다";
        private static readonly string RockHint = "바위틈에서 찾을 수 있다";
        private static readonly string ClearingHint = "탁 트인 평지에 핀다";

        private static readonly MaterialSpec[] Materials =
        {
            M("ShadeMushroom", "그늘버섯", "축축한 그늘에서 자라는 갈색 버섯.", ShadeHint, GatherHabitatKind.TreeShade, true, false, "Cylinder", new Vector3(0.18f, 0.08f, 0.18f), new Color(0.55f, 0.38f, 0.25f)),
            M("MossDrop", "이끼방울", "이슬을 머금은 둥근 이끼 덩이.", ShadeHint, GatherHabitatKind.TreeShade, true, false, "Sphere", new Vector3(0.18f, 0.14f, 0.18f), new Color(0.35f, 0.55f, 0.3f)),
            M("MoonMushroom", "달빛버섯", "밤에만 은은하게 빛나는 버섯.", ShadeHint, GatherHabitatKind.TreeShade, false, true, "Cylinder", new Vector3(0.18f, 0.1f, 0.18f), new Color(0.75f, 0.85f, 1f)),
            M("WildBerry", "산딸기", "덤불에 열린 새콤한 빨간 열매.", BushHint, GatherHabitatKind.Bush, true, false, "Sphere", new Vector3(0.14f, 0.14f, 0.14f), new Color(0.85f, 0.15f, 0.2f)),
            M("ThornLeaf", "가시풀잎", "가장자리에 작은 가시가 난 잎.", BushHint, GatherHabitatKind.Bush, true, false, "Cube", new Vector3(0.22f, 0.04f, 0.14f), new Color(0.45f, 0.65f, 0.2f)),
            M("FireflyFruit", "반딧불열매", "밤이 되면 노랗게 빛나는 열매.", BushHint, GatherHabitatKind.Bush, false, true, "Sphere", new Vector3(0.14f, 0.14f, 0.14f), new Color(1f, 0.9f, 0.3f)),
            M("RockMoss", "돌이끼", "바위에 붙어 자라는 납작한 이끼.", RockHint, GatherHabitatKind.RockCrevice, true, false, "Cube", new Vector3(0.24f, 0.05f, 0.2f), new Color(0.4f, 0.5f, 0.35f)),
            M("SparkPowder", "반짝돌가루", "부서진 바위에서 나온 반짝이는 가루 덩이.", RockHint, GatherHabitatKind.RockCrevice, true, false, "Cube", new Vector3(0.14f, 0.14f, 0.14f), new Color(0.8f, 0.8f, 0.85f)),
            M("StarShard", "별조각돌", "밤하늘 빛을 닮은 푸른 돌 조각.", RockHint, GatherHabitatKind.RockCrevice, false, true, "Capsule", new Vector3(0.1f, 0.1f, 0.1f), new Color(0.3f, 0.4f, 0.95f)),
            M("Dandelion", "민들레", "평지에 흔한 노란 꽃.", ClearingHint, GatherHabitatKind.Clearing, true, false, "Capsule", new Vector3(0.1f, 0.12f, 0.1f), new Color(1f, 0.8f, 0.1f)),
            M("Mugwort", "쑥", "향이 진한 약초 잎.", ClearingHint, GatherHabitatKind.Clearing, true, false, "Cylinder", new Vector3(0.14f, 0.1f, 0.14f), new Color(0.5f, 0.6f, 0.45f)),
            M("DewFlower", "이슬꽃", "밤이슬이 맺힐 때만 피는 하얀 꽃.", ClearingHint, GatherHabitatKind.Clearing, false, true, "Capsule", new Vector3(0.1f, 0.12f, 0.1f), new Color(0.95f, 0.95f, 1f)),
        };

        // A1(z 0~16) → A3(z 32~48)에 골고루, 같은 종류가 한쪽에 몰리지 않게 좌우를 섞는다.
        private static readonly Placement[] Placements =
        {
            P(GatherHabitatKind.Clearing, 3f, -1), P(GatherHabitatKind.Clearing, 7f, 1),
            P(GatherHabitatKind.Bush, 10f, -1), P(GatherHabitatKind.Clearing, 13.5f, 1),
            P(GatherHabitatKind.Bush, 18f, 1), P(GatherHabitatKind.TreeShade, 21.5f, -1),
            P(GatherHabitatKind.Bush, 26f, -1), P(GatherHabitatKind.RockCrevice, 29f, 1),
            P(GatherHabitatKind.TreeShade, 34f, 1), P(GatherHabitatKind.RockCrevice, 37.5f, -1),
            P(GatherHabitatKind.TreeShade, 42f, -1), P(GatherHabitatKind.RockCrevice, 45f, 1),
        };

        [MenuItem("VoxelMagic/뒷산/채집 서식지 만들기·배치")]
        public static void BuildAll()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.name != "BackMountain")
            {
                EditorUtility.DisplayDialog("채집 서식지", "BackMountain 씬을 연 상태에서 실행하세요.", "확인");
                return;
            }
            EnsureFolder(DataFolder);
            EnsureFolder(PrefabFolder);
            EnsureFolder(MaterialFolder);

            var itemMaterial = MakeMaterial("M_GatherItem", Color.white);
            var materials = CreateMaterialAssets();
            var config = LoadOrCreate<GatheringConfig>(ConfigPath);
            GatheringBalanceTool.EnsureDefaults(config); // 서식지 종류별 밸런스가 비어 있으면 기본값
            var prefabs = new Dictionary<GatherHabitatKind, GameObject>
            {
                { GatherHabitatKind.TreeShade, BuildPrefab(GatherHabitatKind.TreeShade, itemMaterial) },
                { GatherHabitatKind.Bush, BuildPrefab(GatherHabitatKind.Bush, itemMaterial) },
                { GatherHabitatKind.RockCrevice, BuildPrefab(GatherHabitatKind.RockCrevice, itemMaterial) },
                { GatherHabitatKind.Clearing, BuildPrefab(GatherHabitatKind.Clearing, itemMaterial) },
            };
            PlaceInScene(prefabs, materials, config);
            AssetDatabase.SaveAssets();
            Debug.Log($"[뒷산 채집] 재료 {materials.Count}종, 서식지 프리팹 4종, 배치 {Placements.Length}개 (씬 저장은 직접)");
        }

        // ---------- 의뢰 ----------
        private const string RequestFolder = "Assets/_Project/Data/Mountain/Requests";

        private struct RequestSpec
        {
            public string Id, Title, Line, Done;
            public (string material, int amount)[] Needs;
        }

        // 요구량 최대 3개: 하나 1개 / 같은 재료 3개 / 서로 다른 재료 2종 / 밤 재료 1개
        private static readonly RequestSpec[] Requests =
        {
            new RequestSpec { Id = "Mtn_Mushroom", Title = "'그것'의 버섯 심부름", Line = "버섯… 그늘에서 나는 거… 하나만…", Done = "음음, 이 향이야. 고마워.", Needs = new[] { ("ShadeMushroom", 1) } },
            new RequestSpec { Id = "Mtn_Berries", Title = "새콤한 간식", Line = "빨갛고 새콤한 거… 세 알 먹고 싶어…", Done = "냠… 세 알 다 맛있어!", Needs = new[] { ("WildBerry", 3) } },
            new RequestSpec { Id = "Mtn_Herbs", Title = "약초 두 가지", Line = "쑥이랑 돌이끼… 하나씩 섞으면 약이 돼…", Done = "좋아, 이제 약을 달일 수 있어.", Needs = new[] { ("Mugwort", 1), ("RockMoss", 1) } },
            new RequestSpec { Id = "Mtn_Moon", Title = "밤의 버섯", Line = "밤에만 빛나는 버섯… 보고 싶어…", Done = "와아… 정말 빛나네.", Needs = new[] { ("MoonMushroom", 1) } },
        };

        private static readonly Dictionary<string, string> CreatureNotes = new Dictionary<string, string>
        {
            { "ShadeMushroom", "그늘버섯은 햇빛을 싫어해… 큰 나무 밑에만 숨어 있어." },
            { "WildBerry", "산딸기는 덤불 가시가 지켜 줘서 새들이 잘 못 먹어." },
            { "Mugwort", "쑥은 마력을 가라앉혀… 그래서 약에 꼭 넣어." },
            { "RockMoss", "돌이끼는 바위의 기운을 빨아들여 자라." },
            { "MoonMushroom", "달빛버섯은 달빛을 모아 두었다가 밤에 내보내." },
        };

        [MenuItem("VoxelMagic/뒷산/의뢰 데이터 만들기")]
        public static List<RequestDefinition> BuildRequests()
        {
            EnsureFolder(RequestFolder);
            var list = new List<RequestDefinition>();
            foreach (var spec in Requests)
            {
                string path = $"{RequestFolder}/Request_{spec.Id}.asset";
                var asset = AssetDatabase.LoadAssetAtPath<RequestDefinition>(path);
                if (asset == null)
                {
                    asset = ScriptableObject.CreateInstance<RequestDefinition>();
                    AssetDatabase.CreateAsset(asset, path);
                    var so = new SerializedObject(asset);
                    so.FindProperty("_title").stringValue = spec.Title;
                    so.FindProperty("_creatureLine").stringValue = spec.Line;
                    so.FindProperty("_completionLine").stringValue = spec.Done;
                    so.FindProperty("_baseReward").intValue = 10;
                    var needs = so.FindProperty("_requirements");
                    needs.arraySize = spec.Needs.Length;
                    for (int i = 0; i < spec.Needs.Length; i++)
                    {
                        var e = needs.GetArrayElementAtIndex(i);
                        e.FindPropertyRelative("_item").objectReferenceValue = LoadMaterial(spec.Needs[i].material);
                        e.FindPropertyRelative("_amount").intValue = spec.Needs[i].amount;
                    }
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
                list.Add(asset);
            }
            // 크리처 대사(필기 공부)용 재료 설명: 비어 있을 때만 채운다
            foreach (var pair in CreatureNotes)
            {
                var material = LoadMaterial(pair.Key);
                if (material == null) continue;
                var so = new SerializedObject(material);
                var note = so.FindProperty("_creatureNote");
                if (!string.IsNullOrEmpty(note.stringValue)) continue;
                note.stringValue = pair.Value;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            AssetDatabase.SaveAssets();
            return list;
        }

        private static GatherMaterialDefinition LoadMaterial(string id)
            => AssetDatabase.LoadAssetAtPath<GatherMaterialDefinition>($"{DataFolder}/Gather_{id}.asset");

        // ---------- 데이터 ----------
        private static List<GatherMaterialDefinition> CreateMaterialAssets()
        {
            var list = new List<GatherMaterialDefinition>();
            foreach (var spec in Materials)
            {
                string path = $"{DataFolder}/Gather_{spec.Id}.asset";
                var asset = AssetDatabase.LoadAssetAtPath<GatherMaterialDefinition>(path);
                if (asset == null)
                {
                    asset = ScriptableObject.CreateInstance<GatherMaterialDefinition>();
                    AssetDatabase.CreateAsset(asset, path);
                    var so = new SerializedObject(asset);
                    so.FindProperty("_id").stringValue = "gather_" + spec.Id;
                    so.FindProperty("_displayName").stringValue = spec.Name;
                    so.FindProperty("_description").stringValue = spec.Description;
                    so.FindProperty("_iconColor").colorValue = spec.Color;
                    so.FindProperty("_habitat").enumValueIndex = (int)spec.Habitat;
                    so.FindProperty("_appearsAfterSchool").boolValue = spec.AfterSchool;
                    so.FindProperty("_appearsAtNight").boolValue = spec.Night;
                    so.FindProperty("_habitatHint").stringValue = spec.Hint;
                    so.FindProperty("_worldMesh").objectReferenceValue = BuiltinMesh(spec.Mesh);
                    so.FindProperty("_worldScale").vector3Value = spec.Scale;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
                list.Add(asset);
            }
            return list;
        }

        // ---------- 프리팹 ----------
        private static GameObject BuildPrefab(GatherHabitatKind kind, Material itemMaterial)
        {
            string path = $"{PrefabFolder}/Habitat_{kind}.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var root = new GameObject($"Habitat_{kind}");
            var visual = new GameObject("Visual").transform;
            visual.SetParent(root.transform, false);
            Vector3[] slots;
            switch (kind)
            {
                case GatherHabitatKind.TreeShade:
                    Shape(PrimitiveType.Cylinder, visual, "Trunk", new Vector3(0f, 1.5f, 0f), new Vector3(0.5f, 1.5f, 0.5f), 0f, MakeMaterial("M_Mtn_Trunk", new Color(0.4f, 0.27f, 0.17f)), true);
                    Shape(PrimitiveType.Sphere, visual, "Canopy", new Vector3(0f, 3.7f, 0f), new Vector3(3.4f, 2.4f, 3.4f), 0f, MakeMaterial("M_Mtn_Canopy", new Color(0.2f, 0.38f, 0.2f)), false);
                    slots = new[] { new Vector3(-0.9f, 0f, 0.9f), new Vector3(0.9f, 0f, 0.9f), new Vector3(-1.1f, 0f, -0.5f), new Vector3(1.1f, 0f, -0.4f) };
                    break;
                case GatherHabitatKind.Bush:
                    var bush = MakeMaterial("M_Mtn_Bush", new Color(0.28f, 0.48f, 0.22f));
                    Shape(PrimitiveType.Sphere, visual, "BushA", new Vector3(0f, 0.45f, 0f), new Vector3(1.4f, 1f, 1.4f), 0f, bush, false);
                    Shape(PrimitiveType.Sphere, visual, "BushB", new Vector3(0.9f, 0.35f, -0.3f), new Vector3(1f, 0.8f, 1f), 0f, bush, false);
                    Shape(PrimitiveType.Sphere, visual, "BushC", new Vector3(-0.8f, 0.35f, -0.2f), new Vector3(1.1f, 0.8f, 1.1f), 0f, bush, false);
                    slots = new[] { new Vector3(0f, 0f, 0.95f), new Vector3(1.2f, 0f, 0.5f), new Vector3(-1.2f, 0f, 0.55f) };
                    break;
                case GatherHabitatKind.RockCrevice:
                    var rock = MakeMaterial("M_Mtn_Rock", new Color(0.5f, 0.5f, 0.52f));
                    Shape(PrimitiveType.Cube, visual, "RockA", new Vector3(-0.95f, 0.5f, 0f), new Vector3(1.2f, 1f, 1.4f), 10f, rock, true);
                    Shape(PrimitiveType.Cube, visual, "RockB", new Vector3(0.95f, 0.65f, 0.1f), new Vector3(1f, 1.3f, 1.2f), -15f, rock, true);
                    slots = new[] { new Vector3(0f, 0f, 0.1f), new Vector3(0f, 0f, 0.95f), new Vector3(-1.1f, 0f, 1f) };
                    break;
                default:
                    Shape(PrimitiveType.Cylinder, visual, "Grass", new Vector3(0f, 0.03f, 0f), new Vector3(2.6f, 0.05f, 2.6f), 0f, MakeMaterial("M_Mtn_Clearing", new Color(0.55f, 0.7f, 0.38f)), false);
                    slots = new[] { new Vector3(0f, 0f, 0.6f), new Vector3(0.8f, 0f, -0.3f), new Vector3(-0.8f, 0f, -0.3f), new Vector3(0f, 0f, -0.9f) };
                    break;
            }

            var slotRoot = new GameObject("Slots").transform;
            slotRoot.SetParent(root.transform, false);
            var pickups = new GatherPickup[slots.Length];
            for (int i = 0; i < slots.Length; i++) pickups[i] = MakeSlot(slotRoot, i, slots[i], itemMaterial);

            var habitat = root.AddComponent<GatherHabitat>();
            var so = new SerializedObject(habitat);
            so.FindProperty("_kind").enumValueIndex = (int)kind;
            var arr = so.FindProperty("_slots");
            arr.arraySize = pickups.Length;
            for (int i = 0; i < pickups.Length; i++) arr.GetArrayElementAtIndex(i).objectReferenceValue = pickups[i];
            so.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        internal static GatherPickup MakeSlot(Transform parent, int index, Vector3 position, Material itemMaterial)
        {
            var slot = new GameObject($"Slot_{index}");
            slot.transform.SetParent(parent, false);
            slot.transform.localPosition = position;
            var item = new GameObject("Item");
            item.transform.SetParent(slot.transform, false);
            var filter = item.AddComponent<MeshFilter>();
            var renderer = item.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = itemMaterial;
            renderer.enabled = false;

            var pickup = slot.AddComponent<GatherPickup>();
            var so = new SerializedObject(pickup);
            so.FindProperty("_itemFilter").objectReferenceValue = filter;
            so.FindProperty("_itemRenderer").objectReferenceValue = renderer;
            so.FindProperty("_defaultAnchorHeight").floatValue = 0.9f;
            so.ApplyModifiedPropertiesWithoutUndo();
            return pickup;
        }

        // ---------- 씬 배치 ----------
        private static void PlaceInScene(Dictionary<GatherHabitatKind, GameObject> prefabs, List<GatherMaterialDefinition> materials, GatheringConfig config)
        {
            var floor = GameObject.Find("MountainFloor");
            var proto = GameObject.Find("[Prototype]") ?? new GameObject("[Prototype]");
            var old = proto.transform.Find("Gathering");
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            var group = new GameObject("Gathering");
            Undo.RegisterCreatedObjectUndo(group, "채집 서식지 배치");
            group.transform.SetParent(proto.transform, false);

            var counts = new Dictionary<GatherHabitatKind, int>();
            foreach (var p in Placements)
            {
                counts.TryGetValue(p.Kind, out int n);
                counts[p.Kind] = n + 1;

                float center = PathCenterX(p.Z);
                float offset = p.Kind == GatherHabitatKind.TreeShade ? 3.9f : 3.3f;
                var xz = new Vector3(center + p.Side * offset, 0f, p.Z);
                var go = (GameObject)PrefabUtility.InstantiatePrefab(prefabs[p.Kind], group.scene);
                go.name = $"{prefabs[p.Kind].name}_{n + 1}";
                go.transform.SetParent(group.transform, true);
                go.transform.position = Snap(xz, floor);
                // 슬롯 쪽(+Z)이 길을 바라보게
                go.transform.rotation = Quaternion.LookRotation(new Vector3(-p.Side, 0f, 0f));
                foreach (var pickup in go.GetComponentsInChildren<GatherPickup>(true))
                    pickup.transform.position = Snap(pickup.transform.position, floor);
            }

            var bootstrapTransform = proto.transform.Find("_Mountain");
            var bootstrapGo = bootstrapTransform != null ? bootstrapTransform.gameObject : new GameObject("_Mountain");
            bootstrapGo.transform.SetParent(proto.transform, false);
            var bootstrap = bootstrapGo.GetComponent<MountainBootstrap>();
            if (bootstrap == null) bootstrap = bootstrapGo.AddComponent<MountainBootstrap>();
            if (bootstrapGo.GetComponent<MountainDebugInput>() == null) bootstrapGo.AddComponent<MountainDebugInput>();
            EnsureDebugHint();
            var so = new SerializedObject(bootstrap);
            so.FindProperty("_game").objectReferenceValue = Object.FindFirstObjectByType<GameBootstrap>();
            so.FindProperty("_gathering").objectReferenceValue = config;
            var mats = so.FindProperty("_materials");
            mats.arraySize = materials.Count;
            for (int i = 0; i < materials.Count; i++) mats.GetArrayElementAtIndex(i).objectReferenceValue = materials[i];
            var roots = so.FindProperty("_bindRoots");
            roots.arraySize = 2;
            roots.GetArrayElementAtIndex(0).objectReferenceValue = proto;
            roots.GetArrayElementAtIndex(1).objectReferenceValue = GameObject.Find("UI");
            so.ApplyModifiedProperties();
            EditorSceneManager.MarkSceneDirty(group.scene);
        }

        /// <summary>조작법 안내(ControlsHint)를 복제해 같은 글꼴로 디버그 키 안내를 그 위에 만든다.</summary>
        private static void EnsureDebugHint()
        {
            var hint = Object.FindFirstObjectByType<UI.ControlsHintView>(FindObjectsInactive.Include);
            if (hint == null || hint.transform.parent.Find("DebugKeyHint (Prototype)") != null) return;
            var copy = Object.Instantiate(hint.gameObject, hint.transform.parent);
            copy.name = "DebugKeyHint (Prototype)";
            Undo.RegisterCreatedObjectUndo(copy, "디버그 키 안내");
            Object.DestroyImmediate(copy.GetComponent<UI.ControlsHintView>());
            var text = copy.GetComponent<TMPro.TMP_Text>();
            var view = copy.AddComponent<UI.DebugKeyHintView>();
            var so = new SerializedObject(view);
            so.FindProperty("_text").objectReferenceValue = text;
            so.ApplyModifiedPropertiesWithoutUndo();
            var rect = (RectTransform)copy.transform;
            var source = (RectTransform)hint.transform;
            rect.anchoredPosition = source.anchoredPosition + new Vector2(0f, source.rect.height + 12f);
            text.color = new Color(1f, 0.85f, 0.4f);
        }

        /// <summary>바닥 청크(MountainFloor 아래)만 맞혀 그 높이로 올린다.</summary>
        private static Vector3 Snap(Vector3 position, GameObject floor)
        {
            Physics.SyncTransforms();
            var hits = Physics.RaycastAll(new Vector3(position.x, 50f, position.z), Vector3.down, 100f);
            float best = float.NegativeInfinity;
            foreach (var hit in hits)
                if (floor != null && hit.collider.transform.IsChildOf(floor.transform) && hit.point.y > best) best = hit.point.y;
            if (float.IsNegativeInfinity(best))
            {
                Debug.LogWarning($"[뒷산 채집] 바닥을 못 찾음 {position} (청크에 Mesh Collider가 있는지 확인)");
                return position;
            }
            return new Vector3(position.x, best, position.z);
        }

        /// <summary>뒷산 높이장 생성기(hf.py)의 길 중심선 x (z 0~52 직선 구간).</summary>
        internal static float PathCenterX(float z)
        {
            float env = Smooth(z, 0f, 6f) * (1f - Smooth(z, 42f, 52f));
            float wob = 2.0f * Mathf.Sin(2f * Mathf.PI * z / 26f + 0.8f) + 0.7f * Mathf.Sin(2f * Mathf.PI * z / 11f + 2.0f);
            return 8f + env * wob;
        }

        private static float Smooth(float t, float a, float b)
        {
            t = Mathf.Clamp01((t - a) / (b - a));
            return t * t * (3f - 2f * t);
        }

        // ---------- 도우미 ----------
        private static void Shape(PrimitiveType type, Transform parent, string name, Vector3 position, Vector3 scale, float yaw, Material material, bool keepCollider)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            go.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            go.GetComponent<MeshRenderer>().sharedMaterial = material;
            if (!keepCollider) Object.DestroyImmediate(go.GetComponent<Collider>());
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

        private static Mesh BuiltinMesh(string name) => Resources.GetBuiltinResource<Mesh>(name + ".fbx");

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        private static MaterialSpec M(string id, string name, string description, string hint, GatherHabitatKind habitat,
            bool afterSchool, bool night, string mesh, Vector3 scale, Color color)
            => new MaterialSpec { Id = id, Name = name, Description = description, Hint = hint, Habitat = habitat,
                AfterSchool = afterSchool, Night = night, Mesh = mesh, Scale = scale, Color = color };

        private static Placement P(GatherHabitatKind kind, float z, int side) => new Placement { Kind = kind, Z = z, Side = side };
    }
}
