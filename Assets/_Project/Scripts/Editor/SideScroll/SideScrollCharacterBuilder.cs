// 기본 도형(큐브)으로 남학생 교복 치비 캐릭터 프리팹을 만든다. 로직 루트와 모델(Model) 자식을 분리하고,
// 팔·다리는 관절 피벗 아래에 붙인다. 프리팹이 있으면 메뉴로 다시 만들 때만 덮어쓴다 (크기·색은 프리팹에서 직접 수정).
using UnityEditor;
using UnityEngine;
using VoxelMagic.Core;
using VoxelMagic.Gameplay;

namespace VoxelMagic.EditorTools
{
    public static class SideScrollCharacterBuilder
    {
        public const string PrefabPath = "Assets/_Project/Prefabs/SideScroll/SideScrollCharacter.prefab";
        private const string MaterialFolder = "Assets/_Project/Materials/SideScroll";

        // 애니메이션 클립이 쓰는 경로 (Animator가 붙은 Model 기준)
        public const string BodyPath = "Body";
        public static readonly string[] ArmPaths = { "Body/ArmL_Pivot", "Body/ArmR_Pivot" };
        public static readonly string[] LegPaths = { "Hips/LegL_Pivot", "Hips/LegR_Pivot" };

        // 비율 (유닛, 발바닥 y=0, 앞 = 로컬 +Z, 키 약 1.6)
        private const float HipHeight = 0.46f;
        private static readonly Vector3 LegSize = new Vector3(0.17f, 0.40f, 0.20f);
        private static readonly Vector3 ShoeSize = new Vector3(0.18f, 0.06f, 0.26f);
        private const float LegOffsetX = 0.11f;
        private static readonly Vector3 TorsoSize = new Vector3(0.44f, 0.50f, 0.28f);
        private static readonly Vector3 ShirtSize = new Vector3(0.16f, 0.30f, 0.02f);
        private static readonly Vector3 TieSize = new Vector3(0.06f, 0.26f, 0.03f);
        private static readonly Vector3 ArmSize = new Vector3(0.13f, 0.40f, 0.15f);
        private static readonly Vector3 HandSize = new Vector3(0.13f, 0.11f, 0.13f);
        private const float ShoulderY = 0.46f;
        private const float NeckY = 0.52f;
        private const float HeadSize = 0.56f;
        private static readonly Vector3 HairTopSize = new Vector3(0.60f, 0.08f, 0.60f);
        private static readonly Vector3 HairBackSize = new Vector3(0.60f, 0.40f, 0.08f);
        private static readonly Vector3 EyeSize = new Vector3(0.02f, 0.10f, 0.08f);

        private static readonly Color Skin = new Color(1f, 0.82f, 0.66f);
        private static readonly Color HairColor = new Color(0.08f, 0.07f, 0.07f);
        private static readonly Color Navy = new Color(0.12f, 0.16f, 0.36f);
        private static readonly Color Shirt = new Color(0.96f, 0.96f, 0.96f);
        private static readonly Color TieColor = new Color(0.55f, 0.08f, 0.1f);
        private static readonly Color Slacks = new Color(0.45f, 0.46f, 0.5f);

