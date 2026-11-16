// 씬 시작 시 Core 객체를 한 번 조립하고, 월드·플레이어·UI의 IGameContextBinder에 연결한다.
// 플레이어 데이터는 GameSession에서 받고(처음이면 저장 파일에서 불러오거나 새로 만듦), 밭은 연결이 끝난 뒤 보관본대로 다시 심는다.
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
        [Tooltip("새 게임(저장 파일 없음)일 때만 지급")]
        [SerializeField] private List<ItemAmount> _startingItems = new List<ItemAmount>();

        [Header("저장")]
        [Tooltip("저장된 ID를 에셋으로 되찾는 표 (메뉴 VoxelMagic/저장/정의 목록 채우기)")]
        [SerializeField] private DefinitionCatalog _catalog;
        [Tooltip("끄면 씬 이동·종료 때 저장하지 않는다 (시험용)")]
        [SerializeField] private bool _autoSave = true;

        [Header("연결")]
        [Tooltip("이 아래의 모든 IGameContextBinder에 Core를 연결한다")]
        [SerializeField] private List<GameObject> _bindRoots = new List<GameObject>();
        [SerializeField] private PlayerInteractor _playerInteractor;

        public GameContext Context { get; private set; }

        private void Awake()
        {
            GameSaveSystem.AutoSaveEnabled = _autoSave;
            var progress = GameSaveSystem.GetOrCreateProgress(_balance, _requests, _catalog, GiveStartingItems);
            Context = GameContext.Create(_balance, _farm, _fishing, _player, progress, new SystemRandom());

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

            progress.RestoreFarm(Context.FarmGrid);
            GameSession.Capturing += CaptureScene;
        }

        private void OnDestroy()
        {
            GameSession.Capturing -= CaptureScene;
            // 저장 없이 씬이 내려가도(에디터에서 씬 전환 등) 밭은 보관본에 남긴다.
            if (Context != null && GameSession.Progress == Context.Progress) Context.Progress.CaptureFarm(Context.FarmGrid);
        }

        private void CaptureScene(PlayerProgress progress)
        {
            if (Context != null && progress == Context.Progress) progress.CaptureFarm(Context.FarmGrid);
        }

        private void GiveStartingItems(PlayerProgress progress)
        {
            foreach (var item in _startingItems)
                progress.Inventory.TryAdd(item.Item, item.Amount);
        }
    }
}
