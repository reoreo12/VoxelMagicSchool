// 개발용 임시 치트 키 (에디터·개발 빌드만): F9로 플레이어 걷기·달리기 속도 ×3 켜고 끄기. 씬에 놓지 않아도 플레이 시작 때 스스로 생겨 모든 맵에서 동작한다.
// 배율·키는 플레이 중 DontDestroyOnLoad의 "[Cheat] MoveSpeed" 오브젝트 Inspector에서 바꿀 수 있다 (기본값 가정).
using UnityEngine;
using UnityEngine.InputSystem;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class MoveSpeedCheatKey : MonoBehaviour
    {
        [SerializeField] private InputAction _toggle = new InputAction("MoveSpeedCheat", InputActionType.Button, "<Keyboard>/f9");
        [Tooltip("켰을 때 속도 배율 (가정 3)")]
        [SerializeField, Min(1f)] private float _multiplier = 3f;
        [SerializeField] private bool _showLabel = true;

        private GUIStyle _style;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState() => MoveSpeedCheat.Reset();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Create()
        {
            var go = new GameObject("[Cheat] MoveSpeed");
            DontDestroyOnLoad(go);
            go.AddComponent<MoveSpeedCheatKey>();
        }
#endif

        private void OnEnable()
        {
            _toggle.performed += OnToggle;
            _toggle.Enable();
        }

        private void OnDisable()
        {
            _toggle.performed -= OnToggle;
            _toggle.Disable();
        }

        private void OnToggle(InputAction.CallbackContext _) => MoveSpeedCheat.Toggle(_multiplier);

        // 플레이를 끝내면(오브젝트가 사라지면) 치트도 끈다 — 에디터·테스트에 배율이 남지 않게.
        private void OnDestroy() => MoveSpeedCheat.Reset();

        private void OnGUI()
        {
            if (!_showLabel || !MoveSpeedCheat.IsOn) return;
            _style ??= new GUIStyle(GUI.skin.label) { fontSize = 18, normal = { textColor = new Color(1f, 0.8f, 0.3f) } };
            GUI.Label(new Rect(12f, 8f, 400f, 30f), $"[치트] 이동 속도 ×{MoveSpeedCheat.Multiplier:0.#}  ({_toggle.GetBindingDisplayString()})", _style);
        }
    }
}
