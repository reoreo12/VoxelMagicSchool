// 수락한 의뢰 목록과 규칙(QUEST_TIMER 2-3~2-5): 최대 N개 수락·씨앗 지급·다시 받기, 포기, 골라서 전달, 인벤토리가 바뀔 때마다 진행 재계산.
// "완료 가능"은 저장하지 않고 인벤토리로 매번 계산한다(같은 재료를 두 의뢰가 요구하면 먼저 낸 쪽이 가져가고 다른 쪽은 진행 중으로 돌아감).
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelMagic.Core
{
    public sealed class RequestLog : IAcceptedRequestList
    {
        private readonly Inventory _inventory;
        private readonly ScoreBook _score;
        private readonly MaterialJournal _journal;
        private readonly List<AcceptedRequest> _accepted = new List<AcceptedRequest>();
        private readonly HashSet<RequestDefinition> _delivered = new HashSet<RequestDefinition>();
        private readonly HashSet<RequestDefinition> _abandoned = new HashSet<RequestDefinition>();
        private readonly List<AcceptedRequestEntry> _entries = new List<AcceptedRequestEntry>();
        private QuestConfig _config;
        private RequestBoardState _board;
        private IGameTime _time;
        private FarmGrid _farm;
        private int _nextId = 1;
        private bool _silent;

        public event Action AcceptedListChanged;
        public event Action<AcceptedRequestEntry> RequestBecameComplete;
        /// <summary>(의뢰, 이전 상태). 진행 중 ↔ 완료 가능, 전달됨, 포기.</summary>
        public event Action<AcceptedRequest, RequestState> RequestStateChanged;
        public event Action<AcceptedRequest> Accepted;
        public event Action<DeliveryReport> Delivered;
        public event Action<AcceptedRequest> Abandoned;

        public RequestLog(Inventory inventory, ScoreBook score, MaterialJournal journal, QuestConfig config = null)
        {
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            _score = score;
            _journal = journal;
            _config = config != null ? config : ScriptableObject.CreateInstance<QuestConfig>();
            _inventory.SlotChanged += _ => Refresh();
        }

        /// <summary>게시판·시계·밭을 연결한다 (진행 상태의 세계가 생길 때). 시계가 없으면 날짜·시간 제한을 보지 않는다.</summary>
        public void Connect(QuestConfig config, RequestBoardState board, IGameTime time, FarmGrid farm)
        {
            if (config != null) _config = config;
            _board = board;
            _time = time;
            _farm = farm;
        }

        public QuestConfig Config => _config;
        public int MaxAccepted => _config.MaxAccepted;
        public int AcceptedCount => _accepted.Count;
        public bool IsFull => _accepted.Count >= MaxAccepted;
        /// <summary>받아 둔 의뢰 (수락 순서).</summary>
        public IReadOnlyList<AcceptedRequest> Active => _accepted;
        IReadOnlyList<AcceptedRequestEntry> IAcceptedRequestList.Accepted => _entries;
        public IReadOnlyList<AcceptedRequestEntry> Entries => _entries;
        public IEnumerable<RequestDefinition> DeliveredRequests => _delivered;
        public IEnumerable<RequestDefinition> AbandonedRequests => _abandoned;
        public int NextId => _nextId;

        public bool IsAccepted(RequestDefinition request) => FindByRequest(request) != null;
        public bool IsDelivered(RequestDefinition request) => request != null && _delivered.Contains(request);
        public bool IsAbandoned(RequestDefinition request) => request != null && _abandoned.Contains(request);

        public AcceptedRequest Find(int id)
        {
            foreach (var a in _accepted) if (a.Id == id) return a;
            return null;
        }

        public RequestState GetState(RequestDefinition request)
        {
            var accepted = FindByRequest(request);
            if (accepted != null) return accepted.State;
            if (IsDelivered(request)) return RequestState.Delivered;
            if (IsAbandoned(request)) return RequestState.Abandoned;
            if (_board != null && Contains(_board.Offers, request)) return RequestState.Offered;
            return RequestState.None;
        }

        public int ExpectedReward(AcceptedRequestEntry entry) => entry != null ? WrittenScoreRule.Score(entry.Request, _config) : 0;
        public int RewardOf(RequestDefinition request) => WrittenScoreRule.Score(request, _config);

        // ── 게시판·전달을 지금 쓸 수 있는지 ──

        public QuestAccess BoardAccess()
        {
            if (_time == null) return QuestAccess.Open;
            if (!_config.IsBoardOpenOn(_time.Day)) return QuestAccess.Closed;
            return IsNightOk() ? QuestAccess.Open : QuestAccess.NotNight;
        }

        public QuestAccess DeliveryAccess()
        {
            if (_time == null) return QuestAccess.Open;
            if (_time.Day > _config.LastDeliveryDay) return QuestAccess.Closed;
            return IsNightOk() ? QuestAccess.Open : QuestAccess.NotNight;
        }

        private bool IsNightOk() => !_config.OnlyAtNight || _time == null || _time.Period == TimeOfDay.Night;

        // ── 수락 ──

        public AcceptResult CanAccept(RequestDefinition request)
        {
            if (request == null) return AcceptResult.NotOnBoard;
            if (IsAccepted(request)) return AcceptResult.AlreadyAccepted;
            switch (BoardAccess())
            {
                case QuestAccess.Closed: return AcceptResult.BoardClosed;
                case QuestAccess.NotNight: return AcceptResult.NotNight;
            }
            if (_board != null && !Contains(_board.Offers, request)) return AcceptResult.NotOnBoard;
            if (IsFull) return AcceptResult.SlotsFull;
            if (!_inventory.CanAddAll(request.GivenItems)) return AcceptResult.InventoryFull;
            return AcceptResult.Success;
        }

        /// <summary>수락: 칸을 하나 쓰고, 지급 씨앗을 넣고, 게시판에서 뺀다.</summary>
        public AcceptResult Accept(RequestDefinition request)
        {
            var result = CanAccept(request);
            if (result != AcceptResult.Success) return result;

            var accepted = new AcceptedRequest(_nextId++, request);
            _accepted.Add(accepted);
            _board?.Remove(request);
            _silent = true; // 씨앗을 넣는 동안 목록을 여러 번 다시 만들지 않는다
            foreach (var given in request.GivenItems)
                _inventory.TryAdd(given.Item, given.Amount);
            _silent = false;
            Accepted?.Invoke(accepted);
            Refresh();
            return AcceptResult.Success;
        }

        // ── 포기 ──

        /// <summary>포기: 칸이 비고 받은 씨앗은 그대로. 포기한 의뢰는 게시판에 다시 잘 안 올라온다. 확인 창은 부르는 쪽 책임.</summary>
        public bool Abandon(int id)
        {
            var accepted = Find(id);
            if (accepted == null) return false;
            var before = accepted.State;
            _accepted.Remove(accepted);
            _abandoned.Add(accepted.Request);
            accepted.State = RequestState.Abandoned;
            RequestStateChanged?.Invoke(accepted, before);
            Abandoned?.Invoke(accepted);
            Refresh();
            return true;
        }

        /// <summary>치트: 받아 둔 의뢰를 기록 없이 모두 비운다.</summary>
        public void ClearAccepted()
        {
            if (_accepted.Count == 0) return;
            _accepted.Clear();
            Refresh();
        }

        // ── 진행·전달 ──

        public IReadOnlyList<RequirementProgress> ProgressOf(AcceptedRequest accepted)
        {
            var list = new List<RequirementProgress>();
            if (accepted?.Request == null) return list;
            foreach (var need in accepted.Request.Requirements)
                if (need.Item != null) list.Add(new RequirementProgress(need.Item, _inventory.Count(need.Item), need.Amount));
            return list;
        }

        /// <summary>지금 완료 가능한 의뢰 (수락 순서, 부를 때마다 인벤토리로 다시 계산).</summary>
        public List<AcceptedRequest> ReadyRequests()
        {
            var list = new List<AcceptedRequest>();
            foreach (var a in _accepted) if (HasAll(a.Request)) list.Add(a);
            return list;
        }

        public DeliverResult CanDeliver(int id)
        {
            var accepted = Find(id);
            if (accepted == null) return DeliverResult.NotAccepted;
            switch (DeliveryAccess())
            {
                case QuestAccess.Closed: return DeliverResult.TooLate;
                case QuestAccess.NotNight: return DeliverResult.NotNight;
            }
            return HasAll(accepted.Request) ? DeliverResult.Success : DeliverResult.MissingItems;
        }

        /// <summary>전달: 그 순간 다시 확인 → 재료를 빼고 필기 점수(희귀도 합)를 올리고 칸을 비운다.</summary>
        public DeliverResult Deliver(int id, out DeliveryReport report)
        {
            report = null;
            var result = CanDeliver(id);
            if (result != DeliverResult.Success) return result;

            var accepted = Find(id);
            var request = accepted.Request;
            var before = accepted.State;
            _accepted.Remove(accepted); // 재료를 빼는 동안 이 의뢰를 다시 계산하지 않게 먼저 뺀다
            _delivered.Add(request);

            _silent = true;
            foreach (var need in request.Requirements)
                _inventory.TryRemove(need.Item, need.Amount);
            _silent = false;

            var newKinds = new List<ItemDefinition>();
            foreach (var need in request.Requirements)
                if (_journal != null && _journal.Record(need.Item)) newKinds.Add(need.Item);
            int gained = _score != null ? _score.AddWritten(WrittenScoreRule.Score(request, _config)) : 0;

            accepted.State = RequestState.Delivered;
            report = new DeliveryReport(request, gained, newKinds);
            RequestStateChanged?.Invoke(accepted, before);
            Delivered?.Invoke(report);
            Refresh();
            return DeliverResult.Success;
        }

        /// <summary>부족한 재료만 "검은 무 1/2, 붕어 0/1" 형태로.</summary>
        public string DescribeMissing(AcceptedRequest accepted)
        {
            var parts = new List<string>();
            foreach (var p in ProgressOf(accepted)) if (!p.IsMet) parts.Add(p.ToString());
            return string.Join(", ", parts);
        }

        /// <summary>가장 먼저 받은 의뢰의 부족한 재료 (크리처 안내 한 줄). 받은 의뢰가 없으면 빈 문자열.</summary>
        public string DescribeMissingFirst() => _accepted.Count > 0 ? DescribeMissing(_accepted[0]) : string.Empty;

        // ── 씨앗 다시 받기 ──

        public List<ItemAmount> RefillableSeeds() => SeedRefillRule.Compute(AcceptedDefinitions(), _inventory, _farm);

        /// <summary>더 줄 수 있는 씨앗을 넣는다 (가방에 들어가는 만큼). 넣은 씨앗 수.</summary>
        public int GiveRefillSeeds()
        {
            int given = 0;
            foreach (var seed in RefillableSeeds())
            {
                for (int n = seed.Amount; n > 0; n--)
                {
                    if (!_inventory.TryAdd(seed.Item, n)) continue;
                    given += n;
                    break;
                }
            }
            return given;
        }

        // ── 저장 ──

        /// <summary>저장 파일에서 불러온 목록으로 맞춘다. 씨앗은 다시 주지 않고, 완료 알림도 다시 띄우지 않는다.</summary>
        internal void Restore(IEnumerable<RequestDefinition> accepted, IEnumerable<RequestDefinition> delivered,
            IEnumerable<RequestDefinition> abandoned, int nextId)
        {
            _accepted.Clear();
            _delivered.Clear();
            _abandoned.Clear();
            if (delivered != null) foreach (var r in delivered) if (r != null) _delivered.Add(r);
            if (abandoned != null) foreach (var r in abandoned) if (r != null) _abandoned.Add(r);
            _nextId = 1;
            if (accepted != null)
                foreach (var r in accepted)
                    if (r != null && !IsAccepted(r)) _accepted.Add(new AcceptedRequest(_nextId++, r));
            _nextId = Math.Max(_nextId, nextId);
            foreach (var a in _accepted) a.State = HasAll(a.Request) ? RequestState.Ready : RequestState.InProgress;
            Refresh();
        }

        // ── 내부 ──

        private void Refresh()
        {
            if (_silent) return;
            _entries.Clear();
            var becameReady = new List<AcceptedRequestEntry>();
            foreach (var a in _accepted)
            {
                var entry = new AcceptedRequestEntry(a.Request, a.Id, ProgressOf(a));
                _entries.Add(entry);
                var next = entry.IsComplete ? RequestState.Ready : RequestState.InProgress;
                if (next == a.State) continue;
                var before = a.State;
                a.State = next;
                RequestStateChanged?.Invoke(a, before);
                if (next == RequestState.Ready) becameReady.Add(entry);
            }
            AcceptedListChanged?.Invoke();
            foreach (var entry in becameReady) RequestBecameComplete?.Invoke(entry);
        }

        private bool HasAll(RequestDefinition request)
        {
            if (request == null) return false;
            foreach (var need in request.Requirements)
                if (need.Item != null && _inventory.Count(need.Item) < need.Amount) return false;
            return true;
        }

        private AcceptedRequest FindByRequest(RequestDefinition request)
        {
            if (request == null) return null;
            foreach (var a in _accepted) if (a.Request == request) return a;
            return null;
        }

        private IEnumerable<RequestDefinition> AcceptedDefinitions()
        {
            foreach (var a in _accepted) yield return a.Request;
        }

        private static bool Contains(IReadOnlyList<RequestDefinition> list, RequestDefinition request)
        {
            if (list == null) return false;
            for (int i = 0; i < list.Count; i++) if (list[i] == request) return true;
            return false;
        }
    }
}
