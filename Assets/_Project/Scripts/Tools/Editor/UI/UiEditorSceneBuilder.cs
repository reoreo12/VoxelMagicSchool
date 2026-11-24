// UI 에디터 씬(Scenes/Tools/UIEditor, 개발 전용)을 만들거나 연다. 카메라·EventSystem·UI 루트 + 폰트 비교판(크림 종이·갈색·어두운 배경)을 둔다.
// 이미 있으면 열기만 한다. 화면을 만들 때는 이 씬의 UI_Root 아래에서 조합하고 UI 작업대로 검사·굽기를 한다.
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using VoxelMagic.EditorTools;
using VoxelMagic.UI;

namespace VoxelMagic.Tools.EditorTools.UiWorkbench
{
    public static class UiEditorSceneBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/Tools/UIEditor.unity";
        // 비교 배경색(임시): 바 프레임 몸통 갈색, 바 빈 칸의 어두운 보라 갈색 (bar_ui README 팔레트)
        private static readonly Color Brown = new Color32(0xC4, 0x9A, 0x6C, 0xFF);
        private static readonly Color Dark = new Color32(0x2A, 0x1A, 0x28, 0xFF);
        private static readonly Color TextOnLight = new Color32(0x5C, 0x41, 0x28, 0xFF);
        private static readonly Color TextOnDark = new Color32(0xF4, 0xE8, 0xCC, 0xFF);
        private static readonly Color CameraBackground = new Color32(0x4A, 0x4A, 0x4A, 0xFF);

        [MenuItem("VoxelMagic/UI/UI 에디터 씬 열기")]
        public static void OpenOrCreate()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (File.Exists(ScenePath))
            {
                EditorSceneManager.OpenScene(ScenePath);
                return;
            }
            Create();
        }

        private static void Create()
        {
            UiWidgetPresetBuilder.FillThemeDefaults();
            UiRootPrefabBuilder.GetOrCreatePrefab();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            // 새 씬을 열면 쓰지 않는 에셋이 내려가므로 테마·프리팹은 씬을 연 뒤에 다시 읽는다.
            var theme = UiRootPrefabBuilder.GetOrCreateTheme();
            var rootPrefab = UiRootPrefabBuilder.GetOrCreatePrefab();

            var camera = new GameObject("Main Camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = CameraBackground;

            var events = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            events.transform.SetAsLastSibling();

            var root = (GameObject)PrefabUtility.InstantiatePrefab(rootPrefab, scene);
            var hudStatic = root.transform.Find("HudStatic");
            BuildFontBoard(hudStatic, theme);

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[UI 작업대] UI 에디터 씬을 만들었습니다: {ScenePath} (빌드 목록에는 넣지 않음)");
        }

        private static void BuildFontBoard(Transform parent, UiTheme theme)
        {
            var board = new GameObject("FontCompare", typeof(RectTransform));
            board.transform.SetParent(parent, false);
            var rect = (RectTransform)board.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(8, -8);
            rect.sizeDelta = Vector2.zero;

            var set = PixelFontAssetBuilder.GetOrCreateSet();
            var fonts = set.Fonts
                .Select(e => new FontCompareBoard.FontRow(e.AssetName,
                    AssetDatabase.LoadAssetAtPath<TMP_FontAsset>($"{set.OutputFolder.TrimEnd('/')}/{e.AssetName}.asset")))
                .ToList();

            var bands = new List<FontCompareBoard.Band>
            {
                Band(board.transform, "Paper", fonts.Count, TextOnLight, image =>
                {
                    var panel = image.gameObject.AddComponent<ThemedPanel>();
                    panel.SetSkin(UiPanelSkin.QuestPanel);
                    panel.SetTheme(theme);
                }),
                Band(board.transform, "Brown", fonts.Count, TextOnLight, image => image.color = Brown),
                Band(board.transform, "Dark", fonts.Count, TextOnDark, image => image.color = Dark),
            };

            var component = board.AddComponent<FontCompareBoard>();
            component.Setup(fonts, UiWorkbenchWindow.GetOrCreateConfig().FontSamples.ToList(), bands);
        }

        private static FontCompareBoard.Band Band(Transform parent, string name, int rowCount, Color textColor, System.Action<Image> style)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.raycastTarget = false;
            style(image);

            var rows = new List<TMP_Text>();
            for (int i = 0; i < rowCount; i++)
            {
                var row = new GameObject($"Row{i}", typeof(RectTransform));
                row.transform.SetParent(go.transform, false);
                var text = row.AddComponent<TextMeshProUGUI>();
                text.raycastTarget = false;
                rows.Add(text);
            }
            return new FontCompareBoard.Band((RectTransform)go.transform, textColor, rows);
        }
    }
}
