// 미니게임 속 물고기의 위아래 움직임. 데이터의 속도·변덕·급이동 확률로 목표 지점을 바꿔 가며 움직인다.
using UnityEngine;

namespace VoxelMagic.Core
{
    public sealed class FishMotion
    {
        private readonly FishDefinition _fish;
        private readonly IRandom _random;
        private float _target;
        private float _retargetTimer;
        private bool _darting;

        public FishMotion(FishDefinition fish, IRandom random, float startPosition)
        {
            _fish = fish;
            _random = random;
            Position = _target = Mathf.Clamp01(startPosition);
            _retargetTimer = fish.RetargetIntervalSeconds.Roll(random);
        }

        /// <summary>바 안에서의 위치 (0 = 아래, 1 = 위).</summary>
        public float Position { get; private set; }

        public void Tick(float deltaTime)
        {
            _retargetTimer -= deltaTime;
            if (_retargetTimer <= 0f)
            {
                _target = _random.Value;
                _darting = _random.Value < _fish.DartChance;
                _retargetTimer = _fish.RetargetIntervalSeconds.Roll(_random);
            }

            float speed = _fish.MoveSpeed * (_darting ? _fish.DartSpeedMultiplier : 1f);
            Position = Mathf.MoveTowards(Position, _target, speed * deltaTime);
            if (Mathf.Approximately(Position, _target)) _darting = false;
        }
    }
}
