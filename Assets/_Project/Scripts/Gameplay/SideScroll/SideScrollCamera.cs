// 직교 카메라. 세로에 복도 높이를 꼭 맞추고, 대상을 x축으로만 따라가며(부드럽게 또는 가운데 고정) 복도 밖을 비추지 않게 제한한다.
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    [RequireComponent(typeof(Camera))]
    public sealed class SideScrollCamera : MonoBehaviour, ISideScrollBinder
    {
        [SerializeField] private Transform _target;

        private Camera _camera;
        private CorridorConfig _config;
        private CorridorBounds _bounds;
        private CameraFollowState _follow;
        private float _velocity;
        private bool _bound;

        public void Bind(SideScrollContext context)
        {
            _config = context.Config;
            _bounds = context.Bounds;
            _follow = context.CameraFollow;
            _camera = GetComponent<Camera>();
            _camera.orthographic = true;
            _camera.orthographicSize = SideScrollMath.OrthographicSizeFor(_bounds.Height);
            _bound = true;
            Snap();
        }

        public void SetTarget(Transform target)
        {
            _target = target;
            Snap();
        }

        private void LateUpdate()
        {
            if (!_bound) return;
            if (_follow.IsLocked)
            {
                // 고정: 지연 없이 대상 위치로. 부드럽게로 돌아가면 속도 0부터 이어서 따라간다.
                Snap();
                return;
            }
            float goal = ClampX(_target != null ? _target.position.x : transform.position.x);
            float x = Mathf.SmoothDamp(transform.position.x, goal, ref _velocity, _config.CameraFollowTime);
            Place(ClampX(x));
        }

        private void Snap()
        {
            if (!_bound) return;
            _velocity = 0f;
            Place(ClampX(_target != null ? _target.position.x : transform.position.x));
        }

        // 화면 반폭은 창 크기에 따라 바뀌므로 매번 다시 계산한다.
        private float ClampX(float x) =>
            SideScrollMath.ClampCameraX(x, _bounds, _camera.orthographicSize * _camera.aspect);

        private void Place(float x)
        {
            transform.SetPositionAndRotation(new Vector3(x, _bounds.Height * 0.5f, _config.CameraZ), Quaternion.identity);
        }
    }
}
