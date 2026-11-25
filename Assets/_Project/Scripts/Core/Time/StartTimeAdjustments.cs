// "다음 날 방과 후를 N분 늦게/일찍" 요청을 원인별로 모은다. 합칠 때는 늦추기 중 가장 큰 값 + 앞당기기 중 가장 큰 값을 범위로 자른다.
// 같은 원인이 다시 요청하면 덮어쓴다(벌이 두 번 겹치지 않게). 다음 날을 시작할 때 Consume으로 쓰고 비운다.
using System;
using System.Collections.Generic;

namespace VoxelMagic.Core
{
    public sealed class StartTimeAdjustments
    {
        public const string GuardSource = "경비원";

        private readonly Dictionary<string, int> _requests = new Dictionary<string, int>();

        public event Action Changed;

        public IReadOnlyDictionary<string, int> Requests => _requests;

        public void Request(string source, int minutes)
        {
            if (string.IsNullOrEmpty(source)) return;
            _requests[source] = minutes;
            Changed?.Invoke();
        }

        public int Combined(int minMinutes, int maxMinutes)
        {
            int later = 0, earlier = 0;
            foreach (int minutes in _requests.Values)
            {
                if (minutes > later) later = minutes;
                if (minutes < earlier) earlier = minutes;
            }
            return Math.Max(minMinutes, Math.Min(maxMinutes, later + earlier));
        }

        public int Consume(int minMinutes, int maxMinutes)
        {
            int result = Combined(minMinutes, maxMinutes);
            Clear();
            return result;
        }

        public void Clear()
        {
            if (_requests.Count == 0) return;
            _requests.Clear();
            Changed?.Invoke();
        }
    }
}
