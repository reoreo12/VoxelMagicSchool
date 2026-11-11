// 뒷산 전용 부트스트랩. GameBootstrap이 만든 GameContext를 받아 시간대·채집·포획 서비스를 만들고, 아래의 IMountainContextBinder에 연결한다.
using System.Collections.Generic;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    [DefaultExecutionOrder(-900)]
    public sealed class MountainBootstrap : MonoBehaviour
    {
        [SerializeField] private GameBootstrap _game;
        [Tooltip("시작 시간대 (가정: 방과후)")]
        [SerializeField] private TimeOfDay _startTime = TimeOfDay.Day;

        [Header("채집")]
        [SerializeField] private GatheringConfig _gathering;
        [Tooltip("뒷산에 나올 수 있는 채집 재료 전부")]
        [SerializeField] private List<GatherMaterialDefinition> _materials = new List<GatherMaterialDefinition>();

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
            gathering.Respawn();
        }

        private void OnDestroy() => Context?.Gathering.Dispose();

        private GatheringService BuildGathering(GameContext game, out TimeOfDayState time)
        {
            var habitats = Find<GatherHabitat>();
            var kinds = new List<GatherHabitatKind>();
            var slotCounts = new List<int>();
            for (int i = 0; i < habitats.Count; i++)
            {
                habitats[i].AssignIndex(i);
                kinds.Add(habitats[i].Kind);
                slotCounts.Add(habitats[i].SlotCount);
            }
            time = new TimeOfDayState(_startTime);
            return new GatheringService(new GatherSpawner(kinds, slotCounts), _materials, _gathering, time,
                game.Inventory, game.Messages, game.Requests, game.Random);
        }

        private CaptureService BuildCapture(GameContext game, MountainMessages messages)
        {
            var tufts = Find<GrassTuft>();
            for (int i = 0; i < tufts.Count; i++) tufts[i].Setup(i, _capture);
            var capture = new CaptureService(tufts.Count, _animals, _capture, game.Inventory, game.Messages, messages, game.Random);

            var drawers = Find<IMagicCircleDrawer>();
            if (drawers.Count > 0) capture.SetDrawer(drawers[0]);
            else if (tufts.Count > 0) Debug.LogWarning($"[{name}] 마법진(IMagicCircleDrawer)이 없어 포획을 시작할 수 없습니다.", this);
            foreach (var zone in Find<CaptureZone>()) zone.Setup(_capture);
            return capture;
        }

        private List<T> Find<T>()
        {
            var list = new List<T>();
            foreach (var root in _bindRoots)
                if (root != null) list.AddRange(root.GetComponentsInChildren<T>(true));
            return list;
        }
    }
}
