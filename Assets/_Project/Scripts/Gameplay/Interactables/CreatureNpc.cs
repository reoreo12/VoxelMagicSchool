// 크리처. 다가가면 의뢰 조건을 확인해 안내하고, E로 전달하면 대사창(완료 대사 + 재료 설명)을 먼저 보여 준 뒤 마지막 대사가 끝나면 납품·점수를 한 번에 반영한다.
// 대사 도중 강제 종료되면 아무것도 바뀌지 않아 "말 걸기 전" 상태로 남는다(UI_SPEC 6-1).
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
        [Tooltip("대사창에 나올 화자(이름·초상화). 비우면 대사 없이 바로 납품한다")]
        [SerializeField] private SpeakerProfile _speaker;

        /// <summary>이 크리처의 화자 데이터(대사창 이름·초상화).</summary>
        public SpeakerProfile Speaker => _speaker;

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
            TryDeliver(Context, _speaker);
        }

        /// <summary>
        /// 의뢰 전달 (크리처 E, 의뢰 치트 키가 같이 씀). 화자와 대사창이 있으면 대사를 먼저 보여 주고 마지막 대사 뒤에 납품한다.
        /// 지금 전달할 수 없으면 false.
        /// </summary>
        public static bool TryDeliver(GameContext context, SpeakerProfile speaker = null)
        {
            var requests = context.Requests;
            if (requests.CanDeliver() != DeliverResult.Success || context.Dialogue.IsPlaying) return false;
            var lines = BuildLines(requests.Active, speaker);
            // 대사 결과(아이템 소모·의뢰 완료·점수)는 마지막 대사 뒤 한 번에. 도중에는 아무것도 바꾸지 않는다.
            if (speaker != null && context.Dialogue.HasPresenter && context.Dialogue.Play(lines, () => Deliver(context))) return true;
            Deliver(context);
            return true;
        }

        private static void Deliver(GameContext context) => context.Requests.Deliver(out _);

        private static List<DialogueLine> BuildLines(RequestDefinition request, SpeakerProfile speaker)
        {
            var lines = new List<DialogueLine>();
            if (!string.IsNullOrEmpty(request.CompletionLine)) lines.Add(new DialogueLine(speaker, request.CompletionLine));
            foreach (var item in request.Requirements.Select(r => r.Item).Distinct())
                if (item != null && !string.IsNullOrEmpty(item.CreatureNote))
                    lines.Add(new DialogueLine(speaker, string.Format(GameTexts.MaterialNoteFormat, item.DisplayName, item.CreatureNote)));
            return lines;
        }
    }
}
