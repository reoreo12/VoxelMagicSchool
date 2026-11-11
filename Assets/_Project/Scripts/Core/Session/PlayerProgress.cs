// 씬을 옮겨도 유지되는 플레이어 데이터 묶음: 인벤토리·점수·도감·의뢰, 그리고 텃밭 밭 상태 보관본(나중에 스탯도 여기).
// 씬마다 다시 만드는 것(밭 격자·낚시 세션·메시지·화면 상태)은 GameContext 소속이고, 밭은 씬을 떠날 때 여기로 옮겨 담는다.
using System.Collections.Generic;
using UnityEngine;

namespace VoxelMagic.Core
{
    /// <summary>텃밭 작물 하나의 보관본 (씬 사이 유지용, 에셋 참조 그대로).</summary>
    public readonly struct FarmPlotState
    {
        public FarmPlotState(SeedDefinition seed, Vector2Int origin, bool watered)
        {
            Seed = seed;
            Origin = origin;
            Watered = watered;
        }

        public SeedDefinition Seed { get; }
        public Vector2Int Origin { get; }
        public bool Watered { get; }
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
        public IReadOnlyList<FarmPlotState> FarmPlots => _farmPlots;

        /// <summary>빈 진행 상태. 시작 아이템은 부르는 쪽이 저장 파일이 없을 때만 넣는다.</summary>
        public static PlayerProgress CreateNew(GameBalanceConfig balance, IEnumerable<RequestDefinition> requests)
        {
            var inventory = new Inventory(balance.InventorySlotCount);
            var score = new ScoreBook(balance.MaxScore);
            var journal = new MaterialJournal();
            var tracker = new RequestTracker(requests, inventory, score, journal, balance.NewItemKindBonus);
            return new PlayerProgress(inventory, score, journal, tracker);
        }

        /// <summary>지금 밭 격자의 작물을 보관본으로 옮겨 담는다 (씬을 떠나거나 저장하기 전).</summary>
        public void CaptureFarm(FarmGrid grid)
        {
            if (grid == null) return;
            _farmPlots.Clear();
            foreach (var plot in grid.Plots)
                _farmPlots.Add(new FarmPlotState(plot.Seed, plot.Origin, plot.IsWatered));
        }

        /// <summary>보관본대로 밭 격자에 다시 심는다. 뷰가 이벤트로 그리도록 뷰 연결이 끝난 뒤 부른다.</summary>
        public void RestoreFarm(FarmGrid grid)
        {
            if (grid == null) return;
            foreach (var state in _farmPlots)
            {
                if (!grid.TryPlant(state.Seed, state.Origin, out var plot)) continue;
                if (state.Watered) grid.Water(plot);
            }
        }

        internal void SetFarmPlots(IEnumerable<FarmPlotState> plots)
        {
            _farmPlots.Clear();
            _farmPlots.AddRange(plots);
        }
    }
}
