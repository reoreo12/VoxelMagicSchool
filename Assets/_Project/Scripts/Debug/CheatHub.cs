// 치트·디버그 표시의 뿌리 (에디터·개발 빌드만). 플레이 시작 때 스스로 생겨 씬을 넘어 남고, 단축키를 읽고, 지금 씬의 GameBootstrap에서 치트 대상을 찾는다.
// 모든 동작은 패널 버튼으로 한다(왼쪽 가장자리 "치트" 버튼). 키는 ` 패널 여닫기만 기본이고, 나머지는 Inspector "[Cheat] Hub"에서 원하면 지정한다.
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using VoxelMagic.Core;
using VoxelMagic.Gameplay;

namespace VoxelMagic.DebugTools
{
    /// <summary>치트가 다루는 대상 묶음 (프레임마다 찾음).</summary>
    public sealed class CheatTargets
    {
        public GameContext Context;
        public GameClock Clock;
        public DayFlow Days;
        public TimePause ScenePause;
        public GameClockDriver Driver;
    }

    public sealed class CheatHub : MonoBehaviour
    {
        [SerializeField] private Key _panelKey = Key.Backquote;
        [Header("선택 단축키 (기본 없음)")]
        [SerializeField] private Key _overlayKey = Key.None;
        [SerializeField] private Key _pauseKey = Key.None;
        [SerializeField] private Key _fasterKey = Key.None;
        [SerializeField] private Key _slowerKey = Key.None;
        [SerializeField] private Key _addHourKey = Key.None;
        [SerializeField, Tooltip("IMGUI 글자 기준 화면 높이 (이 높이에서 배율 1)")] private float _referenceHeight = 720f;

        private readonly CheatTargets _targets = new CheatTargets();
        private GameBootstrap _bootstrap;
        private bool _searched;

        public static CheatHub Instance { get; private set; }
        public CheatPanel Panel { get; } = new CheatPanel();
        public DebugOverlay Overlay { get; } = new DebugOverlay();
        public CheatTargets Targets => _targets;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Create()
        {
            var go = new GameObject("[Cheat] Hub");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<CheatHub>();
        }

        private void Awake()
        {
            Panel.ToggleOverlay = () => Overlay.Visible = !Overlay.Visible;
            Panel.IsOverlayVisible = () => Overlay.Visible;
            DayStartCheats.Register(Panel, Overlay);
            RequestCheats.Register(Panel, Overlay);
            MapCheats.Register(Panel);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => _searched = false;

        private void Update()
        {
            RefreshTargets();
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (Pressed(keyboard, _panelKey)) Panel.Visible = !Panel.Visible;
            if (Pressed(keyboard, _overlayKey)) Overlay.Visible = !Overlay.Visible;
            if (Pressed(keyboard, _pauseKey)) TimeCheats.TogglePause(_targets.Clock);
            if (Pressed(keyboard, _fasterKey)) TimeCheats.StepSpeed(_targets.Clock, +1, Panel.WorldSpeedToo);
            if (Pressed(keyboard, _slowerKey)) TimeCheats.StepSpeed(_targets.Clock, -1, Panel.WorldSpeedToo);
            if (Pressed(keyboard, _addHourKey)) TimeCheats.AddHour(_targets.Clock);
        }

        private static bool Pressed(Keyboard keyboard, Key key) => key != Key.None && keyboard[key].wasPressedThisFrame;

        // 씬이 바뀐 뒤 한 번 GameBootstrap을 찾는다. 시계는 진행 상태 소속이라 부트스트랩이 없는 씬에서도 보인다.
        private void RefreshTargets()
        {
            if (!_searched || _bootstrap == null)
            {
                _bootstrap = FindFirstObjectByType<GameBootstrap>();
                _searched = true;
            }
            var context = _bootstrap != null ? _bootstrap.Context : null;
            var progress = context?.Progress ?? GameSession.Progress;
            _targets.Context = context;
            _targets.Clock = progress?.Clock;
            _targets.Days = progress?.Days;
            _targets.ScenePause = context?.Pause;
            _targets.Driver = _bootstrap != null ? _bootstrap.GetComponent<GameClockDriver>() : null;
        }

        private void OnGUI()
        {
            float scale = Mathf.Max(1f, Mathf.Floor(Screen.height / _referenceHeight * 2f) / 2f);
            Overlay.Draw(_targets, scale);
            Panel.Draw(_targets, scale);
        }
    }
}
#endif
