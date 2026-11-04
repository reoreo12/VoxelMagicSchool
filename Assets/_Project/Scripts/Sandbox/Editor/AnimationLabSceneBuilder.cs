// 애니메이션 랩 씬(Scenes/Sandbox/AnimationLab)을 만드는 에디터 도구: 원근 카메라, 격자 바닥, 키 비교 큐브, 툴 UI.
// UI 배치는 1920×1080 기준. 설정·머테리얼은 없을 때만 만든다.
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using VoxelMagic.Core;
using VoxelMagic.UI;
using VoxelMagic.EditorTools;

namespace VoxelMagic.Sandbox.EditorTools
{
    public static class AnimationLabSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/Sandbox/AnimationLab.unity";
        private const string DataFolder = "Assets/_Project/Data/Sandbox";
        private const string MaterialFolder = "Assets/_Project/Materials/Sandbox";
        private const string MoveConfigPath = "Assets/_Project/Data/SideScroll/CharacterMoveConfig.asset";
        private const string AnimationConfigPath = "Assets/_Project/Data/SideScroll/CharacterAnimationConfig.asset";

        // 바닥 격자 (m)
        private const float GridHalfSize = 3f;
        private const float GridStep = 0.5f;
        private const float GridLineWidth = 0.01f;
        private static readonly Vector3 HeightReferencePosition = new Vector3(-0.9f, 0f, 0f);
        private static readonly Vector3 LightEuler = new Vector3(40f, -30f, 0f);

        private static readonly Color Background = new Color(0.17f, 0.18f, 0.2f);
        private static readonly Color GroundColor = new Color(0.42f, 0.44f, 0.46f);
        private static readonly Color GridColor = new Color(0.3f, 0.32f, 0.34f);
        private static readonly Color ForwardColor = new Color(0.35f, 0.6f, 0.95f);
        private static readonly Color PanelColor = new Color(0.08f, 0.09f, 0.11f, 0.82f);
        private static readonly Color ButtonColor = new Color(0.24f, 0.27f, 0.32f);
        private static readonly Color AccentColor = new Color(0.25f, 0.45f, 0.75f);
        private static readonly Color TextColor = new Color(0.93f, 0.94f, 0.96f);

        private static TMP_FontAsset _font;
        private static TMP_DefaultControls.Resources _tmpResources;
        private static DefaultControls.Resources _uiResources;

