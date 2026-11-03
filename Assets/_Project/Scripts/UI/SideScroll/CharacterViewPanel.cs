// 복도 화면의 "2D 캐릭터 (V)" 토글. CharacterViewState와 맞추며, V 키로 바꿔도 토글 표시가 따라간다.
using UnityEngine;
using UnityEngine.UI;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class CharacterViewPanel : MonoBehaviour, ISideScrollBinder
    {
        [SerializeField] private Toggle _toggle;

        private CharacterViewState _state;

        public void Bind(SideScrollContext context)
        {
            _state = context.CharacterView;
            _toggle.SetIsOnWithoutNotify(_state.Is2D);
            _toggle.onValueChanged.AddListener(_state.Set2D);
            _state.Changed += Refresh;
        }

        private void OnDestroy()
        {
            if (_state != null) _state.Changed -= Refresh;
        }

        private void Refresh() => _toggle.SetIsOnWithoutNotify(_state.Is2D);
    }
}
