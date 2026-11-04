// 씬 시작 시 Core 객체를 한 번 조립하고, 월드·플레이어·UI의 IGameContextBinder에 연결한다.
// 싱글턴 없이 의존성을 이 한 곳에서만 만든다.
using System.Collections.Generic;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    [DefaultExecutionOrder(-1000)]
    public sealed class GameBootstrap : MonoBehaviour
    {
        [Header("설정")]
        [SerializeField] private GameBalanceConfig _balance;
        [SerializeField] private FarmConfig _farm;
        [SerializeField] private FishingConfig _fishing;
        [SerializeField] private PlayerConfig _player;

        [Header("데이터")]
        [SerializeField] private List<RequestDefinition> _requests = new List<RequestDefinition>();
        [SerializeField] private List<ItemAmount> _startingItems = new List<ItemAmount>();

        [Header("연결")]
        [Tooltip("이 아래의 모든 IGameContextBinder에 Core를 연결한다")]
        [SerializeField] private List<GameObject> _bindRoots = new List<GameObject>();
        [SerializeField] private PlayerInteractor _playerInteractor;

        public GameContext Context { get; private set; }

        private void Awake()
        {
            Context = GameContext.Create(_balance, _farm, _fishing, _player, _requests, new SystemRandom());
            foreach (var item in _startingItems)
                Context.Inventory.TryAdd(item.Item, item.Amount);

            var registry = new InteractableRegistry();
            foreach (var root in _bindRoots)
            {
                if (root == null) continue;
                foreach (var binder in root.GetComponentsInChildren<IGameContextBinder>(true))
                    binder.Bind(Context);
                foreach (var interactable in root.GetComponentsInChildren<IInteractable>(true))
                    registry.Add(interactable);
            }
            _playerInteractor.Initialize(registry);
        }
    }
}
