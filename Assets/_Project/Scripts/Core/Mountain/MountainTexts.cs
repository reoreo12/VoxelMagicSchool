// 뒷산 화면 문구 모음 (채집·포획·디버그 안내).
namespace VoxelMagic.Core
{
    public static class MountainTexts
    {
        public const string Pick = "줍기";

        public const string AfterSchool = "방과후";
        public const string Night = "밤";
        public static string TimeName(TimeOfDay time) => time == TimeOfDay.Night ? Night : AfterSchool;

        public const string DebugHeader = "[디버그 키]";
        public const string DebugTimeFormat = "시간대: {0}";
        public const string DebugRespawn = "새 하루 (재료 다시 배치)";
        public const string DebugToggleTime = "시간대 전환";
        public const string DebugSwapRequest = "의뢰 교체";
        public const string DebugDiscardKey = "우클릭";
        public const string DebugDiscard = "인벤토리 칸 버리기";
        public const string RequestSwappedFormat = "의뢰 교체: {0}";
        public const string NoRequestToSwap = "바꿀 뒷산 의뢰가 없어요";
    }
}
