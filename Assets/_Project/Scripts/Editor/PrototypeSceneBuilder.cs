// 그레이박스 씬(Prototype_FarmFishing)을 기본 도형으로 다시 만드는 에디터 도구.
// 배치를 코드로 남겨 두어 언제든 같은 씬을 재생성할 수 있게 한다. 설정 에셋은 없을 때만 만든다.
using System.Collections.Generic;
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
    public static class PrototypeSceneBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/Prototype_FarmFishing.unity";
        private const string MaterialFolder = "Assets/_Project/Materials";
        private const string ConfigFolder = "Assets/_Project/Data/Config";
        private const string InputPath = "Assets/_Project/Input/GameControls.inputactions";
        private const string PrefabFolder = "Assets/_Project/Prefabs";

        // 배치 (월드 좌표, m)
        private static readonly Vector3 SchoolPosition = new Vector3(0f, 0f, 14f);
        private static readonly Vector3 BoardPosition = new Vector3(-3f, 0f, 6f);
        private static readonly Vector3 FarmOrigin = new Vector3(-10f, 0f, -6f);
        private static readonly Vector3 PondPosition = new Vector3(7f, 0f, -5f);
        private static readonly Vector2 PondSize = new Vector2(8f, 6f);
        private static readonly Vector3 CreaturePosition = new Vector3(11f, 0f, 4f);
        private static readonly Vector3[] TreePositions =
        {
            new Vector3(-9f, 0f, 9f), new Vector3(9f, 0f, 9f), new Vector3(-15f, 0f, -9f), new Vector3(15f, 0f, -11f)
        };

        [MenuItem("VoxelMagic/프로토타입 씬 다시 만들기")]
        private static void BuildFromMenu()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null &&
                !EditorUtility.DisplayDialog("씬 다시 만들기", "기존 Prototype_FarmFishing 씬을 덮어씁니다. 계속할까요?", "덮어쓰기", "취소"))
                return;
            Build();
        }

        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var font = KoreanFontAssetBuilder.GetOrCreate();

            var balance = LoadOrCreate<GameBalanceConfig>("GameBalanceConfig");
            var farm = LoadOrCreate<FarmConfig>("FarmConfig");
            var fishing = LoadOrCreate<FishingConfig>("FishingConfig");
            var player = LoadOrCreate<PlayerConfig>("PlayerConfig");
            var data = PrototypeDataBuilder.CreateAll();

            var world = new GameObject("World");
            BuildGround(world.transform);
            BuildSchool(world.transform);
            BuildTrees(world.transform);
            BuildRequestBoard(world.transform);
            BuildFarm(world.transform, farm);
            BuildPond(world.transform);
            BuildCreature(world.transform);

            var playerRoot = BuildPlayer(out var interactor);
            var camera = Camera.main;
            camera.gameObject.AddComponent<QuarterViewCamera>().SetRef("_target", playerRoot.transform);

            var ui = BuildUI(font, camera);

            var bootstrap = new GameObject("_Bootstrap").AddComponent<GameBootstrap>();
            bootstrap.transform.SetAsFirstSibling();
            bootstrap.SetRef("_balance", balance)
                .SetRef("_farm", farm)
                .SetRef("_fishing", fishing)
                .SetRef("_player", player)
                .SetRef("_playerInteractor", interactor)
                .SetRefList("_bindRoots", new Object[] { world, playerRoot, camera.gameObject, ui })
                .SetRefList("_requests", data.Requests.ToArray())
                .SetAmountList("_startingItems", new ItemDefinition[] { data.Wand }, new[] { 1 });

            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[PrototypeSceneBuilder] {ScenePath} 생성 완료");
        }

        // ── 월드 ─────────────────────────────────────────────

        private static void BuildGround(Transform parent)
        {
            // 두께 없는 Plane은 캐릭터가 뚫고 떨어질 수 있어 윗면이 y=0인 두꺼운 박스로 만든다.
            var ground = Prim(PrimitiveType.Cube, "Ground", parent, new Vector3(0f, -0.5f, 0f), new Vector3(40f, 1f, 40f),
                Mat("Ground", new Color(0.45f, 0.72f, 0.35f)), keepCollider: true);
            ground.isStatic = true;
        }

        private static void BuildSchool(Transform parent)
        {
            var school = Group("School", parent, SchoolPosition);
            Prim(PrimitiveType.Cube, "Building", school, new Vector3(0f, 4f, 0f), new Vector3(20f, 8f, 6f),
                Mat("SchoolCream", new Color(0.96f, 0.92f, 0.8f)), keepCollider: true);
            Prim(PrimitiveType.Cube, "Band", school, new Vector3(0f, 5.5f, 0f), new Vector3(20.2f, 0.6f, 6.2f),
                Mat("SchoolBand", new Color(0.62f, 0.82f, 0.6f)));
            var clock = Prim(PrimitiveType.Cylinder, "Clock", school, new Vector3(0f, 6.8f, -3.15f),
                new Vector3(1.6f, 0.05f, 1.6f), Mat("Clock", Color.white));
            clock.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        }

        private static void BuildTrees(Transform parent)
        {
            var trees = Group("GinkgoTrees", parent, Vector3.zero);
            for (int i = 0; i < TreePositions.Length; i++)
            {
                var tree = Group($"Ginkgo_{i + 1}", trees, TreePositions[i]);
                Prim(PrimitiveType.Cylinder, "Trunk", tree, new Vector3(0f, 1.5f, 0f), new Vector3(0.5f, 1.5f, 0.5f),
                    Mat("Trunk", new Color(0.45f, 0.3f, 0.18f)), keepCollider: true);
                Prim(PrimitiveType.Cube, "Leaves", tree, new Vector3(0f, 3.8f, 0f), Vector3.one * 2.6f,
                    Mat("GinkgoLeaves", new Color(1f, 0.85f, 0.2f)));
            }
        }

        private static void BuildRequestBoard(Transform parent)
        {
            var board = Group("RequestBoard", parent, BoardPosition);
            var area = board.gameObject.AddComponent<BoxCollider>();
            area.center = new Vector3(0f, 1.2f, 0f);
            area.size = new Vector3(2.2f, 2.4f, 0.4f);
            Prim(PrimitiveType.Cube, "Panel", board, new Vector3(0f, 1.6f, 0f), new Vector3(2f, 1.2f, 0.1f),
                Mat("Board", new Color(0.6f, 0.42f, 0.25f)));
            var post = Mat("BoardPost", new Color(0.38f, 0.26f, 0.15f));
            Prim(PrimitiveType.Cube, "PostL", board, new Vector3(-0.85f, 0.8f, 0f), new Vector3(0.15f, 1.6f, 0.15f), post);
            Prim(PrimitiveType.Cube, "PostR", board, new Vector3(0.85f, 0.8f, 0f), new Vector3(0.15f, 1.6f, 0.15f), post);
            var anchor = Group("PromptAnchor", board, BoardPosition + new Vector3(0f, 2.6f, 0f));

            board.gameObject.AddComponent<RequestBoard>()
                .SetRef("_area", area)
                .SetRef("_promptAnchor", anchor);
        }

        private static void BuildFarm(Transform parent, FarmConfig config)
        {
            var field = Group("FarmField", parent, FarmOrigin);
            var cells = Group("Cells", field, FarmOrigin);
            var soil = Mat("Soil", new Color(0.42f, 0.28f, 0.17f));
            float size = config.CellSize;
            for (int x = 0; x < config.GridSize; x++)
                for (int y = 0; y < config.GridSize; y++)
                {
                    var local = new Vector3((x + 0.5f) * size, 0.05f, (y + 0.5f) * size);
                    Prim(PrimitiveType.Cube, $"Cell_{x}_{y}", cells, local,
                        new Vector3(size * 0.94f, 0.1f, size * 0.94f), soil);
                }
            var crops = Group("Crops", field, FarmOrigin);

            var farmField = field.gameObject.AddComponent<FarmField>();
            field.gameObject.AddComponent<FarmFieldView>()
                .SetRef("_field", farmField)
                .SetRef("_cropRoot", crops);

            var quad = Prim(PrimitiveType.Plane, "PlacementPreview", field, Vector3.zero, Vector3.one * 0.1f,
                Mat("Preview", new Color(1f, 1f, 1f, 0.5f), transparent: true));
            quad.AddComponent<PlacementPreview>()
                .SetRef("_field", farmField)
                .SetRef("_quad", quad.GetComponent<Renderer>());
        }

        private static void BuildPond(Transform parent)
        {
            var pond = Group("Pond", parent, PondPosition);
            Prim(PrimitiveType.Cube, "Water", pond, new Vector3(0f, 0.03f, 0f),
                new Vector3(PondSize.x, 0.05f, PondSize.y), Mat("Water", new Color(0.25f, 0.55f, 0.95f, 0.55f), transparent: true));

            var blocker = Group("Blocker", pond, PondPosition).gameObject.AddComponent<BoxCollider>();
            blocker.center = new Vector3(0f, 1f, 0f);
            blocker.size = new Vector3(PondSize.x, 2f, PondSize.y);

            var stones = Group("Stones", pond, PondPosition);
            var stone = Mat("Stone", new Color(0.55f, 0.55f, 0.58f));
            float hx = PondSize.x * 0.5f + 0.25f, hz = PondSize.y * 0.5f + 0.25f;
            int index = 0;
            for (float x = -hx; x <= hx + 0.01f; x += 1.3f)
            {
                StoneAt(stones, stone, new Vector3(x, 0f, -hz), index++);
                StoneAt(stones, stone, new Vector3(x, 0f, hz), index++);
            }
            for (float z = -hz + 1.3f; z < hz; z += 1.3f)
            {
                StoneAt(stones, stone, new Vector3(-hx, 0f, z), index++);
                StoneAt(stones, stone, new Vector3(hx, 0f, z), index++);
            }
            Group("Bobber", pond, PondPosition);
        }

        private static void StoneAt(Transform parent, Material material, Vector3 offset, int index)
        {
            float s = 0.45f + (index % 3) * 0.12f;
            Prim(PrimitiveType.Cube, $"Stone_{index}", parent, offset + new Vector3(0f, s * 0.5f, 0f),
                new Vector3(s, s, s), material);
        }

        private static void BuildCreature(Transform parent)
        {
            var creature = Group("Creature", parent, CreaturePosition);
            var area = creature.gameObject.AddComponent<BoxCollider>();
            area.center = new Vector3(0f, 0.5f, 0f);
            area.size = Vector3.one;
            Prim(PrimitiveType.Cube, "Body", creature, new Vector3(0f, 0.5f, 0f), Vector3.one,
                Mat("CreatureBody", new Color(0.12f, 0.14f, 0.32f)));
            var eye = Mat("CreatureEye", new Color(1f, 0.9f, 0.2f), emission: new Color(1f, 0.85f, 0.1f) * 3f);
            Prim(PrimitiveType.Cube, "EyeL", creature, new Vector3(-0.22f, 0.65f, -0.51f),
                new Vector3(0.18f, 0.12f, 0.04f), eye);
            Prim(PrimitiveType.Cube, "EyeR", creature, new Vector3(0.22f, 0.65f, -0.51f),
                new Vector3(0.18f, 0.12f, 0.04f), eye);
        }

        // ── 플레이어 ──────────────────────────────────────────

        private static GameObject BuildPlayer(out PlayerInteractor interactor)
        {
            var root = new GameObject("Player");
            var controller = root.AddComponent<CharacterController>();
            controller.center = new Vector3(0f, 0.9f, 0f);
            controller.height = 1.8f;
            controller.radius = 0.35f;

            var t = root.transform;
            Prim(PrimitiveType.Capsule, "Body", t, new Vector3(0f, 0.8f, 0f), new Vector3(0.7f, 0.8f, 0.7f),
                Mat("Uniform", new Color(0.13f, 0.18f, 0.4f)));
            Prim(PrimitiveType.Cube, "Head", t, new Vector3(0f, 1.85f, 0f), Vector3.one * 0.5f,
                Mat("Skin", new Color(1f, 0.8f, 0.65f)));
            Prim(PrimitiveType.Cube, "Tie", t, new Vector3(0f, 1.3f, 0.36f), new Vector3(0.1f, 0.4f, 0.04f),
                Mat("Tie", new Color(0.85f, 0.12f, 0.15f)));
            Group("WandTip", t, new Vector3(0.4f, 1.2f, 0.4f));
            t.position = new Vector3(0f, 0.1f, 0f);

            var input = root.AddComponent<GameInputReader>()
                .SetRef("_actions", AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputPath));
            root.AddComponent<PlayerMover>().SetRef("_input", input);
            interactor = root.AddComponent<PlayerInteractor>().SetRef("_input", input);
            return root;
        }

        // ── UI ───────────────────────────────────────────────

        private static GameObject BuildUI(TMP_FontAsset font, Camera camera)
        {
            var ui = new GameObject("UI");

            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystem.transform.SetParent(ui.transform, false);

            var canvasGo = new GameObject("HUDCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(ui.transform, false);
            canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            var canvas = canvasGo.transform;

            // 조작법 (왼쪽 아래)
            var hint = Text("ControlsHint", canvas, font, "", 22, TextAlignmentOptions.BottomLeft);
            Place(hint.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(24f, 24f), new Vector2(460f, 220f));
            var hintBg = hint.gameObject.AddComponent<Outline>();
            hintBg.effectColor = new Color(0f, 0f, 0f, 0.6f);
            hint.gameObject.AddComponent<ControlsHintView>().SetRef("_text", hint);

            // 상호작용 안내 (대상 근처)
            var prompt = new GameObject("InteractionPrompt", typeof(RectTransform), typeof(Image));
            prompt.transform.SetParent(canvas, false);
            var promptRect = (RectTransform)prompt.transform;
            Place(promptRect, Vector2.zero, new Vector2(0.5f, 0f), Vector2.zero, new Vector2(220f, 84f));
            prompt.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);
            var key = Text("Key", promptRect, font, "E", 34, TextAlignmentOptions.Center);
            Place(key.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -4f), new Vector2(200f, 40f));
            var action = Text("Action", promptRect, font, "", 24, TextAlignmentOptions.Center);
            Place(action.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 6f), new Vector2(210f, 36f));

            var promptHolder = new GameObject("InteractionPromptView");
            promptHolder.transform.SetParent(canvas, false);
            promptHolder.AddComponent<InteractionPromptView>()
                .SetRef("_root", promptRect)
                .SetRef("_keyText", key)
                .SetRef("_actionText", action)
                .SetRef("_worldCamera", camera);
            prompt.SetActive(false);

            BuildQuestTracker(canvas, font);
            BuildInventoryBar(canvas, font);
            BuildToast(canvas, font);
            BuildAcquisitionPopup(canvas, font);
            BuildRequestBoardWindow(canvas, font);
            return ui;
        }

        private static void BuildQuestTracker(Transform canvas, TMP_FontAsset font)
        {
            var panel = Panel("QuestTracker", canvas, new Color(0f, 0f, 0f, 0.45f));
            Place(panel, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -24f), new Vector2(380f, 200f));
            var header = Text("Header", panel, font, "", 24, TextAlignmentOptions.TopLeft);
            Place(header.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(14f, -10f), new Vector2(352f, 36f));
            var body = Text("Body", panel, font, "", 22, TextAlignmentOptions.TopLeft);
            Place(body.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(14f, -52f), new Vector2(352f, 140f));

            panel.gameObject.AddComponent<QuestTrackerPanel>()
                .SetRef("_header", header)
                .SetRef("_body", body);
        }

        private static void BuildInventoryBar(Transform canvas, TMP_FontAsset font)
        {
            var bar = new GameObject("InventoryBar", typeof(RectTransform)).GetComponent<RectTransform>();
            bar.SetParent(canvas, false);
            Place(bar, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(560f, 140f));

            var slots = new GameObject("Slots", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter))
                .GetComponent<RectTransform>();
            slots.SetParent(bar, false);
            Place(slots, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(520f, 96f));
            var layout = slots.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 10f;
            layout.childControlWidth = layout.childControlHeight = false;
            layout.childForceExpandWidth = layout.childForceExpandHeight = false;
            slots.GetComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            var selected = Text("SelectedName", bar, font, "", 22, TextAlignmentOptions.Bottom);
            Place(selected.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 104f), new Vector2(400f, 32f));
            selected.gameObject.AddComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.7f);

            bar.gameObject.AddComponent<InventoryBar>()
                .SetRef("_slotRoot", slots)
                .SetRef("_slotPrefab", BuildInventorySlotPrefab(font))
                .SetRef("_selectedName", selected);
        }

        private static InventorySlotView BuildInventorySlotPrefab(TMP_FontAsset font)
        {
            var slot = Panel("InventorySlot", null, new Color(0f, 0f, 0f, 0.5f));
            slot.sizeDelta = new Vector2(96f, 96f);
            var button = slot.gameObject.AddComponent<Button>();

            var highlight = Panel("Highlight", slot, new Color(1f, 0.85f, 0.25f));
            Stretch(highlight);
            highlight.offsetMin = new Vector2(-4f, -4f);
            highlight.offsetMax = new Vector2(4f, 4f);
            highlight.SetAsFirstSibling();
            highlight.GetComponent<Image>().raycastTarget = false;
            var inner = Panel("Inner", slot, new Color(0.12f, 0.12f, 0.14f, 0.9f));
            Stretch(inner);
            inner.GetComponent<Image>().raycastTarget = false;

            var icon = Panel("Icon", slot, Color.white);
            Place(icon, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(60f, 60f));
            icon.GetComponent<Image>().raycastTarget = false;
            var letter = Text("Letter", icon, font, "", 32, TextAlignmentOptions.Center);
            Stretch(letter.rectTransform);
            var count = Text("Count", slot, font, "", 22, TextAlignmentOptions.BottomRight);
            Place(count.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-6f, 2f), new Vector2(60f, 28f));
            count.gameObject.AddComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.8f);
            var key = Text("KeyNumber", slot, font, "", 16, TextAlignmentOptions.TopLeft);
            key.color = new Color(1f, 1f, 1f, 0.7f);
            Place(key.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(6f, -4f), new Vector2(30f, 22f));

            slot.gameObject.AddComponent<InventorySlotView>()
                .SetRef("_highlight", highlight.GetComponent<Image>())
                .SetRef("_icon", icon.GetComponent<Image>())
                .SetRef("_letter", letter)
                .SetRef("_count", count)
                .SetRef("_keyNumber", key)
                .SetRef("_button", button);

            var prefab = PrefabUtility.SaveAsPrefabAsset(slot.gameObject, $"{PrefabFolder}/UI_InventorySlot.prefab");
            Object.DestroyImmediate(slot.gameObject);
            return prefab.GetComponent<InventorySlotView>();
        }

        private static void BuildToast(Transform canvas, TMP_FontAsset font)
        {
            var toast = Panel("Toast", canvas, new Color(0f, 0f, 0f, 0.65f));
            Place(toast, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -120f), new Vector2(560f, 64f));
            toast.GetComponent<Image>().raycastTarget = false;
            var text = Text("Text", toast, font, "", 26, TextAlignmentOptions.Center);
            Stretch(text.rectTransform);
            toast.gameObject.AddComponent<ToastView>().SetRef("_text", text);
        }

        private static void BuildAcquisitionPopup(Transform canvas, TMP_FontAsset font)
        {
            var popup = Panel("AcquisitionPopup", canvas, new Color(0.1f, 0.1f, 0.12f, 0.85f));
            Place(popup, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -110f), new Vector2(560f, 130f));
            popup.GetComponent<Image>().raycastTarget = false;

            var icon = Panel("Icon", popup, Color.white);
            Place(icon, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(20f, 0f), new Vector2(80f, 80f));
            icon.GetComponent<Image>().raycastTarget = false;
            var title = Text("Title", popup, font, "", 30, TextAlignmentOptions.TopLeft);
            Place(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(118f, -12f), new Vector2(420f, 40f));
            var description = Text("Description", popup, font, "", 20, TextAlignmentOptions.TopLeft);
            description.color = new Color(0.85f, 0.85f, 0.85f);
            Place(description.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(118f, -56f), new Vector2(420f, 30f));
            var size = Text("Size", popup, font, "", 22, TextAlignmentOptions.TopLeft);
            size.color = new Color(1f, 0.85f, 0.35f);
            Place(size.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(118f, -88f), new Vector2(420f, 30f));

            popup.gameObject.AddComponent<AcquisitionPopup>()
                .SetRef("_icon", icon.GetComponent<Image>())
                .SetRef("_title", title)
                .SetRef("_description", description)
                .SetRef("_size", size);
        }

        private static void BuildRequestBoardWindow(Transform canvas, TMP_FontAsset font)
        {
            var dim = Panel("RequestBoardWindow", canvas, new Color(0f, 0f, 0f, 0.5f));
            Stretch(dim);
            var window = Panel("Window", dim, new Color(0.27f, 0.19f, 0.12f, 0.97f));
            Place(window, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(920f, 680f));

            var title = Text("Title", window, font, GameTexts.BoardTitle, 34, TextAlignmentOptions.Center);
            Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -16f), new Vector2(600f, 48f));

            var list = new GameObject("List", typeof(RectTransform), typeof(VerticalLayoutGroup)).GetComponent<RectTransform>();
            list.SetParent(window, false);
            Place(list, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -80f), new Vector2(880f, 500f));
            var layout = list.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 12f;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var close = MakeButton("CloseButton", window, font, GameTexts.Close, out _);
            Place(close.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-20f, 20f), new Vector2(150f, 52f));

            dim.gameObject.AddComponent<RequestBoardWindow>()
                .SetRef("_listRoot", list)
                .SetRef("_entryPrefab", BuildRequestEntryPrefab(font))
                .SetRef("_closeButton", close);
        }

        private static RequestEntryView BuildRequestEntryPrefab(TMP_FontAsset font)
        {
            var entry = Panel("RequestEntry", null, new Color(1f, 1f, 1f, 0.08f));
            entry.sizeDelta = new Vector2(880f, 150f);

            var title = Text("Title", entry, font, "", 28, TextAlignmentOptions.TopLeft);
            Place(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -10f), new Vector2(580f, 36f));
            var line = Text("CreatureLine", entry, font, "", 20, TextAlignmentOptions.TopLeft);
            line.color = new Color(0.85f, 0.85f, 0.85f);
            line.fontStyle = FontStyles.Italic;
            Place(line.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -48f), new Vector2(600f, 30f));
            var needs = Text("Needs", entry, font, "", 20, TextAlignmentOptions.TopLeft);
            Place(needs.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -82f), new Vector2(600f, 28f));
            var reward = Text("Reward", entry, font, "", 20, TextAlignmentOptions.TopLeft);
            reward.color = new Color(1f, 0.85f, 0.35f);
            Place(reward.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -112f), new Vector2(600f, 28f));

            var accept = MakeButton("AcceptButton", entry, font, GameTexts.Accept, out var acceptLabel);
            Place(accept.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-20f, 14f), new Vector2(160f, 56f));
            var status = Text("Status", entry, font, "", 18, TextAlignmentOptions.TopRight);
            status.color = new Color(1f, 0.6f, 0.35f);
            Place(status.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 1f), new Vector2(-20f, -20f), new Vector2(260f, 28f));

            var view = entry.gameObject.AddComponent<RequestEntryView>()
                .SetRef("_title", title)
                .SetRef("_creatureLine", line)
                .SetRef("_needs", needs)
                .SetRef("_reward", reward)
                .SetRef("_status", status)
                .SetRef("_acceptButton", accept)
                .SetRef("_acceptLabel", acceptLabel);

            var prefab = PrefabUtility.SaveAsPrefabAsset(entry.gameObject, $"{PrefabFolder}/UI_RequestEntry.prefab");
            Object.DestroyImmediate(entry.gameObject);
            return prefab.GetComponent<RequestEntryView>();
        }

        private static RectTransform Panel(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
            return (RectTransform)go.transform;
        }

        private static Button MakeButton(string name, Transform parent, TMP_FontAsset font, string label, out TextMeshProUGUI labelText)
        {
            var rect = Panel(name, parent, new Color(0.95f, 0.85f, 0.55f));
            var button = rect.gameObject.AddComponent<Button>();
            labelText = Text("Label", rect, font, label, 24, TextAlignmentOptions.Center);
            labelText.color = new Color(0.2f, 0.13f, 0.07f);
            Stretch(labelText.rectTransform);
            return button;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        // ── 도우미 ───────────────────────────────────────────

        private static Transform Group(string name, Transform parent, Vector3 worldPosition)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = worldPosition;
            return go.transform;
        }

        /// <summary>기본 도형 생성. localPosition은 부모 기준 좌표.</summary>
        internal static GameObject Prim(PrimitiveType type, string name, Transform parent, Vector3 localPosition, Vector3 scale,
            Material material, bool keepCollider = false)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            if (!keepCollider) Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        private static TextMeshProUGUI Text(string name, Transform parent, TMP_FontAsset font, string content, float size,
            TextAlignmentOptions alignment)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<TextMeshProUGUI>();
            if (font != null) text.font = font;
            text.text = content;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        private static void Place(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        internal static Material Mat(string name, Color color, bool transparent = false, Color? emission = null)
        {
            string path = $"{MaterialFolder}/M_{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;

            material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = $"M_{name}" };
            material.SetColor("_BaseColor", color);
            if (transparent)
            {
                material.SetFloat("_Surface", 1f);
                material.SetFloat("_Blend", 0f);
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetFloat("_ZWrite", 0f);
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }
            if (emission.HasValue)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emission.Value);
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static T LoadOrCreate<T>(string fileName) where T : ScriptableObject
        {
            string path = $"{ConfigFolder}/{fileName}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
    }

    /// <summary>private [SerializeField] 필드를 SerializedObject로 채우는 확장 메서드.</summary>
    internal static class SerializedFieldExtensions
    {
        public static T SetRef<T>(this T target, string field, Object value) where T : Object
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
            return target;
        }

        public static T SetAmountList<T>(this T target, string field, IList<ItemDefinition> items, IList<int> amounts)
            where T : Object
        {
            var so = new SerializedObject(target);
            var list = so.FindProperty(field);
            list.arraySize = items.Count;
            for (int i = 0; i < items.Count; i++)
            {
                var element = list.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("_item").objectReferenceValue = items[i];
                element.FindPropertyRelative("_amount").intValue = amounts[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            return target;
        }

        public static T SetRefList<T>(this T target, string field, IList<Object> values) where T : Object
        {
            var so = new SerializedObject(target);
            var list = so.FindProperty(field);
            list.arraySize = values.Count;
            for (int i = 0; i < values.Count; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
            return target;
        }
    }
}
