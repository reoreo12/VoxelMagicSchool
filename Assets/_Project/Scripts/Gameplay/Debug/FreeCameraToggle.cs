// 개발용 프리 카메라 (에디터·개발 빌드만): F8로 게임 카메라 ↔ 프리 카메라 전환. 씬에 놓지 않아도 플레이 시작 때 스스로 생겨 모든 씬에서 쓴다.
// 프리 카메라 중에는 플레이어 입력(3D·횡스크롤 입력 리더)을 꺼서 WASD는 카메라만 움직인다. 우클릭 끌기 회전, Q/E 아래/위, Shift 빠르게.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace VoxelMagic.Gameplay
{
    public sealed class FreeCameraToggle : MonoBehaviour
    {
        [SerializeField] private Key _toggleKey = Key.F8;
        [SerializeField, Min(0.1f)] private float _moveSpeed = 4f;
        [SerializeField, Min(1f)] private float _fastMultiplier = 3f;
        [SerializeField, Min(0.01f)] private float _lookSensitivity = 0.15f;
        [SerializeField] private bool _showLabel = true;

        private Camera _freeCamera;
        private Camera _gameCamera;
        private AudioListener _gameListener;
        private readonly List<Behaviour> _pausedInputs = new List<Behaviour>();
        private float _yaw;
        private float _pitch;
        private GUIStyle _style;

        public bool IsOn => _freeCamera != null && _freeCamera.enabled;

        /// <summary>지금 프리 카메라가 켜져 있는지 (다른 디버그 기능이 프리 카메라 중에만 동작할 때 확인용).</summary>
        public static bool AnyOn { get; private set; }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Create()
        {
            var go = new GameObject("[Debug] FreeCamera");
            DontDestroyOnLoad(go);
            go.AddComponent<FreeCameraToggle>();
            AnyOn = false;
        }
#endif

        private void Awake()
        {
            _freeCamera = gameObject.AddComponent<Camera>();
            _freeCamera.enabled = false;
        }

        private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
        private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

        // 켠 채로 씬이 바뀌면 새 씬의 카메라·입력이 살아 있으니 프리 카메라만 끈다.
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode != LoadSceneMode.Single) return;
            _freeCamera.enabled = false;
            _gameCamera = null;
            _gameListener = null;
            _pausedInputs.Clear();
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb != null && kb[_toggleKey].wasPressedThisFrame) Toggle();
            AnyOn = IsOn;
            if (kb == null) return;
            if (IsOn) Fly(kb, Mouse.current);
        }

        private void Toggle()
        {
            if (IsOn)
            {
                _freeCamera.enabled = false;
                if (_gameCamera != null) _gameCamera.enabled = true;
                if (_gameListener != null) _gameListener.enabled = true;
                SetPlayerInput(true);
                return;
            }

            _gameCamera = FindGameCamera();
            if (_gameCamera != null)
            {
                transform.SetPositionAndRotation(_gameCamera.transform.position, _gameCamera.transform.rotation);
                _freeCamera.CopyFrom(_gameCamera); // 렌더 설정·FOV를 같게
                _gameCamera.enabled = false;
                _gameListener = _gameCamera.GetComponent<AudioListener>();
                if (_gameListener != null) _gameListener.enabled = false;
            }
            var e = transform.eulerAngles;
            _yaw = e.y;
            _pitch = e.x > 180f ? e.x - 360f : e.x;
            SetPlayerInput(false);
            _freeCamera.enabled = true;
        }

        // 입력 리더를 끄면 그 액션 맵이 꺼져 이동·상호작용 키가 들어가지 않는다 (다시 켜면 씬 이동 중이 아닐 때만 맵이 켜짐).
        private void SetPlayerInput(bool on)
        {
            if (!on)
            {
                _pausedInputs.Clear();
                foreach (var reader in FindObjectsByType<GameInputReader>(FindObjectsSortMode.None)) Pause(reader);
                foreach (var reader in FindObjectsByType<SideScrollInputReader>(FindObjectsSortMode.None)) Pause(reader);
                return;
            }
            foreach (var reader in _pausedInputs)
                if (reader != null) reader.enabled = true;
            _pausedInputs.Clear();
        }

        private void Pause(Behaviour reader)
        {
            if (!reader.enabled) return;
            reader.enabled = false;
            _pausedInputs.Add(reader);
        }

        private Camera FindGameCamera()
        {
            if (Camera.main != null) return Camera.main;
            foreach (var cam in Camera.allCameras)
                if (cam != _freeCamera) return cam;
            return null;
        }

        private void Fly(Keyboard kb, Mouse mouse)
        {
            if (mouse != null && mouse.rightButton.isPressed)
            {
                Vector2 d = mouse.delta.ReadValue() * _lookSensitivity;
                _yaw += d.x;
                _pitch = Mathf.Clamp(_pitch - d.y, -89f, 89f);
                transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            }

            Vector3 move = Vector3.zero;
            if (kb.wKey.isPressed) move += transform.forward;
            if (kb.sKey.isPressed) move -= transform.forward;
            if (kb.dKey.isPressed) move += transform.right;
            if (kb.aKey.isPressed) move -= transform.right;
            if (kb.eKey.isPressed) move += Vector3.up;
            if (kb.qKey.isPressed) move -= Vector3.up;
            if (move.sqrMagnitude < 0.0001f) return;

            float speed = _moveSpeed * (kb.leftShiftKey.isPressed ? _fastMultiplier : 1f);
            transform.position += move.normalized * speed * Time.unscaledDeltaTime;
        }

        private void OnGUI()
        {
            if (!_showLabel || !IsOn) return;
            _style ??= new GUIStyle(GUI.skin.label) { fontSize = 16, normal = { textColor = new Color(0.6f, 0.9f, 1f) } };
            GUI.Label(new Rect(12f, 34f, 760f, 26f), $"[프리 카메라] WASD 이동 · Q/E 아래/위 · Shift 빠르게 · 우클릭 끌기 회전 · {_toggleKey} 돌아가기 (플레이어 조작 멈춤)", _style);
        }
    }
}
