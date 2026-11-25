// FarmGrid 이벤트를 받아 작물 모양(CropView)을 만들고, 바꾸고, 지운다. 로직은 모르고 보여 주기만 한다.
// 격자는 씬을 넘어 유지되므로 연결할 때 이미 심긴 작물을 먼저 그린다.
using System.Collections.Generic;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class FarmFieldView : MonoBehaviour, IGameContextBinder
    {
        [SerializeField] private FarmField _field;
        [SerializeField] private Transform _cropRoot;

        private readonly Dictionary<FarmPlot, CropView> _views = new Dictionary<FarmPlot, CropView>();
        private FarmGrid _grid;
        private float _growSeconds;

        public void Bind(GameContext context)
        {
            _grid = context.FarmGrid;
            _growSeconds = context.FarmConfig.GrowAnimationSeconds;
            _grid.PlotAdded += OnPlotAdded;
            _grid.PlotChanged += OnPlotChanged;
            _grid.PlotRemoved += OnPlotRemoved;
            foreach (var plot in _grid.Plots) AddView(plot, animate: false);
        }

        private void OnDestroy()
        {
            if (_grid == null) return;
            _grid.PlotAdded -= OnPlotAdded;
            _grid.PlotChanged -= OnPlotChanged;
            _grid.PlotRemoved -= OnPlotRemoved;
        }

        private void OnPlotAdded(FarmPlot plot)
        {
            if (this == null) { OnDestroy(); return; } // 꺼진 채 씬이 내려간 구독자 정리 (격자는 씬을 넘어 유지)
            AddView(plot, animate: true);
        }

        private void AddView(FarmPlot plot, bool animate)
        {
            if (_views.ContainsKey(plot)) return;
            var go = new GameObject($"Crop_{plot.Seed.Id}_{plot.Id}");
            go.transform.SetParent(_cropRoot, false);
            go.transform.position = _field.AreaCenter(plot.Origin, plot.Size);
            var view = go.AddComponent<CropView>();
            view.Show(plot, _growSeconds, animate);
            _views.Add(plot, view);
        }

        private void OnPlotChanged(FarmPlot plot)
        {
            if (this == null) { OnDestroy(); return; }
            if (_views.TryGetValue(plot, out var view)) view.Show(plot, _growSeconds, animate: plot.IsGrown);
        }

        private void OnPlotRemoved(FarmPlot plot)
        {
            if (this == null) { OnDestroy(); return; }
            if (!_views.TryGetValue(plot, out var view)) return;
            _views.Remove(plot);
            Destroy(view.gameObject);
        }
    }
}
