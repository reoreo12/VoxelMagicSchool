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
            if (isActiveAndEnabled && _theme != null) _theme.Changed -= ApplyTheme;
            _theme = theme;
            if (isActiveAndEnabled && _theme != null) _theme.Changed += ApplyTheme;
            ApplyTheme();
        }

        protected virtual void OnEnable()
        {
            if (_theme != null) _theme.Changed += ApplyTheme;
            ApplyTheme();
        }

        protected virtual void OnDisable()
        {
            if (_theme != null) _theme.Changed -= ApplyTheme;
        }

        protected virtual void OnValidate()
        {
            if (isActiveAndEnabled) ApplyTheme();
        }

        public void ApplyTheme()
        {
            if (_theme != null) Apply(_theme);
        }

        protected abstract void Apply(UiTheme theme);
    }
}
