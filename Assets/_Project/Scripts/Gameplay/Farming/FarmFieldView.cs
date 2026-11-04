// FarmGrid 이벤트를 받아 작물 모양(CropView)을 만들고, 바꾸고, 지운다. 로직은 모르고 보여 주기만 한다.
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
            var go = new GameObject($"Crop_{plot.Seed.Id}_{plot.Id}");
            go.transform.SetParent(_cropRoot, false);
            go.transform.position = _field.AreaCenter(plot.Origin, plot.Size);
            var view = go.AddComponent<CropView>();
            view.Show(plot, _growSeconds, animate: true);
            _views.Add(plot, view);
        }

        private void OnPlotChanged(FarmPlot plot)
        {
            if (_views.TryGetValue(plot, out var view)) view.Show(plot, _growSeconds, animate: true);
        }

        private void OnPlotRemoved(FarmPlot plot)
        {
            if (!_views.TryGetValue(plot, out var view)) return;
            _views.Remove(plot);
            Destroy(view.gameObject);
        }
    }
}
