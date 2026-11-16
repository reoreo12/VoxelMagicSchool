// 저장 파일 내용 (JsonUtility용 순수 데이터). 에셋 대신 ID 문자열로 담고, 형식이 바뀌면 Version을 올린다.
// JsonUtility가 public 필드만 직렬화하므로 이 파일의 클래스만 예외적으로 public 필드를 쓴다.
using System;
using System.Collections.Generic;

namespace VoxelMagic.Core
{
    [Serializable]
    public sealed class SaveData
    {
        public const int CurrentVersion = 1;

        public int Version = CurrentVersion;
        public string SavedAtUtc;
        public List<SlotRecord> Slots = new List<SlotRecord>();
        public int SelectedSlot;
        public int PracticalScore;
        public int WrittenScore;
        public List<string> JournalItemIds = new List<string>();
        public List<string> CompletedRequestIds = new List<string>();
        public string ActiveRequestId;
        public List<FarmPlotRecord> FarmPlots = new List<FarmPlotRecord>();
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
