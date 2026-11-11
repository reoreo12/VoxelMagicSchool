// 뒷산 포획 규칙: 구역 안에서 간격마다 흔들리지 않는 풀숲 1~2개에 동물이 와서 흔들리고(입질), 시간이 지나면 사라진다.
// 근처 길을 지날 때는 맨 앞 풀숲 하나가 동물 없이 짧게 흔들린다. 입질 풀숲에서 마법진을 그려 정확도가 동물 요구치 이상이면 잡아 가방에 넣는다.
using System;
using System.Collections.Generic;

namespace VoxelMagic.Core
{
    public enum TuftShake
    {
        None,
        Pass, // 지나갈 때 (동물 없음, 상호작용 안 됨)
        Bite  // 동물이 와 있음
    }

    public enum ForcedCaptureResult
    {
        None,
        Success,
        Fail
    }

    public sealed class CaptureService
    {
        private struct Tuft
        {
            public TuftShake Shake;
            public float Remaining;
            public AnimalDefinition Animal;
        }

        private readonly Tuft[] _tufts;
        private readonly IReadOnlyList<AnimalDefinition> _animals;
        private readonly CaptureConfig _config;
        private readonly Inventory _inventory;
        private readonly GameMessages _messages;
        private readonly MountainMessages _mountainMessages;
        private readonly IRandom _random;
        private readonly List<int> _candidates = new List<int>();
        private IMagicCircleDrawer _drawer;
        private float _timer;

        public CaptureService(int tuftCount, IReadOnlyList<AnimalDefinition> animals, CaptureConfig config,
            Inventory inventory, GameMessages messages, MountainMessages mountainMessages, IRandom random)
        {
            _tufts = new Tuft[tuftCount];
            _animals = animals;
            _config = config;
            _inventory = inventory;
            _messages = messages;
            _mountainMessages = mountainMessages;
            _random = random;
        }

        /// <summary>풀숲 하나의 흔들림 상태가 바뀌었을 때 (풀숲 번호).</summary>
        public event Action<int> TuftChanged;

        public int TuftCount => _tufts.Length;
        public bool PlayerInZone { get; private set; }
        public int CapturingTuft { get; private set; } = -1;
        public bool IsCapturing => CapturingTuft >= 0;
        public ForcedCaptureResult Forced { get; private set; }
        /// <summary>다음 입질까지 남은 시간 (구역 밖이면 간격 전체).</summary>
        public float NextBiteIn => _config.BiteIntervalSeconds - _timer;

        public int ShakingCount
        {
            get
            {
                int count = 0;
                foreach (var tuft in _tufts) if (tuft.Shake != TuftShake.None) count++;
                return count;
            }
        }

        public TuftShake ShakeOf(int tuft) => _tufts[tuft].Shake;
        public AnimalDefinition AnimalAt(int tuft) => _tufts[tuft].Shake == TuftShake.Bite ? _tufts[tuft].Animal : null;

        public void SetDrawer(IMagicCircleDrawer drawer) => _drawer = drawer;

        /// <summary>디버그: 다음 마법진 한 번의 결과를 강제한다.</summary>
        public void ForceNext(ForcedCaptureResult result) => Forced = result;

        public void Tick(float deltaTime, bool playerInZone)
        {
            PlayerInZone = playerInZone;
            // 이미 흔들리는 풀숲은 구역 밖에서도 끝까지 흔들리다 멈춘다. 포획 중인 풀숲은 멈춰 둔다.
            for (int i = 0; i < _tufts.Length; i++)
            {
                if (_tufts[i].Shake == TuftShake.None || i == CapturingTuft) continue;
                _tufts[i].Remaining -= deltaTime;
                if (_tufts[i].Remaining <= 0f) Stop(i);
            }

            if (!playerInZone || IsCapturing)
            {
                if (!playerInZone) _timer = 0f;
                return;
            }
            _timer += deltaTime;
            if (_timer >= _config.BiteIntervalSeconds) SpawnBites();
        }

        /// <summary>플레이어가 구역 근처 길을 지나갈 때 맨 앞 풀숲을 짧게 한 번 흔든다 (동물 없음). 이미 흔들리는 풀숲은 그대로.</summary>
        public void PassBy(int tuft)
        {
            if (_tufts[tuft].Shake != TuftShake.None) return;
            _tufts[tuft] = new Tuft { Shake = TuftShake.Pass, Remaining = _config.PassShakeSeconds };
            TuftChanged?.Invoke(tuft);
        }

        /// <summary>흔들리지 않는 풀숲 중 무작위 1~2개에 동물을 보낸다. 보낸 수를 돌려준다 (디버그 '입질 즉시'도 이것).</summary>
        public int SpawnBites()
        {
            _timer = 0f;
            if (_animals == null || _animals.Count == 0) return 0;
            _candidates.Clear();
            for (int i = 0; i < _tufts.Length; i++)
                if (_tufts[i].Shake == TuftShake.None) _candidates.Add(i);

            int count = Math.Min(_candidates.Count, _random.Range(_config.MinBites, _config.MaxBites + 1));
            for (int n = 0; n < count; n++)
            {
                int pick = _random.Range(0, _candidates.Count);
                int tuft = _candidates[pick];
                _candidates.RemoveAt(pick);
                _tufts[tuft] = new Tuft
                {
                    Shake = TuftShake.Bite,
                    Remaining = _config.BiteShakeSeconds,
                    Animal = _animals[_random.Range(0, _animals.Count)]
                };
                TuftChanged?.Invoke(tuft);
            }
            return count;
        }

        public bool CanCapture(int tuft) => !IsCapturing && _tufts[tuft].Shake == TuftShake.Bite;
        public bool HasRoomFor(int tuft) => AnimalAt(tuft) is { } animal && _inventory.CanAdd(animal, 1);

        /// <summary>입질 풀숲에서 마법진을 시작한다. 그리는 동안 그 풀숲은 멈춰 기다린다.</summary>
        public bool BeginCapture(int tuft)
        {
            if (!CanCapture(tuft) || _drawer == null) return false;
            if (!HasRoomFor(tuft))
            {
                _messages.ShowToast(GameTexts.InventoryFull);
                return false;
            }
            CapturingTuft = tuft;
            TuftChanged?.Invoke(tuft);
            _drawer.Begin(new MagicCircleRequest(_tufts[tuft].Animal), OnDrawn);
            return true;
        }

        private void OnDrawn(float accuracy)
        {
            int tuft = CapturingTuft;
            if (tuft < 0) return;
            var animal = _tufts[tuft].Animal;
            if (Forced != ForcedCaptureResult.None)
            {
                accuracy = Forced == ForcedCaptureResult.Success ? 1f : 0f;
                Forced = ForcedCaptureResult.None;
            }
            CapturingTuft = -1;

            if (accuracy >= animal.RequiredAccuracy && _inventory.TryAdd(animal, 1))
            {
                _mountainMessages.ShowBanner(string.Format(MountainTexts.CapturedFormat, animal.DisplayName));
                _messages.ShowAcquired(new AcquiredInfo(animal, animal.SizeRangeCm.Roll(_random)));
            }
            else
            {
                _mountainMessages.ShowBanner(MountainTexts.Missed);
            }
            Stop(tuft);
        }

        private void Stop(int tuft)
        {
            _tufts[tuft] = default;
            TuftChanged?.Invoke(tuft);
        }
    }

    /// <summary>뒷산 전용 화면 알림 통로 (화면 정중앙 큰 문구).</summary>
    public sealed class MountainMessages
    {
        public event Action<string> BannerRequested;
        public void ShowBanner(string text) => BannerRequested?.Invoke(text);
    }
}
