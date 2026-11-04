// 횡스크롤 계산식 모음: 패럴랙스 위치, 배경 반복 배치, 카메라 x 제한, 직교 크기, 점 조명 배치. 단위 테스트 대상.
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

        /// <summary>size×size 도트 원 그림에서 (x, y) 픽셀의 밝기. 가운데 1, 바깥으로 갈수록 steps 단계로 끊겨 낮아지고, 원 밖은 0.</summary>
        public static float DotCircleLevel(int x, int y, int size, int steps)
        {
            if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size));
            if (steps <= 0) throw new ArgumentOutOfRangeException(nameof(steps));
            float center = (size - 1) * 0.5f;
            float dx = x - center, dy = y - center;
            float r = (float)Math.Sqrt(dx * dx + dy * dy) / (size * 0.5f);
            return r >= 1f ? 0f : (float)Math.Ceiling((1f - r) * steps) / steps;
        }

        /// <summary>복도 양 끝에서 inset만큼 안쪽 구간에 점 조명 x를 고르게 놓는다. 간격은 spacing 이하로, 양 끝에 하나씩.</summary>
        public static float[] LampPositions(CorridorBounds bounds, float spacing, float inset)
        {
            if (spacing <= 0f) throw new ArgumentOutOfRangeException(nameof(spacing));
            float start = bounds.MinX + inset;
            float span = bounds.MaxX - inset - start;
            if (span <= 0f) return new[] { bounds.CenterX };

            int count = (int)Math.Ceiling(span / spacing) + 1;
            var positions = new float[count];
            for (int i = 0; i < count; i++)
                positions[i] = start + span * i / (count - 1);
            return positions;
        }
    }
}
