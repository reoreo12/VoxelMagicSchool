// 대사 재생 창구 검사(UI_SPEC 6-1): 결과는 마지막 대사 뒤(Finish)에만 한 번 반영되고, 도중에 버리면(Abort) 반영되지 않는다. 대사 화면은 ESC로 닫히지 않는다.
using NUnit.Framework;
using VoxelMagic.Core;

namespace VoxelMagic.Tests
{
    public sealed class DialogueChannelTests
    {
        private static DialogueLine[] Lines() => new[] { new DialogueLine(null, "첫 줄"), new DialogueLine(null, "둘째 줄") };

        [Test]
        public void 결과는_Finish에서_한_번만_반영된다()
        {
            var channel = new DialogueChannel();
            int applied = 0;
            Assert.IsTrue(channel.Play(Lines(), () => applied++));
            Assert.AreEqual(0, applied);
            channel.Finish();
            channel.Finish();
            Assert.AreEqual(1, applied);
            Assert.IsFalse(channel.IsPlaying);
        }

        [Test]
        public void 도중에_버리면_결과가_반영되지_않는다()
        {
            var channel = new DialogueChannel();
            int applied = 0;
            channel.Play(Lines(), () => applied++);
            channel.Abort();
            channel.Finish();
            Assert.AreEqual(0, applied);
        }

        [Test]
        public void 재생_중에는_새_대사를_받지_않는다()
        {
            var channel = new DialogueChannel();
            Assert.IsTrue(channel.Play(Lines(), null));
            Assert.IsFalse(channel.Play(Lines(), null));
            Assert.IsFalse(new DialogueChannel().Play(new DialogueLine[0], null));
        }

        [Test]
        public void 대사_화면은_ESC로_닫히지_않고_게임_시계만_멈춘다()
        {
            var pause = new TimePause();
            var stack = new ScreenStack(pause);
            stack.Open(GameScreen.Dialogue);
            Assert.IsTrue(stack.Cancel());
            Assert.IsTrue(stack.IsOpen(GameScreen.Dialogue));
            Assert.IsTrue(pause.IsClockPaused);
            Assert.IsFalse(pause.IsFullyPaused);
        }
    }
}
