// 테마를 따르는 위젯의 공통 기반(UI_SPEC 4-A-3). 켜질 때와 테마 값이 바뀔 때 Apply로 모양을 다시 입힌다.
// 위젯에는 게임 규칙을 넣지 않는다. 화면은 위젯을 배치하고 값만 채운다.
using UnityEngine;

namespace VoxelMagic.UI
{
    [ExecuteAlways]
    public abstract class ThemedWidget : MonoBehaviour
    {
        [SerializeField] private UiTheme _theme;

        protected UiTheme Theme => _theme;

        public void SetTheme(UiTheme theme)
        {
            if (_theme == theme) return;
            if (isActiveAndEnabled && _theme != null) _theme.Changed -= RequestApply;
            _theme = theme;
            if (isActiveAndEnabled && _theme != null) _theme.Changed += RequestApply;
            ApplyTheme();
        }

        protected virtual void OnEnable()
        {
            if (_theme != null) _theme.Changed += RequestApply;
            ApplyTheme();
        }

        protected virtual void OnDisable()
        {
            if (_theme != null) _theme.Changed -= RequestApply;
        }

        protected virtual void OnValidate()
        {
            if (isActiveAndEnabled) RequestApply();
        }

        // 인스펙터에서 값을 바꾸는 순간(OnValidate 안)에는 RectTransform을 바꿀 수 없어서, 에디터에서는 한 박자 뒤에 적용한다.
        private void RequestApply()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (_applyQueued) return;
                _applyQueued = true;
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    _applyQueued = false;
                    if (this != null && isActiveAndEnabled) ApplyTheme();
                };
                return;
            }
#endif
            ApplyTheme();
        }

#if UNITY_EDITOR
        private bool _applyQueued;
#endif

        public void ApplyTheme()
        {
            if (_theme != null) Apply(_theme);
        }

        protected abstract void Apply(UiTheme theme);
    }
}
