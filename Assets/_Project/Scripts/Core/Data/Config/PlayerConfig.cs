// 플레이어 이동과 쿼터뷰 카메라 설정.
using UnityEngine;

namespace VoxelMagic.Core
{
    [CreateAssetMenu(menuName = "VoxelMagic/Config/Player", fileName = "PlayerConfig")]
    public sealed class PlayerConfig : ScriptableObject
    {
        [SerializeField, Min(0.1f)] private float _moveSpeed = 4f;
        [SerializeField, Min(1f)] private float _turnSpeedDegrees = 720f;
        [SerializeField, Min(0.1f)] private float _interactRadius = 1.6f;
        [SerializeField, Min(1f)] private float _cameraDistance = 14f;
        [SerializeField, Range(10f, 89f)] private float _cameraPitch = 45f;
        [SerializeField] private float _cameraYaw = 45f;
        [SerializeField, Min(0.1f)] private float _cameraFollowSharpness = 8f;

        public float MoveSpeed => _moveSpeed;
        public float TurnSpeedDegrees => _turnSpeedDegrees;
        public float InteractRadius => _interactRadius;
        public float CameraDistance => _cameraDistance;
        public float CameraPitch => _cameraPitch;
        public float CameraYaw => _cameraYaw;
        public float CameraFollowSharpness => _cameraFollowSharpness;
    }
}
