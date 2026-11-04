// 게임 전반의 숫자 설정(인벤토리 칸 수, 점수 규칙, UI 표시 시간).
using UnityEngine;

namespace VoxelMagic.Core
{
    [CreateAssetMenu(menuName = "VoxelMagic/Config/Game Balance", fileName = "GameBalanceConfig")]
    public sealed class GameBalanceConfig : ScriptableObject
    {
        [SerializeField, Min(1)] private int _inventorySlotCount = 5;
        [SerializeField, Min(1)] private int _maxScore = 100;
        [Tooltip("처음 전달하는 아이템 종류 1개당 추가 필기 점수")]
        [SerializeField, Min(0)] private int _newItemKindBonus = 5;
        [SerializeField, Min(0.1f)] private float _acquiredInfoSeconds = 3f;
        [SerializeField, Min(0.1f)] private float _toastSeconds = 2f;
        [Tooltip("점수 바가 차오르는 속도 (점/초)")]
        [SerializeField, Min(1f)] private float _scoreFillSpeed = 40f;
        [Tooltip("의뢰 진행 토글이 '완료'를 보여 준 뒤 사라지기까지 시간(초)")]
        [SerializeField, Min(0f)] private float _questCompleteDisplaySeconds = 2f;

        public int InventorySlotCount => _inventorySlotCount;
        public int MaxScore => _maxScore;
        public int NewItemKindBonus => _newItemKindBonus;
        public float AcquiredInfoSeconds => _acquiredInfoSeconds;
        public float ToastSeconds => _toastSeconds;
        public float ScoreFillSpeed => _scoreFillSpeed;
        public float QuestCompleteDisplaySeconds => _questCompleteDisplaySeconds;
    }
}
