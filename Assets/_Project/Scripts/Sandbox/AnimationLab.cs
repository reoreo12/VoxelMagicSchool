// 애니메이션 랩 본체: 아바타 선택·생성, 클립 목록(새로고침·Humanoid 임포트), 재생 조작, A↔B 반복 전환, 디딤발 측정, 단축키.
// 화면 요소 참조는 씬 생성기가 채운다. 게임 코드는 이 클래스를 참조하지 않는다.
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VoxelMagic.Core;

namespace VoxelMagic.Sandbox
{
    public sealed class AnimationLab : MonoBehaviour
    {
        private static readonly CharacterStateId[] States = { CharacterStateId.Idle, CharacterStateId.Walk, CharacterStateId.Run };

        [Serializable]
        private sealed class Ui
        {
            public TMP_Dropdown AvatarDropdown;
            public TMP_Text AvatarInfo;
            public Toggle HeightReferenceToggle;
            public Button[] StateButtons = new Button[3];
            public TMP_Text NowPlaying;
            public TMP_Text ClipCount;
            public Button RefreshButton;
            public RectTransform ClipListContent;
            public Button ClipButtonTemplate;
            public Button PlayButton;
            public TMP_Text PlayLabel;
            public Button StepBackButton;
            public Button StepForwardButton;
            public Slider TimeSlider;
            public TMP_Text TimeLabel;
            public Toggle LoopToggle;
            public Slider SpeedSlider;
            public TMP_Text SpeedLabel;
            public Slider BlendSlider;
            public TMP_Text BlendLabel;
            public Toggle AbToggle;
            public Slider AbSlider;
            public TMP_Text AbLabel;
            public TMP_Text MeterText;
            public Button[] CameraButtons = new Button[4];
            public Toggle PixelToggle;
            public Slider PixelSlider;
            public TMP_Text PixelLabel;
            public TMP_Text Status;
        }

        [SerializeField] private AnimationLabConfig _config;
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private GameObject _heightReference;
        [SerializeField] private LabCamera _camera;
        [SerializeField] private LabPixelView _pixelView;
        [Tooltip("에디터 검색이 안 될 때(빌드) 쓰는 아바타 목록")]
        [SerializeField] private List<AvatarSetup> _fallbackAvatars = new List<AvatarSetup>();
        [SerializeField] private Ui _ui = new Ui();

        private readonly List<AvatarSetup> _avatars = new List<AvatarSetup>();
        private readonly List<GameObject> _clipButtons = new List<GameObject>();
        private readonly FootSlideMeter _meter = new FootSlideMeter();
        private AvatarSetup _setup;
        private GameObject _instance;
        private Animator _animator;
        private LabClipPlayer _player;
        private AnimationClip _abFirst;
        private AnimationClip _abSecond;
        private float _abTimer;

        private void Start()
        {
            WireUi();
            RefreshCatalog();
            // 아바타 선택이 상태 메시지를 지우므로, 새 모델 추가 안내는 선택 뒤에 다시 보여 준다.
            string catalogMessage = _ui.Status.text;
            SelectAvatar(0);
            if (string.IsNullOrEmpty(_ui.Status.text)) Status(catalogMessage);
        }

        private void OnDestroy() => _player?.Dispose();

        private void Update()
        {
            HandleShortcuts();
            RefreshPixelLabel();
            if (_player == null) return;

            if (_ui.AbToggle.isOn && _abFirst != null && _abSecond != null && !_player.Paused)
            {
                _abTimer += Time.deltaTime;
                if (_abTimer >= _ui.AbSlider.value)
                {
                    _abTimer = 0f;
                    _player.Play(_player.Clip == _abFirst ? _abSecond : _abFirst, _ui.BlendSlider.value);
                }
            }

            _player.Tick(Time.deltaTime);
            if (!_player.Paused && !_player.IsBlending)
                _meter.Sample(_instance.transform, _animator, Time.deltaTime, _player.Speed, _config.MeterSmoothing);
            RefreshPlaybackUi();
        }

        // ── 아바타 ───────────────────────────────────────────

