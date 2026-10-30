// 고정 각도(회전 없음)로 플레이어를 부드럽게 따라가는 쿼터뷰 카메라.
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class QuarterViewCamera : MonoBehaviour, IGameContextBinder
    {
        [SerializeField] private Transform _target;

        private PlayerConfig _config;

        public void Bind(GameContext context)
        {
            _config = context.PlayerConfig;
            SnapToTarget();
        }

        private void LateUpdate()
        {
            if (_config == null || _target == null) return;
            float t = 1f - Mathf.Exp(-_config.CameraFollowSharpness * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, DesiredPosition(), t);
            transform.rotation = ViewRotation();
        }

        private void SnapToTarget()
        {
            if (_target == null) return;
            transform.SetPositionAndRotation(DesiredPosition(), ViewRotation());
        }

        private Quaternion ViewRotation() => Quaternion.Euler(_config.CameraPitch, _config.CameraYaw, 0f);

        private Vector3 DesiredPosition()
            => _target.position + ViewRotation() * Vector3.back * _config.CameraDistance;
    }
}
