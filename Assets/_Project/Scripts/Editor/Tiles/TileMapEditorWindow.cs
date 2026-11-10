// 타일 맵 에디터 도구 창: 맵 목록(선택·보이기), 새 맵, 구운 맵 불러오기, 작업 대상 맵(고른 맵, 없으면 첫 맵)의 이름·칸 크기·굽기·보기 전환·칠하기 시작, 해 높이.
// 맵 = Map_<이름> 루트(TileMapDocument). 비교 오브젝트는 작업 대상 맵을 따라간다.
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;
using VoxelMagic.Core;
using VoxelMagic.Gameplay;

namespace VoxelMagic.EditorTools
{
    public sealed class TileMapEditorWindow : EditorWindow
    {
        private const float SunHeightMin = 5f;
        private const float SunHeightMax = 85f;

        private const string DefaultFillPath = "Assets/_Project/Art/Map/TileAssets/farm/floors/meadow_grass.asset";

        private TileMapConfig _config;
        private string _lastResult;
        private TileMapDocument _lastTarget;
        private Vector2 _scroll;
        private bool _showNewMap = true;
        private string _newName;
        private Vector2Int _newSize;
        private TileBase _newFill;
        private bool _newFillCleared;
        private bool _newHideOthers = true;
        private bool _showLoad = true;
        private bool _loadHideOthers = true;

        [MenuItem("VoxelMagic/타일 맵/타일 맵 에디터 창")]
        public static void Open() => GetWindow<TileMapEditorWindow>("타일 맵 에디터");

        /// <summary>작업 대상: 선택한 오브젝트가 속한 맵, 없으면 씬의 첫 맵</summary>
        public static TileMapDocument CurrentMap()
        {
            var selected = Selection.activeGameObject;
            if (selected != null)
            {
                var document = selected.GetComponentInParent<TileMapDocument>(true);
                if (document != null) return document;
            }
            return FindFirstObjectByType<TileMapDocument>(FindObjectsInactive.Exclude);
        }

        private void OnSelectionChange()
        {
            FollowTarget(CurrentMap());
            Repaint();
        }

        private void FollowTarget(TileMapDocument document)
        {
            if (document == null || document == _lastTarget) return;
            _lastTarget = document;
            var rig = FindFirstObjectByType<TileMapCompareRig>();
            if (rig != null && rig.Target != document)
            {
                Undo.RecordObject(rig, "비교 오브젝트 옮기기");
                rig.SetTarget(document);
            }
            if (GridPaintingState.palette != null && document.Paint != null)
                GridPaintingState.scenePaintTarget = document.Paint.gameObject;
        }

        private void OnGUI()
        {
            if (_config == null) _config = FarmTileSetBuilder.LoadOrCreateConfig();
            var document = CurrentMap();
            bool editorScene = document != null || FindFirstObjectByType<TileMapCompareRig>() != null;
            if (!editorScene)
            {
                EditorGUILayout.HelpBox("지금 열린 씬은 타일 맵 에디터 씬이 아닙니다.", MessageType.Info);
                if (GUILayout.Button("타일 맵 에디터 씬 열기")) OpenEditorScene();
                return;
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            DrawMapList(document);
            EditorGUILayout.Space();
            DrawNewMap();
            EditorGUILayout.Space();
            DrawLoadBaked();
            if (document == null || document.Paint == null)
            {
                EditorGUILayout.HelpBox("맵이 없습니다. 위의 '새 맵'으로 만드세요.", MessageType.Info);
                EditorGUILayout.EndScrollView();
                return;
            }
            FollowTarget(document);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"작업 대상: {document.MapName}", EditorStyles.largeLabel);
            EditorGUILayout.LabelField("맵 목록의 [선택]이나 Hierarchy에서 맵(Map_…)·그 안의 오브젝트를 고르면 대상이 바뀝니다", EditorStyles.miniLabel);
            EditorGUILayout.Space();
            DrawMap(document);
            EditorGUILayout.Space();
            DrawCellSize(document);
            EditorGUILayout.Space();
            DrawBake(document);
            EditorGUILayout.Space();
            DrawView(document);
            EditorGUILayout.Space();
            DrawSun();
            EditorGUILayout.EndScrollView();
        }

