// 뒷산 채집 밸런스 도구: 서식지 종류별 슬롯 수·재료 개수(최소~최대)·확률을 GatheringConfig에서 조절하는 창과,
// 슬롯 수를 기본 도형 프리팹·씬에 놓인 서식지(기능 붙이기로 만든 것 포함)에 맞추는 기능. 개수·확률은 데이터라 바로 적용된다.
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VoxelMagic.Core;
using VoxelMagic.Gameplay;

namespace VoxelMagic.EditorTools
{
    public static class GatheringBalanceTool
    {
        public const string ConfigPath = "Assets/_Project/Data/Config/GatheringConfig.asset";
        private const string HabitatPrefabFormat = "Assets/_Project/Prefabs/Mountain/Habitat_{0}.prefab";
        private const string ItemMaterialPath = "Assets/_Project/Materials/Mountain/M_GatherItem.mat";

        // 처음 기본 도형 서식지의 슬롯 배치 (서식지 로컬, +Z = 길 쪽). 앞에서부터 슬롯 수만큼 쓴다.
        private static readonly Dictionary<GatherHabitatKind, Vector3[]> DefaultLayouts = new Dictionary<GatherHabitatKind, Vector3[]>
        {
            { GatherHabitatKind.TreeShade, new[] { new Vector3(-0.9f, 0f, 0.9f), new Vector3(0.9f, 0f, 0.9f), new Vector3(-1.1f, 0f, -0.5f), new Vector3(1.1f, 0f, -0.4f) } },
            { GatherHabitatKind.Bush, new[] { new Vector3(0f, 0f, 0.95f), new Vector3(1.2f, 0f, 0.5f), new Vector3(-1.2f, 0f, 0.55f) } },
            { GatherHabitatKind.RockCrevice, new[] { new Vector3(0f, 0f, 0.1f), new Vector3(0f, 0f, 0.95f), new Vector3(-1.1f, 0f, 1f) } },
            { GatherHabitatKind.Clearing, new[] { new Vector3(0f, 0f, 0.6f), new Vector3(0.8f, 0f, -0.3f), new Vector3(-0.8f, 0f, -0.3f), new Vector3(0f, 0f, -0.9f) } },
        };

        public static GatheringConfig LoadConfig() => AssetDatabase.LoadAssetAtPath<GatheringConfig>(ConfigPath);

        /// <summary>목록에 없는 서식지 종류를 기본값(슬롯 2·0~1개·확률 0.5, 가정)으로 채운다.</summary>
        public static bool EnsureDefaults(GatheringConfig config)
        {
            var list = config.Habitats.Where(h => h != null).ToList();
            bool changed = false;
            foreach (GatherHabitatKind kind in System.Enum.GetValues(typeof(GatherHabitatKind)))
            {
                if (list.Any(h => h.Kind == kind)) continue;
                list.Add(new HabitatBalance(kind, 2, 0, 1, 0.5f, DefaultLayouts[kind]));
                changed = true;
            }
            if (!changed) return false;
            Undo.RecordObject(config, "채집 밸런스 기본값");
            config.SetHabitats(list);
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            return true;
        }

        /// <summary>슬롯 배치의 i번째 자리. 배치 목록보다 많으면 같은 반지름 원 위에 이어서 놓는다.</summary>
        public static Vector3 LayoutPoint(HabitatBalance balance, int index)
        {
            var layout = balance.SlotLayout.Count > 0 ? balance.SlotLayout : (IReadOnlyList<Vector3>)DefaultLayouts[balance.Kind];
            if (index < layout.Count) return layout[index];
            float radius = layout.Max(p => new Vector2(p.x, p.z).magnitude);
            float angle = index * 137.5f * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(angle) * radius, 0f, Mathf.Cos(angle) * radius);
        }

        public static float LayoutRadius(HabitatBalance balance, int count)
        {
            float radius = 0f;
            for (int i = 0; i < count; i++)
            {
                var p = LayoutPoint(balance, i);
                radius = Mathf.Max(radius, new Vector2(p.x, p.z).magnitude);
            }
            return Mathf.Max(0.01f, radius);
        }

        /// <summary>슬롯 수를 기본 도형 프리팹과 지금 씬의 모든 서식지에 맞춘다. 바뀐 서식지 수를 돌려준다.</summary>
        public static int ApplySlotCounts(GatheringConfig config)
        {
            int changed = 0;
            var itemMaterial = AssetDatabase.LoadAssetAtPath<Material>(ItemMaterialPath);

            // 1) 기본 도형 프리팹 → 씬의 프리팹 인스턴스도 따라온다
            foreach (GatherHabitatKind kind in System.Enum.GetValues(typeof(GatherHabitatKind)))
            {
                string path = string.Format(HabitatPrefabFormat, kind);
                if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null) continue;
                var contents = PrefabUtility.LoadPrefabContents(path);
                var habitat = contents.GetComponent<GatherHabitat>();
                if (habitat != null && Resize(habitat, config.For(kind), itemMaterial, inScene: false))
                {
                    PrefabUtility.SaveAsPrefabAsset(contents, path);
                    changed++;
                }
                PrefabUtility.UnloadPrefabContents(contents);
            }

