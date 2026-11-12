// 직접 배치한 FBX에 뒷산 기능을 붙이는 도구: 선택한 오브젝트 위에 기능 루트(지면 높이)를 씌우고 FBX를 그 아래 Visual로 옮긴 뒤
// 서식지(줍기 슬롯)·포획 풀숲·의뢰 게시판·크리처 컴포넌트를 연결한다. 위치·회전·크기와 FBX에 붙여 둔 콜라이더는 그대로 둔다.
// 서식지 슬롯: FBX 아래에 이름이 "Slot"으로 시작하는 빈 오브젝트가 있으면 그 자리, 없으면 채집 밸런스(GatheringConfig)의 슬롯 수·배치를 FBX 크기에 맞춰 늘린다.
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VoxelMagic.Core;
using VoxelMagic.Gameplay;

namespace VoxelMagic.EditorTools
{
    public static class BackMountainAttachTool
    {
        private const string Menu = "VoxelMagic/뒷산/선택한 오브젝트에 기능 붙이기/";
        private const string ItemMaterialPath = "Assets/_Project/Materials/Mountain/M_GatherItem.mat";
        // 기본 도형 슬롯 배치를 FBX 크기에 맞출 때: 슬롯 고리 반지름 = FBX 가로 반지름 × 이 값
        private const float SlotRingRatio = 0.8f;
        private const float SlotAnchorHeight = 0.9f;
        private const float TuftAnchorHeight = 1.3f;
        private const float PromptAboveTop = 0.4f;

        internal enum Feature { Habitat, Tuft, Board, Creature }

        [MenuItem(Menu + "서식지 - 큰 나무 밑 그늘")] private static void TreeShade() => Run(Feature.Habitat, GatherHabitatKind.TreeShade);
        [MenuItem(Menu + "서식지 - 덤불")] private static void Bush() => Run(Feature.Habitat, GatherHabitatKind.Bush);
        [MenuItem(Menu + "서식지 - 바위틈")] private static void Rock() => Run(Feature.Habitat, GatherHabitatKind.RockCrevice);
        [MenuItem(Menu + "서식지 - 초입 평지")] private static void Clearing() => Run(Feature.Habitat, GatherHabitatKind.Clearing);
        [MenuItem(Menu + "포획 풀숲")] private static void Tuft() => Run(Feature.Tuft, default);
        [MenuItem(Menu + "의뢰 게시판")] private static void Board() => Run(Feature.Board, default);
        [MenuItem(Menu + "크리처")] private static void Creature() => Run(Feature.Creature, default);

        [MenuItem(Menu + "서식지 - 큰 나무 밑 그늘", true)]
        [MenuItem(Menu + "서식지 - 덤불", true)]
        [MenuItem(Menu + "서식지 - 바위틈", true)]
        [MenuItem(Menu + "서식지 - 초입 평지", true)]
        [MenuItem(Menu + "포획 풀숲", true)]
        [MenuItem(Menu + "의뢰 게시판", true)]
        [MenuItem(Menu + "크리처", true)]
        private static bool HasSceneSelection() => Selection.gameObjects.Any(g => g.scene.IsValid());

        private static void Run(Feature feature, GatherHabitatKind kind)
        {
            var proto = GameObject.Find("[Prototype]");
            if (proto == null)
            {
                EditorUtility.DisplayDialog("기능 붙이기", "씬에 [Prototype]이 없습니다. 뒷산 씬에서 실행하세요.", "확인");
                return;
            }
            // 선택 안에서 가장 위의 오브젝트만 (부모·자식을 함께 고르면 부모만)
            var targets = Selection.gameObjects.Where(g => g.scene.IsValid())
                .Where(g => !Selection.gameObjects.Any(o => o != g && g.transform.IsChildOf(o.transform))).ToList();
            var created = new List<Object>();
            foreach (var target in targets)
            {
                var root = Convert(target, feature, kind, proto.transform);
                if (root != null) created.Add(root);
            }
            Selection.objects = created.ToArray();
            if (created.Count > 0) EditorSceneManager.MarkSceneDirty(((GameObject)created[0]).scene);
            Debug.Log($"[기능 붙이기] {created.Count}개에 '{Label(feature, kind)}' 붙임 (씬 저장은 직접)");
        }

