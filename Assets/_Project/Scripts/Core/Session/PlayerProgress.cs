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
        public FarmPlotState(SeedDefinition seed, Vector2Int origin, bool watered, float wateredAt = 0f, bool grown = false)
        {
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

        private PlayerProgress(Inventory inventory, ScoreBook score, MaterialJournal journal, RequestTracker requests)
        {
            Inventory = inventory;
            Score = score;
            Journal = journal;
            Requests = requests;
        }

        public Inventory Inventory { get; }
        public ScoreBook Score { get; }
        public MaterialJournal Journal { get; }
        public RequestTracker Requests { get; }
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
        public static PlayerProgress CreateNew(GameBalanceConfig balance, IEnumerable<RequestDefinition> requests)
        {
            var inventory = new Inventory(balance.InventorySlotCount);
            var score = new ScoreBook(balance.MaxScore);
            var journal = new MaterialJournal();
            var tracker = new RequestTracker(requests, inventory, score, journal, balance.NewItemKindBonus);
            return new PlayerProgress(inventory, score, journal, tracker);
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
                    // 의뢰 규칙 개편(3단계) 전까지는 기존 RequestTracker 기준
                    IsAccepted = r => Requests.Active == r,
                    IsDelivered = r => Requests.GetStatus(r) == RequestStatus.Completed
                };
                Days.AddStep(BoardStep);
            }
            Days.BeginNewGame();
            return true;
        }

        /// <summary>밭 격자의 작물을 보관본으로 옮겨 담는다 (저장 직전).</summary>
        public void CaptureFarm(FarmGrid grid)
        {
            if (grid == null) return;
            _farmPlots.Clear();
            foreach (var plot in grid.Plots)
                _farmPlots.Add(new FarmPlotState(plot.Seed, plot.Origin, plot.IsWatered, plot.WateredAt, plot.IsGrown));
        }

        /// <summary>보관본대로 밭 격자를 되살린다 (격자를 비운 뒤, 성장 규칙을 다시 적용하지 않음).</summary>
        public void RestoreFarm(FarmGrid grid)
        {
            if (grid == null) return;
            grid.Clear();
            foreach (var state in _farmPlots)
                grid.TryRestore(state.Seed, state.Origin, state.Watered, state.WateredAt, state.Grown, out _);
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
