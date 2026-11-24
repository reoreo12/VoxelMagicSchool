// 화면 스택·시간 정지 참조 카운트·HUD 상태·문구 표 검사 (UI_SPEC 4-A-2, 4-A-3, 13장).
using NUnit.Framework;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Tests
{
    public sealed class ScreenStackTests
    {
        private TimePause _pause;
        private ScreenStack _stack;

        [SetUp]
        public void SetUp()
        {
            _pause = new TimePause();
            _stack = new ScreenStack(_pause);
        }

        [Test]
        public void 책을_열면_전체_정지되고_닫으면_풀린다()
        {
            _stack.Open(GameScreen.Book);
            Assert.IsTrue(_pause.IsFullyPaused);
            Assert.IsTrue(_pause.IsClockPaused);
            _stack.Close(GameScreen.Book);
            Assert.IsFalse(_pause.IsFullyPaused);
            Assert.IsFalse(_pause.IsClockPaused);
        }

        [Test]
        public void 대화는_게임_시계만_멈춘다()
        {
            _stack.Open(GameScreen.Dialogue);
            Assert.IsTrue(_pause.IsClockPaused);
            Assert.IsFalse(_pause.IsFullyPaused);
        }

        [Test]
        public void 겹쳐_연_화면은_마지막이_닫힐_때만_시간이_흐른다()
        {
            _stack.Open(GameScreen.Book);
            _stack.Open(GameScreen.RequestDetail);
            _stack.Close(GameScreen.RequestDetail);
            Assert.IsTrue(_pause.IsFullyPaused);
            _stack.Close(GameScreen.Book);
            Assert.IsFalse(_pause.IsFullyPaused);
        }

        [Test]
        public void ESC는_맨_위부터_닫고_비어_있으면_false()
        {
            _stack.Open(GameScreen.Book);
            _stack.Open(GameScreen.RequestDetail);
            Assert.IsTrue(_stack.Cancel());
            Assert.AreEqual(GameScreen.Book, _stack.Top);
            Assert.IsTrue(_stack.Cancel());
            Assert.IsFalse(_stack.HasAny);
            Assert.IsFalse(_stack.Cancel());
        }

        [Test]
        public void ESC를_받지_않는_화면은_닫히지_않는다()
        {
            _stack.OverridePolicy(GameScreen.Dialogue, new ScreenPolicy(PauseLevel.GameClock, false, false, true));
            _stack.Open(GameScreen.Dialogue);
            Assert.IsTrue(_stack.Cancel());
            Assert.IsTrue(_stack.IsOpen(GameScreen.Dialogue));
        }

        [Test]
        public void 메뉴_요청은_열린_화면이_없을_때만_보낸다()
        {
            int count = 0;
            _stack.MenuRequested += () => count++;
            _stack.RequestMenu();
            _stack.Open(GameScreen.RequestBoard);
            _stack.RequestMenu();
            Assert.AreEqual(1, count);
        }

        [Test]
        public void 같은_화면을_두_번_열어도_한_번만_센다()
        {
            Assert.IsTrue(_stack.Open(GameScreen.Inventory));
            Assert.IsFalse(_stack.Open(GameScreen.Inventory));
            _stack.Close(GameScreen.Inventory);
            Assert.IsFalse(_pause.IsFullyPaused);
        }

        [Test]
        public void 시간_정지_변화가_있을_때만_알린다()
        {
            int changes = 0;
            _pause.Changed += () => changes++;
            _stack.Open(GameScreen.Book);
            _stack.Open(GameScreen.Inventory);
            _stack.CloseAll();
            Assert.AreEqual(2, changes);
        }

        [Test]
        public void 의뢰_목록_토글()
        {
            var hud = new HudState();
            bool? last = null;
            hud.QuestListToggled += v => last = v;
            hud.ToggleQuestList();
            Assert.AreEqual(!true, hud.IsQuestListExpanded);
            Assert.AreEqual(false, last);
        }

        [TestCase(1080, 3)]
        [TestCase(1440, 4)]
        [TestCase(2160, 6)]
        [TestCase(720, 2)]
        [TestCase(768, 2)]
        [TestCase(1200, 3)]
        [TestCase(300, 1)]
        public void 정수_배율은_화면_높이를_기준_높이로_나눈_내림(int height, int expected)
        {
            Assert.AreEqual(expected, VoxelMagic.UI.PixelCanvasScaler.ComputeScale(height, 360, 1));
        }

        [Test]
        public void 문구_표는_키로_찾고_없으면_샵_키()
        {
            var table = ScriptableObject.CreateInstance<TextTable>();
            table.Set("ui.test", "시험");
            Assert.AreEqual("시험", table.Get("ui.test"));
            Assert.AreEqual("#ui.none", table.Get("ui.none"));
            Object.DestroyImmediate(table);
        }
    }
}
