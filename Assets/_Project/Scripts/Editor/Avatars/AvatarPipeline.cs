// 아바타 파이프라인 메뉴. 폴더 규칙(Art/Characters/<캐릭터>/Model/모델.fbx, Animations/**/애니메이션.fbx)만 지키면
// 1) 모델 준비(Humanoid·키 배율·텍스처·AvatarSetup) 2) 애니메이션 Humanoid 임포트 3) 복도 캐릭터 프리팹 생성·지정까지 처리한다.
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VoxelMagic.Core;
using VoxelMagic.Gameplay;

namespace VoxelMagic.EditorTools
{
    public static class AvatarPipeline
    {
        public const string CharactersRoot = "Assets/_Project/Art/Characters";
        public const string AnimationsRoot = "Assets/_Project/Animations";
        public const string AvatarDataFolder = "Assets/_Project/Data/Avatars";
        private const string ControllerFolder = "Assets/_Project/Animations/Avatars";
        private const string PrefabFolder = "Assets/_Project/Prefabs/SideScroll";
        private const string MenuRoot = "VoxelMagic/아바타/";

        private static readonly CharacterStateId[] States = { CharacterStateId.Idle, CharacterStateId.Walk, CharacterStateId.Run };

        // ── 메뉴 ─────────────────────────────────────────────

        [MenuItem(MenuRoot + "1. 선택한 캐릭터 모델 준비")]
        private static void PrepareSelectedModels()
        {
            foreach (var path in SelectedPaths(p => p.StartsWith(CharactersRoot) && IsModel(p)))
                Selection.activeObject = PrepareModel(path);
        }

        [MenuItem(MenuRoot + "1. 선택한 캐릭터 모델 준비", true)]
        private static bool CanPrepare() => SelectedPaths(p => p.StartsWith(CharactersRoot) && IsModel(p)).Any();

        [MenuItem(MenuRoot + "2. 선택한 애니메이션 Humanoid로 임포트")]
        private static void ImportSelectedAnimations()
        {
            foreach (var path in SelectedAnimationFiles()) ImportAnimationAsHumanoid(path);
        }

        [MenuItem(MenuRoot + "2. 선택한 애니메이션 Humanoid로 임포트", true)]
        private static bool CanImportAnimations() => SelectedAnimationFiles().Any();

        [MenuItem(MenuRoot + "3. 선택한 아바타로 복도 캐릭터 만들기")]
        private static void BuildSelectedCharacter()
        {
            if (!(Selection.activeObject is AvatarSetup setup)) return;
            var prefab = BuildCharacter(setup);
            if (prefab == null) return;
            UseInCorridor(prefab);
            if (EditorUtility.DisplayDialog("복도 씬 다시 만들기", $"복도 캐릭터를 {prefab.name}(으)로 지정했습니다. 복도 씬을 다시 만들까요?", "다시 만들기", "나중에"))
                CorridorSceneBuilder.Build();
        }

        [MenuItem(MenuRoot + "3. 선택한 아바타로 복도 캐릭터 만들기", true)]
        private static bool CanBuildCharacter() => Selection.activeObject is AvatarSetup;

        // ── 1. 모델 준비 ─────────────────────────────────────

        /// <summary>Humanoid로 임포트하고 목표 키에 맞춰 배율을 정한 뒤, 이 모델의 AvatarSetup을 만들거나 갱신한다.</summary>
        public static AvatarSetup PrepareModel(string modelPath)
        {
            var setup = FindSetupFor(modelPath) ?? LoadOrCreateSetup(SetupNameFor(modelPath));
            setup.SetRef("_model", AssetDatabase.LoadAssetAtPath<GameObject>(modelPath));

            foreach (var texture in AssetDatabase.FindAssets("t:Texture2D", new[] { Path.GetDirectoryName(modelPath).Replace('\\', '/') }))
                ApplyTextureSettings(AssetDatabase.GUIDToAssetPath(texture));

            var importer = (ModelImporter)AssetImporter.GetAtPath(modelPath);
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.SaveAndReimport();

            // 현재 배율에서 잰 키로 목표 키 배율을 계산한다 (한 번 더 임포트).
            float height = MeasureHeight(modelPath);
            if (height > 0.0001f)
            {
                importer.globalScale *= setup.TargetHeight / height;
                importer.SaveAndReimport();
            }

            var avatar = LoadAvatar(modelPath);
            float finalHeight = MeasureHeight(modelPath);
            AutoMapClips(setup);
            EditorUtility.SetDirty(setup);
            AssetDatabase.SaveAssets();

            if (avatar == null || !avatar.isValid || !avatar.isHuman)
                Debug.LogError($"[AvatarPipeline] {modelPath}: Humanoid 아바타를 만들지 못했습니다. 임포트 설정의 Rig 탭에서 뼈 매핑을 확인하세요.");
            else
                Debug.Log($"[AvatarPipeline] {setup.name} 준비 완료: 키 {finalHeight:0.###} (배율 {importer.globalScale:0.###}), Humanoid 아바타 OK");
            return setup;
        }

