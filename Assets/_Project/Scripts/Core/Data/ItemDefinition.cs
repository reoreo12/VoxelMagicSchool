// 모든 아이템 정의의 공통 데이터(이름·아이콘·쌓기·크리처 설명·희귀도·출처). 새 아이템 종류는 이 클래스를 상속한 ScriptableObject로 만든다.
using UnityEngine;

namespace VoxelMagic.Core
{
    public abstract class ItemDefinition : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField, TextArea] private string _description;
        [SerializeField] private Color _iconColor = Color.white;
        [Tooltip("UI 아이콘(16×16 도트, UI_SPEC 7-3). 비우면 단색 칸 + 첫 글자로 보인다")]
        [SerializeField] private Sprite _icon;
        [SerializeField, Min(1)] private int _maxStack = 99;
        [Tooltip("크리처가 이 재료를 전달받았을 때 들려주는 설명 (필기 공부)")]
        [SerializeField, TextArea] private string _creatureNote;
        [Tooltip("희귀도: 필기 점수와 출현 확률 기준 (씨앗·도구는 상관없음)")]
        [SerializeField] private ItemRarity _rarity = ItemRarity.Common;
        [Tooltip("어디서 얻는 재료인지 (씨앗·도구는 None)")]
        [SerializeField] private ItemSource _source = ItemSource.None;

        public string Id => string.IsNullOrEmpty(_id) ? name : _id;
        public string DisplayName => _displayName;
        public string Description => _description;
        public Color IconColor => _iconColor;
        public Sprite Icon => _icon;
        public int MaxStack => _maxStack;
        public string CreatureNote => _creatureNote;
        public ItemRarity Rarity => _rarity;
        public ItemSource Source => _source;
    }
}
