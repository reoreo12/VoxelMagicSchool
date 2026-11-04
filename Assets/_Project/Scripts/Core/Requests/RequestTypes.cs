// 의뢰 관련 결과 값과 진행 상황 타입 모음.
using System.Collections.Generic;

namespace VoxelMagic.Core
{
    public enum RequestStatus
    {
        Available,
        Active,
        Completed
    }

    public enum AcceptResult
    {
        Success,
        AlreadyActive,
        AlreadyCompleted,
        InventoryFull
    }

    public enum DeliverResult
    {
        Success,
        NoActiveRequest,
        MissingItems
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
        public IReadOnlyList<ItemDefinition> NewItemKinds { get; }
    }
}
