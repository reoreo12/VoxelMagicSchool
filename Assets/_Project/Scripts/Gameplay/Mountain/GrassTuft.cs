// 포획 구역의 풀숲 하나. 흔들림 상태에 맞춰 모양을 흔들고(끝날 때는 뚝 멈추지 않고 잦아든다),
// 동물이 와 있을 때(입질)만 "E: 포획"으로 마법진을 시작한다.
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class GrassTuft : InteractableBase, IMountainContextBinder
    {
        // 흔들기 시작할 때 세기가 다 오르는 시간 (짧게 고정)
        private const float RiseSeconds = 0.08f;

        [Tooltip("흔들 모양 (풀숲 도형들의 부모)")]
        [SerializeField] private Transform _visual;

        private CaptureService _capture;
        private CaptureConfig _config;
        private float _phase;
        private float _amplitude;
        private float _time;

        public int Index { get; private set; } = -1;

        public override bool CanBeTargeted => base.CanBeTargeted && _capture != null && _capture.ShakeOf(Index) == TuftShake.Bite;

        public void Setup(int index, CaptureConfig config)
        {
            Index = index;
            _config = config;
            _phase = index * 1.7f;
        }

        public void Bind(MountainContext context) => _capture = context.Capture;

        private void Update()
        {
            if (_capture == null || _visual == null) return;
            var shake = _capture.ShakeOf(Index);
            float full = _config.ShakeAngle;
            float target = shake == TuftShake.Bite ? full : shake == TuftShake.Pass ? full * _config.PassShakeScale : 0f;

            // 오를 때는 빠르게, 끝날 때는 settleSeconds 동안 잦아들게 (상태가 None이 되어도 바로 멈추지 않음)
            float rate = target > _amplitude ? full / RiseSeconds
                : _config.SettleSeconds > 0f ? full / _config.SettleSeconds : float.MaxValue;
            _amplitude = Mathf.MoveTowards(_amplitude, target, rate * Time.deltaTime);
            if (_amplitude <= 0f)
            {
                _visual.localRotation = Quaternion.identity;
                return;
            }
            _time += Time.deltaTime;
            float t = _time * _config.ShakeFrequency + _phase;
            _visual.localRotation = Quaternion.Euler(Mathf.Sin(t) * _amplitude, 0f, Mathf.Sin(t * 1.3f + 0.5f) * _amplitude);
        }

        public override InteractionPrompt GetPrompt(in InteractionContext context)
        {
            if (_capture == null || _capture.ShakeOf(Index) != TuftShake.Bite) return InteractionPrompt.None;
            if (_capture.CapturingTuft == Index)
                return InteractionPrompt.Blocked(MountainTexts.Capture, MountainTexts.Drawing, PromptAnchor);
            if (_capture.IsCapturing) return InteractionPrompt.None;
            return _capture.HasRoomFor(Index)
                ? InteractionPrompt.Available(MountainTexts.Capture, PromptAnchor)
                : InteractionPrompt.Blocked(MountainTexts.Capture, GameTexts.InventoryFull, PromptAnchor);
        }

        public override void Interact(in InteractionContext context) => _capture.BeginCapture(Index);
    }
}
