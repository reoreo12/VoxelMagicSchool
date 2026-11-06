// 맵 루트에 붙어 TileMapConfig의 칸 크기를 자기 맵의 Grid와 구운 바닥 배율로 바로 반영한다 (에디터·플레이 모두).
// 위치는 건드리지 않는다: 맵 루트를 옮기면 Grid·구운 바닥이 함께 옮겨진다. Grid 칸 = 1, 타일 그림 1장 = 1칸.
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    [ExecuteAlways]
    [RequireComponent(typeof(TileMapDocument))]
    public sealed class TileGridScaler : MonoBehaviour
    {
        // 바닥 Grid는 X 90° 회전: 로컬 +Y(타일 위쪽) = 월드 +Z(북)
        private static readonly Quaternion GridRotation = Quaternion.Euler(90f, 0f, 0f);

        [SerializeField] private TileMapConfig _config;

        public TileMapConfig Config => _config;
        public float AppliedCellSize { get; private set; }

        public void SetConfig(TileMapConfig config)
        {
            if (_config != null) _config.Changed -= Apply;
            _config = config;
            if (isActiveAndEnabled && _config != null) _config.Changed += Apply;
            Apply();
        }

        private void OnEnable()
        {
            if (_config != null) _config.Changed += Apply;
            Apply();
        }

        private void OnDisable()
        {
            if (_config != null) _config.Changed -= Apply;
        }

        private void OnValidate() => Apply();

        public void Apply()
        {
            if (_config == null || this == null) return;
            var document = GetComponent<TileMapDocument>();
            float size = _config.CellSize;
            AppliedCellSize = size;
            var scale = new Vector3(size, size, size);

            if (document.Grid != null)
            {
                var grid = document.Grid.transform;
                grid.localPosition = Vector3.zero;
                grid.localRotation = GridRotation;
                grid.localScale = scale;
            }
            if (document.BakedFloor != null)
            {
                var baked = document.BakedFloor.transform;
                baked.localPosition = Vector3.zero;
                baked.localRotation = Quaternion.identity;
                baked.localScale = scale;
            }
        }
    }
}
