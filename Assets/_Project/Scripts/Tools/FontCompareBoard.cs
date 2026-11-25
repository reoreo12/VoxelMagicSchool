// UI 에디터 씬의 한글 폰트 비교판(UI_SPEC 1-1). 같은 문장을 여러 폰트로, 크림 종이·갈색·어두운 배경 위에 나란히 보여 준다.
// 인스펙터에서 문장 번호·크기 배수(설계 크기의 정수 배)·보일 배경을 바꾸면 바로 다시 배치한다. 개발 전용 도구다.
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace VoxelMagic.Tools
{
    [ExecuteAlways]
    public sealed class FontCompareBoard : MonoBehaviour
    {
        [Serializable]
        public sealed class FontRow
        {
            [SerializeField] private string _label;
            [SerializeField] private TMP_FontAsset _font;

            public FontRow(string label, TMP_FontAsset font)
            {
                _label = label;
                _font = font;
            }

            public string Label => _label;
            public TMP_FontAsset Font => _font;
        }

        [Serializable]
        public sealed class Band
        {
            [SerializeField] private RectTransform _background;
            [SerializeField] private Color _textColor = Color.black;
            [SerializeField] private List<TMP_Text> _rows = new List<TMP_Text>();

            public Band(RectTransform background, Color textColor, List<TMP_Text> rows)
            {
                _background = background;
                _textColor = textColor;
                _rows = rows;
            }

            public RectTransform Background => _background;
            public Color TextColor => _textColor;
            public IReadOnlyList<TMP_Text> Rows => _rows;
        }

        [SerializeField] private List<FontRow> _fonts = new List<FontRow>();
        [SerializeField, TextArea] private List<string> _samples = new List<string>();
        [SerializeField, Min(0)] private int _sampleIndex;
        [SerializeField, Range(1, 3), Tooltip("글자 크기 = 폰트 설계 크기 × 이 값")] private int _sizeMultiplier = 1;
        [SerializeField, Tooltip("-1이면 배경 전부, 아니면 그 번호 배경만")] private int _onlyBand = -1;
        [SerializeField] private int _width = 400;
        [SerializeField, Tooltip("종이 패널 테두리(6px)보다 크게")] private int _padding = 8;
        [SerializeField] private int _bandGap = 4;
        [SerializeField] private List<Band> _bands = new List<Band>();

        private bool _dirty = true;

        public void Setup(List<FontRow> fonts, List<string> samples, List<Band> bands)
        {
            _fonts = fonts;
            _samples = samples;
            _bands = bands;
            Refresh();
        }

        private void OnEnable() => _dirty = true;
        private void OnValidate() => _dirty = true; // OnValidate 안에서는 RectTransform을 바꾸지 않고 다음 Update에서 바꾼다

        private void Update()
        {
            if (_dirty) Refresh();
        }

        public void Refresh()
        {
            _dirty = false;
            string sample = _samples.Count > 0 ? _samples[Mathf.Clamp(_sampleIndex, 0, _samples.Count - 1)] : string.Empty;
            int y = 0;
            for (int b = 0; b < _bands.Count; b++)
            {
                var band = _bands[b];
                bool visible = _onlyBand < 0 || _onlyBand == b;
                if (band.Background == null) continue;
                band.Background.gameObject.SetActive(visible);
                if (!visible) continue;

                int rowY = _padding;
                for (int i = 0; i < band.Rows.Count; i++)
                {
                    var text = band.Rows[i];
                    if (text == null) continue;
                    var row = i < _fonts.Count ? _fonts[i] : null;
                    bool hasFont = row != null && row.Font != null;
                    text.gameObject.SetActive(hasFont);
                    if (!hasFont) continue;

                    int design = Mathf.Max(1, Mathf.RoundToInt(row.Font.faceInfo.pointSize));
                    int size = design * _sizeMultiplier;
                    int lineHeight = Mathf.CeilToInt(row.Font.faceInfo.lineHeight / row.Font.faceInfo.pointSize * size);
                    text.font = row.Font;
                    text.fontSize = size;
                    text.color = band.TextColor;
                    text.raycastTarget = false;
                    text.enableAutoSizing = false;
                    text.textWrappingMode = TextWrappingModes.NoWrap;
                    text.SetText($"{row.Label} {design * _sizeMultiplier}px  {sample}");
                    Place((RectTransform)text.transform, _padding, -rowY, _width - _padding * 2, lineHeight);
                    rowY += lineHeight;
                }
                int height = rowY + _padding;
                Place(band.Background, 0, -y, _width, height);
                y += height + _bandGap;
            }
        }

        // 왼쪽 위 기준, 정수 px 배치.
        private static void Place(RectTransform rect, int x, int y, int width, int height)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
        }
    }
}
