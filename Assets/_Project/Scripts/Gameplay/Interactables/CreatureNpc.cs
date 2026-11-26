// 크리처. E로 말을 걸면 대사창 안 메뉴(이야기하기·과외 받기·의뢰 전달하기·그만두기, UI_SPEC 6-4)를 연다. 전달은 DeliverySession을 따라
// 완료 가능 의뢰를 대사창 선택지로 고르고(둘 이상일 때) → 전달 대사 → 마지막 대사 뒤 전달 → 남았으면 다시 고르기 → 메뉴로. 대사 도중 끊기면 아무것도 바뀌지 않는다(6-1).
// 대사창·화자가 없는 씬은 메뉴 없이 예전처럼 바로 전달(첫 의뢰부터)하거나 안내만 띄운다.
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class CreatureNpc : InteractableBase
    {
        [Tooltip("비우면 전달할 수 없을 때 안내만 띄운다 (기본 도형 크리처). '이야기하기' 때도 반응 동작을 한다")]
        [SerializeField] private CreatureReactions _reactions;
        [Tooltip("대사창에 나올 화자(이름·초상화·잡담). 비우면 메뉴·대사 없이 바로 납품한다")]
        [SerializeField] private SpeakerProfile _speaker;

        /// <summary>이 크리처의 화자 데이터(대사창 이름·초상화).</summary>
        public SpeakerProfile Speaker => _speaker;

        private bool HasMenu => _speaker != null && Context.Dialogue.HasPresenter;

        public override InteractionPrompt GetPrompt(in InteractionContext context)
        {
            if (HasMenu) return InteractionPrompt.Available(GameTexts.Talk, PromptAnchor);
            string blocked = BlockedReason(Context.Requests);
            if (blocked == null) return InteractionPrompt.Available(GameTexts.Deliver, PromptAnchor);
            if (_reactions != null) return InteractionPrompt.Available(GameTexts.Talk, PromptAnchor);
            return InteractionPrompt.Blocked(GameTexts.Deliver, blocked, PromptAnchor);
        }

        public override void Interact(in InteractionContext context)
        {
            if (HasMenu)
            {
                if (!Context.Delivery.IsActive && !Context.Dialogue.IsPlaying) new Conversation(Context, _speaker, _reactions).ShowMenu();
                return;
            }
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
        /// 메뉴 없이 전달 대화부터 시작한다 (치트가 씀). 화자와 대사창이 있으면 대사창 선택지·대사를 거치고, 없으면 바로 납품한다.
        /// 전달할 의뢰가 없거나 이미 대화 중이면 false.
        /// </summary>
        public static bool StartDelivery(GameContext context, SpeakerProfile speaker = null)
        {
            if (context.Delivery.IsActive || context.Dialogue.IsPlaying) return false;
            if (speaker != null && context.Dialogue.HasPresenter)
                return new Conversation(context, speaker, null, returnToMenu: false).StartDelivery();
            return DeliverWithoutDialogue(context);
        }

        // 대사창이 없을 때: 완료 가능한 의뢰를 앞에서부터 차례로 바로 전달한다.
        private static bool DeliverWithoutDialogue(GameContext context)
        {
            var session = context.Delivery;
            if (!session.Begin()) return false;
            int guard = 0;
            while (session.IsActive && guard++ < 16)
            {
                if (session.Step == DeliveryStep.Choosing && session.Choices.Count > 0) session.Choose(session.Choices[0].Id);
                else if (session.Step == DeliveryStep.Talking) session.CompleteTalk();
                else session.Cancel();
            }
            return session.DeliveredCount > 0;
        }

        // 대화 한 번(메뉴 → 이야기/과외/전달)을 진행한다. 전달 중에는 세션 단계 알림을 듣고, 끝나면 구독을 푼다.
        private sealed class Conversation
        {
            private const int MenuTalk = 0, MenuLesson = 1, MenuDeliver = 2, MenuLeave = 3;

            private readonly GameContext _context;
            private readonly SpeakerProfile _speaker;
            private readonly CreatureReactions _reactions;
            private readonly bool _returnToMenu;
            private bool _aborted;

            public Conversation(GameContext context, SpeakerProfile speaker, CreatureReactions reactions, bool returnToMenu = true)
            {
                _context = context;
                _speaker = speaker;
                _reactions = reactions;
                _returnToMenu = returnToMenu;
            }

            public void ShowMenu()
            {
                if (_aborted) return;
                var options = new[]
                {
                    new DialogueOption(GameTexts.MenuTalk),
                    new DialogueOption(GameTexts.MenuLesson),
                    new DialogueOption(GameTexts.MenuDeliver, dimmed: BlockedReason(_context.Requests) != null),
                    new DialogueOption(GameTexts.MenuLeave)
                };
                _context.Dialogue.Ask(Line(GameTexts.CreatureMenuPrompt), options, OnMenu, MenuLeave, OnAborted);
            }

            private void OnMenu(int choice)
            {
                switch (choice)
                {
                    case MenuTalk:
                        if (_reactions != null) _reactions.PlayRandom();
                        Say(SmallTalk());
                        break;
                    case MenuLesson:
                        Say(GameTexts.LessonNotReady); // 과외 시스템이 생기면 여기서 연다
                        break;
                    case MenuDeliver:
                        string blocked = BlockedReason(_context.Requests);
                        if (blocked != null) Say(blocked);
                        else StartDelivery();
                        break;
                    // MenuLeave: 아무것도 열지 않으면 대사창이 닫힌다
                }
            }

            // 한 줄 말하고 메뉴로 돌아간다.
            private void Say(string text) => _context.Dialogue.Play(new[] { Line(text) }, ShowMenu, OnAborted);

            private string SmallTalk()
            {
                int count = _speaker.SmallTalkCount;
                return count > 0 ? _speaker.SmallTalk(Random.Range(0, count)) : GameTexts.SmallTalkFallback;
            }

            public bool StartDelivery()
            {
                _context.Delivery.StepChanged += OnStep;
                if (_context.Delivery.Begin()) return true;
                _context.Delivery.StepChanged -= OnStep;
                return false;
            }

            private void OnStep()
            {
                var session = _context.Delivery;
                switch (session.Step)
                {
                    case DeliveryStep.Choosing:
                        AskWhichRequest(session);
                        break;
                    case DeliveryStep.Talking:
                        PlayTalk(session.Current.Request);
                        break;
                    case DeliveryStep.Idle:
                        session.StepChanged -= OnStep;
                        if (_returnToMenu) ShowMenu(); // 다 전달했거나 "돌아가기" → 메뉴 (대사가 끊긴 경우는 _aborted라 열지 않음)
                        break;
                }
            }

            // 완료 가능 의뢰 + "돌아가기"를 대사창 선택지로. 고르는 순간 세션이 다시 확인한다(재료가 겹쳐 빠졌으면 목록을 새로 묻는다).
            private void AskWhichRequest(DeliverySession session)
            {
                var choices = session.Choices.ToList();
                var options = new List<DialogueOption>();
                foreach (var request in choices)
                    options.Add(new DialogueOption(request.Title, string.Format(GameTexts.RewardShortFormat, _context.Requests.RewardOf(request.Request))));
                options.Add(new DialogueOption(GameTexts.ChoiceBack));
                int back = options.Count - 1;
                _context.Dialogue.Ask(Line(GameTexts.DeliveryChoiceTitle), options, choice =>
                {
                    if (choice >= back) session.Cancel();
                    else session.Choose(choices[choice].Id);
                }, back, OnAborted);
            }

            private void PlayTalk(RequestDefinition request)
            {
                var lines = BuildLines(request, _speaker);
                // 대사 결과(아이템 소모·의뢰 완료·점수)는 마지막 대사 뒤 한 번에. 도중에 끊기면 아무것도 바꾸지 않는다.
                if (lines.Count > 0 && _context.Dialogue.Play(lines, () => _context.Delivery.CompleteTalk(), OnAborted)) return;
                _context.Delivery.CompleteTalk();
            }

            // 대사창이 도중에 버려짐(씬 이동 등): 진행 중인 전달을 결과 없이 끝내고 메뉴도 다시 열지 않는다.
            private void OnAborted()
            {
                _aborted = true;
                _context.Delivery.Cancel();
            }

            private DialogueLine Line(string text) => new DialogueLine(_speaker, text);
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
