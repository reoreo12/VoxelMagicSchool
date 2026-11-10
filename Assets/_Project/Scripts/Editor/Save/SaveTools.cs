// 저장 에디터 도구: 정의 목록(DefinitionCatalog) 채우기·지금 씬 부트스트랩에 연결, 저장 폴더 열기, 저장 지우기(시험용).
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VoxelMagic.Core;
using VoxelMagic.Gameplay;

namespace VoxelMagic.EditorTools
{
    public static class SaveTools
    {
        private const string MenuRoot = "VoxelMagic/저장/";
        public const string CatalogPath = "Assets/_Project/Data/Save/DefinitionCatalog.asset";

        [MenuItem(MenuRoot + "정의 목록 채우기 (+ 지금 씬 부트스트랩에 연결)")]
        public static void FillCatalogMenu()
        {
            var catalog = FillCatalog();
            var bootstrap = Object.FindFirstObjectByType<GameBootstrap>(FindObjectsInactive.Include);
            if (bootstrap != null && ConnectCatalog(bootstrap, catalog))
                EditorSceneManager.MarkSceneDirty(bootstrap.gameObject.scene);
            Debug.Log($"[저장] 정의 목록: 아이템 {catalog.Items.Count}개, 의뢰 {catalog.Requests.Count}개" +
                      (bootstrap != null ? $" · '{bootstrap.gameObject.scene.name}' 부트스트랩에 연결 (씬 저장은 직접)" : ""), catalog);
        }

        [MenuItem(MenuRoot + "저장 폴더 열기")]
        public static void RevealFolder()
        {
            System.IO.Directory.CreateDirectory(JsonFileSaveStore.Folder);
            EditorUtility.RevealInFinder(JsonFileSaveStore.Folder);
        }

        [MenuItem(MenuRoot + "저장 지우기 (슬롯 0)")]
        public static void DeleteDefaultSlot()
        {
            string path = JsonFileSaveStore.PathOf(GameSaveSystem.DefaultSlot);
            if (!System.IO.File.Exists(path))
            {
                EditorUtility.DisplayDialog("저장 지우기", "슬롯 0 저장 파일이 없어요.", "확인");
                return;
            }
            if (!EditorUtility.DisplayDialog("저장 지우기", $"슬롯 0 저장 파일을 지웁니다. 되돌릴 수 없어요.\n{path}", "지우기", "취소")) return;
            new JsonFileSaveStore().Delete(GameSaveSystem.DefaultSlot);
            Debug.Log("[저장] 슬롯 0 저장 파일을 지웠어요. 다음 플레이는 새 게임으로 시작합니다.");
        }

        /// <summary>프로젝트의 모든 아이템·의뢰 정의로 목록을 채운다 (없으면 에셋을 만든다).</summary>
        public static DefinitionCatalog FillCatalog()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<DefinitionCatalog>(CatalogPath);
            if (catalog == null)
            {
                EnsureFolder("Assets/_Project/Data/Save");
                catalog = ScriptableObject.CreateInstance<DefinitionCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }
            catalog.SetEntries(LoadAll<ItemDefinition>().OrderBy(i => i.Id), LoadAll<RequestDefinition>().OrderBy(r => r.name));
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            return catalog;
        }

        /// <summary>비어 있을 때만 연결한다. 연결했으면 true.</summary>
        public static bool ConnectCatalog(GameBootstrap bootstrap, DefinitionCatalog catalog)
        {
            var so = new SerializedObject(bootstrap);
            var property = so.FindProperty("_catalog");
            if (property.objectReferenceValue == catalog) return false;
            property.objectReferenceValue = catalog;
            so.ApplyModifiedProperties();
            return true;
        }

        private static System.Collections.Generic.IEnumerable<T> LoadAll<T>() where T : Object =>
            AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { "Assets/_Project" })
                .Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(asset => asset != null);

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            var parent = System.IO.Path.GetDirectoryName(folder).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(folder));
        }
    }
}
