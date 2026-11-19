// 개발용 임시 치트 키 (에디터·개발 빌드만): F10 = 어디서나 의뢰 게시판 창 열기, F11 = 어디서나 크리처에게 의뢰 전달(완료).
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
            _openBoard.Enable();
            _deliver.Enable();
        }

        private void OnDisable()
        {
            _openBoard.performed -= OnOpenBoard;
            _deliver.performed -= OnDeliver;
            _openBoard.Disable();
            _deliver.Disable();
        }

        private void OnOpenBoard(InputAction.CallbackContext _)
        {
            var context = FindContext();
            if (context == null || context.Screen.IsModalOpen) return;
            context.Screen.Open(GameWindow.RequestBoard);
        }

        private void OnDeliver(InputAction.CallbackContext _)
        {
            var context = FindContext();
            if (context == null || context.Screen.IsModalOpen) return;
            if (CreatureNpc.TryDeliver(context)) return;
            context.Messages.ShowToast(context.Requests.CanDeliver() == DeliverResult.MissingItems
                ? $"{GameTexts.MissingItems}: {context.Requests.DescribeMissing()}"
                : GameTexts.NoActiveRequest);
        }

        private static GameContext FindContext()
        {
            if (SceneTravel.IsTraveling) return null;
            var bootstrap = FindFirstObjectByType<GameBootstrap>();
            return bootstrap != null ? bootstrap.Context : null;
        }
    }
}
