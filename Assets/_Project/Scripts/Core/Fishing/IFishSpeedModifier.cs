// 미니게임 물고기 속도 배율 자리(QUEST_TIMER 3-3). 나중에 주문 "느려져라"가 구현해 FishingSession.SpeedModifier에 넣는다. 없으면 배율 1.
namespace VoxelMagic.Core
{
    public interface IFishSpeedModifier
    {
        /// <summary>이 물고기의 이동 속도에 곱할 값 (1 = 그대로, 0.5 = 절반).</summary>
        float SpeedMultiplier(FishDefinition fish);
    }
}
