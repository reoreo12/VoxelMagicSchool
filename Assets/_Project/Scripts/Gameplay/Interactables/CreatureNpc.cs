// 크리처. E로 말을 걸면 전달 대화(DeliverySession)를 연다: 완료 가능 의뢰를 고르고(둘 이상일 때 고르기 창) → 전달 대사 → 마지막 대사 뒤 전달 → 남았으면 다시 고르기.
// 대사 도중 끊기거나 고르기 창을 닫으면 아무것도 바뀌지 않는다(UI_SPEC 6-1). 낮·전달 마감 뒤·전달할 것이 없을 때는 안내(반응이 연결돼 있으면 반응 동작).
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
            string blocked = BlockedReason(requests);
            if (blocked == null) return InteractionPrompt.Available(GameTexts.Deliver, PromptAnchor);
            if (_reactions != null) return InteractionPrompt.Available(GameTexts.Talk, PromptAnchor);
            return InteractionPrompt.Blocked(GameTexts.Deliver, blocked, PromptAnchor);
        }

        public override void Interact(in InteractionContext context)
        {
            if (StartDelivery(Context, _speaker)) return;
            if (_reactions != null) _reactions.PlayRandom();
            else Context.Messages.ShowToast(BlockedReason(Context.Requests) ?? GameTexts.NoActiveRequest);
        }

        /// <summary>지금 전달할 수 없는 이유 (전달할 수 있으면 null).</summary>
        public static string BlockedReason(RequestLog requests)
        {
            switch (requests.DeliveryAccess())
            {
                case QuestAccess.Closed: return GameTexts.CreatureTooLate;
                case QuestAccess.NotNight: return GameTexts.CreatureNotNight;
            }
            if (requests.AcceptedCount == 0) return GameTexts.NoActiveRequest;
            if (requests.ReadyRequests().Count == 0) return $"{GameTexts.MissingItems}: {requests.DescribeMissingFirst()}";
            return null;
        }

        /// <summary>
        /// 전달 대화를 시작한다 (크리처 E, 치트가 같이 씀). 화자와 대사창이 있으면 대사를 먼저 보여 주고 마지막 대사 뒤에 납품한다.
        /// 전달할 의뢰가 없거나 이미 대화 중이면 false.
        /// </summary>
        public static bool StartDelivery(GameContext context, SpeakerProfile speaker = null)
        {
            if (context.Delivery.IsActive || context.Dialogue.IsPlaying) return false;
            new DeliveryConversation(context, speaker).Start();
            return context.Delivery.IsActive || context.Delivery.DeliveredCount > 0;
        }

        // 대화 한 번 동안만 단계 알림을 듣는 진행자. 대화가 끝나면 구독을 푼다.
        private sealed class DeliveryConversation
        {
            private readonly GameContext _context;
            private readonly SpeakerProfile _speaker;

            public DeliveryConversation(GameContext context, SpeakerProfile speaker)
            {
                _context = context;
                _speaker = speaker;
            }

            public void Start()
            {
                _context.Delivery.StepChanged += OnStep;
                if (!_context.Delivery.Begin()) _context.Delivery.StepChanged -= OnStep;
            }

            private void OnStep()
            {
                var session = _context.Delivery;
                switch (session.Step)
                {
                    case DeliveryStep.Choosing:
                        // 고르기 창은 화면 스택을 보고 뜬다(UI). 이미 열려 있으면 창이 목록만 새로 고친다.
                        if (_context.Screens.Top != GameScreen.DeliveryChoice) _context.Screens.Open(GameScreen.DeliveryChoice);
                        break;
                    case DeliveryStep.Talking:
                        if (_context.Screens.IsOpen(GameScreen.DeliveryChoice)) _context.Screens.Close(GameScreen.DeliveryChoice);
                        PlayTalk(session.Current.Request);
                        break;
                    case DeliveryStep.Idle:
                        session.StepChanged -= OnStep;
                        if (_context.Screens.IsOpen(GameScreen.DeliveryChoice)) _context.Screens.Close(GameScreen.DeliveryChoice);
                        break;
                }
            }

            private void PlayTalk(RequestDefinition request)
            {
                var lines = BuildLines(request, _speaker);
                // 대사 결과(아이템 소모·의뢰 완료·점수)는 마지막 대사 뒤 한 번에. 도중에 끊기면 아무것도 바꾸지 않는다.
                if (_speaker != null && lines.Count > 0 && _context.Dialogue.HasPresenter
                    && _context.Dialogue.Play(lines, () => _context.Delivery.CompleteTalk(), () => _context.Delivery.Cancel()))
                    return;
                _context.Delivery.CompleteTalk();
            }
        }

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
