// UI 루트 아래 캔버스 층(HUD 정적·동적·연출, 팝업, 오버레이)의 정렬 순서를 테마에서 읽어 맞춘다(UI_SPEC 4-A-5 캔버스 분리).
// 층마다 캔버스를 나눠 한 요소가 바뀌어도 그 층만 다시 그리게 한다.
using UnityEngine;

namespace VoxelMagic.UI
{
    public enum UiLayer
    {
        HudStatic,
        HudDynamic,
        HudAnimated,
        Popup,
        Overlay
    }

    [ExecuteAlways]
    [RequireComponent(typeof(Canvas))]
    public sealed class UiLayerCanvas : MonoBehaviour
    {
        [SerializeField] private UiTheme _theme;
        [SerializeField] private UiLayer _layer;

        public UiLayer Layer => _layer;

        private void OnEnable()
        {
            if (_theme != null) _theme.Changed += Apply;
            Apply();
        }

        private void OnDisable()
        {
            if (_theme != null) _theme.Changed -= Apply;
        }

        private void OnValidate() => Apply();

        /// <summary>테마의 정렬 순서를 캔버스에 적용한다(프리팹을 만들 때도 부름).</summary>
        public void Apply()
        {
            if (_theme == null) return;
            var canvas = GetComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = OrderOf(_layer, _theme.Layers);
        }

        private static int OrderOf(UiLayer layer, UiTheme.LayerValues values)
        {
            switch (layer)
            {
                case UiLayer.HudDynamic: return values.HudDynamic;
                case UiLayer.HudAnimated: return values.HudAnimated;
                case UiLayer.Popup: return values.Popup;
                case UiLayer.Overlay: return values.Overlay;
                default: return values.HudStatic;
            }
        }
    }
}
