// 횡스크롤 계산식 모음: 패럴랙스 위치, 배경 반복 배치, 카메라 x 제한, 직교 크기. 단위 테스트 대상.
using System;

namespace VoxelMagic.Core
{
    public static class SideScrollMath
    {
        /// <summary>계수 1 = 월드에 고정, 0 = 카메라에 붙음. 화면에서 보이는 이동량 = 카메라 이동량 × 계수.</summary>
        public static float ParallaxOriginX(float cameraX, float factor, float anchorX = 0f) =>
            anchorX + cameraX * (1f - factor);

        /// <summary>폭 width인 이미지를 originX 기준으로 반복할 때, viewLeft를 덮는 첫 장의 왼쪽 끝 x.</summary>
        public static float FirstLoopLeft(float originX, float width, float viewLeft)
        {
            if (width <= 0f) throw new ArgumentOutOfRangeException(nameof(width));
            return originX + (float)Math.Floor((viewLeft - originX) / width) * width;
        }

        /// <summary>카메라 중심 x를 복도 안으로 제한한다. 화면이 복도보다 넓으면 복도 가운데.</summary>
        public static float ClampCameraX(float targetX, CorridorBounds bounds, float halfViewWidth)
        {
            float min = bounds.MinX + halfViewWidth;
            float max = bounds.MaxX - halfViewWidth;
            if (min > max) return bounds.CenterX;
            return Math.Min(Math.Max(targetX, min), max);
        }

        /// <summary>화면 세로에 height가 꼭 맞는 직교 크기 (세로의 절반).</summary>
        public static float OrthographicSizeFor(float height) => height * 0.5f;
    }
}