        // 버전 비교가 쉽도록, 바꾸기 전의 클립·재생 시간·일시정지·카메라를 새 아바타에 그대로 이어 간다.
        private void SelectAvatar(int index)
        {
            bool firstSpawn = _instance == null;
            var keepClip = _player?.Clip;
            float keepTime = _player?.Time ?? 0f;
            bool keepPaused = _player != null && _player.Paused;
            _player?.Dispose();
            _player = null;
            if (_instance != null) Destroy(_instance);
            if (_avatars.Count == 0)
            {
                Status(LabTexts.NoAvatar);
                return;
            }

            _setup = _avatars[Mathf.Clamp(index, 0, _avatars.Count - 1)];
            _instance = Instantiate(_setup.Model, _spawnPoint.position, _spawnPoint.rotation);
            _animator = _instance.GetComponent<Animator>();
            if (_animator == null || _animator.avatar == null || !_animator.avatar.isHuman)
            {
                Status(string.Format(LabTexts.NotHuman, _setup.name));
                _ui.AvatarInfo.text = _setup.name;
                return;
            }
            _animator.runtimeAnimatorController = null;
            _animator.applyRootMotion = false;
            _animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            _player = new LabClipPlayer(_animator) { Loop = _ui.LoopToggle.isOn, Speed = _ui.SpeedSlider.value };
            float height = MeasureHeight(_instance);
            if (firstSpawn) _camera.Focus(_spawnPoint.position + Vector3.up * height * _config.FocusHeightRatio, height);
            _ui.AvatarInfo.text = string.Format(LabTexts.AvatarInfo, _setup.name, height, LabTexts.Yes, CountMappedBones(),
                ClipName(CharacterStateId.Idle), ClipName(CharacterStateId.Walk), ClipName(CharacterStateId.Run));

            if (keepClip != null)
            {
                _player.Play(keepClip, 0f);
                _player.Scrub(keepTime);
                _player.Paused = keepPaused;
                _meter.Reset();
            }
            else
            {
                _abFirst = _abSecond = null;
                var first = _setup.ClipFor(CharacterStateId.Idle);
                if (first != null) Play(first);
            }
            Status("");
        }

        private string ClipName(CharacterStateId state) => _setup.ClipFor(state) != null ? _setup.ClipFor(state).name : LabTexts.None;

        private int CountMappedBones()
        {
            int count = 0;
            for (int i = 0; i < (int)HumanBodyBones.LastBone; i++)
                if (_animator.GetBoneTransform((HumanBodyBones)i) != null) count++;
            return count;
        }

        // 렌더러 경계 상자는 여유가 붙어 실제보다 크므로, 메시를 구워 정점 높이로 잰다.
        private static float MeasureHeight(GameObject instance)
        {
            float min = float.MaxValue, max = float.MinValue;
            var mesh = new Mesh();
            foreach (var skinned in instance.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                skinned.BakeMesh(mesh, true);
                foreach (var vertex in mesh.vertices)
                {
                    float y = skinned.transform.TransformPoint(vertex).y;
                    min = Mathf.Min(min, y);
                    max = Mathf.Max(max, y);
                }
            }
            Destroy(mesh);
            return max > min ? max - min : 1f;
        }

        // ── 클립 ─────────────────────────────────────────────

        // Art/Characters에 새로 넣은 모델은 여기서 자동으로 준비돼 아바타 목록에 들어간다.
        private void RefreshCatalog()
        {
            var added = AnimationLabBridge.PrepareNewModels?.Invoke();
            if (added != null && added.Count > 0)
                Status(string.Format(LabTexts.ModelsAdded, added.Count, string.Join(", ", added)));

            _avatars.Clear();
            var found = AnimationLabBridge.ScanAvatars?.Invoke();
            _avatars.AddRange(found != null && found.Count > 0 ? found : _fallbackAvatars.Where(a => a != null));
            int keep = _setup != null ? Mathf.Max(0, _avatars.IndexOf(_setup)) : 0;
            _ui.AvatarDropdown.ClearOptions();
            _ui.AvatarDropdown.AddOptions(_avatars.Select(a => a.name).ToList());
            _ui.AvatarDropdown.SetValueWithoutNotify(keep);

            foreach (var button in _clipButtons) Destroy(button);
            _clipButtons.Clear();
            if (!AnimationLabBridge.IsConnected)
            {
                _ui.ClipCount.text = LabTexts.NoBridge;
                return;
            }

            var clips = AnimationLabBridge.ScanClips();
            _ui.ClipCount.text = string.Format(LabTexts.ClipCount, clips.Count, clips.Count(c => c.IsHuman));
            foreach (var entry in clips)
            {
                var button = Instantiate(_ui.ClipButtonTemplate, _ui.ClipListContent);
                button.gameObject.SetActive(true);
                var label = button.GetComponentInChildren<TMP_Text>();
                var captured = entry;
                if (entry.IsHuman)
                {
                    label.text = string.Format(LabTexts.ClipEntry, entry.Clip.name, entry.Clip.length, System.IO.Path.GetFileName(entry.Path));
                    button.onClick.AddListener(() => Play(captured.Clip));
                }
                else if (entry.CanImport)
                {
                    label.text = string.Format(LabTexts.GenericEntry, System.IO.Path.GetFileName(entry.Path));
                    button.onClick.AddListener(() => ImportAsHumanoid(captured.Path));
                }
                else
                {
                    label.text = string.Format(LabTexts.UnusableEntry, entry.Clip.name, System.IO.Path.GetFileName(entry.Path));
                    button.interactable = false;
                }
                _clipButtons.Add(button.gameObject);
            }
        }