        /// <summary>Art/Characters 아래에서 AvatarSetup이 없거나 Humanoid가 풀린 .fbx를 모두 준비한다 (애니메이션 랩 시작·새로고침 때).
        /// 모델 파일을 다시 복사해 넣으면 .meta가 기본값(Generic)으로 돌아갈 수 있어, 그런 모델도 다시 준비한다.</summary>
        public static List<AvatarSetup> PrepareNewModels()
        {
            var models = AssetDatabase.FindAssets("t:Model", new[] { CharactersRoot })
                .Select(AssetDatabase.GUIDToAssetPath).Where(IsModel).Distinct().OrderBy(p => p);
            return models.Where(NeedsPreparation).ToList().Select(PrepareModel).ToList();
        }

        private static bool NeedsPreparation(string modelPath) =>
            FindSetupFor(modelPath) == null ||
            !(AssetImporter.GetAtPath(modelPath) is ModelImporter importer) ||
            importer.animationType != ModelImporterAnimationType.Human;

        // ── 2. 애니메이션 임포트 ─────────────────────────────

        /// <summary>Humanoid, 반복 재생, 루트 회전·높이·앞뒤 이동을 포즈에 굽기. 클립 이름은 파일 이름의 '@' 뒤.</summary>
        public static bool ImportAnimationAsHumanoid(string path)
        {
            if (!(AssetImporter.GetAtPath(path) is ModelImporter importer)) return false;
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.importAnimation = true;

            string fileName = Path.GetFileNameWithoutExtension(path);
            string clipName = fileName.Contains("@") ? fileName.Substring(fileName.IndexOf('@') + 1) : fileName;
            var clips = importer.defaultClipAnimations;
            for (int i = 0; i < clips.Length; i++)
            {
                var clip = clips[i];
                clip.name = clips.Length == 1 ? clipName : $"{clipName}_{i}";
                clip.loopTime = true;
                clip.loopPose = true;
                clip.lockRootRotation = true;
                clip.lockRootHeightY = true;
                clip.lockRootPositionXZ = true;
                clip.keepOriginalOrientation = true;
                clip.keepOriginalPositionY = true;
                clip.keepOriginalPositionXZ = true;
            }
            importer.clipAnimations = clips;
            importer.SaveAndReimport();

            var avatar = LoadAvatar(path);
            bool ok = avatar != null && avatar.isHuman;
            if (ok) Debug.Log($"[AvatarPipeline] {path}: Humanoid 클립 {clips.Length}개 ({clipName})");
            else Debug.LogError($"[AvatarPipeline] {path}: Humanoid로 임포트하지 못했습니다 (사람 뼈대가 아닐 수 있음)");
            return ok;
        }

        // ── 3. 복도 캐릭터 ───────────────────────────────────

