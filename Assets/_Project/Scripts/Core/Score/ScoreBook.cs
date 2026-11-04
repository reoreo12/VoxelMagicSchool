// 시험 예상 점수(실기, 필기). 최대값을 넘지 않게 더하고, 바뀌면 이벤트로 알린다.
using System;

namespace VoxelMagic.Core
{
    public enum ScoreKind
    {
        Practical,
        Written
    }

    public sealed class ScoreBook
    {
        private int _practical;
        private int _written;

        /// <summary>(종류, 이전 값, 새 값)</summary>
        public event Action<ScoreKind, int, int> ScoreChanged;

        public ScoreBook(int maxScore)
        {
            if (maxScore < 1) throw new ArgumentOutOfRangeException(nameof(maxScore));
            MaxScore = maxScore;
        }

        public int MaxScore { get; }
        public int Practical => _practical;
        public int Written => _written;

        public int Get(ScoreKind kind) => kind == ScoreKind.Practical ? _practical : _written;

        /// <summary>점수를 더하고 실제로 오른 양을 돌려준다.</summary>
        public int AddWritten(int amount) => Add(ScoreKind.Written, ref _written, amount);
        public int AddPractical(int amount) => Add(ScoreKind.Practical, ref _practical, amount);

        private int Add(ScoreKind kind, ref int value, int amount)
        {
            if (amount <= 0) return 0;
            int before = value;
            value = Math.Min(MaxScore, value + amount);
            if (value != before) ScoreChanged?.Invoke(kind, before, value);
            return value - before;
        }
    }
}
