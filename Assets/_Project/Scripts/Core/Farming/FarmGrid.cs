// N×N 텃밭 격자. 작물 배치 가능 여부를 판단하고, 심기·물 주기·제거 결과를 이벤트로 알린다.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelMagic.Core
{
    public enum PlacementResult
    {
        Ok,
        OutOfBounds,
        Occupied
    }

    public sealed class FarmGrid
    {
        private readonly FarmPlot[,] _cells;
        private readonly List<FarmPlot> _plots = new List<FarmPlot>();
        private readonly IGrowthRule _growthRule;
        private int _nextId;

        public event Action<FarmPlot> PlotAdded;
        public event Action<FarmPlot> PlotChanged;
        public event Action<FarmPlot> PlotRemoved;

        public FarmGrid(int size, IGrowthRule growthRule)
        {
            if (size < 1) throw new ArgumentOutOfRangeException(nameof(size));
            Size = size;
            _cells = new FarmPlot[size, size];
            _growthRule = growthRule ?? throw new ArgumentNullException(nameof(growthRule));
        }

        public int Size { get; }
        public IReadOnlyList<FarmPlot> Plots => _plots;

        public bool IsInside(Vector2Int cell)
            => cell.x >= 0 && cell.y >= 0 && cell.x < Size && cell.y < Size;

        public FarmPlot GetPlotAt(Vector2Int cell) => IsInside(cell) ? _cells[cell.x, cell.y] : null;

        public PlacementResult CanPlace(Vector2Int origin, Vector2Int footprint)
        {
            var far = origin + footprint - Vector2Int.one;
            if (!IsInside(origin) || !IsInside(far)) return PlacementResult.OutOfBounds;

            for (int x = origin.x; x <= far.x; x++)
                for (int y = origin.y; y <= far.y; y++)
                    if (_cells[x, y] != null) return PlacementResult.Occupied;
            return PlacementResult.Ok;
        }

        public bool TryPlant(SeedDefinition seed, Vector2Int origin, out FarmPlot plot)
        {
            plot = null;
            if (seed == null || CanPlace(origin, seed.Footprint) != PlacementResult.Ok) return false;

            plot = new FarmPlot(_nextId++, seed, origin);
            SetCells(plot, plot);
            _plots.Add(plot);
            PlotAdded?.Invoke(plot);
            return true;
        }

        /// <summary>물을 준다. 상태가 바뀌었으면 true.</summary>
        public bool Water(FarmPlot plot)
        {
            if (plot == null || !_plots.Contains(plot) || plot.IsGrown || plot.IsWatered) return false;

            plot.MarkWatered();
            plot.SetStage(_growthRule.StageAfterWatering(plot));
            PlotChanged?.Invoke(plot);
            return true;
        }

        public bool Remove(FarmPlot plot)
        {
            if (plot == null || !_plots.Remove(plot)) return false;
            SetCells(plot, null);
            PlotRemoved?.Invoke(plot);
            return true;
        }

        private void SetCells(FarmPlot area, FarmPlot value)
        {
            for (int x = 0; x < area.Size.x; x++)
                for (int y = 0; y < area.Size.y; y++)
                    _cells[area.Origin.x + x, area.Origin.y + y] = value;
        }
    }
}
