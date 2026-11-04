// 고정 각도(회전 없음)로 플레이어를 부드럽게 따라가는 쿼터뷰 카메라. 마우스 스크롤로 거리를 줄이고 늘린다(줌).
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class QuarterViewCamera : MonoBehaviour, IGameContextBinder
    {
        [SerializeField] private Transform _target;
        [SerializeField] private GameInputReader _input;

        private PlayerConfig _config;
        private float _distance;
        private float _targetDistance;

        public void Bind(GameContext context)
        {
            _config = context.PlayerConfig;
            _distance = _targetDistance = ClampDistance(_config.CameraDistance);
            SnapToTarget();
        }

        private void OnEnable()
        {
            if (_input != null) _input.ZoomRequested += OnZoom;
        }

        private void OnDisable()
        {
            if (_input != null) _input.ZoomRequested -= OnZoom;
        }

        // 위로 굴리면 가까이, 아래로 굴리면 멀리.
        private void OnZoom(float direction)
        {
            if (_config == null) return;
            _targetDistance = ClampDistance(_targetDistance - direction * _config.CameraZoomStep);
        }

        private void LateUpdate()
        {
            if (_config == null || _target == null) return;
            float t = 1f - Mathf.Exp(-_config.CameraFollowSharpness * Time.deltaTime);
            _distance = Mathf.Lerp(_distance, _targetDistance, t);
            transform.position = Vector3.Lerp(transform.position, DesiredPosition(), t);
            transform.rotation = ViewRotation();
        }

        private void SnapToTarget()
        {
            if (_target == null) return;
            transform.SetPositionAndRotation(DesiredPosition(), ViewRotation());
        }

        private float ClampDistance(float distance) =>
            Mathf.Clamp(distance, _config.CameraZoomRange.x, _config.CameraZoomRange.y);

        private Quaternion ViewRotation() => Quaternion.Euler(_config.CameraPitch, _config.CameraYaw, 0f);

        private Vector3 DesiredPosition()
            => _target.position + ViewRotation() * Vector3.back * _distance;
    }
}
