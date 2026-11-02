// 횡스크롤 계산식 단위 테스트: 패럴랙스 계수, 배경 반복 배치에 빈틈 없음, 카메라 x 제한, 직교 크기.
using NUnit.Framework;
using VoxelMagic.Core;

namespace VoxelMagic.Tests
{
    public class SideScrollMathTests
    {
        private const float Tolerance = 0.0001f;
        private static readonly CorridorBounds Corridor = new CorridorBounds(0f, 35.84f, 2.88f);

        [TestCase(1f, 0f)]      // 월드에 고정: 레이어가 움직이지 않음
        [TestCase(0f, 10f)]     // 카메라에 붙음: 카메라만큼 같이 움직임
        [TestCase(0.3f, 7f)]    // 화면에서는 카메라 이동의 30%만큼 지나감
        public void ParallaxOrigin_MovesByOneMinusFactor(float factor, float expected)
        {
            Assert.AreEqual(expected, SideScrollMath.ParallaxOriginX(10f, factor), Tolerance);
        }

        [Test]
        public void ParallaxOrigin_ScreenMotionEqualsFactor()
        {
            const float factor = 0.3f;
            float screenBefore = SideScrollMath.ParallaxOriginX(0f, factor) - 0f;
            float screenAfter = SideScrollMath.ParallaxOriginX(4f, factor) - 4f;
            Assert.AreEqual(-4f * factor, screenAfter - screenBefore, Tolerance);
        }

        [TestCase(0f, -2.56f)]
        [TestCase(3.5f, 1.2f)]
        [TestCase(-20f, 50f)]
        public void FirstLoopLeft_CoversViewLeftWithoutGap(float origin, float viewLeft)
        {
            const float width = 8.96f;
            float left = SideScrollMath.FirstLoopLeft(origin, width, viewLeft);
            Assert.LessOrEqual(left, viewLeft + Tolerance);
            Assert.Greater(left + width, viewLeft);
            float steps = (left - origin) / width;
            Assert.AreEqual(System.Math.Round(steps), steps, Tolerance, "반복 간격은 이미지 폭의 정수배");
        }

        [Test]
        public void ClampCameraX_StaysInsideCorridor()
        {
            const float half = 2.56f;
            Assert.AreEqual(half, SideScrollMath.ClampCameraX(-5f, Corridor, half), Tolerance);
            Assert.AreEqual(35.84f - half, SideScrollMath.ClampCameraX(100f, Corridor, half), Tolerance);
            Assert.AreEqual(10f, SideScrollMath.ClampCameraX(10f, Corridor, half), Tolerance);
        }

        [Test]
        public void ClampCameraX_CentersWhenViewWiderThanCorridor()
        {
            Assert.AreEqual(Corridor.CenterX, SideScrollMath.ClampCameraX(3f, Corridor, 30f), Tolerance);
        }

        [Test]
        public void OrthographicSize_FitsHeight()
        {
            Assert.AreEqual(1.44f, SideScrollMath.OrthographicSizeFor(2.88f), Tolerance);
        }
    }
}
