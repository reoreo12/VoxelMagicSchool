// 매 프레임 FishingSession을 진행시키고, 상태에 맞게 찌와 낚싯줄을 연출한다 (던지기 포물선, 기다림 출렁임, 입질 가라앉음).
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class FishingDriver : MonoBehaviour, IGameContextBinder
    {
        [SerializeField] private PondArea _pond;
        [SerializeField] private Transform _bobber;
        [SerializeField] private LineRenderer _line;
        [SerializeField] private Transform _wandTip;
        [Header("연출")]
        [SerializeField] private float _castArcHeight = 1.5f;
        [SerializeField] private float _bobAmplitude = 0.03f;
        [SerializeField] private float _bobFrequency = 2f;
        [SerializeField] private float _biteSink = 0.12f;
        [SerializeField] private float _biteShake = 0.05f;

        private FishingSession _session;
        private float _castDuration;
        private float _stateTime;
        private Vector3 _castStart;

        public void Bind(GameContext context)
        {
            _session = context.Fishing;
            _castDuration = Mathf.Max(0.01f, context.FishingConfig.CastSeconds);
            _session.StateChanged += OnStateChanged;
            OnStateChanged(_session.State);
        }

        private void OnDestroy()
        {
            if (_session != null) _session.StateChanged -= OnStateChanged;
        }

        private void OnStateChanged(FishingState state)
        {
            _stateTime = 0f;
            if (state == FishingState.Casting)
            {
                _pond.UpdateCastPoint(_wandTip.position);
                _castStart = _wandTip.position;
            }
            bool visible = state != FishingState.Idle && state != FishingState.Result;
            _bobber.gameObject.SetActive(visible);
            _line.enabled = visible;
        }

        private void Update()
        {
            if (_session == null) return;
            _session.Tick(Time.deltaTime);
            _stateTime += Time.deltaTime;
            if (!_bobber.gameObject.activeSelf) return;

            _bobber.position = BobberPosition(_session.State);
            _line.SetPosition(0, _wandTip.position);
            _line.SetPosition(1, _bobber.position);
        }

        private Vector3 BobberPosition(FishingState state)
        {
            var rest = _pond.CastPoint;
            switch (state)
            {
                case FishingState.Casting:
                    float t = Mathf.Clamp01(_stateTime / _castDuration);
                    return Vector3.Lerp(_castStart, rest, t) + Vector3.up * (_castArcHeight * 4f * t * (1f - t));
                case FishingState.Waiting:
                    return rest + Vector3.up * (Mathf.Sin(_stateTime * _bobFrequency * Mathf.PI * 2f) * _bobAmplitude);
                case FishingState.Biting:
                case FishingState.Minigame:
                    var shake = new Vector3(Mathf.Sin(_stateTime * 37f), 0f, Mathf.Cos(_stateTime * 29f)) * _biteShake;
                    return rest + Vector3.down * _biteSink + shake;
                default:
                    return rest;
            }
        }
    }
}
