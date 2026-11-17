// 횃불·촛불 불꽃. 불꽃 MeshFilter의 메시를 프레임 배열에서 갈아 끼우고(횃불=무작위, 촛불=가끔 바람이 스치는 순서 재생),
// 함께 둔 Point Light의 밝기·범위를 Perlin 노이즈로 살짝 흔든다.
using UnityEngine;

namespace VoxelMagic.Gameplay
{
    public class FlameFlicker : MonoBehaviour
    {
        public enum FlickerMode { Random, Sequence }

        [Header("불꽃 메시")]
        [SerializeField] private MeshFilter flameFilter;
        [SerializeField] private Mesh[] frames;
        [SerializeField] private FlickerMode mode = FlickerMode.Random;

        [Header("무작위 모드 (횃불)")]
        [SerializeField, Min(0.1f)] private float framesPerSecond = 8f;
        [SerializeField, Range(0f, 0.9f)] private float intervalJitter = 0.35f;
        [Tooltip("다음 프레임은 배열에서 이만큼 떨어진 칸까지만 고른다 (0 = 제한 없음). 배열을 기울기 순으로 두면 왼쪽 끝↔오른쪽 끝으로 튀지 않는다.")]
        [SerializeField, Min(0)] private int maxRandomStep = 2;

        [Header("시퀀스 모드 (촛불): F0 유지 후 F1→…→마지막→F0 한 번")]
        [SerializeField] private Vector2 idleSeconds = new Vector2(2f, 5f);
        [SerializeField] private Vector2 sequenceFrameSeconds = new Vector2(0.12f, 0.18f);

        [Header("조명 흔들림")]
        [SerializeField] private Light flameLight;
        [SerializeField, Range(0f, 1f)] private float intensityVariation = 0.15f;
        [SerializeField, Range(0f, 1f)] private float rangeVariation = 0.06f;
        [SerializeField, Min(0f)] private float noiseSpeed = 3f;

        [Header("바닥 빛 그림 (선택): 조명 밝기와 같은 노이즈로 투명도를 흔든다")]
        [SerializeField] private SpriteRenderer floorGlow;

        private float baseIntensity;
        private float baseRange;
        private float baseGlowAlpha;
        private float noiseSeed;
        private System.Random rng;
        private int currentFrame;
        private float timer;
        private bool inSequence;

        public FlickerMode Mode => mode;
        public int CurrentFrame => currentFrame;

        private void Awake()
        {
            rng = new System.Random(GetInstanceID() ^ System.Environment.TickCount);
            noiseSeed = (float)rng.NextDouble() * 1000f;
            if (flameLight != null)
            {
                baseIntensity = flameLight.intensity;
                baseRange = flameLight.range;
            }
            if (floorGlow != null) baseGlowAlpha = floorGlow.color.a;
        }

        private void OnEnable()
        {
            if (!HasFrames()) return;
            if (mode == FlickerMode.Random)
            {
                SetFrame(rng.Next(frames.Length));
                timer = NextRandomInterval() * (float)rng.NextDouble(); // 횃불마다 박자도 어긋나게
            }
            else
            {
                inSequence = false;
                SetFrame(0);
                timer = Range(idleSeconds) * (float)rng.NextDouble();
            }
        }

        private void Update()
        {
            if (HasFrames()) UpdateFrames(Time.deltaTime);
            UpdateLight();
        }

        private void UpdateFrames(float dt)
        {
            timer -= dt;
            if (timer > 0f) return;

            if (mode == FlickerMode.Random)
            {
                if (frames.Length > 1) SetFrame(PickRandomNeighbor());
                timer += NextRandomInterval();
                return;
            }

            // 시퀀스: F0 대기 → F1 … 마지막 → F0
            if (!inSequence)
            {
                inSequence = frames.Length > 1;
                SetFrame(inSequence ? 1 : 0);
                timer = inSequence ? Range(sequenceFrameSeconds) : Range(idleSeconds);
            }
            else if (currentFrame + 1 < frames.Length)
            {
                SetFrame(currentFrame + 1);
                timer = Range(sequenceFrameSeconds);
            }
            else
            {
                inSequence = false;
                SetFrame(0);
                timer = Range(idleSeconds);
            }
        }

        private void UpdateLight()
        {
            if (flameLight == null && floorGlow == null) return;
            float t = Time.time * noiseSpeed;
            float a = Mathf.PerlinNoise(noiseSeed, t) * 2f - 1f;
            float b = Mathf.PerlinNoise(t, noiseSeed + 37f) * 2f - 1f;
            if (flameLight != null)
            {
                flameLight.intensity = baseIntensity * (1f + a * intensityVariation);
                flameLight.range = baseRange * (1f + b * rangeVariation);
            }
            if (floorGlow != null)
            {
                var c = floorGlow.color;
                c.a = baseGlowAlpha * (1f + a * intensityVariation);
                floorGlow.color = c;
            }
        }

        private void SetFrame(int index)
        {
            currentFrame = index;
            if (flameFilter != null && frames[index] != null) flameFilter.sharedMesh = frames[index];
        }

        // 현재 칸 ±maxRandomStep 안에서 현재 칸을 뺀 하나 (같은 프레임 연속 금지).
        private int PickRandomNeighbor()
        {
            int step = maxRandomStep <= 0 ? frames.Length : maxRandomStep;
            int lo = Mathf.Max(0, currentFrame - step);
            int hi = Mathf.Min(frames.Length - 1, currentFrame + step);
            int next = lo + rng.Next(hi - lo); // [lo, hi-1]
            if (next >= currentFrame) next++;
            return next;
        }

        private float NextRandomInterval()
        {
            float j = 1f + ((float)rng.NextDouble() * 2f - 1f) * intervalJitter;
            return j / framesPerSecond;
        }

        private float Range(Vector2 r) => Mathf.Lerp(r.x, r.y, (float)rng.NextDouble());

        private bool HasFrames() => frames != null && frames.Length > 0;
    }
}
