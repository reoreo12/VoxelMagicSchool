// 뒷산 포획 조절 값. 입질 간격·흔들림 시간·동시 흔들림 수·근처 길을 지날 때 맨 앞 풀숲 흔들림을 정한다 (기본값은 모두 가정).
using UnityEngine;

namespace VoxelMagic.Core
{
    [CreateAssetMenu(menuName = "VoxelMagic/Config/Capture", fileName = "CaptureConfig")]
    public sealed class CaptureConfig : ScriptableObject
    {
        [Header("입질")]
        [Tooltip("구역 안에 있을 때 입질이 오는 간격 (초)")]
        [SerializeField, Min(0.1f)] private float _biteIntervalSeconds = 5f;
        [Tooltip("입질 풀숲이 흔들리는 시간 (초). 끝나면 동물이 사라진다")]
        [SerializeField, Min(0.1f)] private float _biteShakeSeconds = 3f;
        [Tooltip("한 번에 흔들리기 시작하는 풀숲 수 (최소, 최대)")]
        [SerializeField] private Vector2Int _bitesPerWave = new Vector2Int(1, 2);

        [Header("근처 산길을 지나갈 때 (맨 앞 풀숲 하나, 동물 없음)")]
        [Tooltip("맨 앞 풀숲이 짧게 흔들리는 시간 (초)")]
        [SerializeField, Min(0.05f)] private float _passShakeSeconds = 0.6f;
        [Tooltip("한 번 흔든 뒤 다시 흔들 수 있기까지 (초). 근처 길을 벗어났다 다시 와야 한다")]
        [SerializeField, Min(0f)] private float _passCooldownSeconds = 8f;

        [Header("흔들림 모양")]
        [SerializeField, Min(0f)] private float _shakeAngle = 12f;
        [SerializeField, Min(0.1f)] private float _shakeFrequency = 14f;
        [Tooltip("지나갈 때 흔들림 세기 (입질 흔들림 대비 배율)")]
        [SerializeField, Range(0f, 1f)] private float _passShakeScale = 0.7f;
        [Tooltip("흔들림이 끝날 때 잦아드는 시간 (초). 0이면 뚝 멈춘다")]
        [SerializeField, Min(0f)] private float _settleSeconds = 0.35f;

        public float BiteIntervalSeconds => _biteIntervalSeconds;
        public float BiteShakeSeconds => _biteShakeSeconds;
        public int MinBites => Mathf.Max(1, Mathf.Min(_bitesPerWave.x, _bitesPerWave.y));
        public int MaxBites => Mathf.Max(1, Mathf.Max(_bitesPerWave.x, _bitesPerWave.y));
        public float PassShakeSeconds => _passShakeSeconds;
        public float PassCooldownSeconds => _passCooldownSeconds;
        public float ShakeAngle => _shakeAngle;
        public float ShakeFrequency => _shakeFrequency;
        public float PassShakeScale => _passShakeScale;
        public float SettleSeconds => _settleSeconds;
    }
}