            // 2) 씬의 서식지: 프리팹이 아닌 것(기능 붙이기로 만든 FBX 서식지)은 직접 맞추고, 모든 슬롯을 지면 높이로
            var scene = EditorSceneManager.GetActiveScene();
            foreach (var habitat in Object.FindObjectsByType<GatherHabitat>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (habitat.gameObject.scene != scene) continue;
                bool isPrefabInstance = PrefabUtility.IsPartOfPrefabInstance(habitat);
                if (!isPrefabInstance && Resize(habitat, config.For(habitat.Kind), itemMaterial, inScene: true)) changed++;
                foreach (var pickup in Slots(habitat))
                {
                    var grounded = BackMountainAttachTool.Ground(pickup.transform.position, habitat.transform);
                    if (Mathf.Abs(grounded.y - pickup.transform.position.y) < 0.001f) continue;
                    Undo.RecordObject(pickup.transform, "슬롯 지면 맞추기");
                    pickup.transform.position = grounded;
                }
            }
            EditorSceneManager.MarkSceneDirty(scene);
            return changed;
        }

        public static List<GatherPickup> Slots(GatherHabitat habitat)
        {
            var list = new List<GatherPickup>();
            var array = new SerializedObject(habitat).FindProperty("_slots");
            for (int i = 0; i < array.arraySize; i++)
                if (array.GetArrayElementAtIndex(i).objectReferenceValue is GatherPickup p) list.Add(p);
            return list;
        }

        /// <summary>서식지 슬롯을 목표 수로: 많으면 뒤에서부터 지우고, 적으면 배치 모양대로(지금 서식지 크기에 맞춰) 더한다.</summary>
        private static bool Resize(GatherHabitat habitat, HabitatBalance balance, Material itemMaterial, bool inScene)
        {
            var slots = Slots(habitat);
            int target = balance.SlotCount;
            if (slots.Count == target) return false;

            if (inScene) Undo.RecordObject(habitat, "슬롯 수 맞추기");
            for (int i = slots.Count - 1; i >= target; i--)
            {
                if (inScene) Undo.DestroyObjectImmediate(slots[i].gameObject);
                else Object.DestroyImmediate(slots[i].gameObject);
                slots.RemoveAt(i);
            }

            if (slots.Count < target)
            {
                var slotRoot = habitat.transform.Find("Slots");
                if (slotRoot == null)
                {
                    slotRoot = new GameObject("Slots").transform;
                    slotRoot.SetParent(habitat.transform, false);
                    if (inScene) Undo.RegisterCreatedObjectUndo(slotRoot.gameObject, "슬롯 수 맞추기");
                }
                // 지금 슬롯이 배치 모양보다 넓게 퍼져 있으면(FBX에 맞춰 늘린 서식지) 같은 비율로 늘린다
                float scale = 1f;
                if (slots.Count > 0)
                {
                    float current = slots.Max(s => new Vector2(s.transform.localPosition.x, s.transform.localPosition.z).magnitude);
                    scale = Mathf.Max(0.5f, current / LayoutRadius(balance, slots.Count));
                }
                for (int i = slots.Count; i < target; i++)
                {
                    var p = LayoutPoint(balance, i) * scale;
                    var pickup = BackMountainGatheringBuilder.MakeSlot(slotRoot, i, new Vector3(p.x, 0f, p.z), itemMaterial);
                    if (inScene) Undo.RegisterCreatedObjectUndo(pickup.gameObject, "슬롯 수 맞추기");
                    slots.Add(pickup);
                }
            }

            for (int i = 0; i < slots.Count; i++) slots[i].name = $"Slot_{i}";
            var so = new SerializedObject(habitat);
            var array = so.FindProperty("_slots");
            array.arraySize = slots.Count;
            for (int i = 0; i < slots.Count; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = slots[i];
            so.ApplyModifiedPropertiesWithoutUndo();
            if (inScene) EditorUtility.SetDirty(habitat);
            return true;
        }
    }

    public sealed class GatheringBalanceWindow : EditorWindow
    {
        private static readonly string[] KindNames = { "큰 나무 밑 그늘", "덤불", "바위틈", "초입 평지" };

        private GatheringConfig _config;
        private Vector2 _scroll;
        private bool _showLayout;

        [MenuItem("VoxelMagic/뒷산/채집 밸런스")]
        private static void Open() => GetWindow<GatheringBalanceWindow>("채집 밸런스");

        private void OnEnable() => _config = GatheringBalanceTool.LoadConfig();

        private void OnGUI()
        {
            _config = (GatheringConfig)EditorGUILayout.ObjectField("밸런스 데이터", _config, typeof(GatheringConfig), false);
            if (_config == null)
            {
                EditorGUILayout.HelpBox($"{GatheringBalanceTool.ConfigPath}이 없습니다.", MessageType.Warning);
                return;
            }
            if (System.Enum.GetValues(typeof(GatherHabitatKind)).Length > _config.Habitats.Count(h => h != null)
                && GUILayout.Button("빠진 서식지 종류를 기본값으로 채우기 (슬롯 2 · 0~1개 · 확률 0.5)"))
                GatheringBalanceTool.EnsureDefaults(_config);

            EditorGUILayout.HelpBox("개수·확률은 다시 배치할 때 바로 쓰입니다 (플레이 중이면 F1 새 하루).\n" +
                                    "슬롯 수를 바꾸면 아래 '슬롯 수 적용'을 눌러야 프리팹·씬·기능 붙이기 도구에 반영됩니다.\n" +
                                    "의뢰에 필요한 재료는 이와 별개로 맵 전체에 최소 1개 보장됩니다.", MessageType.Info);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            var so = new SerializedObject(_config);
            var list = so.FindProperty("_habitats");
            var counts = SceneSlotCounts();
            for (int i = 0; i < list.arraySize; i++)
            {
                var entry = list.GetArrayElementAtIndex(i);
                var kind = (GatherHabitatKind)entry.FindPropertyRelative("_kind").enumValueIndex;
                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField(KindNames[(int)kind], EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(entry.FindPropertyRelative("_slotCount"), new GUIContent("슬롯 수"));
                EditorGUILayout.PropertyField(entry.FindPropertyRelative("_minItems"), new GUIContent("최소 개수"));
                EditorGUILayout.PropertyField(entry.FindPropertyRelative("_maxItems"), new GUIContent("최대 개수"));
                EditorGUILayout.PropertyField(entry.FindPropertyRelative("_extraChance"), new GUIContent("추가분 확률"));
                if (_showLayout) EditorGUILayout.PropertyField(entry.FindPropertyRelative("_slotLayout"), new GUIContent("슬롯 배치 모양"), true);

                var balance = _config.For(kind);
                int slots = balance.SlotCount;
                float empty = balance.MinItems > 0 ? 0f : Mathf.Pow(1f - balance.ExtraChance, Mathf.Min(balance.MaxItems, slots) - balance.MinItems);
                float average = Mathf.Min(balance.ExpectedItems, slots);
                EditorGUILayout.LabelField($"→ 서식지 하나에 평균 {average:0.##}개, 비어 있을 확률 {empty * 100f:0}%");
                if (counts.TryGetValue(kind, out var placed))
                {
                    string mismatch = placed.Any(c => c != slots) ? "  ⚠ 슬롯 수 적용 필요" : "";
                    EditorGUILayout.LabelField($"   씬에 {placed.Count}개 (지금 슬롯 {string.Join("·", placed.Distinct())}칸){mismatch}");
                }
                EditorGUI.indentLevel--;
            }
            so.ApplyModifiedProperties();
            EditorGUILayout.EndScrollView();

            _showLayout = EditorGUILayout.Toggle("슬롯 배치 모양 보기", _showLayout);
            EditorGUILayout.Space(6);
            if (GUILayout.Button("슬롯 수 적용 (기본 도형 프리팹 + 지금 씬의 서식지)", GUILayout.Height(28))
                && EditorUtility.DisplayDialog("슬롯 수 적용",
                    "기본 도형 서식지 프리팹 4종과 지금 씬의 서식지 슬롯을 위 '슬롯 수'에 맞춥니다.\n줄이면 뒤쪽 슬롯부터 지웁니다. 프리팹 변경은 Ctrl+Z로 되돌릴 수 없습니다.", "적용", "취소"))
            {
                int changed = GatheringBalanceTool.ApplySlotCounts(_config);
                Debug.Log($"[채집 밸런스] 슬롯 수 적용: 프리팹·서식지 {changed}곳 바뀜 (씬 저장은 직접)");
            }
        }

        private static Dictionary<GatherHabitatKind, List<int>> SceneSlotCounts()
        {
            var result = new Dictionary<GatherHabitatKind, List<int>>();
            foreach (var habitat in Object.FindObjectsByType<GatherHabitat>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                if (!result.TryGetValue(habitat.Kind, out var list)) result[habitat.Kind] = list = new List<int>();
                list.Add(habitat.SlotCount);
            }
            return result;
        }
    }
}
