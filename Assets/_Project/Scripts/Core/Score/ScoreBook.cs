// 시험 예상 점수(실기, 필기). 상한 없이 쌓고(QUEST_TIMER 2-7, 통과 기준은 시스템만 앎), 바뀌면 이벤트로 알린다.
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

        public int Practical => _practical;
        public int Written => _written;

        public int Get(ScoreKind kind) => kind == ScoreKind.Practical ? _practical : _written;

        /// <summary>점수를 더하고 실제로 오른 양을 돌려준다.</summary>
        public int AddWritten(int amount) => Add(ScoreKind.Written, ref _written, amount);
        public int AddPractical(int amount) => Add(ScoreKind.Practical, ref _practical, amount);

        /// <summary>저장 파일에서 불러온 값으로 맞춘다 (치트 점수 설정도).</summary>
        public void Restore(int practical, int written)
        {
            Set(ScoreKind.Practical, ref _practical, practical);
            Set(ScoreKind.Written, ref _written, written);
        }

        private void Set(ScoreKind kind, ref int value, int next)
        {
            next = Math.Max(0, next);
            if (next == value) return;
            int before = value;
            value = next;
            ScoreChanged?.Invoke(kind, before, value);
        }

        private int Add(ScoreKind kind, ref int value, int amount)
        {
            if (amount <= 0) return 0;
            int before = value;
            value += amount;
            if (value != before) ScoreChanged?.Invoke(kind, before, value);
            return value - before;
        }
    }
}