        private void ImportAsHumanoid(string path)
        {
            bool ok = AnimationLabBridge.ImportAsHumanoid != null && AnimationLabBridge.ImportAsHumanoid(path);
            Status(string.Format(ok ? LabTexts.Imported : LabTexts.ImportFailed, System.IO.Path.GetFileName(path)));
            RefreshCatalog();
        }

        private void Play(AnimationClip clip)
        {
            if (_player == null || clip == null) return;
            if (clip != _abSecond)
            {
                _abFirst = _abSecond;
                _abSecond = clip;
            }
            _abTimer = 0f;
            _player.Paused = false;
            _player.Play(clip, _player.Clip == null ? 0f : _ui.BlendSlider.value);
            _meter.Reset();
        }

        private void PlayState(CharacterStateId state)
        {
            if (_setup == null) return;
            var clip = _setup.ClipFor(state);
            if (clip == null) Status(string.Format(LabTexts.StateMissing, state));
            else Play(clip);
        }

        // ── UI ───────────────────────────────────────────────

        private void WireUi()
        {
            _ui.ClipButtonTemplate.gameObject.SetActive(false);
            _ui.AvatarDropdown.onValueChanged.AddListener(SelectAvatar);
            _ui.HeightReferenceToggle.onValueChanged.AddListener(on => _heightReference.SetActive(on));
            for (int i = 0; i < States.Length; i++)
            {
                var state = States[i];
                _ui.StateButtons[i].onClick.AddListener(() => PlayState(state));
            }
            _ui.RefreshButton.onClick.AddListener(RefreshCatalog);
            _ui.PlayButton.onClick.AddListener(TogglePause);
            _ui.StepBackButton.onClick.AddListener(() => _player?.StepFrames(-1, _config.StepFrameRate));
            _ui.StepForwardButton.onClick.AddListener(() => _player?.StepFrames(1, _config.StepFrameRate));
            _ui.TimeSlider.onValueChanged.AddListener(t =>
            {
                if (_player == null) return;
                _player.Paused = true;
                _player.Scrub(t);
            });
            _ui.LoopToggle.onValueChanged.AddListener(on => { if (_player != null) _player.Loop = on; });

            _ui.SpeedSlider.minValue = 0f;
            _ui.SpeedSlider.maxValue = _config.MaxSpeed;
            _ui.SpeedSlider.SetValueWithoutNotify(1f);
            _ui.SpeedSlider.onValueChanged.AddListener(v => { if (_player != null) _player.Speed = v; });
            _ui.BlendSlider.minValue = 0f;
            _ui.BlendSlider.maxValue = _config.MaxBlend;
            _ui.BlendSlider.SetValueWithoutNotify(_config.DefaultBlend);
            _ui.AbSlider.minValue = 0.2f;
            _ui.AbSlider.maxValue = _config.AbMaxInterval;
            _ui.AbSlider.SetValueWithoutNotify(_config.AbDefaultInterval);
            _ui.AbToggle.onValueChanged.AddListener(_ => _abTimer = 0f);

            var views = new[] { LabView.Front, LabView.Side, LabView.Back, LabView.ThreeQuarter };
            for (int i = 0; i < views.Length; i++)
            {
                var view = views[i];
                _ui.CameraButtons[i].onClick.AddListener(() => _camera.SetView(view));
            }

            _ui.PixelSlider.wholeNumbers = true;
            _ui.PixelSlider.minValue = _config.PixelSizeRange.x;
            _ui.PixelSlider.maxValue = _config.PixelSizeRange.y;
            _ui.PixelSlider.SetValueWithoutNotify(_config.DefaultPixelSize);
            _pixelView.SetPixelSize(_config.DefaultPixelSize);
            _ui.PixelSlider.onValueChanged.AddListener(v => _pixelView.SetPixelSize(Mathf.RoundToInt(v)));
            _ui.PixelToggle.onValueChanged.AddListener(_pixelView.SetOn);
            _pixelView.SetOn(_ui.PixelToggle.isOn);
        }