        [MenuItem("VoxelMagic/아바타/애니메이션 랩 씬 다시 만들기")]
        private static void BuildFromMenu()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null &&
                !EditorUtility.DisplayDialog("씬 다시 만들기", "기존 AnimationLab 씬을 덮어씁니다. 계속할까요?", "덮어쓰기", "취소"))
                return;
            Build();
        }

        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            // 새 씬을 연 뒤에 에셋을 읽는다 (열 때 쓰지 않는 에셋이 메모리에서 내려가 참조가 풀림).
            var config = LoadOrCreateConfig();
            _font = KoreanFontAssetBuilder.GetOrCreate();
            LoadResources();

            var camera = Camera.main;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Background;
            camera.fieldOfView = 40f;
            camera.nearClipPlane = 0.05f;
            var labCamera = camera.gameObject.AddComponent<LabCamera>();
            labCamera.SetRef("_config", config);

            var light = Object.FindFirstObjectByType<Light>();
            light.transform.rotation = Quaternion.Euler(LightEuler);
            light.shadows = LightShadows.Soft;
            VoxelShadowSettings.Apply(light);

            BuildGround();
            var spawn = new GameObject("SpawnPoint").transform;
            var heightReference = BuildHeightReference();

            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            var lab = new GameObject("AnimationLab").AddComponent<AnimationLab>();
            lab.SetRef("_config", config)
                .SetRef("_spawnPoint", spawn)
                .SetRef("_heightReference", heightReference)
                .SetRef("_camera", labCamera)
                .SetRefList("_fallbackAvatars", AvatarPipeline.ScanAvatars().ConvertAll(a => (Object)a));
            BuildUi(lab);
            eventSystem.transform.SetAsLastSibling();

            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[AnimationLabSceneBuilder] {ScenePath} 생성 완료");
        }

        // ── 월드 ─────────────────────────────────────────────

        private static void BuildGround()
        {
            var root = new GameObject("Ground").transform;
            var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.name = "Floor";
            plane.transform.SetParent(root, false);
            plane.GetComponent<Renderer>().sharedMaterial = Mat("Ground", GroundColor);

            var gridMaterial = Mat("Grid", GridColor);
            int lines = Mathf.RoundToInt(GridHalfSize * 2f / GridStep);
            for (int i = 0; i <= lines; i++)
            {
                float offset = -GridHalfSize + i * GridStep;
                Line(root, $"GridX_{i}", new Vector3(offset, 0.001f, 0f), new Vector3(GridLineWidth, 0.001f, GridHalfSize * 2f), gridMaterial);
                Line(root, $"GridZ_{i}", new Vector3(0f, 0.001f, offset), new Vector3(GridHalfSize * 2f, 0.001f, GridLineWidth), gridMaterial);
            }
            // 아바타 정면(+Z) 표시
            Line(root, "Forward", new Vector3(0f, 0.002f, GridHalfSize * 0.25f), new Vector3(GridLineWidth * 3f, 0.001f, GridHalfSize * 0.5f), Mat("Forward", ForwardColor));
        }

        private static void Line(Transform parent, string name, Vector3 position, Vector3 scale, Material material) =>
            LabEditorExtensions.Prim(PrimitiveType.Cube, name, parent, position, scale, material);

        private static GameObject BuildHeightReference()
        {
            var prefab = SideScrollCharacterBuilder.GetOrCreate(AssetDatabase.LoadAssetAtPath<CharacterAnimationConfig>(AnimationConfigPath));
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = "HeightReference (큐브 1.6)";
            instance.transform.SetPositionAndRotation(HeightReferencePosition, Quaternion.identity);
            return instance;
        }

        // ── UI ───────────────────────────────────────────────

        private static void BuildUi(AnimationLab lab)
        {
            var canvasGo = new GameObject("LabCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            var canvas = canvasGo.transform;
            var ui = new Dictionary<string, Object>();

            // 도트 렌더링 출력: 화면 맨 뒤 전체 화면. 마우스 입력(카메라 조작)을 막지 않게 Raycast Target 끔.
            var pixelOutput = new GameObject("PixelOutput", typeof(RectTransform), typeof(RawImage));
            pixelOutput.transform.SetParent(canvas, false);
            var outputRect = (RectTransform)pixelOutput.transform;
            outputRect.anchorMin = Vector2.zero;
            outputRect.anchorMax = Vector2.one;
            outputRect.offsetMin = outputRect.offsetMax = Vector2.zero;
            var outputImage = pixelOutput.GetComponent<RawImage>();
            outputImage.raycastTarget = false;
            outputImage.enabled = false;
            var pixelView = Camera.main.gameObject.AddComponent<PixelRenderView>();
            pixelView.SetRef("_output", outputImage);
            lab.SetRef("_pixelView", pixelView);

            // 왼쪽 위: 아바타
            var avatarPanel = Panel("AvatarPanel", canvas, new Vector2(0f, 1f), new Vector2(20f, -20f), new Vector2(440f, 270f));
            Label("Title", avatarPanel, "아바타", 24, new Vector2(16f, -12f), new Vector2(400f, 32f), FontStyles.Bold);
            var dropdown = TMP_DefaultControls.CreateDropdown(_tmpResources);
            dropdown.name = "AvatarDropdown";
            Place(dropdown.transform, avatarPanel, new Vector2(16f, -50f), new Vector2(408f, 40f));
            ui["AvatarDropdown"] = dropdown.GetComponent<TMP_Dropdown>();
            ui["AvatarInfo"] = Label("AvatarInfo", avatarPanel, "", 18, new Vector2(16f, -100f), new Vector2(408f, 100f));
            ui["HeightReferenceToggle"] = MakeToggle("HeightReference", avatarPanel, "키 비교 큐브 (1.6)", true, new Vector2(16f, -216f), new Vector2(300f, 36f));

            // 위 가운데: 상태 버튼 + 재생 중
            var statePanel = Panel("StatePanel", canvas, new Vector2(0.5f, 1f), new Vector2(-300f, -20f), new Vector2(600f, 120f));
            string[] states = { "1  Idle", "2  Walk", "3  Run" };
            var stateButtons = new Object[3];
            for (int i = 0; i < 3; i++)
                stateButtons[i] = MakeButton($"State{i}", statePanel, states[i], new Vector2(16f + i * 192f, -14f), new Vector2(184f, 46f), AccentColor);
            ui["NowPlaying"] = Label("NowPlaying", statePanel, "", 18, new Vector2(16f, -66f), new Vector2(568f, 50f));

            // 오른쪽: 애니메이션 목록
            var listPanel = Panel("ClipPanel", canvas, new Vector2(1f, 1f), new Vector2(-500f, -20f), new Vector2(480f, 820f));
            Label("Title", listPanel, "애니메이션 목록", 24, new Vector2(16f, -12f), new Vector2(300f, 32f), FontStyles.Bold);
            ui["RefreshButton"] = MakeButton("Refresh", listPanel, "새로고침", new Vector2(344f, -10f), new Vector2(120f, 38f), AccentColor);
            ui["ClipCount"] = Label("ClipCount", listPanel, "", 15, new Vector2(16f, -50f), new Vector2(448f, 26f));
            var scroll = DefaultControls.CreateScrollView(_uiResources);
            scroll.name = "ClipList";
            Place(scroll.transform, listPanel, new Vector2(16f, -82f), new Vector2(448f, 722f));
            scroll.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.25f);
            scroll.GetComponent<ScrollRect>().horizontal = false;
            scroll.GetComponent<ScrollRect>().scrollSensitivity = 30f;
            Object.DestroyImmediate(scroll.transform.Find("Scrollbar Horizontal").gameObject);
            var content = (RectTransform)scroll.transform.Find("Viewport/Content");
            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 4f;
            layout.padding = new RectOffset(6, 6, 6, 6);
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            ui["ClipListContent"] = content;
            var template = MakeButton("ClipButton", content, "클립", Vector2.zero, new Vector2(420f, 38f), ButtonColor);
            template.gameObject.AddComponent<LayoutElement>().minHeight = 38f;
            var templateLabel = template.GetComponentInChildren<TMP_Text>();
            templateLabel.alignment = TextAlignmentOptions.MidlineLeft;
            templateLabel.fontSize = 17;
            templateLabel.margin = new Vector4(10f, 0f, 10f, 0f);
            templateLabel.richText = true;
            ui["ClipButtonTemplate"] = template;

            // 아래: 재생
            var playPanel = Panel("PlaybackPanel", canvas, new Vector2(0.5f, 0f), new Vector2(-750f, 20f), new Vector2(1500f, 170f));
            ui["TimeSlider"] = MakeSlider("TimeSlider", playPanel, new Vector2(20f, -22f), new Vector2(900f, 24f));
            ui["TimeLabel"] = Label("TimeLabel", playPanel, "", 18, new Vector2(940f, -16f), new Vector2(370f, 32f));
            var play = MakeButton("PlayButton", playPanel, "일시정지", new Vector2(20f, -64f), new Vector2(150f, 44f), AccentColor);
            ui["PlayButton"] = play;
            ui["PlayLabel"] = play.GetComponentInChildren<TMP_Text>();
            ui["StepBackButton"] = MakeButton("StepBack", playPanel, "< 1프레임", new Vector2(180f, -64f), new Vector2(130f, 44f), ButtonColor);
            ui["StepForwardButton"] = MakeButton("StepForward", playPanel, "1프레임 >", new Vector2(318f, -64f), new Vector2(130f, 44f), ButtonColor);
            ui["LoopToggle"] = MakeToggle("Loop", playPanel, "반복 재생 (L)", true, new Vector2(466f, -68f), new Vector2(180f, 36f));
            ui["SpeedSlider"] = MakeSlider("SpeedSlider", playPanel, new Vector2(660f, -74f), new Vector2(250f, 24f));
            ui["SpeedLabel"] = Label("SpeedLabel", playPanel, "", 18, new Vector2(920f, -66f), new Vector2(180f, 32f));
            ui["BlendSlider"] = MakeSlider("BlendSlider", playPanel, new Vector2(660f, -124f), new Vector2(250f, 24f));
            ui["BlendLabel"] = Label("BlendLabel", playPanel, "", 18, new Vector2(920f, -116f), new Vector2(180f, 32f));
            ui["AbToggle"] = MakeToggle("AbToggle", playPanel, "A↔B 반복 전환", false, new Vector2(20f, -120f), new Vector2(200f, 36f));
            ui["AbSlider"] = MakeSlider("AbSlider", playPanel, new Vector2(230f, -128f), new Vector2(220f, 24f));
            ui["AbLabel"] = Label("AbLabel", playPanel, "", 18, new Vector2(466f, -120f), new Vector2(190f, 32f));
            ui["PixelToggle"] = MakeToggle("PixelToggle", playPanel, LabTexts.PixelToggle, false, new Vector2(1120f, -68f), new Vector2(240f, 36f));
            ui["PixelSlider"] = MakeSlider("PixelSlider", playPanel, new Vector2(1120f, -128f), new Vector2(160f, 24f));
            ui["PixelLabel"] = Label("PixelLabel", playPanel, "", 18, new Vector2(1292f, -120f), new Vector2(200f, 32f));

            // 왼쪽 아래: 측정 + 카메라
            var meterPanel = Panel("MeterPanel", canvas, new Vector2(0f, 0f), new Vector2(20f, 240f), new Vector2(560f, 300f));
            Label("Title", meterPanel, "측정 · 카메라", 24, new Vector2(16f, -12f), new Vector2(400f, 32f), FontStyles.Bold);
            ui["MeterText"] = Label("MeterText", meterPanel, "", 17, new Vector2(16f, -50f), new Vector2(528f, 120f));
            string[] views = { "정면", "옆 (복도 시점)", "뒤", "3/4" };
            float[] widths = { 100f, 170f, 100f, 100f };
            var cameraButtons = new Object[4];
            float x = 16f;
            for (int i = 0; i < 4; i++)
            {
                cameraButtons[i] = MakeButton($"View{i}", meterPanel, views[i], new Vector2(x, -180f), new Vector2(widths[i], 40f), ButtonColor);
                x += widths[i] + 8f;
            }
            Label("CameraHelp", meterPanel, LabTexts.CameraHelp, 15, new Vector2(16f, -230f), new Vector2(528f, 60f)).color = new Color(0.7f, 0.73f, 0.78f);

            // 상태 메시지 (아래 가운데)
            var status = Label("Status", canvas, "", 18, Vector2.zero, new Vector2(1320f, 32f));
            var statusRect = status.rectTransform;
            statusRect.anchorMin = statusRect.anchorMax = new Vector2(0.5f, 0f);
            statusRect.pivot = new Vector2(0.5f, 0f);
            statusRect.anchoredPosition = new Vector2(0f, 196f);
            status.alignment = TextAlignmentOptions.Center;
            status.color = new Color(1f, 0.85f, 0.5f);
            ui["Status"] = status;

            foreach (var text in canvasGo.GetComponentsInChildren<TMP_Text>(true)) text.font = _font;

            foreach (var pair in ui) lab.SetRef($"_ui.{pair.Key}", pair.Value);
            lab.SetRefList("_ui.StateButtons", stateButtons).SetRefList("_ui.CameraButtons", cameraButtons);
        }

        // 화면 아래쪽에 붙는 패널은 position.y가 아래 여백이다.
        private static RectTransform Panel(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = PanelColor;
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position + new Vector2(0f, anchor.y < 0.5f ? size.y : 0f);
            rect.sizeDelta = size;
            return rect;
        }

        // 부모 왼쪽 위 기준으로 놓는다.
        private static void Place(Transform child, Transform parent, Vector2 position, Vector2 size)
        {
            child.SetParent(parent, false);
            var rect = (RectTransform)child;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static TextMeshProUGUI Label(string name, Transform parent, string content, float size, Vector2 position,
            Vector2 boxSize, FontStyles style = FontStyles.Normal)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            Place(go.transform, parent, position, boxSize);
            var text = go.GetComponent<TextMeshProUGUI>();
            // 글자를 넣기 전에 폰트부터 정한다. 기본 폰트로 한글을 먼저 그리면 TMP 기본 폴백 폰트 파일이 바뀐다.
            text.font = _font;
            text.text = content;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = TextColor;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.raycastTarget = false;
            text.richText = true;
            return text;
        }

        private static Button MakeButton(string name, Transform parent, string label, Vector2 position, Vector2 size, Color color)
        {
            var go = TMP_DefaultControls.CreateButton(_tmpResources);
            go.name = name;
            Place(go.transform, parent, position, size);
            go.GetComponent<Image>().color = color;
            var text = go.GetComponentInChildren<TMP_Text>();
            text.font = _font;
            text.text = label;
            text.fontSize = 18;
            text.color = TextColor;
            return go.GetComponent<Button>();
        }

        private static Slider MakeSlider(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var go = DefaultControls.CreateSlider(_uiResources);
            go.name = name;
            Place(go.transform, parent, position, size);
            go.transform.Find("Fill Area/Fill").GetComponent<Image>().color = AccentColor;
            return go.GetComponent<Slider>();
        }

        // 기본 토글의 옛 Text 라벨을 TMP 라벨로 바꾼다.
        private static Toggle MakeToggle(string name, Transform parent, string label, bool on, Vector2 position, Vector2 size)
        {
            var go = DefaultControls.CreateToggle(_uiResources);
            go.name = name;
            Place(go.transform, parent, position, size);
            Object.DestroyImmediate(go.transform.Find("Label").gameObject);
            var text = Label("Label", go.transform, label, 18, new Vector2(26f, -6f), new Vector2(size.x - 26f, size.y));
            text.raycastTarget = false;
            var toggle = go.GetComponent<Toggle>();
            toggle.isOn = on;
            return toggle;
        }

        // ── 에셋 ─────────────────────────────────────────────

        private static void LoadResources()
        {
            Sprite Builtin(string file) => AssetDatabase.GetBuiltinExtraResource<Sprite>($"UI/Skin/{file}.psd");
            _tmpResources = new TMP_DefaultControls.Resources
            {
                standard = Builtin("UISprite"), background = Builtin("Background"), inputField = Builtin("InputFieldBackground"),
                knob = Builtin("Knob"), checkmark = Builtin("Checkmark"), dropdown = Builtin("DropdownArrow"), mask = Builtin("UIMask")
            };
            _uiResources = new DefaultControls.Resources
            {
                standard = Builtin("UISprite"), background = Builtin("Background"), inputField = Builtin("InputFieldBackground"),
                knob = Builtin("Knob"), checkmark = Builtin("Checkmark"), dropdown = Builtin("DropdownArrow"), mask = Builtin("UIMask")
            };
        }

        private static AnimationLabConfig LoadOrCreateConfig()
        {
            if (!AssetDatabase.IsValidFolder(DataFolder)) AssetDatabase.CreateFolder("Assets/_Project/Data", "Sandbox");
            string path = $"{DataFolder}/AnimationLabConfig.asset";
            var config = AssetDatabase.LoadAssetAtPath<AnimationLabConfig>(path);
            if (config != null) return config;
            config = ScriptableObject.CreateInstance<AnimationLabConfig>();
            AssetDatabase.CreateAsset(config, path);
            config.SetRef("_moveConfig", AssetDatabase.LoadAssetAtPath<CharacterMoveConfig>(MoveConfigPath));
            return config;
        }

        private static Material Mat(string name, Color color)
        {
            if (!AssetDatabase.IsValidFolder(MaterialFolder)) AssetDatabase.CreateFolder("Assets/_Project/Materials", "Sandbox");
            string path = $"{MaterialFolder}/M_Lab_{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = $"M_Lab_{name}" };
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", 0.1f);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }
    }
}
