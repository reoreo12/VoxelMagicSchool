// 저장 파일 내용 (JsonUtility용 순수 데이터). 에셋 대신 ID 문자열로 담고, 형식이 바뀌면 Version을 올린다.
// JsonUtility가 public 필드만 직렬화하므로 이 파일의 클래스만 예외적으로 public 필드를 쓴다.
using System;
using System.Collections.Generic;

namespace VoxelMagic.Core
{
    [Serializable]
    public sealed class SaveData
    {
        /// <summary>2: 시계·보정·게시판·채집 자리·수락 목록 (2026-11-25). 1은 새 게임으로 시작.</summary>
        public const int CurrentVersion = 2;

        public int Version = CurrentVersion;
        public string SavedAtUtc;
        public List<SlotRecord> Slots = new List<SlotRecord>();
        public int SelectedSlot;
        public int PracticalScore;
        public int WrittenScore;
        public List<string> JournalItemIds = new List<string>();
        /// <summary>전달한 의뢰</summary>
        public List<string> CompletedRequestIds = new List<string>();
        /// <summary>받아 둔 의뢰 (수락 순서)</summary>
        public List<string> AcceptedRequestIds = new List<string>();
        public List<string> AbandonedRequestIds = new List<string>();
        public int NextRequestId = 1;
        public List<FarmPlotRecord> FarmPlots = new List<FarmPlotRecord>();

        /// <summary>진행 상태 세계(시계·게시판·채집)가 저장돼 있는지</summary>
        public bool HasWorld;
        public int Day;
        public float Minute;
        public List<ShiftRecord> NextDayShifts = new List<ShiftRecord>();
        public int BoardDay;
        public List<string> BoardOfferIds = new List<string>();
        public List<BoardHistoryRecord> BoardHistory = new List<BoardHistoryRecord>();
        public int GatherDay;
        public List<GatherSpotRecord> GatherSpots = new List<GatherSpotRecord>();
    }

    [Serializable]
    public sealed class ShiftRecord
    {
        public string Source;
        public int Minutes;
    }

    [Serializable]
    public sealed class BoardHistoryRecord
    {
        public string RequestId;
        public int LastDay;
        public int Times;
    }

    [Serializable]
    public sealed class GatherSpotRecord
    {
        public int Kind;
        public int Spot;
        public string MaterialId;
    }

    [Serializable]
    public sealed class SlotRecord
    {
        public string ItemId;
        public int Count;
    }

    [Serializable]
    public sealed class FarmPlotRecord
    {
        public string SeedId;
        public int OriginX;
        public int OriginY;
        public bool Watered;
        public float WateredAt;
        public bool Grown;
        /// <summary>심은 방향 (FarmFacing, 0 = Up = 예전 저장의 안 돌린 모양).</summary>
        public int Facing;
    }

    /// <summary>저장 파일 읽기·쓰기 (슬롯 번호별). 실제 위치·형식은 구현이 정한다.</summary>
    public interface ISaveStore
    {
        bool Exists(int slot);
        bool TryRead(int slot, out string json);
        void Write(int slot, string json);
        void Delete(int slot);
    }
}
