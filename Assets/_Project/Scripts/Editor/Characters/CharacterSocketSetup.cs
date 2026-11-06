// 캐릭터 소켓·장착 에디터 도구: 기본 데이터(오른손 소켓·칩비 소켓 프로필·임시 지팡이 프리팹)를 없을 때만 만들고,
// 지금 열린 씬의 플레이어에 소켓·장착 부품을 붙인다. 씬을 새로 만들지 않고, 기존 Wand는 끄기만 하며, 저장은 사용자가 한다.
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VoxelMagic.Core;
using VoxelMagic.Gameplay;

namespace VoxelMagic.EditorTools
{
    public static class CharacterSocketSetup
    {
        private const string MenuRoot = "VoxelMagic/캐릭터/";
        public const string RightHandSocketPath = "Assets/_Project/Data/Characters/Sockets/Socket_RightHand.asset";
        public const string ChibiProfilePath = "Assets/_Project/Data/Characters/SocketProfiles/SocketProfile_Chibi.asset";
        public const string WandAttachmentPath = "Assets/_Project/Data/Characters/Attachments/Attachment_WandTemp.asset";
        public const string WandPrefabPath = "Assets/_Project/Prefabs/Attachments/Wand_Temp.prefab";
        private const string ChibiAvatarPath = "Assets/_Project/Data/Avatars/Student_Chibi_v2.asset";
        private const string WandItemPath = "Assets/_Project/Data/Items/Tool_Wand.asset";
        private const string WandMaterialPath = "Assets/_Project/Materials/M_Wand.mat";

        // 지금 몸에 붙은 Wand와 같은 상자 (PrototypeSceneBuilder.BuildPlayer와 같은 값)
        private static readonly Vector3 WandSize = new Vector3(0.05f, 0.05f, 0.58f);

        [MenuItem(MenuRoot + "기본 소켓·지팡이 데이터 만들기 (없는 것만)")]
        public static void EnsureDefaultAssetsMenu()
        {
            EnsureDefaultAssets();
            Debug.Log("[캐릭터] 기본 소켓·지팡이 데이터 확인 완료");
        }

        [MenuItem(MenuRoot + "농장 플레이어에 소켓·장착 붙이기")]
        public static void AttachToFarmPlayer()
        {
            EnsureDefaultAssets();

            var interactor = Object.FindFirstObjectByType<PlayerInteractor>(FindObjectsInactive.Include);
            if (interactor == null)
            {
                EditorUtility.DisplayDialog("소켓·장착 붙이기", "지금 열린 씬에서 플레이어(PlayerInteractor)를 찾지 못했어요. 농장 씬을 열고 다시 실행해 주세요.", "확인");
                return;
            }
            var fishing = Object.FindFirstObjectByType<FishingDriver>(FindObjectsInactive.Include);
            if (!AttachTo(interactor.gameObject, fishing))
            {
                EditorUtility.DisplayDialog("소켓·장착 붙이기", "플레이어 아래에서 아바타 Animator를 찾지 못했어요.", "확인");
                return;
            }
            EditorSceneManager.MarkSceneDirty(interactor.gameObject.scene);
        }

        /// <summary>플레이어에 소켓·장착·인벤토리 연결 부품을 붙이고 낚싯줄 시작점을 잇는다. 기존 Wand는 끄기만 한다. 여러 번 실행해도 안전.</summary>
        public static bool AttachTo(GameObject player, FishingDriver fishing)
        {
            var animatorView = player.GetComponent<PlayerAvatarAnimator>();
            var animator = animatorView != null
                ? new SerializedObject(animatorView).FindProperty("_animator").objectReferenceValue as Animator
                : player.GetComponentInChildren<Animator>(true);
            if (animator == null) return false;

            var profile = FindProfileFor(animator.gameObject);
            var source = GetOrAdd<HumanoidSocketSource>(animator.gameObject);
            var host = GetOrAdd<SocketHost>(player);
            host.SetRef("_profile", profile).SetRef("_source", source);
            var equipment = GetOrAdd<CharacterEquipment>(player);
            equipment.SetRef("_sockets", host);
            GetOrAdd<InventoryEquipmentLink>(player).SetRef("_equipment", equipment);

            if (fishing != null)
            {
                Undo.RecordObject(fishing, "낚싯줄 시작점 연결");
                fishing.SetRef("_castOriginSource", equipment);
            }

            var oldWand = player.transform.Find("Wand");
            if (oldWand != null && oldWand.gameObject.activeSelf)
            {
                Undo.RecordObject(oldWand.gameObject, "기존 Wand 끄기");
                oldWand.gameObject.SetActive(false);
            }

            Debug.Log($"[캐릭터] '{player.name}'에 소켓·장착 부품을 붙였어요 (소켓 프로필: {(profile != null ? profile.name : "없음")}, 낚시 연결: {(fishing != null ? "됨" : "없음")}, 기존 Wand 끔: {(oldWand != null ? "예" : "없음")}). 확인 후 씬을 저장해 주세요.", player);
            return true;
        }

