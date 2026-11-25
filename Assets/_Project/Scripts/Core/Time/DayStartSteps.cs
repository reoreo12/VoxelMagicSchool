// 하루 시작 3단계(QUEST_TIMER 1-4): 1. 작물이 자란다(다음 날 자라는 희귀 작물, 밤사이 시간이 다 된 작물) → 2. 채집 자리를 새로 뽑는다 → 3. 게시판이 새 의뢰로 바뀐다(받지 않은 의뢰는 사라짐).
// 모두 씬과 무관한 진행 상태 데이터에 적용하므로 플레이어가 어느 씬에 있든 같은 결과가 된다.
using System;
using System.Collections.Generic;

namespace VoxelMagic.Core
{
    public sealed class CropGrowthStep : IDayStartStep
    {
        private readonly FarmGrid _farm;

        public CropGrowthStep(FarmGrid farm) => _farm = farm;

        public int Order => 1;

        public void Run(int day, DayKind kind, DayStartReason reason)
        {
            if (reason != DayStartReason.NewGame) _farm.AdvanceDay();
        }
    }

    public sealed class GatherRespawnStep : IDayStartStep
    {
        private readonly GatherDayState _state;
        private readonly GatheringConfig _config;
        private readonly IRandom _random;

        public GatherRespawnStep(GatherDayState state, GatheringConfig config, IRandom random)
        {
            _state = state;
            _config = config;
            _random = random;
        }

        public int Order => 2;

        /// <summary>반드시 나와야 하는 재료 (튜토리얼 결과 보장). 없으면 null.</summary>
        public Func<IEnumerable<GatherMaterialDefinition>> Guaranteed { get; set; }

        public void Run(int day, DayKind kind, DayStartReason reason) => Reroll(day);

        /// <summary>그날 자리를 다시 뽑는다 (하루 시작·치트).</summary>
        public void Reroll(int day) => _state.Set(day, GatherDayRoller.Roll(_config, _random, Guaranteed?.Invoke()));
    }

    public sealed class BoardRefreshStep : IDayStartStep
    {
        private readonly RequestBoardState _board;
        private readonly QuestConfig _config;
        private readonly RequestPool _pool;
        private readonly IRandom _random;

        public BoardRefreshStep(RequestBoardState board, QuestConfig config, RequestPool pool, IRandom random)
        {
            _board = board;
            _config = config;
            _pool = pool;
            _random = random;
        }

        public int Order => 3;

        /// <summary>지금 받아 둔 의뢰인지 (의뢰 규칙 쪽이 넣는다).</summary>
        public Func<RequestDefinition, bool> IsAccepted { get; set; }
        /// <summary>이미 전달한 의뢰인지.</summary>
        public Func<RequestDefinition, bool> IsDelivered { get; set; }

        public void Run(int day, DayKind kind, DayStartReason reason) => Refresh(day);

        /// <summary>그날 게시판을 다시 뽑는다 (하루 시작·치트).</summary>
        public void Refresh(int day) => _board.Refresh(day, _config, _pool != null ? _pool.Requests : null, IsAccepted, IsDelivered, _random);
    }
}
