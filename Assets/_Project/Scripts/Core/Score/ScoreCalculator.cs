// 의뢰 보상 계산: 기본 보상 + 처음 전달한 아이템 종류 수 × 보너스.
namespace VoxelMagic.Core
{
    public static class ScoreCalculator
    {
        public static int RequestReward(int baseReward, int newItemKindCount, int bonusPerNewKind)
            => System.Math.Max(0, baseReward) + System.Math.Max(0, newItemKindCount) * System.Math.Max(0, bonusPerNewKind);
    }
}