        /// <summary>공용 Controller의 Override(이 아바타 클립)와, 큐브와 같은 로직 루트에 Model만 다른 프리팹을 만든다.</summary>
        public static GameObject BuildCharacter(AvatarSetup setup)
        {
            if (setup.Model == null || LoadAvatar(AssetDatabase.GetAssetPath(setup.Model)) == null)
            {
                Debug.LogError($"[AvatarPipeline] {setup.name}: 모델이 없거나 Humanoid가 아닙니다. 먼저 메뉴 1을 실행하세요.");
                return null;
            }
            var missing = States.Where(s => setup.ClipFor(s) == null).ToArray();
            if (missing.Length > 0)
                Debug.LogWarning($"[AvatarPipeline] {setup.name}: 클립이 비어 있는 상태 {string.Join(", ", missing)} (AvatarSetup에서 연결하세요)");

            EnsureFolder(ControllerFolder);
            var controller = CharacterControllerBuilder.BuildOverride(setup, CharacterControllerBuilder.LoadOrBuildBase());

            string modelPath = AssetDatabase.GetAssetPath(setup.Model);
            var root = new GameObject($"SideScrollCharacter_{setup.name}");
            var model = (GameObject)PrefabUtility.InstantiatePrefab(setup.Model);
            model.name = "Model";
            model.transform.SetParent(root.transform, false);
            foreach (var renderer in model.GetComponentsInChildren<Renderer>())
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            // Humanoid Avatar는 FBX 루트 기준으로 뼈를 찾으므로 Animator는 FBX 인스턴스(Model)에 둔다.
            var animator = model.GetComponent<Animator>();
            if (animator == null) animator = model.AddComponent<Animator>();
            animator.avatar = LoadAvatar(modelPath);
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            model.AddComponent<AvatarAnimationSpeed>().SetRef("_setup", setup).SetRef("_animator", animator);

            var capsule = root.AddComponent<CapsuleCollider>();
            capsule.height = setup.TargetHeight;
            capsule.radius = setup.ColliderRadius;
            capsule.center = new Vector3(0f, setup.TargetHeight * 0.5f, 0f);

            root.AddComponent<SideScrollCharacter>();
            root.AddComponent<CharacterAnimatorView>()
                .SetRef("_animator", animator)
                .SetRef("_config", CorridorSceneBuilder.LoadOrCreateAnimationConfig());
            AddSprite2D(root, model, setup.Sprites);

            EnsureFolder(PrefabFolder);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, $"{PrefabFolder}/{root.name}.prefab");
            Object.DestroyImmediate(root);

            setup.SetRef("_controller", controller).SetRef("_characterPrefab", prefab);
            AssetDatabase.SaveAssets();
            Debug.Log($"[AvatarPipeline] {prefab.name} 생성 완료");
            return prefab;
        }

        // 2D 버전이 있으면 Sprite2D 자식(스프라이트 + 프레임 재생)을 만들고, 루트에 2D/3D 전환을 붙인다. 시작 모드는 CorridorConfig.
        private static void AddSprite2D(GameObject root, GameObject model, SpriteCharacterSet sprites)
        {
            GameObject sprite = null;
            if (sprites != null)
            {
                var config = CorridorSceneBuilder.LoadOrCreateConfig();
                sprite = new GameObject("Sprite2D");
                sprite.transform.SetParent(root.transform, false);
                var renderer = sprite.AddComponent<SpriteRenderer>();
                renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(CorridorSceneBuilder.SpriteMaterialPath);
                renderer.sortingOrder = config.CharacterSpriteSortingOrder;
                renderer.sprite = sprites.ClipFor(CharacterStateId.Idle).IsEmpty ? null : sprites.ClipFor(CharacterStateId.Idle).Frames[0];
                sprite.AddComponent<SpriteFrameAnimator>().SetRef("_renderer", renderer).SetRef("_set", sprites);
            }
            root.AddComponent<CharacterViewSwitcher>().SetRef("_model", model).SetRef("_sprite", sprite);
        }

        public static void UseInCorridor(GameObject prefab)
        {
            CorridorSceneBuilder.LoadOrCreateConfig().SetRef("_characterPrefab", prefab);
            AssetDatabase.SaveAssets();
        }

        // ── 검색 (애니메이션 랩도 사용) ──────────────────────

        public static List<(string path, AnimationClip clip, bool isHuman)> ScanClips()
        {
            var result = new List<(string, AnimationClip, bool)>();
            var paths = AssetDatabase.FindAssets("t:AnimationClip", new[] { AnimationsRoot })
                .Select(AssetDatabase.GUIDToAssetPath).Distinct().OrderBy(p => p);
            foreach (var path in paths)
            {
                bool human = AssetImporter.GetAtPath(path) is ModelImporter importer &&
                             importer.animationType == ModelImporterAnimationType.Human;
                foreach (var clip in AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>())
                {
                    if (clip.name.StartsWith("__preview__")) continue;
                    result.Add((path, clip, human || clip.isHumanMotion));
                }
            }
            return result;
        }

