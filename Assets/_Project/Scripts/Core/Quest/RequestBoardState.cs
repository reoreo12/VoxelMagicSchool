// 크리처 굴 게시판의 오늘 의뢰(QUEST_TIMER 2-2). 하루 시작 때 난이도별로 하나씩 새로 뽑고, 받지 않은 의뢰는 그때 사라진다(만료).
// 반복 방지: 지금 받아 둔·이미 전달한 의뢰와 최근 N일 안에 게시된 의뢰는 빼고, 덜 게시된 의뢰일수록 잘 뽑힌다. 후보가 모자라면 제한을 차례로 풀되 가장 오래전 게시된 것부터.
using System;
using System.Collections.Generic;

namespace VoxelMagic.Core
{
    public sealed class RequestBoardState
    {
        private static readonly RequestDifficulty[] Order = { RequestDifficulty.Easy, RequestDifficulty.Normal, RequestDifficulty.Hard };

        private readonly List<RequestDefinition> _offers = new List<RequestDefinition>();
        private readonly Dictionary<RequestDefinition, int> _lastOfferedDay = new Dictionary<RequestDefinition, int>();
        private readonly Dictionary<RequestDefinition, int> _timesOffered = new Dictionary<RequestDefinition, int>();

        /// <summary>오늘 게시판이 바뀔 때 (새로 뽑기·수락으로 빠짐).</summary>
        public event Action OffersChanged;
        /// <summary>하루 시작 때 받지 않고 사라진 의뢰들.</summary>
        public event Action<IReadOnlyList<RequestDefinition>> Expired;

        public IReadOnlyList<RequestDefinition> Offers => _offers;
        public int Day { get; private set; }

        public int TimesOffered(RequestDefinition request) => request != null && _timesOffered.TryGetValue(request, out int n) ? n : 0;

        /// <summary>하루 시작: 남은 의뢰는 만료, 오늘 의뢰를 새로 뽑는다.</summary>
        /// <param name="isAccepted">지금 받아 둔 의뢰 (절대 게시하지 않음)</param>
        /// <param name="isDelivered">이미 전달한 의뢰 (후보가 모자랄 때만 다시 게시)</param>
        public void Refresh(int day, QuestConfig config, IReadOnlyList<RequestDefinition> pool,
            Func<RequestDefinition, bool> isAccepted, Func<RequestDefinition, bool> isDelivered, IRandom random)
        {
            if (_offers.Count > 0) Expired?.Invoke(_offers.ToArray());
            _offers.Clear();
            Day = day;

            if (config != null && config.IsBoardOpenOn(day))
            {
                if (day == config.TutorialDay)
                {
                    var tutorial = config.TutorialRequest;
                    if (tutorial != null && !Is(isAccepted, tutorial) && !Is(isDelivered, tutorial))
                        _offers.Add(tutorial);
                }
                else if (pool != null)
                {
                    foreach (var difficulty in Order)
                    {
                        if (!config.Appears(difficulty, day)) continue;
                        var pick = Pick(difficulty, day, config, pool, isAccepted, isDelivered, random);
                        if (pick != null) _offers.Add(pick);
                    }
                }
            }

            foreach (var offer in _offers)
            {
                _lastOfferedDay[offer] = day;
                _timesOffered[offer] = TimesOffered(offer) + 1;
            }
            OffersChanged?.Invoke();
        }

        /// <summary>치트: 특정 의뢰를 오늘 게시판에 올린다.</summary>
        public void Add(RequestDefinition request)
        {
            if (request == null || _offers.Contains(request)) return;
            _offers.Add(request);
            OffersChanged?.Invoke();
        }

        /// <summary>수락했거나 포기해 게시판에서 뺀다.</summary>
        public bool Remove(RequestDefinition request)
        {
            if (!_offers.Remove(request)) return false;
            OffersChanged?.Invoke();
            return true;
        }

        private static bool Is(Func<RequestDefinition, bool> predicate, RequestDefinition request) => predicate != null && predicate(request);

        private RequestDefinition Pick(RequestDifficulty difficulty, int day, QuestConfig config, IReadOnlyList<RequestDefinition> pool,
            Func<RequestDefinition, bool> isAccepted, Func<RequestDefinition, bool> isDelivered, IRandom random)
        {
            int cooldown = config.RepeatCooldownDays;
            // 마지막 전달 날까지 끝낼 수 없는 의뢰(다음 날 자라는 작물)는 처음부터 뺀다.
            // 0) 받아 둔·전달한·최근 N일 게시 의뢰 빼기 → 1) 최근 게시 제한 풀기 → 2) 전달한 의뢰도 허용. 받아 둔 의뢰는 끝까지 뺀다.
            for (int relax = 0; relax < 3; relax++)
            {
                var candidates = new List<RequestDefinition>();
                foreach (var request in pool)
                {
                    if (request == null || request.Difficulty != difficulty || _offers.Contains(request)) continue;
                    if (Is(isAccepted, request) || !config.CanFinishInTime(request, day)) continue;
                    if (relax < 2 && Is(isDelivered, request)) continue;
                    if (relax == 0 && _lastOfferedDay.TryGetValue(request, out int last) && day - last <= cooldown) continue;
                    candidates.Add(request);
                }
                if (candidates.Count == 0) continue;
                // 제한을 푼 단계에서는 가장 오래전에 게시된 의뢰만 남겨 어제 것이 바로 다시 나오지 않게 한다.
                if (relax > 0) candidates = LeastRecent(candidates);
                return Weighted(candidates, random);
            }
            return null;
        }

        private List<RequestDefinition> LeastRecent(List<RequestDefinition> candidates)
        {
            int oldest = int.MaxValue;
            foreach (var c in candidates)
                oldest = System.Math.Min(oldest, _lastOfferedDay.TryGetValue(c, out int last) ? last : int.MinValue);
            return candidates.FindAll(c => (_lastOfferedDay.TryGetValue(c, out int last) ? last : int.MinValue) == oldest);
        }

        // 덜 게시된 의뢰일수록 가중치가 크다: 1 / (1 + 게시 횟수).
        private RequestDefinition Weighted(List<RequestDefinition> candidates, IRandom random)
        {
            float total = 0f;
            foreach (var c in candidates) total += 1f / (1 + TimesOffered(c));
            float roll = random.Value * total;
            foreach (var c in candidates)
            {
                roll -= 1f / (1 + TimesOffered(c));
                if (roll < 0f) return c;
            }
            return candidates[candidates.Count - 1];
        }
    }
}
