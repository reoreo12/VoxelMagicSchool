// 도트 렌더링 상태 단위 테스트: 켜기·끄기, 도트 크기 범위 제한, 바뀔 때만 이벤트.
using NUnit.Framework;
using VoxelMagic.Core;

namespace VoxelMagic.Tests
{
    public class PixelRenderStateTests
    {
        [Test]
        public void StartsWithGivenValues_SizeClamped()
        {
            var state = new PixelRenderState(true, 20, 2, 8);
            Assert.IsTrue(state.IsOn);
            Assert.AreEqual(8, state.Size);
        }

        [Test]
        public void Toggle_FlipsAndNotifies()
        {
            var state = new PixelRenderState(false, 4, 2, 8);
            int changes = 0;
            state.Changed += () => changes++;
            state.Toggle();
            Assert.IsTrue(state.IsOn);
            state.Toggle();
            Assert.IsFalse(state.IsOn);
            Assert.AreEqual(2, changes);
        }

        [Test]
        public void SetSize_ClampsToRange()
        {
            var state = new PixelRenderState(false, 4, 2, 8);
            state.SetSize(1);
            Assert.AreEqual(2, state.Size);
            state.SetSize(99);
            Assert.AreEqual(8, state.Size);
        }

        [Test]
        public void SameValue_DoesNotNotify()
        {
            var state = new PixelRenderState(true, 4, 2, 8);
            int changes = 0;
            state.Changed += () => changes++;
            state.SetOn(true);
            state.SetSize(4);
            state.SetSize(100);
            state.SetSize(50);
            Assert.AreEqual(1, changes, "8로 바뀐 한 번만 알림");
        }
    }
}
