// Core 서비스 묶음. 부트스트랩이 한 번 만들고 IGameContextBinder로 월드와 UI에 나눠 준다.
using System.Collections.Generic;

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

        public static GameContext Create(GameBalanceConfig balance, FarmConfig farm, FishingConfig fishing,
            PlayerConfig player, IEnumerable<RequestDefinition> requests, IRandom random)
        {
            var context = new GameContext
            {
                Balance = balance,
                FarmConfig = farm,
                FishingConfig = fishing,
                PlayerConfig = player,
                Random = random,
                Messages = new GameMessages(),
                Prompt = new PromptChannel(),
                Screen = new ScreenState(),
                Journal = new MaterialJournal(),
                Inventory = new Inventory(balance.InventorySlotCount),
                FarmGrid = new FarmGrid(farm.GridSize, new InstantGrowthRule()),
                Score = new ScoreBook(balance.MaxScore)
            };
            context.Farm = new FarmService(context.FarmGrid, context.Inventory, random, context.Messages);
            context.Requests = new RequestTracker(requests, context.Inventory, context.Score, context.Journal,
                balance.NewItemKindBonus);
            context.Fishing = new FishingSession(fishing, context.Inventory, random, context.Messages,
                balance.AcquiredInfoSeconds);
            return context;
        }
    }
}
