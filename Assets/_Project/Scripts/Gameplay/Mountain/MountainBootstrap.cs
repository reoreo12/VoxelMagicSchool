// 뒷산 전용 부트스트랩. GameBootstrap이 만든 GameContext를 받아 시간대·채집 서비스를 만들고, 아래의 IMountainContextBinder에 연결한다.
using System.Collections.Generic;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    [DefaultExecutionOrder(-900)]
    public sealed class MountainBootstrap : MonoBehaviour
    {
        [SerializeField] private GameBootstrap _game;
        [SerializeField] private GatheringConfig _gathering;
        [Tooltip("뒷산에 나올 수 있는 채집 재료 전부")]
        [SerializeField] private List<GatherMaterialDefinition> _materials = new List<GatherMaterialDefinition>();
        [Tooltip("시작 시간대 (가정: 방과후)")]
        [SerializeField] private TimeOfDay _startTime = TimeOfDay.Day;

        [Header("연결")]
        [Tooltip("이 아래의 서식지와 IMountainContextBinder를 찾는다")]
        [SerializeField] private List<GameObject> _bindRoots = new List<GameObject>();

        public MountainContext Context { get; private set; }

        private void Awake()
        {
            var game = _game.Context;
            var habitats = new List<GatherHabitat>();
            foreach (var root in _bindRoots)
                if (root != null) habitats.AddRange(root.GetComponentsInChildren<GatherHabitat>(true));

            var kinds = new List<GatherHabitatKind>();
            var slotCounts = new List<int>();
            for (int i = 0; i < habitats.Count; i++)
            {
                habitats[i].AssignIndex(i);
                kinds.Add(habitats[i].Kind);
                slotCounts.Add(habitats[i].SlotCount);
            }

            var time = new TimeOfDayState(_startTime);
            var gathering = new GatheringService(new GatherSpawner(kinds, slotCounts), _materials, _gathering, time,
                game.Inventory, game.Messages, game.Requests, game.Random);
            Context = new MountainContext(game, time, gathering, new DebugCommandList());

            foreach (var root in _bindRoots)
            {
                if (root == null) continue;
                foreach (var binder in root.GetComponentsInChildren<IMountainContextBinder>(true))
                    binder.Bind(Context);
            }
            gathering.Respawn();
        }

        private void OnDestroy() => Context?.Gathering.Dispose();
    }
}
