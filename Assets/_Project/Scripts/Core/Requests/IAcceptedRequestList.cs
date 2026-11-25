// 수락한 의뢰 목록을 UI가 읽는 창구(UI_SPEC 4-A-3, 10장). UI는 의뢰 규칙 대신 이 인터페이스만 본다.
// 항목은 제목·요구 아이템별 진행·완료(납품 가능) 여부·수락 순서를 담는다. 보상까지 받은 의뢰는 목록에서 빠진다.
using System;
using System.Collections.Generic;

namespace VoxelMagic.Core
{
    public interface IAcceptedRequestList
    {
        /// <summary>지금 수락해 진행 중인 의뢰 수 (납품 가능한 것 포함).</summary>
        int AcceptedCount { get; }

        /// <summary>수락한 의뢰 항목. 순서는 정해져 있지 않다(정렬은 UI가 한다).</summary>
        IReadOnlyList<AcceptedRequestEntry> Accepted { get; }

        /// <summary>수락·납품·포기, 또는 진행이 바뀌어 목록 내용이 바뀔 때.</summary>
        event Action AcceptedListChanged;

        /// <summary>지금 납품하면 받을 필기 점수 예상값 (처음 납품하는 재료 보너스 포함). 점수 규칙은 Core가 계산한다.</summary>
        int ExpectedReward(AcceptedRequestEntry entry);

        /// <summary>의뢰 조건을 다 채워 납품할 수 있게 된 순간 (완료 알림용, 10-6).</summary>
        event Action<AcceptedRequestEntry> RequestBecameComplete;
    }

    /// <summary>수락한 의뢰 한 건의 보기 값. 진행이 바뀌면 새 항목으로 바뀐다.</summary>
    public sealed class AcceptedRequestEntry
    {
        public AcceptedRequestEntry(RequestDefinition request, int acceptedOrder, IReadOnlyList<RequirementProgress> requirements)
        {
            Request = request;
            AcceptedOrder = acceptedOrder;
            Requirements = requirements;
            int have = 0, need = 0;
            bool complete = true;
            foreach (var r in requirements)
            {
                have += Math.Min(r.Current, r.Required);
                need += r.Required;
                if (!r.IsMet) complete = false;
            }
            Progress = need > 0 ? (float)have / need : 1f;
            IsComplete = complete;
        }

        /// <summary>원래 의뢰 데이터 (자세히 패널에서 대사·보상을 읽을 때).</summary>
        public RequestDefinition Request { get; }
        public string Title => Request != null ? Request.Title : string.Empty;
        /// <summary>먼저 수락할수록 작다.</summary>
        public int AcceptedOrder { get; }
        public IReadOnlyList<RequirementProgress> Requirements { get; }
        /// <summary>요구 아이템 전체 기준 0~1.</summary>
        public float Progress { get; }
        /// <summary>조건을 다 채워 납품할 수 있는 상태.</summary>
        public bool IsComplete { get; }
    }
}