        /// <summary>테스트·다른 도구에서 쓰는 진입점. 만든 기능 루트를 돌려준다.</summary>
        internal static GameObject Convert(GameObject target, Feature feature, GatherHabitatKind kind, Transform proto)
        {
            if (target.GetComponentInParent<IInteractable>(true) != null)
            {
                Debug.LogWarning($"[기능 붙이기] '{target.name}'에는 이미 상호작용 기능이 있어 건너뜀", target);
                return null;
            }
            var bounds = RendererBounds(target);
            var basePoint = Ground(target.transform.position, target.transform);

            var root = new GameObject($"{target.name} ({Label(feature, kind)})");
            Undo.RegisterCreatedObjectUndo(root, "기능 붙이기");
            root.transform.SetPositionAndRotation(basePoint, FacingRotation(feature, basePoint));
            Undo.SetTransformParent(root.transform, Group(proto, feature), "기능 붙이기");
            var visual = new GameObject("Visual").transform;
            Undo.RegisterCreatedObjectUndo(visual.gameObject, "기능 붙이기");
            visual.SetParent(root.transform, false);
            var slotMarkers = SlotMarkers(target);
            Undo.SetTransformParent(target.transform, visual, "기능 붙이기");

            switch (feature)
            {
                case Feature.Habitat: AttachHabitat(root, kind, bounds, slotMarkers, target.transform); break;
                case Feature.Tuft: AttachTuft(root, visual); break;
                case Feature.Board: AttachInteractable<RequestBoard>(root, bounds); break;
                case Feature.Creature: AttachInteractable<CreatureNpc>(root, bounds); break;
            }
            return root;
        }

