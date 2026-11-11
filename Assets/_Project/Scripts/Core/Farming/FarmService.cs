// 텃밭 칸과 선택한 아이템으로 할 수 있는 행동(심기/물 주기/뽑기)을 판단하고 실행한다.
// 안내 문구 판단과 실행 규칙을 여기 모아, Gameplay는 플레이어 앞 칸 계산만 하면 되게 한다.
using System;
using UnityEngine;

namespace VoxelMagic.Core
{
    public enum FarmActionType
    {
        None,
        Plant,
        Water,
        Harvest
    }

    public readonly struct FarmAction
    {
        public static readonly FarmAction None = default;

        public FarmActionType Type { get; }
        public bool CanExecute { get; }
        public string Label { get; }
        public string BlockedReason { get; }
        /// <summary>심기: 심을 위치와 크기. 그 외: 대상 작물이 차지한 영역.</summary>
        public Vector2Int Origin { get; }
        public Vector2Int Size { get; }
        public SeedDefinition Seed { get; }
        public FarmPlot Plot { get; }

        public FarmAction(FarmActionType type, bool canExecute, string label, string blockedReason,
            Vector2Int origin, Vector2Int size, SeedDefinition seed, FarmPlot plot)
        {
            Type = type;
            CanExecute = canExecute;
            Label = label;
            BlockedReason = blockedReason;
            Origin = origin;
            Size = size;
            Seed = seed;
            Plot = plot;
        }
    }

    public sealed class FarmService
    {
        private readonly Inventory _inventory;
        private readonly IRandom _random;
        private readonly GameMessages _messages;
        private FarmPlot _pendingWater;

        /// <summary>물 주기 동작 시작 (애니메이션 재생). 물은 ApplyPendingWater 때 들어간다.</summary>
        public event Action<FarmPlot> WateringStarted;
        public event Action WateringFinished;

        /// <summary>켜면 물 주기가 바로 끝나지 않고 동작(애니메이션)을 거친다. 끄면 예전처럼 즉시 (애니메이션 없는 씬).</summary>
        public bool UseWateringAction { get; set; }
        /// <summary>물 주기 동작 중 (이동·다른 상호작용 막기).</summary>
        public bool IsWatering { get; private set; }

        public FarmService(FarmGrid grid, Inventory inventory, IRandom random, GameMessages messages)
        {
            Grid = grid;
            _inventory = inventory;
            _random = random;
            _messages = messages;
        }

        public FarmGrid Grid { get; }

        /// <summary>물 주기 동작 중 물이 들어가는 순간 (애니메이션 이벤트). 한 번만 들어간다.</summary>
        public void ApplyPendingWater()
        {
            if (_pendingWater == null) return;
            Grid.Water(_pendingWater);
            _pendingWater = null;
        }

        /// <summary>물 주기 동작이 끝남. 이벤트를 못 받았어도 물은 이때 들어간다.</summary>
        public void FinishWatering()
        {
            if (!IsWatering) return;
            ApplyPendingWater();
            IsWatering = false;
            WateringFinished?.Invoke();
        }

        public FarmAction Resolve(Vector2Int cell, ItemDefinition selected)
        {
            var plot = Grid.GetPlotAt(cell);

            if (plot != null && plot.IsGrown)
            {
                bool hasRoom = _inventory.CanAdd(plot.Seed.Produce, 1);
                return new FarmAction(FarmActionType.Harvest, hasRoom, GameTexts.Harvest,
                    hasRoom ? null : GameTexts.InventoryFull, plot.Origin, plot.Size, plot.Seed, plot);
            }

            if (selected is SeedDefinition seed)
            {
                var result = Grid.CanPlace(cell, seed.Footprint);
                string reason = result switch
                {
                    PlacementResult.OutOfBounds => GameTexts.NotEnoughCells,
                    PlacementResult.Occupied => GameTexts.AlreadyPlanted,
                    _ => null
                };
                return new FarmAction(FarmActionType.Plant, result == PlacementResult.Ok, GameTexts.Plant,
                    reason, cell, seed.Footprint, seed, null);
            }

            if (plot != null)
            {
                bool hasWand = ToolDefinition.IsWand(selected);
                return new FarmAction(FarmActionType.Water, hasWand, GameTexts.Water,
                    hasWand ? null : GameTexts.NeedWandToWater, plot.Origin, plot.Size, plot.Seed, plot);
            }

            return FarmAction.None;
        }

        public bool Execute(in FarmAction action)
        {
            if (!action.CanExecute) return false;

            switch (action.Type)
            {
                case FarmActionType.Plant:
                    if (Grid.CanPlace(action.Origin, action.Seed.Footprint) != PlacementResult.Ok) return false;
                    if (!_inventory.TryRemove(action.Seed, 1)) return false;
                    return Grid.TryPlant(action.Seed, action.Origin, out _);

                case FarmActionType.Water:
                    if (!UseWateringAction) return Grid.Water(action.Plot);
                    if (IsWatering) return false;
                    _pendingWater = action.Plot;
                    IsWatering = true;
                    WateringStarted?.Invoke(action.Plot);
                    return true;

                case FarmActionType.Harvest:
                    var produce = action.Plot.Seed.Produce;
                    if (!_inventory.TryAdd(produce, 1)) return false;
                    float size = produce.SizeRangeCm.Roll(_random);
                    Grid.Remove(action.Plot);
                    _messages.ShowAcquired(new AcquiredInfo(produce, size));
                    return true;

                default:
                    return false;
            }
        }
    }
}
