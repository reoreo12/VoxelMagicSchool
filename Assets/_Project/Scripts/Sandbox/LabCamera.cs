// 애니메이션 랩 원근 카메라: 우클릭 드래그 회전, 휠 확대, 가운데 드래그 이동, 정면·옆·뒤·3/4 프리셋.
// UI 위에 마우스가 있으면 조작하지 않는다. 랩 전용이라 마우스를 직접 읽는다.
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace VoxelMagic.Sandbox
{
    public enum LabView
    {
        Front,
        Side,
        Back,
        ThreeQuarter
    }

    public sealed class LabCamera : MonoBehaviour
    {
        // 아바타는 +Z를 바라본다. 카메라 위치 = 대상 + 회전(pitch, yaw) × (0, 0, -거리)
        private static readonly float[] PresetYaw = { 180f, -90f, 0f, -145f };
        private const float PresetPitch = 10f;

        [SerializeField] private AnimationLabConfig _config;

        private Vector3 _target;
        private float _yaw = PresetYaw[(int)LabView.ThreeQuarter];
        private float _pitch = PresetPitch;
        private float _distance = 3f;

        public void Focus(Vector3 target, float avatarHeight)
        {
            _target = target;
            _distance = Mathf.Clamp(avatarHeight * _config.DistancePerHeight, _config.MinDistance, _config.MaxDistance);
            Place();
        }

        public void SetView(LabView view)
        {
            _yaw = PresetYaw[(int)view];
            _pitch = PresetPitch;
            Place();
        }

        private void LateUpdate()
        {
            var mouse = Mouse.current;
            if (mouse != null && !IsPointerOverUi())
            {
                var delta = mouse.delta.ReadValue();
                if (mouse.rightButton.isPressed)
                {
                    _yaw += delta.x * _config.OrbitSpeed;
                    _pitch = Mathf.Clamp(_pitch - delta.y * _config.OrbitSpeed, _config.PitchRange.x, _config.PitchRange.y);
                }
                if (mouse.middleButton.isPressed)
                    _target -= (transform.right * delta.x + transform.up * delta.y) * _config.PanSpeed * _distance;

                float scroll = mouse.scroll.ReadValue().y;
                if (!Mathf.Approximately(scroll, 0f))
                    _distance = Mathf.Clamp(_distance * (1f - scroll * _config.ZoomSpeed), _config.MinDistance, _config.MaxDistance);
            }
            Place();
        }

        private void Place()
        {
            var rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            transform.SetPositionAndRotation(_target + rotation * new Vector3(0f, 0f, -_distance), rotation);
        }

        private static bool IsPointerOverUi() =>
            EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}
