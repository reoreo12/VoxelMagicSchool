// Corridor_SideScroll 씬을 다시 만드는 에디터 도구. 이미지 임포트 설정을 적용하고, 데이터 에셋·캐릭터 프리팹은 없을 때만 만든다.
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using VoxelMagic.Core;
using VoxelMagic.Gameplay;
using VoxelMagic.UI;

namespace VoxelMagic.EditorTools
{
    public static class CorridorSceneBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/Corridor_SideScroll.unity";
        private const string DataFolder = "Assets/_Project/Data/SideScroll";
        private const string SpriteMaterialPath =
            "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat";
        private const string InputPath = "Assets/_Project/Input/GameControls.inputactions";

        // 기본 복도 순서: 문 → 창문 → 민벽 → 창문+사물함 → 창문 → 민벽 → 문(반전)
        private static readonly (string file, bool flip)[] DefaultTiles =
        {
            ("tile_door_glass", false), ("tile_window_glass", false), ("tile_wall", false),
            ("tile_locker_glass", false), ("tile_window_glass", false), ("tile_wall", false),
            ("tile_door_glass", true)
        };

        private static readonly Vector3 LightEuler = new Vector3(30f, 20f, 0f);
        private static readonly Color HudPanelColor = new Color(0f, 0f, 0f, 0.45f);
        private static readonly Vector2 PixelPanelSize = new Vector2(280f, 112f);

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
            var moveConfig = LoadOrCreate<CharacterMoveConfig>("CharacterMoveConfig");
            var animationConfig = LoadOrCreateAnimationConfig();
            // 설정에 지정한 아바타 캐릭터가 있으면 그것을, 없으면 큐브 캐릭터를 쓴다.
            var characterPrefab = config.CharacterPrefab != null
                ? config.CharacterPrefab
                : SideScrollCharacterBuilder.GetOrCreate(animationConfig);
            var font = KoreanFontAssetBuilder.GetOrCreate();
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

