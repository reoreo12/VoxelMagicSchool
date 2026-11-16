// 매 프레임 FishingSession을 진행시키고, 상태에 맞게 찌와 낚싯줄을 연출한다 (던지기 포물선, 기다림 출렁임, 입질 가라앉음).
// 낚싯줄 시작점은 손에 든 지팡이 끝(ICastOrigin), 없으면 기본 WandTip. 찌는 던지기의 "놓는 순간"부터 날아간다.
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class FishingDriver : MonoBehaviour, IGameContextBinder
    {
        [SerializeField] private PondArea _pond;
        [SerializeField] private Transform _bobber;
        [SerializeField] private LineRenderer _line;
        [Tooltip("시작점이 없을 때(지팡이를 안 들었을 때) 쓰는 기본 낚싯줄 시작점")]
        [SerializeField] private Transform _wandTip;
        [Tooltip("ICastOrigin 구현 (플레이어의 CharacterEquipment). 손에 든 지팡이 끝을 시작점으로 쓴다")]
        [SerializeField] private MonoBehaviour _castOriginSource;
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
        private ICastOrigin _castOrigin;

        private void Awake() => _castOrigin = _castOriginSource as ICastOrigin;

        private Vector3 LineStart() =>
            _castOrigin != null && _castOrigin.TryGetOrigin(out var origin) ? origin : _wandTip.position;

        public void Bind(GameContext context)
        {
            _session = context.Fishing;
            _castDuration = Mathf.Max(0.01f, context.FishingConfig.CastSeconds);
            _session.StateChanged += OnStateChanged;
            _session.CastReleased += OnCastReleased;
            OnStateChanged(_session.State);
        }

        private void OnDestroy()
        {
            if (_session == null) return;
            _session.StateChanged -= OnStateChanged;
            _session.CastReleased -= OnCastReleased;
        }

        // 던지기 상태에서는 "놓는 순간"(애니메이션 이벤트)까지 찌·줄을 숨긴다.
        private void OnStateChanged(FishingState state)
        {
            _stateTime = 0f;
            bool visible = state == FishingState.Casting ? _session.IsCastReleased
                : state != FishingState.Idle && state != FishingState.Result;
            SetVisible(visible);
        }

        // 놓는 순간부터 지팡이 끝에서 찌가 포물선으로 날아간다 (CastSeconds 동안).
        private void OnCastReleased()
        {
            _stateTime = 0f;
            _castStart = LineStart();
            _pond.UpdateCastPoint(_castStart);
            SetVisible(true);
        }

        private void SetVisible(bool visible)
        {
            _bobber.gameObject.SetActive(visible);
            _line.enabled = visible;
        }

        private void Update()
        {
            if (_session == null) return;
            _session.Tick(Time.deltaTime);
            _stateTime += Time.deltaTime;
        }

        // 손에 든 지팡이는 Update 뒤 애니메이션이 움직이므로, 줄은 그 다음(LateUpdate)에 그려야 한 프레임 밀리지 않는다.
        private void LateUpdate()
        {
            if (_session == null || !_bobber.gameObject.activeSelf) return;

            _bobber.position = BobberPosition(_session.State);
            _line.SetPosition(0, LineStart());
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
