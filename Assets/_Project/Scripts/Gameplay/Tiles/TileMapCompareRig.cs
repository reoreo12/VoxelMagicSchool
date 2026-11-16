// 비교용 오브젝트: 가운데(카메라·치비·1 m 상자)는 작업 대상 맵 가운데로, 동쪽(연못·밭)은 붙여 둔 맵의 동쪽 끝에 둔다. 칸 크기가 바뀌어도 다시 맞춘다.
// 비교 오브젝트 자신의 배율은 바꾸지 않는다.
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    [ExecuteAlways]
    public sealed class TileMapCompareRig : MonoBehaviour
    {
        [SerializeField] private TileMapConfig _config;
        [Tooltip("가운데 기준점이 따라가는 맵 (작업 대상 맵)")]
        [SerializeField] private TileMapDocument _target;
        [Tooltip("동쪽 기준점이 붙어 있는 맵. 작업 대상이 바뀌어도 그대로 (비면 _target)")]
        [SerializeField] private TileMapDocument _eastTarget;
        [Tooltip("맵 가운데로 옮길 기준점 (카메라·치비·상자)")]
        [SerializeField] private Transform _center;
        [Tooltip("맵 동쪽 끝 가운데로 옮길 기준점 (연못·밭)")]
        [SerializeField] private Transform _east;

        public TileMapDocument Target => _target;
        public TileMapDocument EastTarget => _eastTarget != null ? _eastTarget : _target;

        public void Setup(TileMapConfig config, Transform center, Transform east)
        {
            if (_config != null) _config.Changed -= Place;
            _config = config;
            _center = center;
            _east = east;
            if (isActiveAndEnabled && _config != null) _config.Changed += Place;
            Place();
        }

        /// <summary>가운데·동쪽 기준점을 모두 이 맵에 맞춘다</summary>
        public void SetTarget(TileMapDocument target)
        {
            _target = target;
            _eastTarget = target;
            Place();
        }

        /// <summary>가운데 기준점(카메라·치비)만 이 맵으로 옮긴다. 동쪽(연못·밭)은 붙어 있던 맵에 그대로</summary>
        public void FollowCenter(TileMapDocument target)
        {
            if (_eastTarget == null) _eastTarget = _target;
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
            if (_config == null || this == null) return;
            float size = _config.CellSize;
            if (_center != null && _target != null)
            {
                var rect = _target.PaintedCellRect();
                _center.position = _target.CellToWorld(new Vector2(rect.x + rect.width * 0.5f, rect.y + rect.height * 0.5f), size);
            }
            var east = EastTarget;
            if (_east != null && east != null)
            {
                var rect = east.PaintedCellRect();
                _east.position = east.CellToWorld(new Vector2(rect.xMax, rect.y + rect.height * 0.5f), size);
            }
        }
    }
}
