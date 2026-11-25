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
        public const string DebugRespawn = "채집 자리 다시 뽑기";
        public const string DebugToggleTime = "방과후↔밤 (시각 이동)";
        public const string Capture = "포획";
        public const string CapturedFormat = "{0} 포획!";
        public const string Missed = "놓쳤다";
        public const string Drawing = "마법진을 그리는 중…";

        public const string DebugForceBite = "입질 즉시";
        public const string DebugForceSuccess = "다음 마법진 성공";
        public const string DebugForceFail = "다음 마법진 실패";
        public const string DebugForcedFormat = "마법진 강제: {0}";
        public static string ForcedName(ForcedCaptureResult forced)
            => forced == ForcedCaptureResult.Success ? "성공" : forced == ForcedCaptureResult.Fail ? "실패" : "없음";

    }
}
