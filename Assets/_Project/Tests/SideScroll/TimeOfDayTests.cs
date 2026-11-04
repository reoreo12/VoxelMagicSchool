// 복도 낮/밤 전환 상태, 밤 조명 배치, 바닥 빛 도트 원 계산 단위 테스트.
using NUnit.Framework;
using VoxelMagic.Core;

namespace VoxelMagic.Tests
{
    public class TimeOfDayTests
    {
        [Test]
        public void Toggle_SwitchesAndNotifiesOnce()
        {
            var state = new TimeOfDayState(TimeOfDay.Day);
            int changes = 0;
            state.Changed += () => changes++;
            state.Toggle();
            Assert.IsTrue(state.IsNight);
            state.SetNight(true);
            Assert.AreEqual(1, changes, "같은 값으로는 알리지 않음");
            state.Toggle();
            Assert.IsFalse(state.IsNight);
            Assert.AreEqual(2, changes);
        }

        [Test]
        public void LampPositions_EvenlySpacedWithinInset()
        {
            var bounds = new CorridorBounds(0f, 20f, 3f);
            var xs = SideScrollMath.LampPositions(bounds, 4f, 2f);
            Assert.AreEqual(5, xs.Length, "구간 16 ÷ 간격 4 → 5개");
            Assert.AreEqual(2f, xs[0], 1e-4f);
            Assert.AreEqual(18f, xs[xs.Length - 1], 1e-4f);
            for (int i = 1; i < xs.Length; i++)
                Assert.LessOrEqual(xs[i] - xs[i - 1], 4f + 1e-4f);
        }

        [Test]
        public void LampPositions_SpacingNotDividing_StaysUnderSpacing()
        {
            var xs = SideScrollMath.LampPositions(new CorridorBounds(0f, 10f, 3f), 4f, 0f);
            Assert.AreEqual(4, xs.Length, "구간 10 ÷ 간격 4 → 올림 3칸 → 4개");
            Assert.AreEqual(10f / 3f, xs[1] - xs[0], 1e-4f);
        }

        [Test]
        public void DotCircleLevel_SteppedFromCenterToEdge()
        {
            const int size = 32, steps = 4;
            Assert.AreEqual(1f, SideScrollMath.DotCircleLevel(16, 16, size, steps), 1e-4f, "가운데는 가장 밝음");
            Assert.AreEqual(0f, SideScrollMath.DotCircleLevel(0, 0, size, steps), "모서리는 원 밖");
            float previous = 1f;
            for (int x = 16; x < size; x++)
            {
                float level = SideScrollMath.DotCircleLevel(x, 16, size, steps);
                Assert.LessOrEqual(level, previous, "바깥으로 갈수록 어두워짐");
                Assert.AreEqual(0f, level * steps % 1f, 1e-4f, "단계 값만 나옴");
                previous = level;
            }
        }

        [Test]
        public void LampPositions_NoSpan_SingleCenter()
        {
            var xs = SideScrollMath.LampPositions(new CorridorBounds(0f, 2f, 3f), 4f, 1.5f);
            Assert.AreEqual(1, xs.Length);
            Assert.AreEqual(1f, xs[0], 1e-4f);
        }
    }
}
