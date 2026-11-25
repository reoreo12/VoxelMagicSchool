// 진행 상태 세계(시계·다음 날 보정·게시판·오늘 채집 자리)의 저장용 보관본(에셋 참조). 저장 파일을 읽을 때는 세계가 아직 없어서
// PlayerProgress가 들고 있다가 EnsureWorld에서 새 게임 시작 대신 이 상태로 이어 간다. D-N 날짜별 세이브도 이 보관본을 쓰면 된다.
using System.Collections.Generic;

namespace VoxelMagic.Core
{
    public sealed class WorldSnapshot
    {
        public int Day;
        public float Minute;
        /// <summary>다음 날 시작 보정 요청 (원인, 분)</summary>
        public readonly List<KeyValuePair<string, int>> Shifts = new List<KeyValuePair<string, int>>();

        public int BoardDay;
        public readonly List<RequestDefinition> Offers = new List<RequestDefinition>();
        public readonly List<BoardHistoryEntry> BoardHistory = new List<BoardHistoryEntry>();

        public int GatherDay;
        public readonly List<GatherSpotEntry> GatherSpots = new List<GatherSpotEntry>();
    }

    /// <summary>게시판 반복 방지 기록 한 줄.</summary>
    public readonly struct BoardHistoryEntry
    {
        public BoardHistoryEntry(RequestDefinition request, int lastDay, int times)
        {
            Request = request;
            LastDay = lastDay;
            Times = times;
        }

        public RequestDefinition Request { get; }
        public int LastDay { get; }
        public int Times { get; }
    }

    /// <summary>오늘 켜진 채집 자리 하나.</summary>
    public readonly struct GatherSpotEntry
    {
        public GatherSpotEntry(GatherHabitatKind kind, int spot, GatherMaterialDefinition material)
        {
            Kind = kind;
            Spot = spot;
            Material = material;
        }

        public GatherHabitatKind Kind { get; }
        public int Spot { get; }
        public GatherMaterialDefinition Material { get; }
    }
}
