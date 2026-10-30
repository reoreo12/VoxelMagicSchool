// 낚시 미니게임 계산. 잡기 바와 물고기가 겹치면 게이지가 차고, 벗어나면 줄어든다.
using UnityEngine;

namespace VoxelMagic.Core
{
    public enum MinigameStatus
    {
        Running,
        Succeeded,
        Failed
    }

    public sealed class FishingMinigame
    {
        private readonly FishMotion _motion;
        private readonly float _fillPerSecond;
        private readonly float _drainPerSecond;

        public FishingMinigame(FishingConfig config, FishDefinition fish, IRandom random)
        {
            Fish = fish;
            BarSize = config.CatchBarSize;
            BarCenter = 0.5f;
            Gauge = config.GaugeStart;
            _fillPerSecond = config.GaugeFillPerSecond;
            _drainPerSecond = config.GaugeDrainPerSecond;
            _motion = new FishMotion(fish, random, 0.5f);
        }

        public FishDefinition Fish { get; }
        public float FishPosition => _motion.Position;
        public float BarCenter { get; private set; }
        public float BarSize { get; }
        /// <summary>0 = 놓침, 1 = 잡기 성공.</summary>
        public float Gauge { get; private set; }
        public bool IsFishInBar => Mathf.Abs(FishPosition - BarCenter) <= BarSize * 0.5f;

        public void MoveBar(float delta)
        {
            float half = BarSize * 0.5f;
            BarCenter = Mathf.Clamp(BarCenter + delta, half, 1f - half);
        }

        public MinigameStatus Tick(float deltaTime)
        {
            _motion.Tick(deltaTime);
            float rate = IsFishInBar ? _fillPerSecond : -_drainPerSecond;
            Gauge = Mathf.Clamp01(Gauge + rate * deltaTime);

            if (Gauge >= 1f) return MinigameStatus.Succeeded;
            if (Gauge <= 0f) return MinigameStatus.Failed;
            return MinigameStatus.Running;
        }
    }
}
