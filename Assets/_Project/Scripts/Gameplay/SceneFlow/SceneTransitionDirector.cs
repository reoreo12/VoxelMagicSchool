// 씬 이동 진행자 (씬마다 하나). 나갈 때: 입력 막기 → 원형 페이드 아웃 → 비동기 로드. 들어올 때: 가린 채 시작 → 페이드 인 → 입력 풀기.
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    [DefaultExecutionOrder(-2000)]
    public sealed class SceneTransitionDirector : MonoBehaviour
    {
        [SerializeField] private SceneTransitionConfig _config;
        [Tooltip("IScreenFade를 구현한 컴포넌트 (PixelCircleFadeView)")]
        [SerializeField] private MonoBehaviour _fadeView;
        [Tooltip("원 중심이 따라갈 대상 (플레이어)")]
        [SerializeField] private Transform _focus;

        private IScreenFade _fade;
        private bool _leaving;

        private void Awake()
        {
            _fade = _fadeView as IScreenFade;
            if (_fade == null) { Debug.LogError("[SceneTransition] 페이드 화면이 연결되지 않음", this); return; }
            _fade.Configure(_config);
            _fade.SetFade(SceneTravel.IsTraveling ? 1f : 0f, Center());
        }

        private void Start()
        {
            if (SceneTravel.IsTraveling && !_leaving) StartCoroutine(FadeIn());
        }

        /// <summary>이동을 시작했으면 true. 이미 이동 중이거나 씬이 Build Settings에 없으면 false.</summary>
        public bool TravelTo(string sceneName, string arrivalSpawnId)
        {
            if (_leaving || _fade == null) return false;
            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError($"[SceneTransition] '{sceneName}' 씬이 Build Settings 씬 목록에 없습니다.", this);
                return false;
            }
            if (!SceneTravel.Begin(arrivalSpawnId)) return false;
            _leaving = true;
            StartCoroutine(FadeOutAndLoad(sceneName));
            return true;
        }

        private IEnumerator FadeOutAndLoad(string sceneName)
        {
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            yield return Animate(0f, 1f, _config.FadeOutDuration);
            if (_config.HoldDuration > 0f) yield return new WaitForSecondsRealtime(_config.HoldDuration);

            var load = SceneManager.LoadSceneAsync(sceneName);
            while (!load.isDone) yield return null;

            // 새 씬에 진행자가 없으면 페이드 인을 이어받을 곳이 없으니 여기서 입력을 풀어 준다.
            bool handedOver = false;
            foreach (var other in FindObjectsByType<SceneTransitionDirector>(FindObjectsSortMode.None))
                if (other != this) handedOver = true;
            if (!handedOver) SceneTravel.End();
            Destroy(gameObject);
        }

        private IEnumerator FadeIn()
        {
            yield return null; // 카메라·플레이어가 도착 위치로 옮겨진 다음 프레임부터
            yield return Animate(1f, 0f, _config.FadeInDuration);
            SceneTravel.End();
        }

        private IEnumerator Animate(float from, float to, float duration)
        {
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                _fade.SetFade(Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / duration)), Center());
                yield return null;
            }
            _fade.SetFade(to, Center());
        }

        private Vector2 Center()
        {
            var camera = Camera.main;
            if (_config == null || !_config.CenterOnPlayer || _focus == null || camera == null) return new Vector2(0.5f, 0.5f);
            Vector3 viewport = camera.WorldToViewportPoint(_focus.position);
            if (viewport.z < 0f) return new Vector2(0.5f, 0.5f);
            return new Vector2(Mathf.Clamp01(viewport.x), Mathf.Clamp01(viewport.y));
        }
    }
}
