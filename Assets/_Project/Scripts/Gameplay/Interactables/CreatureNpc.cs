// 크리처. 다가가면 의뢰 조건을 확인해 안내하고, E로 전달하면 점수를 받고 재료 설명(필기 공부) 대사를 띄운다.
// 반응(CreatureReactions)이 연결돼 있으면 전달할 수 없을 때 "말 걸기"로 바뀌고, E를 누르면 무작위 반응 동작을 한다.
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class CreatureNpc : InteractableBase
    {
        [Tooltip("비우면 전달할 수 없을 때 안내만 띄운다 (기본 도형 크리처)")]
        [SerializeField] private CreatureReactions _reactions;

        public override InteractionPrompt GetPrompt(in InteractionContext context)
        {
            var requests = Context.Requests;
            var result = requests.CanDeliver();
            if (result == DeliverResult.Success) return InteractionPrompt.Available(GameTexts.Deliver, PromptAnchor);
            if (_reactions != null) return InteractionPrompt.Available(GameTexts.Talk, PromptAnchor);
            return result == DeliverResult.MissingItems
                ? InteractionPrompt.Blocked(GameTexts.Deliver, $"{GameTexts.MissingItems}: {requests.DescribeMissing()}", PromptAnchor)
                : InteractionPrompt.Blocked(GameTexts.Deliver, GameTexts.NoActiveRequest, PromptAnchor);
        }

        public override void Interact(in InteractionContext context)
        {
            if (Context.Requests.CanDeliver() != DeliverResult.Success)
            {
                if (_reactions != null) _reactions.PlayRandom();
                return;
            }
            TryDeliver(Context);
        }

        /// <summary>의뢰를 전달하고 완료 대사 창을 연다 (크리처 E, 의뢰 치트 키가 같이 씀). 전달할 수 없으면 false.</summary>
        public static bool TryDeliver(GameContext context)
        {
            if (context.Requests.Deliver(out var report) != DeliverResult.Success) return false;
            context.Messages.ShowCreatureSpeech(BuildLines(report));
            context.Screen.Open(GameWindow.CreatureDialog);
            return true;
        }

        private static List<string> BuildLines(DeliveryReport report)
        {
            var lines = new List<string>();
            if (!string.IsNullOrEmpty(report.Request.CompletionLine)) lines.Add(report.Request.CompletionLine);
            foreach (var item in report.Request.Requirements.Select(r => r.Item).Distinct())
                if (!string.IsNullOrEmpty(item.CreatureNote))
                    lines.Add(string.Format(GameTexts.MaterialNoteFormat, item.DisplayName, item.CreatureNote));
            return lines;
        }
    }
}
