// 복도 카메라가 플레이어를 부드럽게(늦게) 따라갈지, 가운데에 고정해 따라갈지. 입력(C 키)과 화면 토글이 같은 상태를 바꾸고, 바뀌면 이벤트로 알린다.
using System;

namespace VoxelMagic.Core
{
    public enum CameraFollowMode
    {
        Smooth,
        Locked
    }

    public sealed class CameraFollowState
    {
        public CameraFollowState(CameraFollowMode mode) => Mode = mode;

        public event Action Changed;

        public CameraFollowMode Mode { get; private set; }
        public bool IsLocked => Mode == CameraFollowMode.Locked;

        public void Set(CameraFollowMode mode)
        {
            if (Mode == mode) return;
            Mode = mode;
            Changed?.Invoke();
        }

        public void SetLocked(bool locked) => Set(locked ? CameraFollowMode.Locked : CameraFollowMode.Smooth);

        public void Toggle() => SetLocked(!IsLocked);
    }
}
