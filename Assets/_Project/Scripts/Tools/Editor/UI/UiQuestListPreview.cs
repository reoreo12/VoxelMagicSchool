// 플레이 중 의뢰 목록·완료 알림을 가짜 의뢰 여러 건으로 미리 본다. 지금 의뢰 규칙은 한 번에 하나만 수락해서 스크롤·정렬을 게임에서 볼 수 없기 때문.
// 저장 데이터와 실제 의뢰 진행은 건드리지 않는다(목록 화면만 가짜 원본에 다시 연결).
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VoxelMagic.Core;
using VoxelMagic.UI;

namespace VoxelMagic.Tools.EditorTools.UiWorkbench
{
    public static class UiQuestListPreview
    {
        [MenuItem("VoxelMagic/UI/미리보기/의뢰 목록 가짜 6건 (플레이 중)")]
        private static void FillSix() => Fill(6);

        [MenuItem("VoxelMagic/UI/미리보기/완료 알림 2건 (플레이 중)")]
        private static void ToastTwo()
        {
            var toast = UnityEngine.Object.FindFirstObjectByType<QuestToastView>();
            if (toast == null) return;
            toast.Enqueue("약초 모으기");
            toast.Enqueue("붕어 낚기");
        }

        /// <summary>가짜 의뢰 count건(완료·진행률이 섞인)을 목록에 연결한다. 플레이 모드에서만.</summary>
        public static void Fill(int count)
        {
            if (!Application.isPlaying) { Debug.LogWarning("[UI] 플레이 중에만 미리 볼 수 있습니다."); return; }
            var view = UnityEngine.Object.FindFirstObjectByType<QuestListView>();
            if (view == null) return;

            var items = AssetDatabase.FindAssets("t:ItemDefinition", new[] { "Assets/_Project/Data" })
                .Select(g => AssetDatabase.LoadAssetAtPath<ItemDefinition>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(i => i != null).ToList();
            var list = new FakeList();
            for (int i = 0; i < count; i++)
            {
                var request = ScriptableObject.CreateInstance<RequestDefinition>();
                var so = new SerializedObject(request);
                so.FindProperty("_title").stringValue = $"가짜 의뢰 {i + 1}";
                so.ApplyModifiedPropertiesWithoutUndo();
                int lines = 1 + i % 3;
                var needs = new RequirementProgress[lines];
                for (int k = 0; k < lines; k++)
                {
                    var item = items.Count > 0 ? items[(i + k) % items.Count] : null;
                    int required = 2 + k;
                    int current = i % 4 == 0 ? required : (i + k) % (required + 1);
                    needs[k] = new RequirementProgress(item, current, required);
                }
                list.Entries.Add(new AcceptedRequestEntry(request, i, needs));
            }
            var hud = new HudState();
            view.Bind(list, hud);
            Debug.Log($"[UI] 의뢰 목록에 가짜 {count}건을 연결했습니다.");
        }

        private sealed class FakeList : IAcceptedRequestList
        {
            public readonly List<AcceptedRequestEntry> Entries = new List<AcceptedRequestEntry>();
            public int AcceptedCount => Entries.Count;
            public IReadOnlyList<AcceptedRequestEntry> Accepted => Entries;
            public int ExpectedReward(AcceptedRequestEntry entry) => 10;
            public event Action AcceptedListChanged { add { } remove { } }
            public event Action<AcceptedRequestEntry> RequestBecameComplete { add { } remove { } }
        }
    }
}
