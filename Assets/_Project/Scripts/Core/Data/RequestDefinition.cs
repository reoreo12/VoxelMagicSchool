// 크리처가 내는 의뢰 데이터: 제목, 크리처 대사, 서식지 힌트, 난이도, 필요 재료(합계 최대 3개), 수락 시 지급 씨앗, 전달 대사. 코드 수정 없이 에셋으로 추가한다.
using System.Collections.Generic;
using UnityEngine;

namespace VoxelMagic.Core
{
    [CreateAssetMenu(menuName = "VoxelMagic/Request", fileName = "Request_")]
    public sealed class RequestDefinition : ScriptableObject
    {
        [SerializeField] private string _title;
        [SerializeField, TextArea] private string _creatureLine;
        [Tooltip("의뢰 목록에 보일 서식지 힌트 한 줄 (정확한 위치는 쓰지 않는다)")]
        [SerializeField] private string _habitatHint;
        [Tooltip("게시판 뽑기 난이도 (튜토리얼은 1일 고정 의뢰)")]
        [SerializeField] private RequestDifficulty _difficulty = RequestDifficulty.Easy;
        [SerializeField] private List<ItemAmount> _requirements = new List<ItemAmount>();
        [SerializeField] private List<ItemAmount> _givenItems = new List<ItemAmount>();
        [SerializeField, Min(0)] private int _baseReward = 10;
        [SerializeField, TextArea] private string _completionLine;

        public string Title => _title;
        public string CreatureLine => _creatureLine;
        public string HabitatHint => _habitatHint;
        public RequestDifficulty Difficulty => _difficulty;

        /// <summary>다음 날 아침에야 자라는 작물(희귀)의 씨앗을 주는 의뢰인지 — 받은 날 바로는 끝낼 수 없다.</summary>
        public bool NeedsNextDayCrop
        {
            get
            {
                foreach (var given in _givenItems)
                    if (given.Item is SeedDefinition seed && seed.GrowsNextDay) return true;
                return false;
            }
        }

        /// <summary>필요 재료 개수 합계 (지침: 최대 3).</summary>
        public int RequiredCount
        {
            get
            {
                int total = 0;
                foreach (var need in _requirements) total += need.Amount;
                return total;
            }
        }
        public IReadOnlyList<ItemAmount> Requirements => _requirements;
        public IReadOnlyList<ItemAmount> GivenItems => _givenItems;
        public int BaseReward => _baseReward;
        public string CompletionLine => _completionLine;

        public const int MaxRequiredCount = 3;

        private void OnValidate()
        {
            if (RequiredCount > MaxRequiredCount)
                Debug.LogWarning($"[의뢰] {name}: 필요 재료 합계 {RequiredCount}개 — 최대 {MaxRequiredCount}개 (QUEST_TIMER 2-1)", this);
        }
    }
}
