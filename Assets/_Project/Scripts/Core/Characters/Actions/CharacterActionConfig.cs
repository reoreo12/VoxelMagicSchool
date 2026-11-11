// 동작 애니메이션 설정: 동작별 재생 속도, 동작으로 들어가고 나올 때 섞는 시간, 물 주기 안전 시간. 플레이 중 바꾸면 다음 동작부터 반영.
using UnityEngine;

namespace VoxelMagic.Core
{
    [CreateAssetMenu(menuName = "VoxelMagic/Characters/Action Config", fileName = "CharacterActionConfig")]
    public sealed class CharacterActionConfig : ScriptableObject
    {
        [Header("재생 속도 (1 = 원래 속도)")]
        [SerializeField, Range(0.1f, 4f)] private float _fishCastSpeed = 1f;
        [SerializeField, Range(0.1f, 4f)] private float _fishReelSpeed = 1f;
        [SerializeField, Range(0.1f, 4f)] private float _fishHookSpeed = 1f;
        [SerializeField, Range(0.1f, 4f)] private float _waterSpeed = 1f;

        [Header("섞기 (초)")]
        [Tooltip("이동 → 동작, 동작 → 동작으로 넘어갈 때")]
        [SerializeField, Min(0f)] private float _enterBlendSeconds = 0.2f;
        [Tooltip("동작을 끝내고 이동 상태로 돌아갈 때")]
        [SerializeField, Min(0f)] private float _exitBlendSeconds = 0.25f;
        [Tooltip("동작이 끝나 저절로 이어질 때 (던지기→기다리기, 낚아채기·물 주기→Idle). 바꾸면 메뉴 '공용 Controller 다시 만들기'")]
        [SerializeField, Min(0f)] private float _chainBlendSeconds = 0.25f;

        [Header("안전")]
        [Tooltip("물 주기 애니메이션이 끝났다는 걸 이만큼 못 알아채면 강제로 끝낸다 (초)")]
        [SerializeField, Min(0.5f)] private float _wateringTimeoutSeconds = 10f;

        public float EnterBlendSeconds => _enterBlendSeconds;
        public float ExitBlendSeconds => _exitBlendSeconds;
        public float ChainBlendSeconds => _chainBlendSeconds;
        public float WateringTimeoutSeconds => _wateringTimeoutSeconds;

        public float SpeedFor(CharacterActionId action)
        {
            switch (action)
            {
                case CharacterActionId.FishCast: return _fishCastSpeed;
                case CharacterActionId.FishReel:
                case CharacterActionId.FishReelBack: return _fishReelSpeed;
                case CharacterActionId.FishHook: return _fishHookSpeed;
                case CharacterActionId.Water: return _waterSpeed;
                default: return 1f;
            }
        }
    }
}
