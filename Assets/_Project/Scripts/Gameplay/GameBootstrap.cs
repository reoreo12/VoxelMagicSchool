// 씬 시작 시 Core 객체를 한 번 조립하고, 월드·플레이어·UI의 IGameContextBinder에 연결한다.
// 플레이어 데이터는 GameSession에서 받고(처음이면 저장 파일에서 불러오거나 새로 만듦), 진행 상태의 세계(시계·밭·채집·게시판)를 한 번 만들고, 게임 시계를 구동기에 붙인다.
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
        [Tooltip("하루 구간·경고·하루 시작 시각")]
        [SerializeField] private TimeConfig _time;
        [Tooltip("8일 일정표 (날 종류)")]
        [SerializeField] private CalendarConfig _calendar;
        [Tooltip("채집 자리 수·희귀도·재료 목록 (하루 시작 때 뽑기)")]
        [SerializeField] private GatheringConfig _gathering;
        [Tooltip("의뢰 규칙 (게시판·수락·점수)")]
        [SerializeField] private QuestConfig _quest;
        [Tooltip("게시판에 올라올 수 있는 의뢰 전부")]
        [SerializeField] private RequestPool _requestPool;

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

        private TimeScaleDriver _timeScale;

        private void Awake()
        {
            GameSaveSystem.AutoSaveEnabled = _autoSave;
            var progress = GameSaveSystem.GetOrCreateProgress(_balance, _requests, _catalog, GiveStartingItems);
            if (_time != null && _calendar != null)
                progress.EnsureWorld(new WorldSetup
                {
                    Time = _time, Calendar = _calendar, Farm = _farm, Gathering = _gathering,
                    Quest = _quest, RequestPool = _requestPool, Random = new SystemRandom()
                });
            Context = GameContext.Create(_balance, _farm, _fishing, _player, progress, new SystemRandom());
            // 씬에 들어올 때의 timeScale을 평소 값으로 기억한다(보통 1).
            _timeScale = new TimeScaleDriver(Context.Pause, Time.timeScale > 0f ? Time.timeScale : 1f);
            if (Context.Clock != null)
            {
                var driver = GetComponent<GameClockDriver>();
                if (driver == null) driver = gameObject.AddComponent<GameClockDriver>();
                driver.Initialize(Context.Clock, Context.Pause);
            }
            else Debug.LogWarning("[GameBootstrap] Time·Calendar 설정이 비어 있어 게임 시계가 없습니다.");

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

        private void OnDestroy()
        {
            _timeScale?.Dispose();
        }

        private void GiveStartingItems(PlayerProgress progress)
        {
            foreach (var item in _startingItems)
                progress.Inventory.TryAdd(item.Item, item.Amount);
        }
    }
}
