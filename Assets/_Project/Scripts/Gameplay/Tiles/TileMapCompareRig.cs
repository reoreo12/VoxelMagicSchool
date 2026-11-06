// 비교용 오브젝트(카메라·치비·1 m 상자 / 연못)를 작업 대상 맵의 가운데와 동쪽 끝으로 옮긴다. 칸 크기가 바뀌어도 다시 맞춘다.
// 비교 오브젝트 자신의 배율은 바꾸지 않는다.
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    [ExecuteAlways]
    public sealed class TileMapCompareRig : MonoBehaviour
    {
        [SerializeField] private TileMapConfig _config;
        [SerializeField] private TileMapDocument _target;
        [Tooltip("맵 가운데로 옮길 기준점 (카메라·치비·상자)")]
        [SerializeField] private Transform _center;
        [Tooltip("맵 동쪽 끝 가운데로 옮길 기준점 (연못)")]
        [SerializeField] private Transform _east;

        public TileMapDocument Target => _target;

        public void Setup(TileMapConfig config, Transform center, Transform east)
        {
            if (_config != null) _config.Changed -= Place;
            _config = config;
            _center = center;
            _east = east;
            if (isActiveAndEnabled && _config != null) _config.Changed += Place;
            Place();
        }

        public void SetTarget(TileMapDocument target)
        {
            _target = target;
            Place();
        }

        private void OnEnable()
        {
            if (_config != null) _config.Changed += Place;
            Place();
        }

        private void OnDisable()
        {
            if (_config != null) _config.Changed -= Place;
        }

        private void OnValidate() => Place();

        public void Place()
        {
            if (_config == null || _target == null || this == null) return;
            float size = _config.CellSize;
            var rect = _target.PaintedCellRect();
            float midY = rect.y + rect.height * 0.5f;
            if (_center != null) _center.position = _target.CellToWorld(new Vector2(rect.x + rect.width * 0.5f, midY), size);
            if (_east != null) _east.position = _target.CellToWorld(new Vector2(rect.xMax, midY), size);
        }
    }
}
