// 복도 화면의 "밤 (N)" 토글. TimeOfDayState와 맞추며, N 키로 바꿔도 토글 표시가 따라간다.
using UnityEngine;
using UnityEngine.UI;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class NightTogglePanel : MonoBehaviour, ISideScrollBinder
    {
        [SerializeField] private Toggle _toggle;

        private TimeOfDayState _state;

        public void Bind(SideScrollContext context)
        {
            _state = context.TimeOfDay;
            _toggle.SetIsOnWithoutNotify(_state.IsNight);
            _toggle.onValueChanged.AddListener(_state.SetNight);
            _state.Changed += Refresh;
        }

        private void OnDestroy()
        {
            if (_state != null) _state.Changed -= Refresh;
        }

        private void Refresh() => _toggle.SetIsOnWithoutNotify(_state.IsNight);
    }
}
