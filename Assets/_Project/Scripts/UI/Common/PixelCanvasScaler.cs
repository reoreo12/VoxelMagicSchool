// 루트 캔버스를 정수 배율로 맞춘다(UI_SPEC 2장): 배율 = max(최소, 화면 높이 ÷ 기준 높이의 내림), Pixel Perfect 켬.
// 화면 크기가 바뀐 프레임에만 다시 계산한다. 에디터에서도 돌아 Game View 해상도를 바꾸면 바로 보인다.
using System;
using UnityEngine;
using UnityEngine.UI;

namespace VoxelMagic.UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(Canvas), typeof(CanvasScaler))]
    public sealed class PixelCanvasScaler : MonoBehaviour
    {
        [SerializeField] private UiTheme _theme;

        private Canvas _canvas;
        private CanvasScaler _scaler;
        private int _lastHeight = -1;

        public int Scale { get; private set; } = 1;
        /// <summary>배율을 적용한 가상 화면 크기(예: 1920×1080 → 640×360).</summary>
        public Vector2Int VirtualSize { get; private set; }

        public event Action<int> ScaleChanged;

        public static int ComputeScale(int screenHeight, int referenceHeight, int minScale)
        {
            if (referenceHeight <= 0) return Math.Max(1, minScale);
            return Math.Max(Math.Max(1, minScale), screenHeight / referenceHeight);
        }

        private void OnEnable()
        {
            _canvas = GetComponent<Canvas>();
            _scaler = GetComponent<CanvasScaler>();
            _lastHeight = -1;
            if (_theme != null) _theme.Changed += ForceRefresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (_theme != null) _theme.Changed -= ForceRefresh;
        }

        private void Update() => Refresh();

        private void ForceRefresh()
        {
            _lastHeight = -1;
            Refresh();
        }

        private void Refresh()
        {
            if (_theme == null || _canvas == null) return;
            var size = ScreenSize();
            if (size.y == _lastHeight) return;
            _lastHeight = size.y;

            var values = _theme.Canvas;
            Scale = ComputeScale(size.y, values.ReferenceHeight, values.MinScale);
            VirtualSize = new Vector2Int(size.x / Scale, size.y / Scale);

            _canvas.pixelPerfect = true;
            _scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            _scaler.scaleFactor = Scale;
            _scaler.referencePixelsPerUnit = values.ReferencePixelsPerUnit;
            ScaleChanged?.Invoke(Scale);
        }

        private Vector2Int ScreenSize()
        {
            var display = _canvas.renderingDisplaySize;
            return display.y > 0f
                ? new Vector2Int(Mathf.RoundToInt(display.x), Mathf.RoundToInt(display.y))
                : new Vector2Int(Screen.width, Screen.height);
        }
    }
}
