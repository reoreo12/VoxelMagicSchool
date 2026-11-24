// 개발용 임시 치트 키 (에디터·개발 빌드만): 대사창이 떠 있을 때 ] = 다음 표정, [ = 이전 표정으로 초상화만 바꿔 본다.
// 씬에 놓지 않아도 플레이 시작 때 스스로 생기고, 지금 씬의 GameBootstrap 상태를 쓴다. 키는 "[Cheat] Dialogue" Inspector에서.
using UnityEngine;
using UnityEngine.InputSystem;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class DialogueCheatKeys : MonoBehaviour
    {
        [SerializeField] private InputAction _nextExpression = new InputAction("CheatNextExpression", InputActionType.Button, "<Keyboard>/rightBracket");
        [SerializeField] private InputAction _previousExpression = new InputAction("CheatPreviousExpression", InputActionType.Button, "<Keyboard>/leftBracket");

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Create()
        {
            var go = new GameObject("[Cheat] Dialogue");
            DontDestroyOnLoad(go);
            go.AddComponent<DialogueCheatKeys>();
        }
#endif

        private void OnEnable()
        {
            _nextExpression.performed += OnNext;
            _previousExpression.performed += OnPrevious;
            _nextExpression.Enable();
            _previousExpression.Enable();
        }

        private void OnDisable()
        {
            _nextExpression.performed -= OnNext;
            _previousExpression.performed -= OnPrevious;
            _nextExpression.Disable();
            _previousExpression.Disable();
        }

        private static void OnNext(InputAction.CallbackContext _) => Request(1);
        private static void OnPrevious(InputAction.CallbackContext _) => Request(-1);

        private static void Request(int step)
        {
            if (SceneTravel.IsTraveling) return;
            var bootstrap = FindFirstObjectByType<GameBootstrap>();
            if (bootstrap != null && bootstrap.Context != null) bootstrap.Context.Dialogue.RequestPreviewExpression(step);
        }
    }
}
