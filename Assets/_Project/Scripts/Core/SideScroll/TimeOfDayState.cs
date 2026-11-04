// 복도가 낮인지 밤인지. 입력(N 키)과 화면 토글이 같은 상태를 바꾸고, 바뀌면 이벤트로 알린다.
using System;

namespace VoxelMagic.Core
{
    public enum TimeOfDay
    {
        Day,
        Night
    }

    public sealed class TimeOfDayState
    {
        public TimeOfDayState(TimeOfDay time) => Time = time;

        public event Action Changed;

        public TimeOfDay Time { get; private set; }
        public bool IsNight => Time == TimeOfDay.Night;

        public void Set(TimeOfDay time)
        {
            if (Time == time) return;
            Time = time;
            Changed?.Invoke();
        }

        public void SetNight(bool night) => Set(night ? TimeOfDay.Night : TimeOfDay.Day);

        public void Toggle() => SetNight(!IsNight);
    }
}
