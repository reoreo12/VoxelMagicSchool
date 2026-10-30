// System.Random을 쓰는 IRandom 기본 구현.
using System;

namespace VoxelMagic.Core
{
    public sealed class SystemRandom : IRandom
    {
        private readonly Random _random;

        public SystemRandom() : this(Environment.TickCount) { }
        public SystemRandom(int seed) { _random = new Random(seed); }

        public float Value => (float)_random.NextDouble();
        public float Range(float min, float max) => min + (max - min) * Value;
        public int Range(int minInclusive, int maxExclusive) => _random.Next(minInclusive, maxExclusive);
    }
}