        [MenuItem("VoxelMagic/복도/캐릭터 프리팹 다시 만들기")]
        private static void BuildFromMenu()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null &&
                !EditorUtility.DisplayDialog("캐릭터 프리팹 다시 만들기", "프리팹에서 직접 고친 크기·색이 사라집니다. 계속할까요?", "덮어쓰기", "취소"))
                return;
            Build(CorridorSceneBuilder.LoadOrCreateAnimationConfig());
        }

        public static GameObject GetOrCreate(CharacterAnimationConfig animationConfig)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            return existing != null ? existing : Build(animationConfig);
        }

        private static GameObject Build(CharacterAnimationConfig animationConfig)
        {
            EnsureFolder("Assets/_Project/Prefabs", "SideScroll");
            EnsureFolder("Assets/_Project/Materials", "SideScroll");
            var skin = Mat("Skin", Skin);
            var hair = Mat("Hair", HairColor);
            var navy = Mat("Blazer", Navy);
            var shirt = Mat("Shirt", Shirt);
            var tie = Mat("Tie", TieColor);
            var slacks = Mat("Slacks", Slacks);

            var root = new GameObject("SideScrollCharacter");
            var model = Node("Model", root.transform, Vector3.zero);
            var animator = model.gameObject.AddComponent<Animator>();
            animator.applyRootMotion = false;

            // 다리 (골반 피벗 아래)
            var hips = Node("Hips", model, new Vector3(0f, HipHeight, 0f));
            for (int i = 0; i < 2; i++)
            {
                float side = i == 0 ? -1f : 1f;
                var pivot = Node(i == 0 ? "LegL_Pivot" : "LegR_Pivot", hips, new Vector3(side * LegOffsetX, 0f, 0f));
                Box("Leg", pivot, new Vector3(0f, -LegSize.y * 0.5f, 0f), LegSize, slacks);
                Box("Shoe", pivot, new Vector3(0f, -HipHeight + ShoeSize.y * 0.5f, 0.03f), ShoeSize, shirt);
            }

            // 몸통 (숨쉬기·기울기는 Body 피벗만 움직임)
            var body = Node("Body", model, new Vector3(0f, HipHeight, 0f));
            Box("Torso", body, new Vector3(0f, TorsoSize.y * 0.5f, 0f), TorsoSize, navy);
            Box("Shirt", body, new Vector3(0f, 0.33f, TorsoSize.z * 0.5f + ShirtSize.z * 0.5f), ShirtSize, shirt);
            Box("Tie", body, new Vector3(0f, 0.31f, TorsoSize.z * 0.5f + ShirtSize.z + TieSize.z * 0.5f), TieSize, tie);

            for (int i = 0; i < 2; i++)
            {
                float side = i == 0 ? -1f : 1f;
                float x = side * (TorsoSize.x * 0.5f + ArmSize.x * 0.5f + 0.005f);
                var pivot = Node(i == 0 ? "ArmL_Pivot" : "ArmR_Pivot", body, new Vector3(x, ShoulderY, 0f));
                Box("Arm", pivot, new Vector3(0f, -ArmSize.y * 0.5f, 0f), ArmSize, navy);
                Box("Hand", pivot, new Vector3(0f, -ArmSize.y - HandSize.y * 0.5f + 0.01f, 0f), HandSize, skin);
            }

            // 머리 (몸보다 크게). 눈은 옆모습에서 보이도록 머리 양 옆면 앞쪽에 붙인다.
            var head = Node("Head_Pivot", body, new Vector3(0f, NeckY, 0f));
            Box("Head", head, new Vector3(0f, HeadSize * 0.5f, 0f), Vector3.one * HeadSize, skin);
            Box("HairTop", head, new Vector3(0f, HeadSize + HairTopSize.y * 0.5f - 0.02f, 0f), HairTopSize, hair);
            Box("HairBack", head, new Vector3(0f, HeadSize - HairBackSize.y * 0.5f, -HeadSize * 0.5f), HairBackSize, hair);
            for (int i = 0; i < 2; i++)
            {
                float side = i == 0 ? -1f : 1f;
                Box(i == 0 ? "EyeL" : "EyeR", head,
                    new Vector3(side * (HeadSize * 0.5f + EyeSize.x * 0.5f), HeadSize * 0.52f, HeadSize * 0.22f), EyeSize, hair);
            }

            // 충돌체는 루트에 하나만
            var capsule = root.AddComponent<CapsuleCollider>();
            float height = HipHeight + NeckY + HeadSize + HairTopSize.y;
            capsule.height = height;
            capsule.center = new Vector3(0f, height * 0.5f, 0f);
            capsule.radius = TorsoSize.x * 0.5f + ArmSize.x;

            root.AddComponent<SideScrollCharacter>();
            var view = root.AddComponent<CharacterAnimatorView>();
            view.SetRef("_animator", animator).SetRef("_config", animationConfig);

            animator.runtimeAnimatorController = CharacterAnimationBuilder.Build(animationConfig, HipHeight);

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            Debug.Log($"[SideScrollCharacterBuilder] {PrefabPath} 생성 완료 (키 {height:0.##})");
            return prefab;
        }

        private static Transform Node(string name, Transform parent, Vector3 localPosition)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            return go.transform;
        }

        private static void Box(string name, Transform parent, Vector3 localPosition, Vector3 size, Material material)
        {
            var box = PrototypeSceneBuilder.Prim(PrimitiveType.Cube, name, parent, localPosition, size, material);
            var renderer = box.GetComponent<Renderer>();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        private static Material Mat(string name, Color color)
        {
            string path = $"{MaterialFolder}/M_SS_{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = $"M_SS_{name}" };
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", 0.1f);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void EnsureFolder(string parent, string name)
        {
            if (!AssetDatabase.IsValidFolder($"{parent}/{name}")) AssetDatabase.CreateFolder(parent, name);
        }
    }
}
