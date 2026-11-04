// 카메라 따라가기 방식(부드럽게/고정) 전환 상태 단위 테스트.
using NUnit.Framework;
using VoxelMagic.Core;

namespace VoxelMagic.Tests
{
    public class CameraFollowTests
    {
        [Test]
        public void Toggle_SwitchesModeAndNotifiesOnce()
        {
            var state = new CameraFollowState(CameraFollowMode.Locked);
            int changes = 0;
            state.Changed += () => changes++;
            state.Toggle();
            Assert.IsFalse(state.IsLocked);
            state.SetLocked(false);
            Assert.AreEqual(1, changes, "같은 값으로는 알리지 않음");
            state.Toggle();
            Assert.IsTrue(state.IsLocked);
            Assert.AreEqual(2, changes);
        }
    }
}
