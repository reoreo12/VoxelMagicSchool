// 포획 구역 영역. 플레이어가 영역 안에 있는지 매 프레임 포획 규칙에 알리고(밖이면 새 입질 없음),
// 구역 근처 산길을 걸어 지나갈 때 맨 앞 풀숲 하나만 짧게 한 번 흔들어 "여기에 뭔가 온다"는 신호를 준다.
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class CaptureZone : MonoBehaviour, IMountainContextBinder
    {
        [Tooltip("구역 영역 (Is Trigger). 플레이어 발 위치가 이 안이면 구역 안")]
        [SerializeField] private BoxCollider _area;
        [Tooltip("구역 근처 산길 영역 (Is Trigger). 여기를 걸어 지나가면 맨 앞 풀숲이 흔들린다")]
        [SerializeField] private BoxCollider _nearPath;
        [Tooltip("근처 길을 지날 때 흔들 맨 앞 풀숲 (길에서 보이는 것)")]
        [SerializeField] private GrassTuft _frontTuft;
        [SerializeField] private Transform _player;
        [Tooltip("이 속도(m/s)보다 빠르면 '지나가는 중'")]
        [SerializeField, Min(0f)] private float _passMinSpeed = 0.5f;

        private CaptureService _capture;
        private CaptureConfig _config;
        private Vector3 _lastPosition;
        private bool _armed = true;
        private float _cooldownUntil;

        public bool PlayerInside { get; private set; }
        public bool PlayerNearPath { get; private set; }

        public void Setup(CaptureConfig config) => _config = config;

        public void Bind(MountainContext context)
        {
            _capture = context.Capture;
            if (_player != null) _lastPosition = _player.position;
        }

        private void Update()
        {
            if (_capture == null || _player == null) return;
            var position = _player.position;
            var feet = position + Vector3.up * 0.5f;
            PlayerInside = _area.bounds.Contains(feet);
            PlayerNearPath = !PlayerInside && _nearPath != null && _nearPath.bounds.Contains(feet);
            _capture.Tick(Time.deltaTime, PlayerInside);

            var moved = position - _lastPosition;
            moved.y = 0f;
            _lastPosition = position;
            bool walking = Time.deltaTime > 0f && moved.magnitude / Time.deltaTime > _passMinSpeed;

            // 근처 길에 들어와 걷는 동안 한 번만. 길을 벗어나야 다시 준비되고, 쿨다운도 지나야 한다.
            if (!PlayerNearPath)
            {
                _armed = true;
                return;
            }
            if (!_armed || !walking || _frontTuft == null || Time.time < _cooldownUntil) return;
            _capture.PassBy(_frontTuft.Index);
            _armed = false;
            _cooldownUntil = Time.time + _config.PassCooldownSeconds;
        }
    }
}
