// 복도 화면의 "카메라 고정 (C)" 토글. CameraFollowState와 맞추며, C 키로 바꿔도 토글 표시가 따라간다.
using UnityEngine;
using UnityEngine.UI;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class CameraFollowPanel : MonoBehaviour, ISideScrollBinder
    {
        [SerializeField] private Toggle _toggle;

        private CameraFollowState _state;

        public void Bind(SideScrollContext context)
        {
            _state = context.CameraFollow;
            _toggle.SetIsOnWithoutNotify(_state.IsLocked);
            _toggle.onValueChanged.AddListener(_state.SetLocked);
            _state.Changed += Refresh;
        }

        private void OnDestroy()
        {
            if (_state != null) _state.Changed -= Refresh;
        }

        private void Refresh() => _toggle.SetIsOnWithoutNotify(_state.IsLocked);
    }
}
