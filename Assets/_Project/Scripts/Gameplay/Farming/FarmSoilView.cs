// 밭 칸 모양을 작물 상태에 맞춰 바꾼다: 물 준 작물이 차지한 칸은 젖은 흙(Wet), 나머지는 마른 흙(Dry).
// 칸 오브젝트의 메시·머티리얼만 바꾸고 로직은 모른다 (FarmGrid 이벤트를 듣기만 함).
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class FarmSoilView : MonoBehaviour, IGameContextBinder
    {
        [SerializeField] private FarmField _field;
        [Tooltip("물 주기 전 밭 칸 모델 (FBX)")]
        [SerializeField] private GameObject _dryModel;
        [Tooltip("물 준 뒤 밭 칸 모델 (FBX)")]
        [SerializeField] private GameObject _wetModel;

        private FarmGrid _grid;
        private float _cellSize;
        private MeshFilter[,] _filters;
        private MeshRenderer[,] _renderers;
        private Mesh _dryMesh, _wetMesh;
        private Material[] _dryMaterials, _wetMaterials;

        public void Bind(GameContext context)
        {
            _grid = context.FarmGrid;
            _cellSize = context.FarmConfig.CellSize;
            if (!ReadModel(_dryModel, out _dryMesh, out _dryMaterials)
                || !ReadModel(_wetModel, out _wetMesh, out _wetMaterials))
            {
                Debug.LogWarning($"{name}: Dry/Wet 밭 모델이 비어 있어 물 준 칸 표시를 하지 않아요.", this);
                return;
            }

            CollectCells();
            _grid.PlotAdded += Refresh;
            _grid.PlotChanged += Refresh;
            _grid.PlotRemoved += OnPlotRemoved;
        }

        private void OnDestroy()
        {
            if (_grid == null) return;
            _grid.PlotAdded -= Refresh;
            _grid.PlotChanged -= Refresh;
            _grid.PlotRemoved -= OnPlotRemoved;
        }

        // 자식 칸들을 위치로 격자 칸에 짝짓는다 (이름에 기대지 않음).
        private void CollectCells()
        {
            int size = _grid.Size;
            _filters = new MeshFilter[size, size];
            _renderers = new MeshRenderer[size, size];
            foreach (Transform child in transform)
            {
                var filter = child.GetComponentInChildren<MeshFilter>();
                var renderer = child.GetComponentInChildren<MeshRenderer>();
                if (filter == null || renderer == null) continue;
                var local = (child.position - _field.Origin) / _cellSize;
                var cell = new Vector2Int(Mathf.FloorToInt(local.x), Mathf.FloorToInt(local.z));
                if (!_grid.IsInside(cell)) continue;
                _filters[cell.x, cell.y] = filter;
                _renderers[cell.x, cell.y] = renderer;
            }
        }

        private void Refresh(FarmPlot plot) => SetArea(plot, plot.IsWatered);

        private void OnPlotRemoved(FarmPlot plot) => SetArea(plot, false);

        private void SetArea(FarmPlot plot, bool wet)
        {
            for (int x = 0; x < plot.Size.x; x++)
                for (int y = 0; y < plot.Size.y; y++)
                {
                    var c = plot.Origin + new Vector2Int(x, y);
                    var filter = _filters[c.x, c.y];
                    if (filter == null) continue;
                    filter.sharedMesh = wet ? _wetMesh : _dryMesh;
                    _renderers[c.x, c.y].sharedMaterials = wet ? _wetMaterials : _dryMaterials;
                }
        }

        private static bool ReadModel(GameObject model, out Mesh mesh, out Material[] materials)
        {
            mesh = null;
            materials = null;
            if (model == null) return false;
            var filter = model.GetComponentInChildren<MeshFilter>();
            var renderer = model.GetComponentInChildren<MeshRenderer>();
            if (filter == null || renderer == null) return false;
            mesh = filter.sharedMesh;
            materials = renderer.sharedMaterials;
            return mesh != null;
        }
    }
}
