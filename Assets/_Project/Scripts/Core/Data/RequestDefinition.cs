// 크리처가 내는 의뢰 데이터. 필요 아이템, 수락 시 지급 씨앗, 기본 보상, 대사를 담는다.
using System.Collections.Generic;
using UnityEngine;

namespace VoxelMagic.Core
{
    [CreateAssetMenu(menuName = "VoxelMagic/Request", fileName = "Request_")]
    public sealed class RequestDefinition : ScriptableObject
    {
        [SerializeField] private string _title;
        [SerializeField, TextArea] private string _creatureLine;
        [SerializeField] private List<ItemAmount> _requirements = new List<ItemAmount>();
        [SerializeField] private List<ItemAmount> _givenItems = new List<ItemAmount>();
        [SerializeField, Min(0)] private int _baseReward = 10;
        [SerializeField, TextArea] private string _completionLine;

        public string Title => _title;
        public string CreatureLine => _creatureLine;
        public IReadOnlyList<ItemAmount> Requirements => _requirements;
        public IReadOnlyList<ItemAmount> GivenItems => _givenItems;
        public int BaseReward => _baseReward;
        public string CompletionLine => _completionLine;
    }
}
