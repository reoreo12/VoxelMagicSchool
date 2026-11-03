// 복도 화면의 도트 렌더링 패널: 토글·도트 크기 슬라이더·표시 글자를 PixelRenderState와 맞추고, 바뀌면 PixelRenderView에 적용한다.
// P 키(입력)와 패널 조작이 같은 상태를 바꾸므로 어느 쪽으로 바꿔도 화면이 함께 갱신된다.
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class PixelRenderPanel : MonoBehaviour, ISideScrollBinder
    {
        [SerializeField] private PixelRenderView _view;
        [SerializeField] private Toggle _toggle;
        [SerializeField] private Slider _slider;
        [SerializeField] private TMP_Text _label;

        private PixelRenderState _state;

        public void Bind(SideScrollContext context)
        {
            _state = context.PixelRender;
            _slider.wholeNumbers = true;
            _slider.minValue = _state.MinSize;
            _slider.maxValue = _state.MaxSize;
            _toggle.onValueChanged.AddListener(_state.SetOn);
            _slider.onValueChanged.AddListener(v => _state.SetSize(Mathf.RoundToInt(v)));
            _state.Changed += Refresh;
        }

        // 뷰의 Awake가 끝난 뒤 처음 상태를 적용한다.
        private void Start()
        {
            if (_state != null) Refresh();
        }

        private void OnDestroy()
        {
            if (_state != null) _state.Changed -= Refresh;
        }

        // 화면 크기가 바뀌면 해상도 표시도 바뀌므로 매 프레임 글자를 갱신한다.
        private void Update()
        {
            if (_state == null) return;
            var resolution = _view.Resolution;
            _label.text = _state.IsOn
                ? string.Format(SideScrollTexts.PixelOn, _state.Size, resolution.x, resolution.y)
                : string.Format(SideScrollTexts.PixelOff, _state.Size);
        }

        private void Refresh()
        {
            _toggle.SetIsOnWithoutNotify(_state.IsOn);
            _slider.SetValueWithoutNotify(_state.Size);
            _view.SetPixelSize(_state.Size);
            _view.SetOn(_state.IsOn);
        }
    }
}
