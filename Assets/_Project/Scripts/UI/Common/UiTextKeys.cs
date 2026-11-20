// UI 문구 표(TextTable)의 키 이름 모음. 화면 코드는 문자열 대신 이 키로 문구를 가져온다(UI_SPEC 4-A-3).
namespace VoxelMagic.UI
{
    public static class UiTextKeys
    {
        /// <summary>"수락한 의뢰({0})" — {0}은 의뢰 개수 (10-1). "의뢰"와 괄호 사이 띄어쓰기 없음.</summary>
        public const string QuestBarTitle = "ui.quest.bar.title";
        /// <summary>"필기 예상 점수" (9-1)</summary>
        public const string ScoreWritten = "ui.score.written";
        /// <summary>"실기 예상 점수" (9-1)</summary>
        public const string ScorePractical = "ui.score.practical";
        /// <summary>"방과후" (8-1)</summary>
        public const string TimeAfterSchool = "ui.time.afterSchool";
        /// <summary>"밤" (8-1)</summary>
        public const string TimeNight = "ui.time.night";
        /// <summary>"{0} {1}:00" — {0} 시간대 이름, {1} 두 자리 시 (8-1 툴팁, 분은 표시하지 않음)</summary>
        public const string TimeTooltip = "ui.time.tooltip";
        /// <summary>"{0}일째" — 날짜 패널 (8-1 [미정], 임시)</summary>
        public const string TimeDay = "ui.time.day";
        /// <summary>"멈춰 있음" — 개발 빌드에서만 (8-2)</summary>
        public const string TimePaused = "ui.time.paused";
    }
}