        /// <summary>오른손 소켓, 임시 지팡이 프리팹·정의, 칩비 소켓 프로필을 없을 때만 만들고, 비어 있는 연결만 채운다.</summary>
        public static void EnsureDefaultAssets()
        {
            var rightHand = LoadOrCreate<SocketId>(RightHandSocketPath, asset => asset.SetString("_displayName", "오른손"));

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(WandPrefabPath);
            if (prefab == null) prefab = CreateWandPrefab();

            var wand = LoadOrCreate<AttachmentDefinition>(WandAttachmentPath, asset =>
            {
                asset.SetString("_displayName", "임시 지팡이");
                var so = new SerializedObject(asset);
                so.FindProperty("_prefab").objectReferenceValue = prefab;
                so.FindProperty("_socket").objectReferenceValue = rightHand;
                // 지팡이 뒤쪽 1/4 지점을 쥐도록 지팡이 중심을 앞으로 민다
                so.FindProperty("_holdPosition").vector3Value = new Vector3(0f, 0f, WandSize.z * 0.25f);
                so.ApplyModifiedPropertiesWithoutUndo();
            });

            var profile = LoadOrCreate<SocketProfile>(ChibiProfilePath, asset =>
            {
                var so = new SerializedObject(asset);
                var list = so.FindProperty("_bindings");
                list.arraySize = 1;
                var binding = list.GetArrayElementAtIndex(0);
                binding.FindPropertyRelative("_socket").objectReferenceValue = rightHand;
                binding.FindPropertyRelative("_useHumanBone").boolValue = true;
                binding.FindPropertyRelative("_humanBone").enumValueIndex = (int)HumanBodyBones.RightHand;
                so.ApplyModifiedPropertiesWithoutUndo();
            });

            FillIfEmpty(AssetDatabase.LoadAssetAtPath<AvatarSetup>(ChibiAvatarPath), "_socketProfile", profile);
            FillIfEmpty(AssetDatabase.LoadAssetAtPath<ToolDefinition>(WandItemPath), "_attachment", wand);
            AssetDatabase.SaveAssets();
        }

        private static GameObject CreateWandPrefab()
        {
            EnsureFolder(System.IO.Path.GetDirectoryName(WandPrefabPath));
            var root = new GameObject("Wand_Temp");
            var mesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mesh.name = "Mesh";
            Object.DestroyImmediate(mesh.GetComponent<Collider>());
            mesh.transform.SetParent(root.transform, false);
            mesh.transform.localScale = WandSize;
            mesh.GetComponent<MeshRenderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(WandMaterialPath);
            var tip = new GameObject("Tip").transform;
            tip.SetParent(root.transform, false);
            tip.localPosition = new Vector3(0f, 0f, WandSize.z * 0.5f);
            root.AddComponent<WandAttachment>().SetRef("_tip", tip);

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, WandPrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static SocketProfile FindProfileFor(GameObject modelInstance)
        {
            var source = PrefabUtility.GetCorrespondingObjectFromOriginalSource(modelInstance);
            var avatar = AssetDatabase.FindAssets("t:AvatarSetup")
                .Select(guid => AssetDatabase.LoadAssetAtPath<AvatarSetup>(AssetDatabase.GUIDToAssetPath(guid)))
                .FirstOrDefault(setup => setup != null && setup.Model == source && setup.SocketProfile != null);
            if (avatar == null)
                Debug.LogWarning($"[캐릭터] '{modelInstance.name}' 모델의 AvatarSetup에 소켓 프로필이 없어요.", modelInstance);
            return avatar != null ? avatar.SocketProfile : null;
        }

        private static T LoadOrCreate<T>(string path, System.Action<T> init) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            EnsureFolder(System.IO.Path.GetDirectoryName(path));
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            init(asset);
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static void FillIfEmpty(Object target, string field, Object value)
        {
            if (target == null) return;
            var so = new SerializedObject(target);
            var property = so.FindProperty(field);
            if (property.objectReferenceValue != null) return;
            property.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static T GetOrAdd<T>(GameObject target) where T : Component
        {
            var existing = target.GetComponent<T>();
            return existing != null ? existing : Undo.AddComponent<T>(target);
        }

        private static void EnsureFolder(string folder)
        {
            folder = folder.Replace('\\', '/');
            if (AssetDatabase.IsValidFolder(folder)) return;
            var parent = System.IO.Path.GetDirectoryName(folder).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(folder));
        }
    }
}
