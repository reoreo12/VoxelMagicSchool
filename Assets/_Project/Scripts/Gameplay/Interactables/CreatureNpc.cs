// 크리처. 다가가면 의뢰 조건을 확인해 안내하고, E로 전달하면 점수를 받고 재료 설명(필기 공부) 대사를 띄운다.
using System.Collections.Generic;
using System.Linq;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class CreatureNpc : InteractableBase
    {
        public override InteractionPrompt GetPrompt(in InteractionContext context)
        {
            var requests = Context.Requests;
            return requests.CanDeliver() switch
            {
                DeliverResult.Success => InteractionPrompt.Available(GameTexts.Deliver, PromptAnchor),
                DeliverResult.MissingItems => InteractionPrompt.Blocked(GameTexts.Deliver,
                    $"{GameTexts.MissingItems}: {requests.DescribeMissing()}", PromptAnchor),
                _ => InteractionPrompt.Blocked(GameTexts.Deliver, GameTexts.NoActiveRequest, PromptAnchor)
            };
        }

        public override void Interact(in InteractionContext context)
        {
            if (Context.Requests.Deliver(out var report) != DeliverResult.Success) return;
            Context.Messages.ShowCreatureSpeech(BuildLines(report));
            Context.Screen.Open(GameWindow.CreatureDialog);
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
