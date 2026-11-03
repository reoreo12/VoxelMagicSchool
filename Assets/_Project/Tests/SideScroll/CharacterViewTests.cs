// 2D/3D 캐릭터 전환 상태와 스프라이트 프레임 계산 단위 테스트.
using NUnit.Framework;
using VoxelMagic.Core;

namespace VoxelMagic.Tests
{
    public class CharacterViewTests
    {
        [Test]
        public void Toggle_SwitchesModeAndNotifiesOnce()
        {
            var state = new CharacterViewState(CharacterViewMode.TwoD);
            int changes = 0;
            state.Changed += () => changes++;
            state.Toggle();
            Assert.IsFalse(state.Is2D);
            state.Set2D(false);
            Assert.AreEqual(1, changes, "같은 값으로는 알리지 않음");
            state.Toggle();
            Assert.IsTrue(state.Is2D);
            Assert.AreEqual(2, changes);
        }

        [TestCase(0f, 0)]
        [TestCase(0.09f, 0)]
        [TestCase(0.1f, 1)]
        [TestCase(0.79f, 7)]
        [TestCase(0.8f, 0)]     // 반복: 8프레임 다음은 처음
        [TestCase(1.25f, 4)]
        public void FrameAt_Loops(float time, int expected)
        {
            Assert.AreEqual(expected, SpriteClipMath.FrameAt(time, 10f, 8, true));
        }

        [Test]
        public void FrameAt_HoldsLastFrame_WhenNotLooping()
        {
            Assert.AreEqual(7, SpriteClipMath.FrameAt(5f, 10f, 8, false));
        }

        [Test]
        public void FrameAt_NoFrames_ReturnsMinusOne()
        {
            Assert.AreEqual(-1, SpriteClipMath.FrameAt(1f, 10f, 0, true));
        }
    }
}
