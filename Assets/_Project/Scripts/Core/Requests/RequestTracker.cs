// 의뢰 수락(한 번에 하나), 인벤토리 기준 진행 계산, 전달과 필기 점수 지급을 맡는다.
using System;
using System.Collections.Generic;
using System.Linq;

namespace VoxelMagic.Core
{
    public sealed class RequestTracker
    {
        private readonly List<RequestDefinition> _catalog;
        private readonly HashSet<RequestDefinition> _completed = new HashSet<RequestDefinition>();
        private readonly List<RequirementProgress> _progress = new List<RequirementProgress>();
        private readonly Inventory _inventory;
        private readonly ScoreBook _scoreBook;
        private readonly MaterialJournal _journal;
        private readonly int _newItemKindBonus;

        public event Action<RequestDefinition> ActiveChanged;
        public event Action ProgressChanged;
        public event Action<DeliveryReport> Completed;

        public RequestTracker(IEnumerable<RequestDefinition> catalog, Inventory inventory, ScoreBook scoreBook,
            MaterialJournal journal, int newItemKindBonus)
        {
            _catalog = catalog.Where(r => r != null).ToList();
            _inventory = inventory;
            _scoreBook = scoreBook;
            _journal = journal;
            _newItemKindBonus = newItemKindBonus;
            _inventory.SlotChanged += _ => RefreshProgress();
        }

        public IReadOnlyList<RequestDefinition> Catalog => _catalog;
        public RequestDefinition Active { get; private set; }
        public bool HasActive => Active != null;
        public IReadOnlyList<RequirementProgress> Progress => _progress;
        public bool IsReadyToDeliver => HasActive && _progress.All(p => p.IsMet);

        public RequestStatus GetStatus(RequestDefinition request)
        {
            if (_completed.Contains(request)) return RequestStatus.Completed;
            return request == Active ? RequestStatus.Active : RequestStatus.Available;
        }

        public AcceptResult CanAccept(RequestDefinition request)
        {
            if (_completed.Contains(request)) return AcceptResult.AlreadyCompleted;
            if (HasActive) return AcceptResult.AlreadyActive;
            if (!_inventory.CanAddAll(request.GivenItems)) return AcceptResult.InventoryFull;
            return AcceptResult.Success;
        }

        public AcceptResult Accept(RequestDefinition request)
        {
            var result = CanAccept(request);
            if (result != AcceptResult.Success) return result;

            Active = request;
            foreach (var given in request.GivenItems)
                _inventory.TryAdd(given.Item, given.Amount);

            ActiveChanged?.Invoke(Active);
            RefreshProgress();
            return AcceptResult.Success;
        }

        public DeliverResult CanDeliver()
        {
            if (!HasActive) return DeliverResult.NoActiveRequest;
            return IsReadyToDeliver ? DeliverResult.Success : DeliverResult.MissingItems;
        }

        /// <summary>부족한 항목만 "대파 1/2, 붕어 0/1" 형태로.</summary>
        public string DescribeMissing()
            => string.Join(", ", _progress.Where(p => !p.IsMet).Select(p => p.ToString()));

        public DeliverResult Deliver(out DeliveryReport report)
        {
            report = null;
            var result = CanDeliver();
            if (result != DeliverResult.Success) return result;

            var request = Active;
            foreach (var need in request.Requirements)
                _inventory.TryRemove(need.Item, need.Amount);

            var newKinds = request.Requirements
                .Select(r => r.Item)
                .Distinct()
                .Where(item => _journal.Record(item))
                .ToList();
            int reward = ScoreCalculator.RequestReward(request.BaseReward, newKinds.Count, _newItemKindBonus);
            int gained = _scoreBook.AddWritten(reward);

            _completed.Add(request);
            Active = null;
            _progress.Clear();

            report = new DeliveryReport(request, gained, newKinds);
            Completed?.Invoke(report);
            ActiveChanged?.Invoke(null);
            ProgressChanged?.Invoke();
            return DeliverResult.Success;
        }

        private void RefreshProgress()
        {
            if (!HasActive) return;
            _progress.Clear();
            foreach (var need in Active.Requirements)
                _progress.Add(new RequirementProgress(need.Item, _inventory.Count(need.Item), need.Amount));
            ProgressChanged?.Invoke();
        }
    }
}
