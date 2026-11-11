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
        public ScreenState Screen { get; private set; }
        public GameMessages Messages { get; private set; }

        public PlayerProgress Progress { get; private set; }

        /// <summary>플레이어 데이터(인벤토리·점수·도감·의뢰)는 씬을 넘어 유지되는 progress의 것을 쓰고, 나머지는 씬마다 새로 만든다.</summary>
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
                Messages = new GameMessages(),
                Prompt = new PromptChannel(),
                Screen = new ScreenState(),
                Journal = progress.Journal,
                Inventory = progress.Inventory,
                Score = progress.Score,
                Requests = progress.Requests,
                FarmGrid = new FarmGrid(farm.GridSize, new InstantGrowthRule())
            };
            context.Farm = new FarmService(context.FarmGrid, context.Inventory, random, context.Messages);
            context.Fishing = new FishingSession(fishing, context.Inventory, random, context.Messages,
                balance.AcquiredInfoSeconds);
            return context;
        }
    }
}