        // ---------- 서식지 ----------
        private static void AttachHabitat(GameObject root, GatherHabitatKind kind, Bounds bounds, List<Transform> markers, Transform target)
        {
            var positions = markers.Count > 0
                ? markers.Select(m => m.position).ToList()
                : TemplateSlots(kind, root.transform, bounds);
            var slotRoot = new GameObject("Slots").transform;
            slotRoot.SetParent(root.transform, false);
            var itemMaterial = AssetDatabase.LoadAssetAtPath<Material>(ItemMaterialPath);
            var pickups = new GatherPickup[positions.Count];
            for (int i = 0; i < positions.Count; i++)
            {
                pickups[i] = BackMountainGatheringBuilder.MakeSlot(slotRoot, i, Vector3.zero, itemMaterial);
                pickups[i].transform.position = Ground(positions[i], target);
                SetFloat(pickups[i], "_defaultAnchorHeight", SlotAnchorHeight);
            }
            // 표시용 빈 오브젝트는 쓰고 나면 지운다 (자리는 Slots로 옮겨짐)
            foreach (var marker in markers) Undo.DestroyObjectImmediate(marker.gameObject);

            var habitat = Undo.AddComponent<GatherHabitat>(root);
            var so = new SerializedObject(habitat);
            so.FindProperty("_kind").enumValueIndex = (int)kind;
            var list = so.FindProperty("_slots");
            list.arraySize = pickups.Length;
            for (int i = 0; i < pickups.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = pickups[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>채집 밸런스(GatheringConfig)의 슬롯 수·배치 모양을 FBX 가로 크기에 맞춰 늘린 자리.</summary>
        private static List<Vector3> TemplateSlots(GatherHabitatKind kind, Transform root, Bounds bounds)
        {
            var config = GatheringBalanceTool.LoadConfig();
            var balance = config != null ? config.For(kind) : new HabitatBalance(kind, 2, 0, 1, 0.5f, null);
            var local = new List<Vector3>();
            for (int i = 0; i < balance.SlotCount; i++) local.Add(GatheringBalanceTool.LayoutPoint(balance, i));

            // 배치 모양 전체의 반지름 기준 (슬롯 수를 줄여도 나무와의 거리는 같게)
            float templateRadius = GatheringBalanceTool.LayoutRadius(balance, Mathf.Max(balance.SlotCount, balance.SlotLayout.Count));
            float fbxRadius = Mathf.Max(bounds.extents.x, bounds.extents.z);
            float scale = Mathf.Max(0.5f, fbxRadius * SlotRingRatio / templateRadius);
            return local.Select(p => root.TransformPoint(new Vector3(p.x * scale, 0f, p.z * scale))).ToList();
        }

        private static List<Transform> SlotMarkers(GameObject target)
            => target.GetComponentsInChildren<Transform>(true)
                .Where(t => t != target.transform && t.name.StartsWith("Slot", System.StringComparison.OrdinalIgnoreCase)
                            && t.GetComponent<Renderer>() == null)
                .ToList();

        // ---------- 풀숲 ----------
        private static void AttachTuft(GameObject root, Transform visual)
        {
            var tuft = Undo.AddComponent<GrassTuft>(root);
            var so = new SerializedObject(tuft);
            so.FindProperty("_visual").objectReferenceValue = visual; // 밑동(지면)을 축으로 흔든다
            so.FindProperty("_defaultAnchorHeight").floatValue = TuftAnchorHeight;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---------- 게시판·크리처 ----------
        private static void AttachInteractable<T>(GameObject root, Bounds bounds) where T : InteractableBase
        {
            // 상호작용 거리 계산용 영역 (막지는 않음 — 막는 건 FBX 쪽 콜라이더)
            var area = Undo.AddComponent<BoxCollider>(root);
            area.isTrigger = true;
            area.center = root.transform.InverseTransformPoint(bounds.center);
            var size = root.transform.InverseTransformVector(bounds.size);
            area.size = new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z));

            var anchor = new GameObject("PromptAnchor").transform;
            anchor.SetParent(root.transform, false);
            anchor.position = new Vector3(bounds.center.x, bounds.max.y + PromptAboveTop, bounds.center.z);

            var component = Undo.AddComponent<T>(root);
            var so = new SerializedObject(component);
            so.FindProperty("_area").objectReferenceValue = area;
            so.FindProperty("_promptAnchor").objectReferenceValue = anchor;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---------- 도우미 ----------
        private static Transform Group(Transform proto, Feature feature)
        {
            string path = feature switch
            {
                Feature.Habitat => "Gathering",
                Feature.Tuft => "Capture/Zone",
                _ => "Requests"
            };
            var group = proto.Find(path);
            if (group != null) return group;
            var go = new GameObject(path.Split('/').Last());
            Undo.RegisterCreatedObjectUndo(go, "기능 붙이기");
            go.transform.SetParent(proto, false);
            return go.transform;
        }

        /// <summary>서식지는 슬롯 쪽(+Z)이 가까운 길을 보게, 나머지는 그대로.</summary>
        private static Quaternion FacingRotation(Feature feature, Vector3 position)
        {
            if (feature != Feature.Habitat || position.z < 0f || position.z > 48f) return Quaternion.identity;
            float side = Mathf.Sign(BackMountainGatheringBuilder.PathCenterX(position.z) - position.x);
            return Quaternion.LookRotation(new Vector3(side, 0f, 0f));
        }

        /// <summary>바닥(흙 청크·이끼 바닥·포획 구역 바닥)만 맞혀 그 높이로. 대상 자신과 트리거는 무시한다.</summary>
        internal static Vector3 Ground(Vector3 position, Transform ignore)
        {
            Physics.SyncTransforms();
            var floor = GameObject.Find("MountainFloor")?.transform;
            var backdrop = GameObject.Find("MountainBackdrop")?.transform;
            float best = float.NegativeInfinity;
            foreach (var hit in Physics.RaycastAll(new Vector3(position.x, position.y + 30f, position.z), Vector3.down, 80f))
            {
                var t = hit.collider.transform;
                if (hit.collider.isTrigger || (ignore != null && t.IsChildOf(ignore))) continue;
                bool isGround = (floor != null && t.IsChildOf(floor)) || (backdrop != null && t.IsChildOf(backdrop)) || t.name == "Floor";
                if (isGround && hit.point.y > best) best = hit.point.y;
            }
            if (float.IsNegativeInfinity(best))
            {
                Debug.LogWarning($"[기능 붙이기] {position} 아래에서 바닥을 못 찾아 원래 높이를 씀");
                return position;
            }
            return new Vector3(position.x, best, position.z);
        }

        private static Bounds RendererBounds(GameObject target)
        {
            var renderers = target.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return new Bounds(target.transform.position, Vector3.one);
            var bounds = renderers[0].bounds;
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);
            return bounds;
        }

        private static void SetFloat(Object target, string property, float value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(property).floatValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static string Label(Feature feature, GatherHabitatKind kind) => feature switch
        {
            Feature.Habitat => kind switch
            {
                GatherHabitatKind.TreeShade => "그늘 서식지",
                GatherHabitatKind.Bush => "덤불 서식지",
                GatherHabitatKind.RockCrevice => "바위틈 서식지",
                _ => "평지 서식지"
            },
            Feature.Tuft => "포획 풀숲",
            Feature.Board => "의뢰 게시판",
            _ => "크리처"
        };
    }
}
