// 모든 아이템 정의의 공통 데이터. 새 아이템 종류는 이 클래스를 상속한 ScriptableObject로 만든다.
using UnityEngine;

namespace VoxelMagic.Core
{
    public abstract class ItemDefinition : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField, TextArea] private string _description;
        [SerializeField] private Color _iconColor = Color.white;
        [SerializeField, Min(1)] private int _maxStack = 99;
        [Tooltip("크리처가 이 재료를 전달받았을 때 들려주는 설명 (필기 공부)")]
        [SerializeField, TextArea] private string _creatureNote;

        public string Id => string.IsNullOrEmpty(_id) ? name : _id;
        public string DisplayName => _displayName;
        public string Description => _description;
        public Color IconColor => _iconColor;
        public int MaxStack => _maxStack;
        public string CreatureNote => _creatureNote;
    }
}
