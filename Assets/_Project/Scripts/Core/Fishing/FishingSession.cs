// 낚시 상태 머신: 대기 → 던지기 → 기다림 → 입질 → 미니게임 → 결과.
// MonoBehaviour가 매 프레임 Tick을 불러 주고, 상태 변화는 이벤트로 알린다.
using System;

namespace VoxelMagic.Core
{
    public enum FishingState
    {
        Idle,
        Casting,
        Waiting,
        Biting,
        Minigame,
        Result
    }

    public enum FishingOutcome
    {
        None,
        Caught,
        Escaped,
        ReleasedInventoryFull
    }

    public sealed class FishingSession
    {
        private readonly FishingConfig _config;
        private readonly Inventory _inventory;
        private readonly IRandom _random;
        private readonly GameMessages _messages;
        private readonly float _resultHoldSeconds;
        private float _timer;

        public event Action<FishingState> StateChanged;
        /// <summary>(물고기, 크기 cm)</summary>
        public event Action<FishDefinition, float> Caught;

        public FishingSession(FishingConfig config, Inventory inventory, IRandom random, GameMessages messages,
            float resultHoldSeconds)
        {
            _config = config;
            _inventory = inventory;
            _random = random;
            _messages = messages;
            _resultHoldSeconds = resultHoldSeconds;
        }

        public FishingState State { get; private set; }
        public bool IsBusy => State != FishingState.Idle;
        public FishDefinition CurrentFish { get; private set; }
        /// <summary>미니게임 중일 때만 값이 있다.</summary>
        public FishingMinigame Minigame { get; private set; }
        public FishingOutcome LastOutcome { get; private set; }

        public bool CanCast => State == FishingState.Idle || State == FishingState.Result;

        public bool TryCast()
        {
            if (!CanCast) return false;
            _messages.DismissAcquired();
            CurrentFish = null;
            Minigame = null;
            LastOutcome = FishingOutcome.None;
            Enter(FishingState.Casting, _config.CastSeconds);
            return true;
        }

        /// <summary>낚시 중 E 입력: 입질이면 미니게임 시작, 입질 오기 전이면 거두기, 결과 표시 중이면 다시 던지기.</summary>
        public void PressInteract()
        {
            if (State == FishingState.Biting)
            {
                Minigame = new FishingMinigame(_config, CurrentFish, _random);
                Enter(FishingState.Minigame, 0f);
            }
            else if(State == FishingState.Casting || State == FishingState.Waiting)
            {
                Cancel();
            }
            else if (State == FishingState.Result)
            {
                TryCast();
            }
        }

        /// <summary>스크롤 입력 (한 칸 = ±1).</summary>
        public void Scroll(float notches)
        {
            if (State == FishingState.Minigame) Minigame.MoveBar(notches * _config.ScrollSensitivity);
        }

        public void Cancel()
        {
            if (State == FishingState.Idle) return;
            CurrentFish = null;
            Minigame = null;
            Enter(FishingState.Idle, 0f);
        }

        public void Tick(float deltaTime)
        {
            switch (State)
            {
                case FishingState.Casting:
                    if (CountDown(deltaTime)) EnterWaiting();
                    break;

                case FishingState.Waiting:
                    if (CountDown(deltaTime)) EnterBiting();
                    break;

                case FishingState.Biting:
                    if (!CountDown(deltaTime)) break;
                    _messages.ShowToast(GameTexts.BiteMissed);
                    if (_config.RewaitOnMissedBite) EnterWaiting();
                    else Cancel();
                    break;

                case FishingState.Minigame:
                    var status = Minigame.Tick(deltaTime);
                    if (status == MinigameStatus.Succeeded) FinishCatch();
                    else if (status == MinigameStatus.Failed) FinishEscape();
                    break;

                case FishingState.Result:
                    if (CountDown(deltaTime)) Enter(FishingState.Idle, 0f);
                    break;
            }
        }

        private void EnterWaiting()
        {
            CurrentFish = null;
            Enter(FishingState.Waiting, _config.BiteWaitSeconds.Roll(_random));
        }

        private void EnterBiting()
        {
            CurrentFish = _config.PickFish(_random);
            if (CurrentFish == null)
            {
                Cancel();
                return;
            }
            Enter(FishingState.Biting, _config.BiteWindowSeconds);
        }

        private void FinishCatch()
        {
            var fish = CurrentFish;
            float size = fish.SizeRangeCm.Roll(_random);
            if (_inventory.TryAdd(fish, 1))
            {
                LastOutcome = FishingOutcome.Caught;
                _messages.ShowAcquired(new AcquiredInfo(fish, size));
                Caught?.Invoke(fish, size);
            }
            else
            {
                LastOutcome = FishingOutcome.ReleasedInventoryFull;
                _messages.ShowToast(GameTexts.FishReleasedInventoryFull);
            }
            Minigame = null;
            Enter(FishingState.Result, _resultHoldSeconds);
        }

        private void FinishEscape()
        {
            LastOutcome = FishingOutcome.Escaped;
            _messages.ShowToast(GameTexts.FishEscaped);
            Minigame = null;
            Enter(FishingState.Result, _resultHoldSeconds);
        }

        private bool CountDown(float deltaTime)
        {
            _timer -= deltaTime;
            return _timer <= 0f;
        }

        private void Enter(FishingState state, float duration)
        {
            _timer = duration;
            State = state;
            StateChanged?.Invoke(state);
        }
    }
}
