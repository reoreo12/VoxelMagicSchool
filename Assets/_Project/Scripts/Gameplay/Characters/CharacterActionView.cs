// 낚시 상태·물 주기 요청을 받아 공용 Controller의 동작 상태(FishCast·FishWait·FishReel·FishHook·Water)를 재생하고,
// 클립 이벤트(놓는 순간·물 들어가는 순간)를 게임 로직에 전한다. 동작이 끝나면 지금 이동 상태(Idle/Walk/Run)로 섞어 돌아간다.
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class CharacterActionView : MonoBehaviour, IGameContextBinder
    {
        private const string ActionSpeedParameter = "ActionSpeed";

        [SerializeField] private Animator _animator;
        [SerializeField] private CharacterAnimationEvents _events;
        [Tooltip("동작이 끝났을 때 돌아갈 이동 상태를 알려 줄 쪽 (PlayerMover)")]
        [SerializeField] private PlayerMover _mover;
        [SerializeField] private CharacterActionConfig _config;

        private static readonly int ActionSpeedHash = Animator.StringToHash(ActionSpeedParameter);
        private static readonly int WaterHash = Animator.StringToHash(CharacterActionId.Water.ToString());

        private FishingSession _fishing;
        private FarmService _farm;
        private float _wateringTime;

        public void Bind(GameContext context)
        {
            _fishing = context.Fishing;
            _farm = context.Farm;
            _fishing.WaitForCastRelease = true;
            _farm.UseWateringAction = true;
            _fishing.StateChanged += OnFishingStateChanged;
            _farm.WateringStarted += OnWateringStarted;
            if (_events != null) _events.Fired += OnAnimationEvent;
        }

        private void OnDestroy()
        {
            if (_fishing != null) _fishing.StateChanged -= OnFishingStateChanged;
            if (_farm != null) _farm.WateringStarted -= OnWateringStarted;
            if (_events != null) _events.Fired -= OnAnimationEvent;
        }

        private void OnFishingStateChanged(FishingState state)
        {
            switch (state)
            {
                case FishingState.Casting:
                    Play(CharacterActionId.FishCast);
                    break;
                case FishingState.Waiting:
                    // 던지기 뒤에는 Controller가 FishWait로 넘긴다. 입질을 놓쳐 다시 기다릴 때 등만 직접 튼다.
                    if (!IsIn(CharacterActionId.FishCast) && !IsIn(CharacterActionId.FishWait)) Play(CharacterActionId.FishWait);
                    break;
                case FishingState.Minigame:
                    Play(CharacterActionId.FishReel);
                    break;
                case FishingState.Result:
                    if (_fishing.LastOutcome == FishingOutcome.Caught) Play(CharacterActionId.FishHook);
                    else ReturnToLocomotion();
                    break;
                case FishingState.Idle:
                    // 낚아채기는 끝나면 Controller가 Idle로 보낸다. 그 밖(취소·놓침 후)은 이동 상태로.
                    if (!IsIn(CharacterActionId.FishHook)) ReturnToLocomotion();
                    break;
            }
        }

        private void OnWateringStarted(FarmPlot plot)
        {
            _wateringTime = 0f;
            Play(CharacterActionId.Water);
        }

        private void OnAnimationEvent(string id)
        {
            if (id == CharacterAnimationEvents.CastRelease) _fishing?.ReleaseCast();
            else if (id == CharacterAnimationEvents.WaterApply) _farm?.ApplyPendingWater();
        }

        // 물 주기 중에만: Water 상태를 벗어나면(끝나 Idle로 넘어가기 시작) 물 주기를 끝낸다.
        private void Update()
        {
            if (_farm == null || !_farm.IsWatering) return;
            _wateringTime += Time.deltaTime;
            bool leaving = _animator.IsInTransition(0) && _animator.GetNextAnimatorStateInfo(0).shortNameHash != WaterHash;
            bool started = _wateringTime > _config.EnterBlendSeconds + 0.05f;
            if ((started && (!IsIn(CharacterActionId.Water) || leaving)) || _wateringTime > _config.WateringTimeoutSeconds)
                _farm.FinishWatering();
        }

        private void Play(CharacterActionId action)
        {
            _animator.SetFloat(ActionSpeedHash, _config.SpeedFor(action));
            _animator.CrossFadeInFixedTime(action.ToString(), _config.EnterBlendSeconds, 0);
        }

        private void ReturnToLocomotion()
        {
            var state = _mover != null && _mover.Locomotion != null ? _mover.Locomotion.CurrentId : CharacterStateId.Idle;
            int hash = Animator.StringToHash(state.ToString());
            // 이미 그 이동 상태에 있으면(예: 낚아채기가 먼저 끝나 Idle) 다시 섞지 않는다.
            if (!_animator.IsInTransition(0) && _animator.GetCurrentAnimatorStateInfo(0).shortNameHash == hash) return;
            _animator.CrossFadeInFixedTime(hash, _config.ExitBlendSeconds, 0);
        }

        private bool IsIn(CharacterActionId action)
        {
            int hash = Animator.StringToHash(action.ToString());
            return _animator.GetCurrentAnimatorStateInfo(0).shortNameHash == hash ||
                   (_animator.IsInTransition(0) && _animator.GetNextAnimatorStateInfo(0).shortNameHash == hash);
        }
    }
}
