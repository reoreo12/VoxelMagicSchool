// 의뢰 규칙 설정(QUEST_TIMER 2장): 동시 수락 수, 난이도별 등장 날, 게시판이 열리는 날, 마지막 전달 날, 반복 방지, 희귀도별 필기 점수, 튜토리얼 의뢰.
using UnityEngine;

namespace VoxelMagic.Core
{
    public enum RequestDifficulty
    {
        Easy,
        Normal,
        Hard,
        /// <summary>1일 고정 튜토리얼 의뢰 (게시판 뽑기에 나오지 않음)</summary>
        Tutorial
    }

    [CreateAssetMenu(menuName = "VoxelMagic/Config/Quest", fileName = "QuestConfig")]
    public sealed class QuestConfig : ScriptableObject
    {
        [Header("수락")]
        [Tooltip("동시에 받아 둘 수 있는 의뢰 수 (전달해야 칸이 빈다)")]
        [SerializeField, Min(1)] private int _maxAccepted = 3;

        [Header("게시판")]
        [Tooltip("쉬움 의뢰가 게시되기 시작하는 날")]
        [SerializeField, Min(1)] private int _easyFromDay = 1;
        [Tooltip("보통 의뢰가 게시되기 시작하는 날")]
        [SerializeField, Min(1)] private int _normalFromDay = 1;
        [Tooltip("어려움 의뢰가 게시되기 시작하는 날")]
        [SerializeField, Min(1)] private int _hardFromDay = 2;
        [Tooltip("튜토리얼 의뢰만 게시하는 날")]
        [SerializeField, Min(1)] private int _tutorialDay = 1;
        [SerializeField, Tooltip("1일 고정 튜토리얼 의뢰 (농작 1 + 낚시 1 + 채집 1)")] private RequestDefinition _tutorialRequest;
        [Tooltip("의뢰를 전달할 수 있는 마지막 날 (그날 밤까지)")]
        [SerializeField, Min(1)] private int _lastDeliveryDay = 6;
        [Tooltip("이 날부터 게시판을 열지 않는다")]
        [SerializeField, Min(1)] private int _boardClosedFromDay = 7;
        [Tooltip("게시됐던 의뢰를 이 날 수 동안은 다시 게시하지 않는다 (후보가 모자라면 풀림)")]
        [SerializeField, Min(0)] private int _repeatCooldownDays = 2;
        [Tooltip("다음 날 아침에 자라는 작물이 필요한 의뢰는 '게시 날 + 이 값'이 마지막 전달 날을 넘으면 게시하지 않는다 (밤에 받아 바로 심으면 다음 날 밤 전달 = 1)")]
        [SerializeField, Min(0)] private int _nextDayCropLeadDays = 1;
        [Tooltip("게시판·크리처 전달을 밤에만 허용 (끄면 아무 때나, 시험용)")]
        [SerializeField] private bool _onlyAtNight = true;

        [Header("전달")]
        [Tooltip("완료 가능한 의뢰가 하나뿐이면 고르기 창 없이 바로 전달 대사로")]
        [SerializeField] private bool _skipChoiceWhenSingle = true;

        [Header("필기 점수 (재료 희귀도별, 의뢰 점수 = 필요 재료 점수 합)")]
        [SerializeField, Min(0)] private int _commonScore = 2;
        [SerializeField, Min(0)] private int _uncommonScore = 4;
        [SerializeField, Min(0)] private int _rareScore = 8;

        public int MaxAccepted => _maxAccepted;
        public int TutorialDay => _tutorialDay;
        public RequestDefinition TutorialRequest => _tutorialRequest;
        public int LastDeliveryDay => _lastDeliveryDay;
        public int BoardClosedFromDay => _boardClosedFromDay;
        public int RepeatCooldownDays => _repeatCooldownDays;
        public bool OnlyAtNight => _onlyAtNight;
        public bool SkipChoiceWhenSingle => _skipChoiceWhenSingle;

        /// <summary>그날 게시해도 마지막 전달 날까지 끝낼 수 있는 의뢰인지.</summary>
        public bool CanFinishInTime(RequestDefinition request, int day)
            => request == null || !request.NeedsNextDayCrop || day + _nextDayCropLeadDays <= _lastDeliveryDay;

        /// <summary>그날 게시할 수 있는 난이도인지.</summary>
        public bool Appears(RequestDifficulty difficulty, int day) => difficulty switch
        {
            RequestDifficulty.Easy => day >= _easyFromDay,
            RequestDifficulty.Normal => day >= _normalFromDay,
            RequestDifficulty.Hard => day >= _hardFromDay,
            _ => false
        };

        public bool IsBoardOpenOn(int day) => day < _boardClosedFromDay;

        public int ScoreOf(ItemRarity rarity) => rarity switch
        {
            ItemRarity.Rare => _rareScore,
            ItemRarity.Uncommon => _uncommonScore,
            _ => _commonScore
        };
    }
}
