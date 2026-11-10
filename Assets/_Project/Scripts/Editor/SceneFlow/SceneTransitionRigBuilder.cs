// 씬 이동 장치를 씬에 놓는 에디터 도우미: 진행자 + 페이드 캔버스(픽셀 원형 셰이더), 보이지 않는 이동 트리거, 도착 지점.
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using VoxelMagic.Core;
using VoxelMagic.Gameplay;
using VoxelMagic.UI;

namespace VoxelMagic.EditorTools
{
    public static class SceneTransitionRigBuilder
    {
        public const string ConfigPath = "Assets/_Project/Data/Config/SceneTransitionConfig.asset";
        public const string ShaderPath = "Assets/_Project/Art/Shaders/PixelCircleFade.shader";
        public const string MaterialPath = "Assets/_Project/Art/Materials/UI_PixelCircleFade.mat";

        public static SceneTransitionConfig LoadOrCreateConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<SceneTransitionConfig>(ConfigPath);
            if (config != null) return config;
            config = ScriptableObject.CreateInstance<SceneTransitionConfig>();
            AssetDatabase.CreateAsset(config, ConfigPath);
            return config;
        }

        public static Material LoadOrCreateMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material != null) return material;
            material = new Material(AssetDatabase.LoadAssetAtPath<Shader>(ShaderPath));
            AssetDatabase.CreateAsset(material, MaterialPath);
            return material;
        }

        /// <summary>SceneTransition (진행자) 아래 FadeCanvas(화면 덮기, 클릭 막기)를 만든다. focus = 원 중심이 따라갈 플레이어.</summary>
        public static SceneTransitionDirector CreateRig(Transform focus)
        {
            var config = LoadOrCreateConfig();
            var root = new GameObject("SceneTransition");
            var canvasGo = new GameObject("FadeCanvas", typeof(Canvas), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(root.transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = config.SortingOrder;

            var fadeGo = new GameObject("Fade", typeof(RectTransform), typeof(RawImage), typeof(PixelCircleFadeView));
            fadeGo.transform.SetParent(canvasGo.transform, false);
            var rect = (RectTransform)fadeGo.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = fadeGo.GetComponent<RawImage>();
            image.material = LoadOrCreateMaterial();
            image.raycastTarget = true;
            image.enabled = false;
            var view = fadeGo.GetComponent<PixelCircleFadeView>();
            view.SetRef("_graphic", image).SetRef("_canvas", canvas);

            var director = root.AddComponent<SceneTransitionDirector>();
            director.SetRef("_config", config).SetRef("_fadeView", view).SetRef("_focus", focus);
            return director;
        }

        /// <summary>보이지 않는 상자 트리거. center는 상자 가운데(월드), size는 m.</summary>
        public static SceneTransitionTrigger CreateTrigger(string name, Vector3 center, Vector3 size, string sceneName,
            string arrivalSpawnId, SceneTransitionDirector director, Transform player, Transform parent)
        {
            var go = new GameObject(name, typeof(BoxCollider));
            go.transform.SetParent(parent, false);
            go.transform.position = center;
            var box = go.GetComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = size;
            var trigger = go.AddComponent<SceneTransitionTrigger>();
            trigger.SetString("_sceneName", sceneName).SetString("_arrivalSpawnId", arrivalSpawnId)
                .SetRef("_director", director).SetRef("_player", player);
            return trigger;
        }

        public static SceneSpawnPoint CreateSpawn(string id, Vector3 position, Quaternion rotation, Transform player, Transform parent)
        {
            var go = new GameObject("Spawn_" + id);
            go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(position, rotation);
            var spawn = go.AddComponent<SceneSpawnPoint>();
            spawn.SetString("_id", id).SetRef("_player", player);
            return spawn;
        }
    }
}
