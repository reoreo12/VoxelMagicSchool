// 심는 방향(격자 네 방향)과 "플레이어 앞 칸에서 바라보는 쪽으로 펼쳐 심기" 계산. 씨앗의 칸 크기는 Up 방향 기준이고, 옆을 보면 가로세로가 바뀐다.
using UnityEngine;

namespace VoxelMagic.Core
{
    /// <summary>격자 기준 방향. Up = +y(월드 +z), Right = +x. 값 0(Up)이 예전 저장의 "안 돌림"이다.</summary>
    public enum FarmFacing
    {
        Up,
        Right,
        Down,
        Left
    }

    public static class FarmPlacement
    {
        /// <summary>평면 방향(x, 격자 y)에서 가장 가까운 네 방향.</summary>
        public static FarmFacing FromDirection(Vector2 direction)
        {
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                return direction.x >= 0f ? FarmFacing.Right : FarmFacing.Left;
            return direction.y >= 0f ? FarmFacing.Up : FarmFacing.Down;
        }

        /// <summary>그 방향으로 한 칸.</summary>
        public static Vector2Int Step(FarmFacing facing) => facing switch
        {
            FarmFacing.Right => Vector2Int.right,
            FarmFacing.Down => Vector2Int.down,
            FarmFacing.Left => Vector2Int.left,
            _ => Vector2Int.up
        };

        /// <summary>모양을 세로축 기준으로 돌리는 각도 (Up 0, Right 90, Down 180, Left 270).</summary>
        public static float Yaw(FarmFacing facing) => (int)facing * 90f;

        /// <summary>그 방향으로 돌린 칸 크기 (옆을 보면 가로세로가 바뀜).</summary>
        public static Vector2Int RotatedSize(Vector2Int footprint, FarmFacing facing)
            => facing == FarmFacing.Right || facing == FarmFacing.Left ? new Vector2Int(footprint.y, footprint.x) : footprint;

        /// <summary>
        /// 앞 칸(frontCell)에서 바라보는 쪽으로 펼친 영역의 시작 칸(x, y가 가장 작은 칸). 옆으로 두 칸 이상이면
        /// lateralPositive가 참일 때 +축 쪽(Up·Down은 +x, Right·Left는 +y)으로, 거짓이면 -축 쪽으로 붙인다.
        /// </summary>
        public static Vector2Int Origin(Vector2Int frontCell, Vector2Int rotatedSize, FarmFacing facing, bool lateralPositive)
        {
            var origin = frontCell;
            switch (facing)
            {
                case FarmFacing.Up:
                case FarmFacing.Down:
                    if (facing == FarmFacing.Down) origin.y -= rotatedSize.y - 1;
                    if (!lateralPositive) origin.x -= rotatedSize.x - 1;
                    break;
                default:
                    if (facing == FarmFacing.Left) origin.x -= rotatedSize.x - 1;
                    if (!lateralPositive) origin.y -= rotatedSize.y - 1;
                    break;
            }
            return origin;
        }
    }
}
