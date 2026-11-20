// UI 작업대 창(메뉴 VoxelMagic/UI/UI 작업대, UI_SPEC 3장). 해상도 프리뷰 · 규칙 검사와 프리팹 굽기 · 테마 편집과 에셋 도구를 한곳에 둔다.
// 배치와 편집은 Unity 씬·프리팹 모드에서 하고, 이 창은 확인·저장을 돕는다.
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using VoxelMagic.EditorTools;
using VoxelMagic.UI;

namespace VoxelMagic.Tools.EditorTools.UiWorkbench
{
    public sealed class UiWorkbenchWindow : EditorWindow
    {
        public const string ConfigPath = "Assets/_Project/Data/Tools/UI/UiWorkbenchConfig.asset";
        private static readonly string[] Tabs = { "해상도", "검사 · 굽기", "테마 · 도구" };

        private int _tab;
        private Vector2 _scroll;
        private UiWorkbenchConfig _config;
        private UiTheme _theme;
        private Editor _themeEditor;
        private List<UiLayoutValidator.Issue> _issues = new List<UiLayoutValidator.Issue>();
        private GameObject _checkedRoot;

        [MenuItem("VoxelMagic/UI/UI 작업대")]
        public static void Open() => GetWindow<UiWorkbenchWindow>("UI 작업대");

        public static UiWorkbenchConfig GetOrCreateConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<UiWorkbenchConfig>(ConfigPath);
            if (config != null) return config;
            Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath));
            config = CreateInstance<UiWorkbenchConfig>();
            AssetDatabase.CreateAsset(config, ConfigPath);
            AssetDatabase.SaveAssets();
            return config;
        }

        private void OnEnable()
        {
            _config = GetOrCreateConfig();
            _theme = UiRootPrefabBuilder.GetOrCreateTheme();
        }

        private void OnDisable()
        {
            if (_themeEditor != null) DestroyImmediate(_themeEditor);
        }

        private void OnSelectionChange() => Repaint();

        private void OnGUI()
        {
            _tab = GUILayout.Toolbar(_tab, Tabs);
            EditorGUILayout.Space();
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            switch (_tab)
            {
                case 0: DrawResolutions(); break;
                case 1: DrawValidation(); break;
                default: DrawThemeAndTools(); break;
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawResolutions()
        {
            EditorGUILayout.HelpBox("버튼을 누르면 Game View 해상도가 바뀝니다. 배율 = 화면 높이 ÷ 기준 높이의 내림, 가상 크기 = 화면 ÷ 배율.", MessageType.None);
            int reference = Mathf.Max(1, _theme.Canvas.ReferenceHeight);
            string aspect = null;
            foreach (var r in _config.Resolutions)
            {
                if (r.Aspect != aspect)
                {
                    aspect = r.Aspect;
                    EditorGUILayout.LabelField(aspect, EditorStyles.boldLabel);
                }
                int scale = PixelCanvasScaler.ComputeScale(r.Height, reference, _theme.Canvas.MinScale);
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(r.Label, GUILayout.Width(110))) GameViewSizeSwitcher.TrySet(r.Width, r.Height, $"{r.Aspect} {r.Label}");
                    GUILayout.Label($"{scale}×  →  가상 {r.Width / scale}×{r.Height / scale}");
                }
            }
        }

        private void DrawValidation()
        {
            var target = Selection.activeGameObject;
            EditorGUILayout.LabelField("대상", target != null ? target.name : "(Hierarchy에서 UI 오브젝트를 고르세요)");
            using (new EditorGUI.DisabledScope(target == null))
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("검사"))
                {
                    _checkedRoot = target;
                    _issues = UiLayoutValidator.Validate(target);
                }
                if (GUILayout.Button("프리팹으로 굽기")) Bake(target);
            }

            if (_checkedRoot == null) return;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"{_checkedRoot.name}: 문제 {_issues.Count}개", EditorStyles.boldLabel);
            if (_issues.Count == 0) EditorGUILayout.HelpBox("규칙 위반이 없습니다.", MessageType.Info);
            foreach (var issue in _issues)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(issue.Target != null ? issue.Target.name : "-", GUILayout.Width(140))) UiLayoutValidator.SelectTarget(issue);
                    GUILayout.Label(issue.Message, EditorStyles.wordWrappedLabel);
                }
            }
        }

        private void Bake(GameObject target)
        {
            var issues = UiLayoutValidator.Validate(target);
            if (issues.Count > 0 && !EditorUtility.DisplayDialog("UI 작업대", $"규칙 위반이 {issues.Count}개 있습니다. 그래도 구울까요?", "굽기", "취소"))
            {
                _checkedRoot = target;
                _issues = issues;
                return;
            }
            Directory.CreateDirectory(_config.BakeFolder);
            var path = $"{_config.BakeFolder.TrimEnd('/')}/{target.name}.prefab";
            if (File.Exists(path) && !PrefabUtility.IsPartOfPrefabInstance(target) &&
                !EditorUtility.DisplayDialog("UI 작업대", $"{path}가 이미 있습니다. 덮어쓸까요?", "덮어쓰기", "취소"))
                return;
            var prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(target, path, InteractionMode.UserAction);
            EditorGUIUtility.PingObject(prefab);
            Debug.Log($"[UI 작업대] 프리팹 저장: {path}");
        }

        private void DrawThemeAndTools()
        {
            EditorGUILayout.LabelField("에셋 도구", EditorStyles.boldLabel);
            if (GUILayout.Button("임포트 규칙 적용 (Art/UI)")) Debug.Log(UiImportRuleApplier.ApplyAll());
            if (GUILayout.Button("픽셀 폰트 다시 굽기")) PixelFontAssetBuilder.BakeAll();
            if (GUILayout.Button("테마 기본값 채우기 · 위젯 프리셋 다시 만들기"))
            {
                UiWidgetPresetBuilder.FillThemeDefaults();
                UiWidgetPresetBuilder.BuildPresets(_theme);
            }
            if (GUILayout.Button("UI 에디터 씬 열기 (없으면 만들기)")) UiEditorSceneBuilder.OpenOrCreate();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("테마", EditorStyles.boldLabel);
            Editor.CreateCachedEditor(_theme, null, ref _themeEditor);
            _themeEditor.OnInspectorGUI();
        }
    }
}
