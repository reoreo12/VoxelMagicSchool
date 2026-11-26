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

        [Test]
        public void 선택지를_고르면_번호를_알리고_콜백에서_다음_대사를_이어_열_수_있다()
        {
            var channel = new DialogueChannel();
            int chosen = -1, started = 0;
            channel.Started += () => started++;
            var options = new[] { new DialogueOption("가"), new DialogueOption("나", "+4"), new DialogueOption("그만", dimmed: true) };
            Assert.IsTrue(channel.Ask(new DialogueLine(null, "무슨 일이야?"), options, i =>
            {
                chosen = i;
                channel.Play(Lines(), null); // 같은 대사창에서 이어짐
            }, cancelOption: 2));
            Assert.IsTrue(channel.IsAsking);
            Assert.AreEqual(2, channel.CancelOption);
            channel.Finish(); // 선택지 중에는 Finish로 끝나지 않는다
            Assert.IsTrue(channel.IsAsking);

            channel.Choose(1);
            Assert.AreEqual(1, chosen);
            Assert.IsTrue(channel.IsPlaying);
            Assert.IsFalse(channel.IsAsking);
            Assert.AreEqual(2, started);
        }

        [Test]
        public void 선택지_도중_버려지면_중단_콜백만_부른다()
        {
            var channel = new DialogueChannel();
            int chosen = 0, aborted = 0;
            channel.Ask(new DialogueLine(null, "?"), new[] { new DialogueOption("가") }, _ => chosen++, onAborted: () => aborted++);
            channel.Abort();
            channel.Choose(0);
            Assert.AreEqual(0, chosen);
            Assert.AreEqual(1, aborted);
            Assert.IsFalse(channel.IsPlaying);
        }

        [Test]
        public void ESC로_닫히지_않는_화면이면_무시_알림을_보내고_위아래_이동을_알린다()
        {
            var stack = new ScreenStack(new TimePause());
            int ignored = 0, moved = 0;
            stack.CancelIgnored += () => ignored++;
            stack.Navigated += step => moved += step;
            stack.Navigate(1); // 화면이 없으면 알리지 않음
            stack.Open(GameScreen.Dialogue);
            stack.Cancel();
            stack.Navigate(1);
            stack.Navigate(-1);
            stack.Navigate(1);
            Assert.AreEqual(1, ignored);
            Assert.AreEqual(1, moved);
        }
    }
}
