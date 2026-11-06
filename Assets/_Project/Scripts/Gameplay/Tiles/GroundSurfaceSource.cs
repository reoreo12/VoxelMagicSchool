// 구운 바닥 프리팹 루트에 붙어 월드 위치 → 바닥 종류를 알려 준다. 루트 배율 = 칸 크기라 로컬 좌표가 곧 칸 좌표다.
// 켜질 때 GroundSurfaceQuery에 등록되므로, 쓰는 쪽은 GroundSurfaceQuery.TryGet 하나만 부르면 된다.
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class GroundSurfaceSource : MonoBehaviour
    {
        [SerializeField] private GroundSurfaceMap _map;

        public GroundSurfaceMap Map => _map;

        public void SetMap(GroundSurfaceMap map) => _map = map;

        private void OnEnable() => GroundSurfaceQuery.Register(this);
        private void OnDisable() => GroundSurfaceQuery.Unregister(this);

        /// <summary>이 바닥 위 위치면 true (칠하지 않은 칸은 surface = null)</summary>
        public bool TryGetSurface(Vector3 worldPosition, out GroundSurface surface)
        {
            surface = null;
            if (_map == null) return false;
            var local = transform.InverseTransformPoint(worldPosition);
            surface = _map.SurfaceAtLocal(local);
            return surface != null;
        }
    }
}