        private static void DrawMapList(TileMapDocument current)
        {
            EditorGUILayout.LabelField("맵 목록", EditorStyles.boldLabel);
            var maps = TileMapLibrary.AllMaps();
            foreach (var map in maps)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    bool visible = map.gameObject.activeSelf;
                    bool newVisible = EditorGUILayout.Toggle(visible, GUILayout.Width(16));
                    if (newVisible != visible) TileMapLibrary.SetVisible(map, newVisible);

                    var rect = map.PaintedCellRect();
                    var style = map == current ? EditorStyles.boldLabel : EditorStyles.label;
                    string bake = map.HasBake ? "구움" : "안 구움";
                    EditorGUILayout.LabelField($"{(map == current ? "▶ " : "")}{map.MapName}   {rect.width}×{rect.height}칸 · {bake}", style);
                    if (GUILayout.Button("선택", GUILayout.Width(48))) SelectMap(map);
                }
            }
            using (new EditorGUI.DisabledScope(current == null || maps.Count < 2))
            {
                if (GUILayout.Button("작업 대상 말고 다른 맵 모두 숨기기"))
                    foreach (var map in maps)
                        if (map != current) TileMapLibrary.SetVisible(map, false);
            }
            EditorGUILayout.LabelField("체크 = 보이기 (끄면 칠한 칸·구운 바닥 모두 숨김). 옆으로 치우려면 Map_… 루트의 Transform을 옮기세요", EditorStyles.miniLabel);
        }

        private static void SelectMap(TileMapDocument map)
        {
            if (!map.gameObject.activeSelf) TileMapLibrary.SetVisible(map, true);
            Selection.activeGameObject = map.gameObject;
            EditorGUIUtility.PingObject(map.gameObject);
            if (SceneView.lastActiveSceneView != null) SceneView.lastActiveSceneView.FrameSelected();
        }

        private void DrawNewMap()
        {
            _showNewMap = EditorGUILayout.Foldout(_showNewMap, "새 맵", true, EditorStyles.foldoutHeader);
            if (!_showNewMap) return;
            using (new EditorGUI.IndentLevelScope())
            {
                if (string.IsNullOrEmpty(_newName)) _newName = TileMapLibrary.SuggestName();
                if (_newSize == Vector2Int.zero) _newSize = _config.NewMapSize;
                if (_newFill == null && !_newFillCleared) _newFill = AssetDatabase.LoadAssetAtPath<TileBase>(DefaultFillPath);

                _newName = EditorGUILayout.TextField("이름", _newName);
                _newSize = EditorGUILayout.Vector2IntField("크기 (가로 × 세로 칸)", _newSize);
                EditorGUI.BeginChangeCheck();
                _newFill = (TileBase)EditorGUILayout.ObjectField("바탕 타일 (비우면 빈 맵)", _newFill, typeof(TileBase), false);
                if (EditorGUI.EndChangeCheck()) _newFillCleared = _newFill == null;
                _newHideOthers = EditorGUILayout.Toggle("다른 맵 숨기기", _newHideOthers);

                string problem = TileMapLibrary.ValidateNewName(_newName.Trim());
                if (problem != null) EditorGUILayout.HelpBox(problem, MessageType.Warning);
                using (new EditorGUI.DisabledScope(problem != null))
                {
                    if (GUILayout.Button("새 맵 만들기", GUILayout.Height(24)))
                    {
                        var created = TileMapLibrary.NewMap(_newName.Trim(), _newSize, _newFill, _newHideOthers, _config);
                        if (created != null)
                        {
                            SelectMap(created);
                            StartPainting(created);
                            _newName = null;
                        }
                        GUIUtility.ExitGUI();
                    }
                }
                EditorGUILayout.LabelField("기존 맵들 오른쪽에 겹치지 않게 놓입니다. Ctrl+Z로 취소", EditorStyles.miniLabel);
            }
        }

        private void DrawLoadBaked()
        {
            _showLoad = EditorGUILayout.Foldout(_showLoad, "구운 맵 불러오기 (다시 고치기)", true, EditorStyles.foldoutHeader);
            if (!_showLoad) return;
            using (new EditorGUI.IndentLevelScope())
            {
                _loadHideOthers = EditorGUILayout.Toggle("다른 맵 숨기기", _loadHideOthers);
                var baked = TileMapLibrary.BakedMaps();
                if (baked.Count == 0) EditorGUILayout.LabelField("구운 맵이 없습니다", EditorStyles.miniLabel);
                foreach (var (mapName, cells) in baked)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        var inScene = TileMapLibrary.FindMap(mapName);
                        string state = cells == null ? "칸 데이터 없음 — 다시 구워야 불러올 수 있음" : inScene != null ? "씬에 있음" : $"{cells.Count}칸";
                        EditorGUILayout.LabelField($"{mapName}   {state}");
                        if (inScene != null)
                        {
                            if (GUILayout.Button("선택", GUILayout.Width(64))) SelectMap(inScene);
                        }
                        else
                        {
                            using (new EditorGUI.DisabledScope(cells == null))
                                if (GUILayout.Button("불러오기", GUILayout.Width(64))) Load(cells);
                        }
                    }
                }

                var picked = TileMapLibrary.FindCells(Selection.activeObject);
                if (picked != null && TileMapLibrary.FindMap(picked.MapName) == null &&
                    GUILayout.Button($"Project에서 고른 맵 불러오기: {picked.MapName}"))
                    Load(picked);
                EditorGUILayout.LabelField("Project 창에서 <맵>_Floor.prefab·<맵>_Cells.asset·맵 폴더를 골라도 불러올 수 있습니다", EditorStyles.miniLabel);
            }
        }

        private void Load(TileMapCells cells)
        {
            var loaded = TileMapLibrary.LoadBakedMap(cells, _loadHideOthers, _config);
            if (loaded != null)
            {
                SelectMap(loaded);
                StartPainting(loaded);
            }
            GUIUtility.ExitGUI();
        }

        private static void DrawMap(TileMapDocument document)
        {
            EditorGUILayout.LabelField("맵", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            string name = EditorGUILayout.DelayedTextField("맵 이름", document.MapName);
            if (EditorGUI.EndChangeCheck()) TileMapLibrary.Rename(document, name.Trim());
            EditorGUILayout.LabelField("굽기 결과", TileFloorBaker.PrefabPath(document.MapName), EditorStyles.miniLabel);
        }

        private void DrawCellSize(TileMapDocument document)
        {
            EditorGUILayout.LabelField("칸 크기 (모든 맵 공용)", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            float size = EditorGUILayout.Slider("한 칸 (m)", _config.CellSize, 0.25f, 4f);
            if (EditorGUI.EndChangeCheck()) SetCellSize(size);

            using (new EditorGUILayout.HorizontalScope())
            {
                foreach (var preset in _config.CellSizePresets)
                    if (GUILayout.Button($"{preset:0.###} m")) SetCellSize(preset);
            }
            EditorGUILayout.LabelField($"아틀라스 여백 {_config.AtlasPadding}px · 조각 {_config.ChunkSize}칸 · 타일 {_config.TilePixels}px");
            if (GUILayout.Button("설정 에셋 선택 (TileMapConfig)")) Selection.activeObject = _config;

            if (document.HasBake && !Mathf.Approximately(document.BakedCellSize, _config.CellSize))
                EditorGUILayout.HelpBox($"구운 프리팹은 칸 {document.BakedCellSize:0.###} m입니다. 이 씬에서는 바로 맞춰 보이지만, 게임 씬의 프리팹에 반영하려면 다시 구우세요.", MessageType.Warning);
        }

        private void SetCellSize(float size)
        {
            Undo.RecordObject(_config, "칸 크기");
            _config.SetCellSize(size);
            EditorUtility.SetDirty(_config);
            SceneView.RepaintAll();
        }

        private void DrawBake(TileMapDocument document)
        {
            EditorGUILayout.LabelField("굽기 (작업 대상 맵만)", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            bool bakeCollider = EditorGUILayout.ToggleLeft($"충돌체 함께 굽기 (칠한 범위의 BoxCollider, 두께 {_config.ColliderThickness:0.##} m) — 게임 씬에서 바로 밟고 걸을 수 있음", document.BakeCollider);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(document, "충돌체 함께 굽기");
                document.SetBakeCollider(bakeCollider);
                EditorUtility.SetDirty(document);
            }
            if (GUILayout.Button($"굽기: {document.MapName}", GUILayout.Height(28)))
            {
                var result = TileFloorBaker.Bake(document, _config, askBeforeOverwrite: true);
                if (result != null)
                {
                    TileFloorBaker.SetView(document, showBaked: true);
                    _lastResult = $"{document.MapName}: 칸 {result.Cells} · 타일 {result.UniqueTiles}종 · 조각 {result.Chunks} · 아틀라스 {result.AtlasSize.x}×{result.AtlasSize.y} · 바닥 종류 {result.SurfaceKinds}종" +
                                  (result.UnmappedCells > 0 ? $" · 표에 없는 칸 {result.UnmappedCells} (콘솔 참고)" : "") +
                                  (result.HasCollider ? $" · 충돌체 {result.ColliderCount}개 (칸 모양)" : " · 충돌체 없음");
                }
                GUIUtility.ExitGUI();
            }
            if (!string.IsNullOrEmpty(_lastResult)) EditorGUILayout.LabelField(_lastResult, EditorStyles.wordWrappedMiniLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("바닥 종류 표", _config.SurfaceTable != null ? $"{_config.SurfaceTable.Count}줄" : "없음");
                if (_config.SurfaceTable == null)
                {
                    if (GUILayout.Button("만들기", GUILayout.Width(64))) GroundSurfaceBuilder.Build(_config);
                }
                else if (GUILayout.Button("열기", GUILayout.Width(64))) Selection.activeObject = _config.SurfaceTable;
            }
            if (document.HasBake && GUILayout.Button("구운 프리팹 선택")) Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(TileFloorBaker.PrefabPath(document.MapName));

            var bakedCells = TileMapLibrary.BakedCellsOf(document);
            using (new EditorGUI.DisabledScope(bakedCells == null))
            {
                if (GUILayout.Button("구운 상태로 되돌리기 (칠한 칸을 마지막 굽기 때로)") &&
                    EditorUtility.DisplayDialog("구운 상태로 되돌리기",
                        $"{document.MapName}의 칠한 칸을 마지막으로 구운 상태({bakedCells.Count}칸)로 바꿉니다.\n그 뒤에 칠한 내용은 사라집니다 (Ctrl+Z로 취소 가능). 굽기 결과 파일은 바뀌지 않습니다.",
                        "되돌리기", "취소"))
                {
                    TileMapLibrary.RevertToBaked(document);
                    TileFloorBaker.SetView(document, showBaked: false);
                    GUIUtility.ExitGUI();
                }
            }
            if (bakedCells == null)
                EditorGUILayout.LabelField(document.HasBake ? "되돌리기: 칸 데이터가 없는 옛 굽기 결과라 쓸 수 없음 (다시 구우면 생김)" : "되돌리기: 아직 구운 적 없음", EditorStyles.miniLabel);
        }

        private static void DrawView(TileMapDocument document)
        {
            EditorGUILayout.LabelField("보기", EditorStyles.boldLabel);
            if (GUILayout.Button("칠하기 시작 (Tile Palette 열기 · 팔레트 Farm · 대상 = 이 맵의 Paint)", GUILayout.Height(24)))
                StartPainting(document);
            bool showingBaked = document.BakedFloor != null && document.BakedFloor.activeSelf;
            using (new EditorGUI.DisabledScope(document.BakedFloor == null))
            {
                int pick = GUILayout.Toolbar(showingBaked ? 1 : 0, new[] { "칠하기 (Tilemap)", "구운 바닥" });
                if (pick != (showingBaked ? 1 : 0)) TileFloorBaker.SetView(document, pick == 1);
            }
            EditorGUILayout.HelpBox("칠할 때는 '칠하기' 보기. 다 칠했으면 굽기 → 게임 씬에는 구운 프리팹만 놓습니다. 맵을 옮기거나 숨길 때는 Map_… 루트를 옮기거나 끄세요 (구운 바닥도 함께).", MessageType.None);
        }

        private static void StartPainting(TileMapDocument document)
        {
            TileFloorBaker.SetView(document, showBaked: false);
            EditorApplication.ExecuteMenuItem("Window/2D/Tile Palette");
            var palette = AssetDatabase.LoadAssetAtPath<GameObject>(FarmTileSetBuilder.PalettePath);
            if (palette != null) GridPaintingState.palette = palette;
            GridPaintingState.scenePaintTarget = document.Paint.gameObject;
            Selection.activeGameObject = document.Paint.gameObject;
        }

        private static void DrawSun()
        {
            EditorGUILayout.LabelField("해 (그림자 확인용, 씬 조명만 바뀜)", EditorStyles.boldLabel);
            var sun = FindSun();
            if (sun == null) { EditorGUILayout.LabelField("Directional Light 없음"); return; }
            var euler = sun.transform.eulerAngles;
            EditorGUI.BeginChangeCheck();
            float height = EditorGUILayout.Slider("해 높이 (°)", Mathf.Clamp(euler.x, SunHeightMin, SunHeightMax), SunHeightMin, SunHeightMax);
            float yaw = EditorGUILayout.Slider("해 방향 (°)", euler.y, 0f, 360f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(sun.transform, "해 각도");
                sun.transform.rotation = Quaternion.Euler(height, yaw, 0f);
            }
        }

        private static Light FindSun()
        {
            foreach (var light in FindObjectsByType<Light>(FindObjectsSortMode.None))
                if (light.type == LightType.Directional) return light;
            return null;
        }

        [MenuItem("VoxelMagic/타일 맵/타일 맵 에디터 씬 열기")]
        private static void OpenEditorScene()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(TileMapEditorSceneBuilder.ScenePath) == null)
            {
                EditorUtility.DisplayDialog("타일 맵 에디터", "씬이 없습니다. 메뉴 VoxelMagic/타일 맵/타일 맵 에디터 씬 만들기로 먼저 만드세요.", "확인");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(TileMapEditorSceneBuilder.ScenePath);
            Open();
        }

        private void OnInspectorUpdate() => Repaint();
    }
}
