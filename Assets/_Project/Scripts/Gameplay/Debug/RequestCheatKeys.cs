// 개발용 임시 치트 키 (에디터·개발 빌드만): F10 = 어디서나 의뢰 게시판 창 열기, F11 = 어디서나 크리처에게 의뢰 전달(완료),
// F7 = 진행 중 의뢰의 모자란 재료 1개 넣기, F12 = Inspector에서 고른 아이템을 정한 개수만큼 넣기.
// 씬에 놓지 않아도 플레이 시작 때 스스로 생기고, 지금 씬의 GameBootstrap 상태를 쓴다 (없는 씬(복도)에서는 아무것도 안 함). 키는 "[Cheat] Requests" Inspector에서.
using UnityEngine;
using UnityEngine.InputSystem;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class RequestCheatKeys : MonoBehaviour
    {
        [SerializeField] private InputAction _openBoard = new InputAction("CheatOpenRequestBoard", InputActionType.Button, "<Keyboard>/f10");
        [SerializeField] private InputAction _deliver = new InputAction("CheatDeliverRequest", InputActionType.Button, "<Keyboard>/f11");
        [SerializeField] private InputAction _addMissing = new InputAction("CheatAddMissingMaterial", InputActionType.Button, "<Keyboard>/f7");
        [SerializeField] private InputAction _addChosen = new InputAction("CheatAddChosenItem", InputActionType.Button, "<Keyboard>/f12");
        [SerializeField, Tooltip("F12로 넣을 아이템 (플레이 중 Inspector에서 고른다)")] private ItemDefinition _itemToAdd;
        [SerializeField, Min(1), Tooltip("F12 한 번에 넣을 개수")] private int _amountToAdd = 1;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Create()
        {
            var go = new GameObject("[Cheat] Requests");
            DontDestroyOnLoad(go);
            go.AddComponent<RequestCheatKeys>();
        }
#endif

        private void OnEnable()
        {
            _openBoard.performed += OnOpenBoard;
            _deliver.performed += OnDeliver;
            _addMissing.performed += OnAddMissing;
            _addChosen.performed += OnAddChosen;
            _openBoard.Enable();
            _deliver.Enable();
            _addMissing.Enable();
            _addChosen.Enable();
        }

        private void OnDisable()
        {
            _openBoard.performed -= OnOpenBoard;
            _deliver.performed -= OnDeliver;
            _addMissing.performed -= OnAddMissing;
            _addChosen.performed -= OnAddChosen;
            _openBoard.Disable();
            _deliver.Disable();
            _addMissing.Disable();
            _addChosen.Disable();
        }

        private void OnOpenBoard(InputAction.CallbackContext _)
        {
            var context = FindContext();
            if (context == null || context.Screens.HasAny) return;
            context.Screens.Open(GameScreen.RequestBoard);
        }

        private void OnDeliver(InputAction.CallbackContext _)
        {
            var context = FindContext();
            if (context == null || context.Screens.HasAny) return;
            var creature = FindFirstObjectByType<CreatureNpc>(); // 씬의 크리처 화자로 대사창까지 보여 준다
            if (CreatureNpc.TryDeliver(context, creature != null ? creature.Speaker : null)) return;
            context.Messages.ShowToast(context.Requests.CanDeliver() == DeliverResult.MissingItems
                ? $"{GameTexts.MissingItems}: {context.Requests.DescribeMissing()}"
                : GameTexts.NoActiveRequest);
        }

        // 진행 중 의뢰에서 아직 모자란 첫 재료를 1개 넣는다. 다 채우면 의뢰 목록이 완료 색이 되고 완료 알림이 뜬다.
        private void OnAddMissing(InputAction.CallbackContext _)
        {
            var context = FindContext();
            if (context == null) return;
            if (!context.Requests.HasActive) { context.Messages.ShowToast(GameTexts.NoActiveRequest); return; }
            foreach (var need in context.Requests.Progress)
            {
                if (need.IsMet) continue;
                if (!context.Inventory.TryAdd(need.Item, 1)) context.Messages.ShowToast("[치트] 인벤토리가 가득 찼습니다");
                return;
            }
            context.Messages.ShowToast("[치트] 재료가 이미 다 모였습니다");
        }

        private void OnAddChosen(InputAction.CallbackContext _)
        {
            var context = FindContext();
            if (context == null) return;
            if (_itemToAdd == null) { context.Messages.ShowToast("[치트] [Cheat] Requests의 Item To Add를 고르세요"); return; }
            if (!context.Inventory.TryAdd(_itemToAdd, _amountToAdd)) context.Messages.ShowToast("[치트] 인벤토리가 가득 찼습니다");
        }

        private static GameContext FindContext()
        {
            if (SceneTravel.IsTraveling) return null;
            var bootstrap = FindFirstObjectByType<GameBootstrap>();
            return bootstrap != null ? bootstrap.Context : null;
        }
    }
}
