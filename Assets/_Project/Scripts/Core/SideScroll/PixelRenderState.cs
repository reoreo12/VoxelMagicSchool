// 도트 렌더링 켜짐 여부와 도트 크기. 입력(P 키)과 화면 UI(토글·슬라이더)가 같은 상태를 바꾸고, 바뀌면 이벤트로 알린다.
using System;

namespace VoxelMagic.Core
{
    public sealed class PixelRenderState
    {
        public PixelRenderState(bool isOn, int size, int minSize, int maxSize)
        {
            MinSize = Math.Max(1, minSize);
            MaxSize = Math.Max(MinSize, maxSize);
            IsOn = isOn;
            Size = Clamp(size);
        }

        public event Action Changed;

        public bool IsOn { get; private set; }
        public int Size { get; private set; }
        public int MinSize { get; }
        public int MaxSize { get; }

        public void SetOn(bool on)
        {
            if (IsOn == on) return;
            IsOn = on;
            Changed?.Invoke();
        }

        public void Toggle() => SetOn(!IsOn);

        public void SetSize(int size)
        {
            int clamped = Clamp(size);
            if (Size == clamped) return;
            Size = clamped;
            Changed?.Invoke();
        }

        private int Clamp(int size) => Math.Min(Math.Max(size, MinSize), MaxSize);
    }
}