            var input = new GameObject("Input").AddComponent<SideScrollInputReader>();
            input.SetRef("_actions", AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputPath));

            var character = (GameObject)PrefabUtility.InstantiatePrefab(characterPrefab);
            character.transform.position = new Vector3(moveConfig.StartX, moveConfig.GroundY, moveConfig.Z);
            character.transform.rotation = Quaternion.Euler(0f, moveConfig.FacingYaw, 0f);
            character.GetComponent<SideScrollCharacter>().SetRef("_input", input);
            camera.GetComponent<SideScrollCamera>().SetRef("_target", character.transform);

            var hud = BuildHud(font, camera);

            var bootstrap = new GameObject("_Bootstrap").AddComponent<CorridorBootstrap>();
            bootstrap.transform.SetAsFirstSibling();
            bootstrap.SetRef("_config", config)
                .SetRef("_moveConfig", moveConfig)
                .SetRef("_corridor", corridor)
                .SetRefList("_bindRoots", new Object[] { camera.gameObject, character, hud, input.gameObject });

            // 에디터에서도 플레이 때와 같은 화면이 보이도록 카메라를 복도 왼쪽 끝에 맞춘다.
            var bounds = corridor.Bounds;
            camera.orthographicSize = SideScrollMath.OrthographicSizeFor(bounds.Height);
            camera.transform.SetPositionAndRotation(
                new Vector3(SideScrollMath.ClampCameraX(moveConfig.StartX, bounds, camera.orthographicSize * camera.aspect),
                    bounds.Height * 0.5f, config.CameraZ),
                Quaternion.identity);
            camera.backgroundColor = day.SkyColor;

            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[CorridorSceneBuilder] {ScenePath} 생성 완료 (복도 폭 {bounds.Width:0.##}, 높이 {bounds.Height:0.##})");
        }

        internal static CorridorConfig LoadOrCreateConfig() => LoadOrCreate<CorridorConfig>("CorridorConfig");

        internal static CharacterAnimationConfig LoadOrCreateAnimationConfig() =>
            LoadOrCreate<CharacterAnimationConfig>("CharacterAnimationConfig");

        // 화면 구석 UI: 왼쪽 아래 조작법, 오른쪽 위 현재 상태 (밝은 벽 위에서도 읽히게 반투명 어두운 판을 깐다)
        private static GameObject BuildHud(TMP_FontAsset font, Camera camera)
        {
            var canvasGo = new GameObject("HUD Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

            // 도트 렌더링 출력: 화면 맨 뒤 전체 화면 (마우스 입력을 막지 않게 Raycast Target 끔)
            var output = new GameObject("PixelOutput", typeof(RectTransform), typeof(RawImage));
            output.transform.SetParent(canvasGo.transform, false);
            var outputRect = (RectTransform)output.transform;
            outputRect.anchorMin = Vector2.zero;
            outputRect.anchorMax = Vector2.one;
            outputRect.offsetMin = outputRect.offsetMax = Vector2.zero;
            var outputImage = output.GetComponent<RawImage>();
            outputImage.raycastTarget = false;
            outputImage.enabled = false;
            var pixelView = camera.gameObject.AddComponent<PixelRenderView>();
            pixelView.SetRef("_output", outputImage);

            var hint = HudText("ControlsHint", canvasGo.transform, font, 22, TextAlignmentOptions.TopLeft,
                Vector2.zero, new Vector2(24f, 24f), new Vector2(360f, 140f));
            hint.gameObject.AddComponent<SideScrollHintView>().SetRef("_text", hint);

            var state = HudText("StateLabel", canvasGo.transform, font, 22, TextAlignmentOptions.Center,
                Vector2.one, new Vector2(-24f, -24f), new Vector2(220f, 48f));
            state.gameObject.AddComponent<StateDebugView>().SetRef("_text", state);

            BuildPixelPanel(canvasGo.transform, font, pixelView);
            return canvasGo;
        }

        // 오른쪽 위 상태 표시 아래: 도트 렌더링 토글 + 도트 크기 슬라이더 + 표시 글자
        private static void BuildPixelPanel(Transform canvas, TMP_FontAsset font, PixelRenderView view)
        {
            var panel = new GameObject("PixelPanel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvas, false);
            panel.GetComponent<Image>().color = HudPanelColor;
            var rect = (RectTransform)panel.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.one;
            rect.anchoredPosition = new Vector2(-24f, -84f);
            rect.sizeDelta = PixelPanelSize;

            var resources = new DefaultControls.Resources
            {
                standard = BuiltinSprite("UISprite"), background = BuiltinSprite("Background"),
                knob = BuiltinSprite("Knob"), checkmark = BuiltinSprite("Checkmark")
            };

            var toggleGo = DefaultControls.CreateToggle(resources);
            toggleGo.name = "PixelToggle";
            PlaceTopLeft(toggleGo.transform, panel.transform, new Vector2(14f, -10f), new Vector2(PixelPanelSize.x - 28f, 30f));
            Object.DestroyImmediate(toggleGo.transform.Find("Label").gameObject);
            PanelText("Label", toggleGo.transform, font, SideScrollTexts.PixelToggle, new Vector2(26f, -2f), new Vector2(PixelPanelSize.x - 54f, 28f));
            var toggle = toggleGo.GetComponent<Toggle>();

            var sliderGo = DefaultControls.CreateSlider(resources);
            sliderGo.name = "PixelSlider";
            PlaceTopLeft(sliderGo.transform, panel.transform, new Vector2(14f, -50f), new Vector2(PixelPanelSize.x - 28f, 20f));
            var label = PanelText("SizeLabel", panel.transform, font, "", new Vector2(14f, -78f), new Vector2(PixelPanelSize.x - 28f, 28f));

            panel.AddComponent<PixelRenderPanel>()
                .SetRef("_view", view).SetRef("_toggle", toggle)
                .SetRef("_slider", sliderGo.GetComponent<Slider>()).SetRef("_label", label);
        }

        private static TextMeshProUGUI PanelText(string name, Transform parent, TMP_FontAsset font, string content,
            Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            PlaceTopLeft(go.transform, parent, position, size);
            var text = go.GetComponent<TextMeshProUGUI>();
            // 글자보다 폰트를 먼저 정한다 (기본 폰트로 한글을 그리면 TMP 기본 폴백 폰트 파일이 바뀜).
            text.font = font;
            text.text = content;
            text.fontSize = 20;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.raycastTarget = false;
            return text;
        }

        private static void PlaceTopLeft(Transform child, Transform parent, Vector2 position, Vector2 size)
        {
            child.SetParent(parent, false);
            var rect = (RectTransform)child;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static Sprite BuiltinSprite(string file) =>
            AssetDatabase.GetBuiltinExtraResource<Sprite>($"UI/Skin/{file}.psd");

        private static TextMeshProUGUI HudText(string name, Transform canvas, TMP_FontAsset font, float size,
            TextAlignmentOptions alignment, Vector2 corner, Vector2 offset, Vector2 panelSize)
        {
            var panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvas, false);
            panel.GetComponent<Image>().color = HudPanelColor;
            var rect = (RectTransform)panel.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = corner;
            rect.anchoredPosition = offset;
            rect.sizeDelta = panelSize;

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(panel.transform, false);
            var textRect = (RectTransform)textGo.transform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(14f, 8f);
            textRect.offsetMax = new Vector2(-14f, -8f);
            var text = textGo.GetComponent<TextMeshProUGUI>();
            text.font = font;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

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
