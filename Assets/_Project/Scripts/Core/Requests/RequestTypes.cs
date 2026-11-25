// 의뢰 관련 상태·결과 값과 진행 상황 타입 모음 (QUEST_TIMER 4-1 상태 다이어그램).
using System.Collections.Generic;

namespace VoxelMagic.Core
{
    /// <summary>의뢰 하나의 상태. 칸을 차지하는 것은 진행 중·완료 가능.</summary>
    public enum RequestState
    {
        /// <summary>게시판에도 수락 목록에도 없음</summary>
        None,
        /// <summary>오늘 게시판에 있음</summary>
        Offered,
        InProgress,
        /// <summary>재료를 다 모음 (저장하지 않고 인벤토리로 매번 계산)</summary>
        Ready,
        Delivered,
        Abandoned
    }

    public enum AcceptResult
    {
        Success,
        /// <summary>받아 둔 의뢰가 이미 최대 수</summary>
        SlotsFull,
        /// <summary>오늘 게시판에 없는 의뢰</summary>
        NotOnBoard,
        /// <summary>게시판이 닫힌 날 (7일부터)</summary>
        BoardClosed,
        /// <summary>게시판을 쓸 수 없는 시간 (밤에만)</summary>
        NotNight,
        AlreadyAccepted,
        /// <summary>지급할 씨앗을 넣을 자리가 없음</summary>
        InventoryFull
    }

    public enum DeliverResult
    {
        Success,
        NotAccepted,
        MissingItems,
        /// <summary>마지막 전달 날이 지남</summary>
        TooLate,
        NotNight
    }

    /// <summary>게시판·크리처 전달을 지금 쓸 수 있는지.</summary>
    public enum QuestAccess
    {
        Open,
        NotNight,
        /// <summary>게시판이 닫힌 날, 또는 전달 마지막 날이 지남</summary>
        Closed
    }

    /// <summary>필요 아이템 하나의 진행 상황 (예: 무 2/3).</summary>
    public readonly struct RequirementProgress
    {
        public ItemDefinition Item { get; }
        public int Current { get; }
        public int Required { get; }
        public bool IsMet => Current >= Required;

        public RequirementProgress(ItemDefinition item, int current, int required)
        {
            Item = item;
            Current = current;
            Required = required;
        }

        public override string ToString() => $"{Item.DisplayName} {System.Math.Min(Current, Required)}/{Required}";
    }

    /// <summary>전달 결과 보고. 크리처 대사와 점수 연출에 쓴다.</summary>
    public sealed class DeliveryReport
    {
        public DeliveryReport(RequestDefinition request, int scoreGained, IReadOnlyList<ItemDefinition> newItemKinds)
        {
            Request = request;
            ScoreGained = scoreGained;
            NewItemKinds = newItemKinds;
        }

        public RequestDefinition Request { get; }
        public int ScoreGained { get; }
        /// <summary>처음 전달한 재료 종류 (도감 기록용, 점수와는 무관)</summary>
        public IReadOnlyList<ItemDefinition> NewItemKinds { get; }
    }
}
