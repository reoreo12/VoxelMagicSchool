// 의뢰 치트(3단계 최소분): 게시판·전달 창 열기, 받은 의뢰 재료 넣기(바로 완료 가능), 받은 의뢰 모두 비우기, 디버그 표시에 받은 의뢰 상태. 나머지 의뢰 치트는 5단계.
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Text;
using UnityEngine;
using VoxelMagic.Core;
using VoxelMagic.Gameplay;

namespace VoxelMagic.DebugTools
{
    public static class RequestCheats
    {
        public static void Register(CheatPanel panel, DebugOverlay overlay)
        {
            panel.AddTab("의뢰", DrawTab);
            overlay.AddSection(AppendLines);
        }

        private static void DrawTab(CheatTargets t)
        {
            var context = t.Context;
            if (context == null)
            {
                GUILayout.Label("이 씬에는 게임 상태가 없습니다.");
                return;
            }
            var log = context.Requests;
            GUILayout.Label($"받은 의뢰 {log.AcceptedCount}/{log.MaxAccepted} · 게시판 {Access(log.BoardAccess())} · 전달 {Access(log.DeliveryAccess())}");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("게시판 창 열기")) context.Screens.Open(GameScreen.RequestBoard);
            if (GUILayout.Button("크리처에게 전달 (어디서나)") && !CreatureNpc.StartDelivery(context))
                context.Messages.ShowToast(CreatureNpc.BlockedReason(log) ?? GameTexts.NoActiveRequest);
            GUILayout.EndHorizontal();

            GUILayout.Label("받은 의뢰 재료 넣기 (바로 완료 가능)");
            foreach (var accepted in log.Active)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"#{accepted.Id} {accepted.Title} — {StateName(accepted.State)}", GUILayout.Width(300f));
                if (GUILayout.Button("재료 넣기")) FillMaterials(context, accepted);
                GUILayout.EndHorizontal();
            }
            if (log.AcceptedCount == 0) GUILayout.Label("  (없음)");
            if (GUILayout.Button("받은 의뢰 모두 비우기")) log.ClearAccepted();
        }

        /// <summary>모자란 재료만큼 넣는다 (가방이 꽉 차면 들어가는 만큼).</summary>
        private static void FillMaterials(GameContext context, AcceptedRequest accepted)
        {
            foreach (var p in context.Requests.ProgressOf(accepted))
                if (!p.IsMet && !context.Inventory.TryAdd(p.Item, p.Required - p.Current))
                    context.Messages.ShowToast(GameTexts.InventoryFull);
        }

        private static void AppendLines(StringBuilder text, CheatTargets t)
        {
            var log = t.Context?.Requests ?? GameSession.Progress?.Requests;
            if (log == null) return;
            text.Append("\n받은 의뢰 ").Append(log.AcceptedCount).Append('/').Append(log.MaxAccepted).Append(':');
            foreach (var a in log.Active)
                text.Append("\n  #").Append(a.Id).Append(' ').Append(a.Title).Append(" — ").Append(StateName(a.State))
                    .Append(" (").Append(log.DescribeMissing(a)).Append(')');
        }

        private static string Access(QuestAccess access) => access switch
        {
            QuestAccess.NotNight => "밤에만",
            QuestAccess.Closed => "닫힘",
            _ => "열림"
        };

        private static string StateName(RequestState state) => state switch
        {
            RequestState.Ready => "완료 가능",
            RequestState.InProgress => "진행 중",
            _ => state.ToString()
        };
    }
}
#endif
