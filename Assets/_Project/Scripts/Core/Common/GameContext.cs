// Core 서비스 묶음. 부트스트랩이 씬마다 한 번 만들고 IGameContextBinder로 월드와 UI에 나눠 준다.
// 플레이어 데이터는 GameSession의 PlayerProgress를 그대로 받아 씬을 옮겨도 유지된다.
namespace VoxelMagic.Core
{
    public sealed class GameContext
    {
        private GameContext() { }

        public GameBalanceConfig Balance { get; private set; }
        public FarmConfig FarmConfig { get; private set; }
        public FishingConfig FishingConfig { get; private set; }
        public PlayerConfig PlayerConfig { get; private set; }

        public IRandom Random { get; private set; }
        public Inventory Inventory { get; private set; }
        public FarmGrid FarmGrid { get; private set; }
        public FarmService Farm { get; private set; }
        public ScoreBook Score { get; private set; }
        public MaterialJournal Journal { get; private set; }
        public RequestTracker Requests { get; private set; }
        public FishingSession Fishing { get; private set; }
        public PromptChannel Prompt { get; private set; }
        public TimePause Pause { get; private set; }
        public ScreenStack Screens { get; private set; }
        public HudState Hud { get; private set; }
        /// <summary>책(메뉴) 화면의 탭·먼저 보여 줄 의뢰.</summary>
        public BookState Book { get; private set; }
        /// <summary>초상화가 있는 대사(크리처 대화·수업) 재생 창구.</summary>
        public DialogueChannel Dialogue { get; private set; }
        /// <summary>마력·기력(UI 상태 바가 읽음). 규칙 쪽 모델이 생기면 SetVitals로 연결한다. 없으면 null.</summary>
        public IPlayerVitals Vitals { get; private set; }
        /// <summary>날짜·시각(UI 시간대 HUD가 읽음). 진행 상태에 게임 시계가 있으면 그것, 없으면 null.</summary>
        public IGameTime Time { get; private set; }
        /// <summary>게임 시계 (씬을 넘어 유지, PlayerProgress 소속). 없으면 null.</summary>
        public GameClock Clock => Progress?.Clock;
        /// <summary>하루 시작·끝 진행자 (PlayerProgress 소속). 없으면 null.</summary>
        public DayFlow Days => Progress?.Days;
        /// <summary>오늘의 채집 자리 (PlayerProgress 소속). 없으면 null.</summary>
        public GatherDayState GatherDay => Progress?.Gathering;
        /// <summary>오늘의 게시판 (PlayerProgress 소속). 없으면 null.</summary>
        public RequestBoardState Board => Progress?.Board;
        public GameMessages Messages { get; private set; }

        public PlayerProgress Progress { get; private set; }

        /// <summary>플레이어 데이터(인벤토리·점수·도감·의뢰)는 씬을 넘어 유지되는 progress의 것을 쓰고, 나머지는 씬마다 새로 만든다.</summary>
        public void SetVitals(IPlayerVitals vitals) => Vitals = vitals;
        public void SetTime(IGameTime time) => Time = time;

        public static GameContext Create(GameBalanceConfig balance, FarmConfig farm, FishingConfig fishing,
            PlayerConfig player, PlayerProgress progress, IRandom random)
        {
            if (progress == null) throw new System.ArgumentNullException(nameof(progress));
            var context = new GameContext
            {
                Balance = balance,
                FarmConfig = farm,
                FishingConfig = fishing,
                PlayerConfig = player,
                Random = random,
                Progress = progress,
                Time = progress.Clock,
                Messages = new GameMessages(),
                Prompt = new PromptChannel(),
                Journal = progress.Journal,
                Inventory = progress.Inventory,
                Score = progress.Score,
                Requests = progress.Requests,
                // 밭 격자는 진행 상태 소속(씬을 넘어 하나). 진행 상태에 세계가 아직 없으면(시험용) 이 씬에서만 쓰는 격자.
                FarmGrid = progress.Farm ?? new FarmGrid(farm.GridSize, new TimedGrowthRule())
            };
            context.Pause = new TimePause();
            context.Screens = new ScreenStack(context.Pause);
            context.Hud = new HudState();
            context.Book = new BookState();
            context.Dialogue = new DialogueChannel();
            context.Farm = new FarmService(context.FarmGrid, context.Inventory, random, context.Messages);
            context.Fishing = new FishingSession(fishing, context.Inventory, random, context.Messages,
                balance.AcquiredInfoSeconds);
            return context;
        }
    }
}
