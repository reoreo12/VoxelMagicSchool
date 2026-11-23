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
        /// <summary>"D-{0}" — 날짜 패널, {0} 남은 날 (8-1, 사용자 결정: 8일 플레이)</summary>
        public const string TimeDay = "ui.time.day";
        /// <summary>"멈춰 있음" — 개발 빌드에서만 (8-2)</summary>
        public const string TimePaused = "ui.time.paused";
        /// <summary>"의뢰 완료: {0}" — {0} 의뢰 제목 (10-6 완료 알림, 임시)</summary>
        public const string QuestToast = "ui.quest.toast";
        /// <summary>"{0} {1}/{2}" — 아이템 이름, 지금 개수, 필요 개수 (10-2 목록 항목 한 줄)</summary>
        public const string QuestRequirement = "ui.quest.requirement";
        /// <summary>"· {0}" — 목록 항목의 의뢰 제목 앞 항목 기호 (사용자 요청)</summary>
        public const string QuestItemTitle = "ui.quest.itemTitle";
        /// <summary>"화면에 의뢰 목록 표시" — 책 의뢰 탭의 토글 (10-1)</summary>
        public const string BookQuestShowHud = "ui.book.quest.showHud";
        /// <summary>"필요한 재료" — 책 의뢰 탭 본문</summary>
        public const string BookQuestNeeds = "ui.book.quest.needs";
        /// <summary>"보상"</summary>
        public const string BookQuestReward = "ui.book.quest.reward";
        /// <summary>"필기 점수 +{0}"</summary>
        public const string BookQuestRewardValue = "ui.book.quest.rewardValue";
        /// <summary>"“{0}”" — 크리처 대사 따옴표</summary>
        public const string BookQuestLine = "ui.book.quest.line";
    }
}
