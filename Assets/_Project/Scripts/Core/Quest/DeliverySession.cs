// 크리처 전달 대화 한 번(QUEST_TIMER 2-5, 7-7): 완료 가능 목록 → 고름(그 순간 다시 확인) → 대사 → 마지막 대사 뒤 전달 → 남았으면 다시 고르기.
// Gameplay(크리처·대사창)와 UI(고르기 창)는 단계(StepChanged)만 따라간다. 대사 도중 취소하면 아무것도 바뀌지 않는다.
using System;
using System.Collections.Generic;

namespace VoxelMagic.Core
{
    public enum DeliveryStep
    {
        Idle,
        /// <summary>완료 가능 의뢰 중 하나를 고르는 중</summary>
        Choosing,
        /// <summary>고른 의뢰의 전달 대사 중 (끝나면 CompleteTalk)</summary>
        Talking
    }

    public sealed class DeliverySession
    {
        private readonly RequestLog _log;
        private readonly List<AcceptedRequest> _choices = new List<AcceptedRequest>();

        public DeliverySession(RequestLog log) => _log = log ?? throw new ArgumentNullException(nameof(log));

        public event Action StepChanged;
        /// <summary>한 번 전달할 때마다 (점수 연출 등).</summary>
        public event Action<DeliveryReport> DeliveredOne;

        public DeliveryStep Step { get; private set; }
        public bool IsActive => Step != DeliveryStep.Idle;
        public IReadOnlyList<AcceptedRequest> Choices => _choices;
        /// <summary>대사 중인 의뢰 (Talking일 때).</summary>
        public AcceptedRequest Current { get; private set; }
        /// <summary>이번 대화에서 전달한 수.</summary>
        public int DeliveredCount { get; private set; }

        /// <summary>대화 시작. 전달할 수 있는 의뢰가 없으면 false(시작 안 함).</summary>
        public bool Begin()
        {
            if (IsActive) return false;
            DeliveredCount = 0;
            return Next();
        }

        /// <summary>고르기: 그 순간 다시 확인해서 아직 완료 가능하면 대사로, 아니면 목록을 새로 고친다.</summary>
        public bool Choose(int id)
        {
            if (Step != DeliveryStep.Choosing) return false;
            if (_log.CanDeliver(id) != DeliverResult.Success)
            {
                Next(); // 다른 의뢰에 재료를 내서 빠졌으면 목록을 새로 고친다
                return false;
            }
            Current = _log.Find(id);
            SetStep(DeliveryStep.Talking);
            return true;
        }

        /// <summary>대사가 끝까지 재생됨 → 전달하고 다음 고르기(없으면 끝).</summary>
        public void CompleteTalk()
        {
            if (Step != DeliveryStep.Talking || Current == null) return;
            var id = Current.Id;
            Current = null;
            if (_log.Deliver(id, out var report) == DeliverResult.Success)
            {
                DeliveredCount++;
                DeliveredOne?.Invoke(report);
            }
            Next();
        }

        /// <summary>고르기 창을 닫거나 대사가 도중에 끊김 → 아무것도 바꾸지 않고 끝.</summary>
        public void Cancel()
        {
            if (!IsActive) return;
            Current = null;
            _choices.Clear();
            SetStep(DeliveryStep.Idle);
        }

        // 완료 가능 목록을 다시 계산한다. 비면 끝, 하나뿐이고 설정이 켜져 있으면 바로 대사, 아니면 고르기.
        private bool Next()
        {
            _choices.Clear();
            if (_log.DeliveryAccess() == QuestAccess.Open) _choices.AddRange(_log.ReadyRequests());
            if (_choices.Count == 0)
            {
                Current = null;
                SetStep(DeliveryStep.Idle);
                return false;
            }
            if (_choices.Count == 1 && _log.Config.SkipChoiceWhenSingle)
            {
                Current = _choices[0];
                SetStep(DeliveryStep.Talking);
                return true;
            }
            Current = null;
            SetStep(DeliveryStep.Choosing);
            return true;
        }

        private void SetStep(DeliveryStep step)
        {
            Step = step;
            StepChanged?.Invoke();
        }
    }
}