        private void RefreshPixelLabel()
        {
            var resolution = _pixelView.Resolution;
            _ui.PixelLabel.text = _pixelView.IsOn
                ? string.Format(LabTexts.PixelOn, _pixelView.PixelSize, resolution.x, resolution.y)
                : string.Format(LabTexts.PixelOff, _pixelView.PixelSize);
        }

        private void TogglePause()
        {
            if (_player != null) _player.Paused = !_player.Paused;
        }

        private void HandleShortcuts()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard.spaceKey.wasPressedThisFrame) TogglePause();
            if (keyboard.leftArrowKey.wasPressedThisFrame) _player?.StepFrames(-1, _config.StepFrameRate);
            if (keyboard.rightArrowKey.wasPressedThisFrame) _player?.StepFrames(1, _config.StepFrameRate);
            if (keyboard.digit1Key.wasPressedThisFrame) PlayState(CharacterStateId.Idle);
            if (keyboard.digit2Key.wasPressedThisFrame) PlayState(CharacterStateId.Walk);
            if (keyboard.digit3Key.wasPressedThisFrame) PlayState(CharacterStateId.Run);
            if (keyboard.lKey.wasPressedThisFrame) _ui.LoopToggle.isOn = !_ui.LoopToggle.isOn;
            if (keyboard.pKey.wasPressedThisFrame) _ui.PixelToggle.isOn = !_ui.PixelToggle.isOn;
        }

        private void RefreshPlaybackUi()
        {
            var clip = _player.Clip;
            _ui.PlayLabel.text = _player.Paused ? LabTexts.Play : LabTexts.Pause;
            _ui.SpeedLabel.text = string.Format(LabTexts.SpeedFormat, _ui.SpeedSlider.value);
            _ui.BlendLabel.text = string.Format(LabTexts.BlendFormat, _ui.BlendSlider.value);
            _ui.AbLabel.text = string.Format(LabTexts.AbFormat, _ui.AbSlider.value);

            if (clip == null)
            {
                _ui.NowPlaying.text = LabTexts.NoClip;
                return;
            }
            float frameRate = clip.frameRate > 0f ? clip.frameRate : _config.StepFrameRate;
            _ui.TimeSlider.maxValue = Mathf.Max(0.0001f, clip.length);
            _ui.TimeSlider.SetValueWithoutNotify(_player.Time);
            _ui.TimeLabel.text = string.Format(LabTexts.TimeFormat, _player.Time, clip.length,
                Mathf.RoundToInt(_player.Time * frameRate), Mathf.RoundToInt(clip.length * frameRate));

            string now = string.Format(LabTexts.NowPlaying, clip.name);
            if (_player.IsBlending) now += string.Format(LabTexts.Blending, _player.BlendProgress * 100f);
            if (_ui.AbToggle.isOn)
                now += "\n" + (_abFirst != null && _abSecond != null
                    ? string.Format(LabTexts.AbPair, _abFirst.name, _abSecond.name)
                    : LabTexts.AbNeedTwo);
            _ui.NowPlaying.text = now;

            if (!_meter.HasValue || _meter.NaturalSpeed < 0.05f)
            {
                _ui.MeterText.text = LabTexts.MeterWaiting + HipLine();
                return;
            }
            _ui.MeterText.text = string.Format(LabTexts.MeterFormat, _meter.NaturalSpeed) + SuggestionLine(clip) + HipLine();
        }

        // 지금 클립이 아바타의 걷기/달리기로 연결돼 있으면 그 상태만, 아니면 두 경우를 모두 제안한다.
        private string SuggestionLine(AnimationClip clip)
        {
            var move = _config.MoveConfig;
            if (move == null) return "";
            bool isWalk = _setup != null && clip == _setup.ClipFor(CharacterStateId.Walk);
            bool isRun = _setup != null && clip == _setup.ClipFor(CharacterStateId.Run);
            string walk = string.Format(LabTexts.SuggestWalk, move.WalkSpeed, move.WalkSpeed / _meter.NaturalSpeed);
            string run = string.Format(LabTexts.SuggestRun, move.RunSpeed, move.RunSpeed / _meter.NaturalSpeed);
            if (isWalk) return walk;
            if (isRun) return run;
            return walk + run;
        }

        private string HipLine() => string.Format(LabTexts.HipFormat, _meter.HipDrift,
            _meter.HipDrift < _config.InPlaceTolerance ? LabTexts.InPlace : LabTexts.NotInPlace);

        private void Status(string message) => _ui.Status.text = message;
    }
}
