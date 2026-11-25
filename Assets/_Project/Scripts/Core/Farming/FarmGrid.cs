// N×N 텃밭 격자. 작물 배치 가능 여부를 판단하고, 심기·물 주기·시간 성장·제거 결과를 이벤트로 알린다. 씬을 넘어 진행 상태에 하나만 있다.
// 지금 게임 시각(누적 분)은 SetClock으로 받는다. 시각이 흐를 때 UpdateGrowth, 하루 시작에 AdvanceDay를 부른다.
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
        private IGrowthRule _growthRule;
        private int _nextId;

        public event Action<FarmPlot> PlotAdded;
        public event Action<FarmPlot> PlotChanged;
        public event Action<FarmPlot> PlotRemoved;

        private Func<float> _now = () => 0f;

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

            plot.MarkWatered(_now());
            plot.SetStage(_growthRule.StageAfterWatering(plot));
            PlotChanged?.Invoke(plot);
            return true;
        }

        /// <summary>지금 성장 규칙 (튜토리얼 모드가 바꾼다).</summary>
        public IGrowthRule GrowthRule => _growthRule;

        public void SetGrowthRule(IGrowthRule rule) => _growthRule = rule ?? throw new ArgumentNullException(nameof(rule));

        /// <summary>지금 게임 시각(누적 분)을 알려 주는 함수 (진행 상태가 게임 시계로 연결).</summary>
        public void SetClock(Func<float> now) => _now = now ?? (() => 0f);

        /// <summary>시각이 흐를 때: 시간이 다 된 작물을 다 자라게 한다.</summary>
        public void UpdateGrowth() => ApplyRule(false);

        /// <summary>하루 시작: 다음 날 자라는 작물과 밤사이 시간이 다 된 작물을 키운다.</summary>
        public void AdvanceDay() => ApplyRule(true);

        private void ApplyRule(bool dayStart)
        {
            if (_plots.Count == 0) return;
            float now = _now();
            foreach (var plot in _plots.ToArray())
            {
                if (plot.IsGrown || !plot.IsWatered) continue;
                var stage = _growthRule.StageAt(plot, now, dayStart);
                if (stage == plot.Stage) continue;
                plot.SetStage(stage);
                PlotChanged?.Invoke(plot);
            }
        }

        /// <summary>치트: 모든 작물을 다 자라게 한다.</summary>
        public void GrowAll()
        {
            foreach (var plot in _plots.ToArray())
            {
                if (plot.IsGrown) continue;
                if (!plot.IsWatered) plot.MarkWatered(_now());
                plot.SetStage(GrowthStage.Grown);
                PlotChanged?.Invoke(plot);
            }
        }

        /// <summary>저장본에서 작물을 상태 그대로 되살린다 (성장 규칙을 다시 적용하지 않음).</summary>
        public bool TryRestore(SeedDefinition seed, Vector2Int origin, bool watered, float wateredAt, bool grown, out FarmPlot plot)
        {
            if (!TryPlant(seed, origin, out plot)) return false;
            if (watered) plot.MarkWatered(wateredAt);
            if (grown) plot.SetStage(GrowthStage.Grown);
            if (watered || grown) PlotChanged?.Invoke(plot);
            return true;
        }

        /// <summary>모든 작물을 지운다 (불러오기 전).</summary>
        public void Clear()
        {
            foreach (var plot in _plots.ToArray()) Remove(plot);
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
