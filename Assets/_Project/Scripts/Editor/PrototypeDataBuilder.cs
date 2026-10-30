// 프로토타입 예시 데이터(아이템, 씨앗, 물고기, 의뢰) ScriptableObject를 만든다. 이미 있는 에셋은 건드리지 않는다.
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.EditorTools
{
    public static class PrototypeDataBuilder
    {
        private const string Root = "Assets/_Project/Data";

        public sealed class Result
        {
            public ToolDefinition Wand;
            public List<RequestDefinition> Requests = new List<RequestDefinition>();
        }

        [MenuItem("VoxelMagic/예시 데이터 만들기")]
        private static void CreateFromMenu() => CreateAll();

        public static Result CreateAll()
        {
            var wand = Item<ToolDefinition>("Items/Tool_Wand", "wand", "지팡이", "물도 주고 낚싯줄도 내리는 만능 지팡이.",
                new Color(0.55f, 0.35f, 0.2f), 1,
                "지팡이로 물도 주고 낚시도 한다고? …그게 원래 용도야. 주문은 덤이지.");

            var radish = Produce("Radish", "radish", "무", "하얗고 아삭한 무.", Color.white, 15f, 30f,
                "무는 마력을 흡수해… 그래서 네 불 주문에서 무가 나온 거야.");
            var onion = Produce("GreenOnion", "green_onion", "대파", "길쭉하고 향이 센 대파.", new Color(0.4f, 0.8f, 0.35f), 40f, 70f,
                "대파는 마력 전도율이 높아. 지팡이 대신 쓰는 놈도 있었지. 냄새 때문에 퇴학당했지만.");
            var pumpkin = Produce("Pumpkin", "pumpkin", "호박", "둥글고 묵직한 호박.", new Color(1f, 0.55f, 0.1f), 20f, 45f,
                "호박은 변신 주문 기초 재료야. 마차가 되는 건 동화고, 보통은 더 큰 호박이 돼.");

            var radishSeed = Seed("Radish", "radish_seed", "무 씨앗", radish, new Vector2Int(1, 1));
            var onionSeed = Seed("GreenOnion", "green_onion_seed", "대파 씨앗", onion, new Vector2Int(1, 2));
            var pumpkinSeed = Seed("Pumpkin", "pumpkin_seed", "호박 씨앗", pumpkin, new Vector2Int(2, 2));
            AssignCropVisuals(radishSeed, onionSeed, pumpkinSeed);

            var crucian = Fish("Crucian", "crucian", "붕어", "연못 어디에나 있는 평범한 붕어.", new Color(0.75f, 0.65f, 0.35f),
                10f, 25f, 0.45f, 1.2f, 2.2f, 0f, 2f,
                "붕어는 기억력이 3초라서 저주가 안 걸려. 부러운 녀석.");
            var catfish = Fish("Catfish", "catfish", "메기", "수염이 마력을 감지한다는 소문이 있다.", new Color(0.35f, 0.3f, 0.25f),
                30f, 60f, 0.7f, 0.7f, 1.5f, 0.15f, 3f,
                "메기 수염은 마력을 감지해. 시험 감독관들이 몰래 기른다는 소문이 있지.");
            var thunder = Fish("ThunderCrucian", "thunder_crucian", "번개 붕어", "너무 빨라서 아무도 못 잡는다는 붕어.", new Color(1f, 0.95f, 0.3f),
                8f, 15f, 1.3f, 0.25f, 0.7f, 0.3f, 3f,
                "번개 붕어를 잡았다고? …거짓말이지? 나도 못 잡았는데.");

            var result = new Result { Wand = wand };
            result.Requests.Add(Request("HungryIt", "배고픈 '그것'의 부탁",
                "배고파… 하얗고 아삭한 거. 아니, 너 말고.",
                new[] { (ItemDefinition)radish }, new[] { 3 },
                new[] { (ItemDefinition)radishSeed }, new[] { 3 }, 10,
                "아삭아삭… 좋아, 오늘 수업은 무다."));
            result.Requests.Add(Request("Broth", "국물 재료가 필요해",
                "국물엔 대파랑 붕어지. 이건 상식이야. 마법 상식.",
                new ItemDefinition[] { onion, crucian }, new[] { 2, 1 },
                new[] { (ItemDefinition)onionSeed }, new[] { 2 }, 15,
                "후루룩… 국물에서 시험 문제 맛이 나."));
            result.Requests.Add(Request("PumpkinPorridge", "호박죽이 먹고 싶어",
                "호박죽에 메기를 넣으면 맛있대. 누가? 내가.",
                new ItemDefinition[] { pumpkin, catfish }, new[] { 1, 1 },
                new[] { (ItemDefinition)pumpkinSeed }, new[] { 1 }, 20,
                "…생각보다 별로네. 그래도 공부는 공부다."));

            FillPondFish(new[] { crucian, catfish, thunder }, new[] { 60f, 30f, 10f });
            AssetDatabase.SaveAssets();
            return result;
        }

        // ── 종류별 생성 ──────────────────────────────────────

        private static ProduceDefinition Produce(string key, string id, string name, string description, Color color,
            float minCm, float maxCm, string note)
        {
            var asset = Item<ProduceDefinition>($"Items/Produce_{key}", id, name, description, color, 99, note, out bool created);
            if (created) Edit(asset, so => SetRange(so, "_sizeRangeCm", minCm, maxCm));
            return asset;
        }

        private static SeedDefinition Seed(string key, string id, string name, ProduceDefinition produce, Vector2Int footprint)
        {
            var asset = Item<SeedDefinition>($"Seeds/Seed_{key}", id, name,
                $"{produce.DisplayName}이(가) 되는 씨앗. {footprint.x}×{footprint.y}칸을 차지한다.",
                new Color(0.75f, 0.6f, 0.4f), 20, null, out bool created);
            if (created)
                Edit(asset, so =>
                {
                    so.FindProperty("_produce").objectReferenceValue = produce;
                    so.FindProperty("_footprint").vector2IntValue = footprint;
                });
            return asset;
        }

        private static FishDefinition Fish(string key, string id, string name, string description, Color color,
            float minCm, float maxCm, float speed, float retargetMin, float retargetMax, float dartChance, float dartMultiplier,
            string note)
        {
            var asset = Item<FishDefinition>($"Fish/Fish_{key}", id, name, description, color, 99, note, out bool created);
            if (created)
                Edit(asset, so =>
                {
                    SetRange(so, "_sizeRangeCm", minCm, maxCm);
                    so.FindProperty("_moveSpeed").floatValue = speed;
                    SetRange(so, "_retargetIntervalSeconds", retargetMin, retargetMax);
                    so.FindProperty("_dartChance").floatValue = dartChance;
                    so.FindProperty("_dartSpeedMultiplier").floatValue = dartMultiplier;
                });
            return asset;
        }

        private static RequestDefinition Request(string key, string title, string line,
            ItemDefinition[] needs, int[] needAmounts, ItemDefinition[] given, int[] givenAmounts, int reward, string completion)
        {
            string path = $"{Root}/Requests/Request_{key}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<RequestDefinition>(path);
            if (asset != null) return asset;

            asset = ScriptableObject.CreateInstance<RequestDefinition>();
            AssetDatabase.CreateAsset(asset, path);
            Edit(asset, so =>
            {
                so.FindProperty("_title").stringValue = title;
                so.FindProperty("_creatureLine").stringValue = line;
                SetAmounts(so.FindProperty("_requirements"), needs, needAmounts);
                SetAmounts(so.FindProperty("_givenItems"), given, givenAmounts);
                so.FindProperty("_baseReward").intValue = reward;
                so.FindProperty("_completionLine").stringValue = completion;
            });
            return asset;
        }

        private static void FillPondFish(FishDefinition[] fish, float[] weights)
        {
            var config = AssetDatabase.LoadAssetAtPath<FishingConfig>($"{Root}/Config/FishingConfig.asset");
            if (config == null || config.PondFish.Count > 0) return;
            Edit(config, so =>
            {
                var list = so.FindProperty("_pondFish");
                list.arraySize = fish.Length;
                for (int i = 0; i < fish.Length; i++)
                {
                    var element = list.GetArrayElementAtIndex(i);
                    element.FindPropertyRelative("_fish").objectReferenceValue = fish[i];
                    element.FindPropertyRelative("_weight").floatValue = weights[i];
                }
            });
        }

        // ── 작물 모양 프리팹 (기본 도형) ─────────────────────

        private const string CropFolder = "Assets/_Project/Prefabs/Crops";

        private static void AssignCropVisuals(SeedDefinition radish, SeedDefinition onion, SeedDefinition pumpkin)
        {
            if (!AssetDatabase.IsValidFolder(CropFolder)) AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "Crops");

            var sprout = CropPrefab("Crop_Sprout", root =>
            {
                var green = PrototypeSceneBuilder.Mat("Sprout", new Color(0.45f, 0.85f, 0.35f));
                PrototypeSceneBuilder.Prim(PrimitiveType.Cube, "Stem", root, new Vector3(0f, 0.25f, 0f), new Vector3(0.1f, 0.3f, 0.1f), green);
                PrototypeSceneBuilder.Prim(PrimitiveType.Cube, "Leaf", root, new Vector3(0f, 0.4f, 0f), new Vector3(0.45f, 0.08f, 0.2f), green);
            });
            var leaf = PrototypeSceneBuilder.Mat("CropLeaf", new Color(0.3f, 0.7f, 0.25f));

            var radishGrown = CropPrefab("Crop_Radish", root =>
            {
                PrototypeSceneBuilder.Prim(PrimitiveType.Cylinder, "Root", root, new Vector3(0f, 0.3f, 0f), new Vector3(0.5f, 0.25f, 0.5f),
                    PrototypeSceneBuilder.Mat("Radish", new Color(0.97f, 0.97f, 0.95f)));
                PrototypeSceneBuilder.Prim(PrimitiveType.Cube, "Leaves", root, new Vector3(0f, 0.75f, 0f), new Vector3(0.45f, 0.4f, 0.45f), leaf);
            });
            var onionGrown = CropPrefab("Crop_GreenOnion", root =>
            {
                var white = PrototypeSceneBuilder.Mat("OnionWhite", new Color(0.95f, 0.97f, 0.9f));
                var green = PrototypeSceneBuilder.Mat("OnionGreen", new Color(0.35f, 0.75f, 0.3f));
                foreach (float z in new[] { -0.5f, 0.5f })
                {
                    PrototypeSceneBuilder.Prim(PrimitiveType.Cylinder, "White", root, new Vector3(0f, 0.35f, z), new Vector3(0.28f, 0.25f, 0.28f), white);
                    PrototypeSceneBuilder.Prim(PrimitiveType.Cylinder, "Green", root, new Vector3(0f, 1.05f, z), new Vector3(0.24f, 0.5f, 0.24f), green);
                }
            });
            var pumpkinGrown = CropPrefab("Crop_Pumpkin", root =>
            {
                PrototypeSceneBuilder.Prim(PrimitiveType.Sphere, "Body", root, new Vector3(0f, 0.6f, 0f), new Vector3(1.3f, 1f, 1.3f),
                    PrototypeSceneBuilder.Mat("Pumpkin", new Color(1f, 0.55f, 0.1f)));
                PrototypeSceneBuilder.Prim(PrimitiveType.Cube, "Stem", root, new Vector3(0f, 1.2f, 0f), new Vector3(0.12f, 0.25f, 0.12f), leaf);
            });

            SetVisualsIfMissing(radish, sprout, radishGrown);
            SetVisualsIfMissing(onion, sprout, onionGrown);
            SetVisualsIfMissing(pumpkin, sprout, pumpkinGrown);
        }

        private static GameObject CropPrefab(string name, System.Action<Transform> build)
        {
            string path = $"{CropFolder}/{name}.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var root = new GameObject(name);
            build(root.transform);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static void SetVisualsIfMissing(SeedDefinition seed, GameObject sprout, GameObject grown)
        {
            if (seed.SproutPrefab != null && seed.GrownPrefab != null) return;
            Edit(seed, so =>
            {
                so.FindProperty("_sproutPrefab").objectReferenceValue = sprout;
                so.FindProperty("_grownPrefab").objectReferenceValue = grown;
            });
        }

        // ── 도우미 ───────────────────────────────────────────

        private static T Item<T>(string relativePath, string id, string name, string description, Color color, int maxStack,
            string note) where T : ItemDefinition
            => Item<T>(relativePath, id, name, description, color, maxStack, note, out _);

        private static T Item<T>(string relativePath, string id, string name, string description, Color color, int maxStack,
            string note, out bool created) where T : ItemDefinition
        {
            string path = $"{Root}/{relativePath}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            created = asset == null;
            if (!created) return asset;

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            Edit(asset, so =>
            {
                so.FindProperty("_id").stringValue = id;
                so.FindProperty("_displayName").stringValue = name;
                so.FindProperty("_description").stringValue = description;
                so.FindProperty("_iconColor").colorValue = color;
                so.FindProperty("_maxStack").intValue = maxStack;
                so.FindProperty("_creatureNote").stringValue = note ?? string.Empty;
            });
            return asset;
        }

        private static void Edit(Object asset, System.Action<SerializedObject> change)
        {
            var so = new SerializedObject(asset);
            change(so);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
        }

        private static void SetRange(SerializedObject so, string field, float min, float max)
        {
            var range = so.FindProperty(field);
            range.FindPropertyRelative("_min").floatValue = min;
            range.FindPropertyRelative("_max").floatValue = max;
        }

        private static void SetAmounts(SerializedProperty list, ItemDefinition[] items, int[] amounts)
        {
            list.arraySize = items.Length;
            for (int i = 0; i < items.Length; i++)
            {
                var element = list.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("_item").objectReferenceValue = items[i];
                element.FindPropertyRelative("_amount").intValue = amounts[i];
            }
        }
    }
}