        public static List<AvatarSetup> ScanAvatars() =>
            AssetDatabase.FindAssets("t:AvatarSetup", new[] { AvatarDataFolder })
                .Select(g => AssetDatabase.LoadAssetAtPath<AvatarSetup>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(s => s != null).OrderBy(s => s.name).ToList();

        // ── 도우미 ───────────────────────────────────────────

        private static void AutoMapClips(AvatarSetup setup)
        {
            var human = ScanClips().Where(c => c.isHuman).Select(c => c.clip).ToList();
            // 이름에 키워드가 든 클립 중 이름이 가장 짧은 것 ("Idle"이 "Fishing Idle"보다 먼저).
            AnimationClip Find(params string[] keys) =>
                keys.Select(k => human.Where(c => c.name.ToLowerInvariant().Contains(k)).OrderBy(c => c.name.Length).FirstOrDefault())
                    .FirstOrDefault(c => c != null);

            if (setup.ClipFor(CharacterStateId.Idle) == null) setup.SetRef("_idle", Find("idle"));
            if (setup.ClipFor(CharacterStateId.Walk) == null) setup.SetRef("_walk", Find("walk"));
            if (setup.ClipFor(CharacterStateId.Run) == null) setup.SetRef("_run", Find("run", "sprint", "jog"));
        }

        private static float MeasureHeight(string modelPath)
        {
            var instance = (GameObject)Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(modelPath));
            float min = float.MaxValue, max = float.MinValue;
            var mesh = new Mesh();
            foreach (var skinned in instance.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                skinned.BakeMesh(mesh, true);
                foreach (var v in mesh.vertices) Track(skinned.transform.TransformPoint(v).y);
            }
            foreach (var filter in instance.GetComponentsInChildren<MeshFilter>())
                foreach (var v in filter.sharedMesh.vertices) Track(filter.transform.TransformPoint(v).y);
            Object.DestroyImmediate(mesh);
            Object.DestroyImmediate(instance);
            return max > min ? max - min : 0f;

            void Track(float y)
            {
                min = Mathf.Min(min, y);
                max = Mathf.Max(max, y);
            }
        }

        private static void ApplyTextureSettings(string path)
        {
            if (!(AssetImporter.GetAtPath(path) is TextureImporter importer)) return;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }

        private static Avatar LoadAvatar(string path) =>
            AssetDatabase.LoadAllAssetsAtPath(path).OfType<Avatar>().FirstOrDefault();

        // 이미 이 모델을 쓰는 AvatarSetup이 있으면 그대로 갱신한다 (버전을 바꿔 넣어도 기존 데이터를 덮어쓰지 않게).
        private static AvatarSetup FindSetupFor(string modelPath)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            return ScanAvatars().FirstOrDefault(s => s.Model == model);
        }

        // 캐릭터 폴더(Art/Characters 바로 아래, 예: Student_Male)에 모델이 하나면 캐릭터 폴더 이름,
        // 여러 개(버전 비교 등)면 파일 이름을 아바타 이름으로 쓴다. 모델이 Model/ 같은 하위 폴더에 있어도 같다.
        private static string SetupNameFor(string modelPath)
        {
            string relative = modelPath.Substring(CharactersRoot.Length + 1);
            string character = relative.Split('/')[0];
            string characterFolder = $"{CharactersRoot}/{character}";
            int models = AssetDatabase.FindAssets("t:Model", new[] { characterFolder })
                .Count(g => IsModel(AssetDatabase.GUIDToAssetPath(g)));
            return models > 1 ? Path.GetFileNameWithoutExtension(modelPath) : character;
        }

        private static AvatarSetup LoadOrCreateSetup(string name)
        {
            EnsureFolder(AvatarDataFolder);
            string path = $"{AvatarDataFolder}/{name}.asset";
            var setup = AssetDatabase.LoadAssetAtPath<AvatarSetup>(path);
            if (setup != null) return setup;
            setup = ScriptableObject.CreateInstance<AvatarSetup>();
            AssetDatabase.CreateAsset(setup, path);
            return setup;
        }

        private static bool IsModel(string path) => path.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase);

        private static IEnumerable<string> SelectedPaths(System.Func<string, bool> filter) =>
            Selection.objects.Select(AssetDatabase.GetAssetPath).Where(p => !string.IsNullOrEmpty(p) && filter(p));

        // 폴더를 선택하면 안의 .fbx를 모두 포함한다.
        private static IEnumerable<string> SelectedAnimationFiles()
        {
            foreach (var path in SelectedPaths(p => p.StartsWith(AnimationsRoot)))
            {
                if (AssetDatabase.IsValidFolder(path))
                {
                    foreach (var guid in AssetDatabase.FindAssets("t:Model", new[] { path }))
                        yield return AssetDatabase.GUIDToAssetPath(guid);
                }
                else if (IsModel(path)) yield return path;
            }
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
