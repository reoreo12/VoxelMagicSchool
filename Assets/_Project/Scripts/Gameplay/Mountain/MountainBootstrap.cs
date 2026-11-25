// 뒷산 전용 부트스트랩. GameBootstrap이 만든 GameContext를 받아 시간대(게임 시계를 따라감)·채집(오늘의 채집 자리에 씬 슬롯을 이음)·포획 서비스를 만들고,
// 아래의 IMountainContextBinder에 연결한다.
using System.Collections.Generic;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    [DefaultExecutionOrder(-900)]
    public sealed class MountainBootstrap : MonoBehaviour
    {
        [SerializeField] private GameBootstrap _game;
        [Tooltip("게임 시계가 없을 때의 시간대 (시험용)")]
        [SerializeField] private TimeOfDay _startTime = TimeOfDay.Day;

        [Header("포획")]
        [SerializeField] private CaptureConfig _capture;
        [Tooltip("풀숲에 올 수 있는 동물 전부")]
        [SerializeField] private List<AnimalDefinition> _animals = new List<AnimalDefinition>();

        [Header("연결")]
        [Tooltip("이 아래의 서식지·풀숲·마법진·IMountainContextBinder를 찾는다")]
        [SerializeField] private List<GameObject> _bindRoots = new List<GameObject>();

        public MountainContext Context { get; private set; }

        private void Awake()
        {
            var game = _game.Context;
            var gathering = BuildGathering(game, out var time);
            var messages = new MountainMessages();
            var capture = BuildCapture(game, messages);
            Context = new MountainContext(game, time, gathering, capture, messages, new DebugCommandList());

            foreach (var binder in Find<IMountainContextBinder>())
                binder.Bind(Context);
            if (game.Clock != null) game.Clock.PeriodChanged += OnPeriodChanged;
        }

        private void OnDestroy()
        {
            Context?.Gathering.Dispose();
            if (Context?.Game.Clock != null) Context.Game.Clock.PeriodChanged -= OnPeriodChanged;
        }

        private void OnPeriodChanged(TimeOfDay period) => Context.Time.Set(period);

        private GatheringService BuildGathering(GameContext game, out TimeOfDayState time)
        {
            // 꺼 둔 서식지(교체 전 기본 도형 등)에는 재료를 놓지 않는다.
            var habitats = Find<GatherHabitat>(includeInactive: false);
            var kinds = new List<GatherHabitatKind>();
            var slotCounts = new List<int>();
            for (int i = 0; i < habitats.Count; i++)
            {
                habitats[i].AssignIndex(i);
                kinds.Add(habitats[i].Kind);
                slotCounts.Add(habitats[i].SlotCount);
            }
            time = new TimeOfDayState(game.Clock != null ? game.Clock.Period : _startTime);
            var day = game.GatherDay ?? new GatherDayState(1); // 진행 상태에 세계가 없으면(시험용) 빈 자리
            var service = new GatheringService(day, kinds, slotCounts, game.Inventory, game.Messages, game.Random);
            foreach (var pair in service.SceneSpotCounts)
                if (pair.Value < day.SpotsPerKind)
                    Debug.LogWarning($"[{name}] {pair.Key} 서식지 슬롯이 {pair.Value}개라 채집 자리 {day.SpotsPerKind}곳 중 일부가 씬에 없습니다 (VoxelMagic/뒷산/채집 밸런스 → 슬롯 수 적용).", this);
            return service;
        }

        private CaptureService BuildCapture(GameContext game, MountainMessages messages)
        {
            var tufts = Find<GrassTuft>(includeInactive: false);
            for (int i = 0; i < tufts.Count; i++) tufts[i].Setup(i, _capture);
            var capture = new CaptureService(tufts.Count, _animals, _capture, game.Inventory, game.Messages, messages, game.Random);

            var drawers = Find<IMagicCircleDrawer>();
            if (drawers.Count > 0) capture.SetDrawer(drawers[0]);
            else if (tufts.Count > 0) Debug.LogWarning($"[{name}] 마법진(IMagicCircleDrawer)이 없어 포획을 시작할 수 없습니다.", this);
            foreach (var zone in Find<CaptureZone>()) zone.Setup(_capture);
            return capture;
        }

        private List<T> Find<T>(bool includeInactive = true)
        {
            var list = new List<T>();
            foreach (var root in _bindRoots)
                if (root != null) list.AddRange(root.GetComponentsInChildren<T>(includeInactive));
            return list;
        }
    }
}
