// 씬을 옮겨도 유지되는 진행 데이터 묶음: 인벤토리·점수·도감·의뢰, 게임 시계와 하루 흐름, 텃밭 격자, 오늘의 채집 자리, 오늘의 게시판.
// 하루 시작 처리(작물·채집·게시판)가 씬과 무관하게 돌도록 이 데이터들을 여기 둔다. 씬마다 새로 만드는 것(낚시 세션·메시지·화면 상태)은 GameContext 소속.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelMagic.Core
{
    /// <summary>텃밭 작물 하나의 보관본 (저장용, 에셋 참조 그대로).</summary>
    public readonly struct FarmPlotState
    {
        public FarmPlotState(SeedDefinition seed, Vector2Int origin, bool watered, float wateredAt = 0f, bool grown = false, FarmFacing facing = FarmFacing.Up)
        {
            Facing = facing;
            Seed = seed;
            Origin = origin;
            Watered = watered;
            WateredAt = wateredAt;
            Grown = grown;
        }

        public SeedDefinition Seed { get; }
        public Vector2Int Origin { get; }
        public bool Watered { get; }
        /// <summary>물 준 게임 시각 (누적 분).</summary>
        public float WateredAt { get; }
        public bool Grown { get; }
        /// <summary>심은 방향 (돌린 모양).</summary>
        public FarmFacing Facing { get; }
    }

    /// <summary>진행 상태의 세계(시계·밭·채집·게시판)를 만들 때 필요한 설정 묶음. GameBootstrap이 인스펙터 값으로 채운다.</summary>
    public sealed class WorldSetup
    {
        public TimeConfig Time;
        public CalendarConfig Calendar;
        public FarmConfig Farm;
        public GatheringConfig Gathering;
        public QuestConfig Quest;
        public RequestPool RequestPool;
        public IRandom Random;
    }

    public sealed class PlayerProgress
    {
        private readonly List<FarmPlotState> _farmPlots = new List<FarmPlotState>();
        private WorldSnapshot _pendingWorld;

        private PlayerProgress(Inventory inventory, ScoreBook score, MaterialJournal journal, RequestLog requests)
        {
            Inventory = inventory;
            Score = score;
            Journal = journal;
            Requests = requests;
        }

        public Inventory Inventory { get; }
        public ScoreBook Score { get; }
        public MaterialJournal Journal { get; }
        /// <summary>받아 둔 의뢰와 수락·포기·전달 규칙.</summary>
        public RequestLog Requests { get; }
        /// <summary>저장용 밭 보관본 (밭 격자가 생기면 격자가 원본).</summary>
        public IReadOnlyList<FarmPlotState> FarmPlots => _farmPlots;

        /// <summary>게임 시계 (씬을 넘어 유지). EnsureWorld 전에는 null.</summary>
        public GameClock Clock { get; private set; }
        /// <summary>하루 시작·끝 진행자. EnsureWorld 전에는 null.</summary>
        public DayFlow Days { get; private set; }
        /// <summary>텃밭 격자 (씬을 넘어 하나). EnsureWorld 전에는 null.</summary>
        public FarmGrid Farm { get; private set; }
        /// <summary>오늘의 채집 자리. EnsureWorld 전에는 null.</summary>
        public GatherDayState Gathering { get; private set; }
        /// <summary>오늘의 게시판. EnsureWorld 전에는 null.</summary>
        public RequestBoardState Board { get; private set; }
        public WorldSetup Setup { get; private set; }
        public GatherRespawnStep GatherStep { get; private set; }
        public BoardRefreshStep BoardStep { get; private set; }

        /// <summary>빈 진행 상태. 시작 아이템은 부르는 쪽이 저장 파일이 없을 때만 넣는다.</summary>
        public static PlayerProgress CreateNew(GameBalanceConfig balance)
        {
            var inventory = new Inventory(balance.InventorySlotCount);
            var score = new ScoreBook();
            var journal = new MaterialJournal();
            var requests = new RequestLog(inventory, score, journal);
            return new PlayerProgress(inventory, score, journal, requests);
        }

        /// <summary>
        /// 밭 격자·채집 자리·게시판·시계를 한 번 만들고(이미 있으면 그대로), 하루 시작 단계를 걸고, 새 게임 시작 날로 시작한다.
        /// 새로 만들었으면 true. 밭은 저장 보관본대로 되살린다.
        /// </summary>
        public bool EnsureWorld(WorldSetup setup)
        {
            if (Clock != null) return false;
            if (setup == null || setup.Time == null || setup.Calendar == null || setup.Farm == null)
                throw new ArgumentException("WorldSetup에 Time·Calendar·Farm 설정이 필요합니다.");
            Setup = setup;
            var random = setup.Random ?? new SystemRandom();

            Farm = new FarmGrid(setup.Farm.GridSize, new TimedGrowthRule());
            RestoreFarm(Farm);
            Gathering = new GatherDayState(setup.Gathering != null ? setup.Gathering.SpotsPerKind : 1);
            Board = new RequestBoardState();

            Clock = new GameClock(setup.Time, setup.Calendar);
            Farm.SetClock(() => Clock.AbsoluteMinute);
            Clock.MinuteChanged += Farm.UpdateGrowth;
            Requests.Connect(setup.Quest, Board, Clock, Farm);
            Days = new DayFlow(Clock);
            Days.AddStep(new CropGrowthStep(Farm));
            if (setup.Gathering != null)
            {
                GatherStep = new GatherRespawnStep(Gathering, setup.Gathering, random);
                Days.AddStep(GatherStep);
            }
            if (setup.Quest != null)
            {
                BoardStep = new BoardRefreshStep(Board, setup.Quest, setup.RequestPool, random)
                {
                    IsAccepted = Requests.IsAccepted,
                    // 포기한 의뢰도 전달한 의뢰처럼: 평소엔 빼고 후보가 바닥날 때만 다시 게시(7-8)
                    IsDelivered = r => Requests.IsDelivered(r) || Requests.IsAbandoned(r)
                };
                Days.AddStep(BoardStep);
            }
            if (_pendingWorld != null) RestoreWorld(_pendingWorld);
            else Days.BeginNewGame();
            _pendingWorld = null;
            return true;
        }

        /// <summary>시계·보정·게시판·채집 자리를 보관본으로 꺼낸다 (세계가 없으면 null).</summary>
        public WorldSnapshot CaptureWorld()
        {
            if (Clock == null) return null;
            var snap = new WorldSnapshot { Day = Clock.Day, Minute = Clock.Minute, BoardDay = Board.Day, GatherDay = Gathering.Day };
            foreach (var pair in Days.NextDayShift.Requests) snap.Shifts.Add(pair);
            snap.Offers.AddRange(Board.Offers);
            snap.BoardHistory.AddRange(Board.History);
            foreach (GatherHabitatKind kind in Enum.GetValues(typeof(GatherHabitatKind)))
                for (int i = 0; i < Gathering.SpotsPerKind; i++)
                {
                    var m = Gathering.Get(kind, i);
                    if (m != null) snap.GatherSpots.Add(new GatherSpotEntry(kind, i, m));
                }
            return snap;
        }

        /// <summary>저장 파일에서 읽은 세계. 세계가 이미 있으면 바로, 없으면 EnsureWorld 때 새 게임 대신 이어 간다.</summary>
        internal void SetWorld(WorldSnapshot snapshot)
        {
            if (Clock != null) RestoreWorld(snapshot);
            else _pendingWorld = snapshot;
        }

        private void RestoreWorld(WorldSnapshot snap)
        {
            if (snap == null) return;
            Days.NextDayShift.Clear();
            foreach (var pair in snap.Shifts) Days.NextDayShift.Request(pair.Key, pair.Value);
            Board.Restore(snap.BoardDay, snap.Offers, snap.BoardHistory);
            var spots = new Dictionary<GatherHabitatKind, GatherMaterialDefinition[]>();
            foreach (GatherHabitatKind kind in Enum.GetValues(typeof(GatherHabitatKind)))
                spots[kind] = new GatherMaterialDefinition[Gathering.SpotsPerKind];
            foreach (var s in snap.GatherSpots)
                if (s.Spot >= 0 && s.Spot < Gathering.SpotsPerKind) spots[s.Kind][s.Spot] = s.Material;
            Gathering.Set(snap.GatherDay, spots);
            Days.ResumeDay(Math.Max(1, snap.Day), snap.Minute);
        }

        /// <summary>밭 격자의 작물을 보관본으로 옮겨 담는다 (저장 직전).</summary>
        public void CaptureFarm(FarmGrid grid)
        {
            if (grid == null) return;
            _farmPlots.Clear();
            foreach (var plot in grid.Plots)
                _farmPlots.Add(new FarmPlotState(plot.Seed, plot.Origin, plot.IsWatered, plot.WateredAt, plot.IsGrown, plot.Facing));
        }

        /// <summary>보관본대로 밭 격자를 되살린다 (격자를 비운 뒤, 성장 규칙을 다시 적용하지 않음).</summary>
        public void RestoreFarm(FarmGrid grid)
        {
            if (grid == null) return;
            grid.Clear();
            foreach (var state in _farmPlots)
                grid.TryRestore(state.Seed, state.Origin, state.Facing, state.Watered, state.WateredAt, state.Grown, out _);
        }

        /// <summary>저장 파일에서 읽은 밭. 격자가 이미 있으면 바로 되살린다.</summary>
        internal void SetFarmPlots(IEnumerable<FarmPlotState> plots)
        {
            _farmPlots.Clear();
            _farmPlots.AddRange(plots);
            if (Farm != null) RestoreFarm(Farm);
        }
    }
}
