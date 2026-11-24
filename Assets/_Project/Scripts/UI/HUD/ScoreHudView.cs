// 화면 위 가운데 점수 HUD(UI_SPEC 9장, 13장): "필기 예상 점수" + 숫자(v1 밑줄), "실기 예상 점수" + 숫자(v2 밑줄)를 여백을 두고 나란히 놓는다.
// 점수는 Core의 ScoreBook을 이벤트로만 읽는다(점수 규칙은 다른 문서). 숫자 폭이 바뀔 때만 다시 배치하고, 가운데는 왼쪽 끝 기준 정수 px로 계산한다.
using TMPro;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class ScoreHudView : ThemedWidget, IGameContextBinder
    {
        [SerializeField] private RectTransform _row;
        [SerializeField] private ThemedText _writtenLabel;
        [SerializeField] private ScoreNumberView _writtenNumber;
        [SerializeField] private ThemedText _practicalLabel;
        [SerializeField] private ScoreNumberView _practicalNumber;

        private ScoreBook _score;
        private int _written;
        private int _practical;

        public void Bind(GameContext context)
        {
            _score = context.Score;
            _score.ScoreChanged += OnScoreChanged;
            _written = _score.Get(ScoreKind.Written);
            _practical = _score.Get(ScoreKind.Practical);
            Relayout();
        }

        // 점수는 씬을 넘어 유지되므로 꼭 구독을 푼다.
        private void OnDestroy()
        {
            if (_score != null) _score.ScoreChanged -= OnScoreChanged;
        }

        private void OnScoreChanged(ScoreKind kind, int _, int __)
        {
            if (this == null) { OnDestroy(); return; } // 꺼진 채 씬이 내려가 OnDestroy가 불리지 않은 구독자: 여기서 구독을 푼다
            int written = _score.Get(ScoreKind.Written);
            int practical = _score.Get(ScoreKind.Practical);
            if (written == _written && practical == _practical) return;
            _written = written;
            _practical = practical;
            Relayout();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (Theme != null && _row != null) PlaceRow(Theme.Scores);
        }

        protected override void Apply(UiTheme theme)
        {
            var root = (RectTransform)transform;
            // 뿌리는 화면 위쪽 띠(가로로 늘림). 줄은 그 안에서 가운데.
            root.anchorMin = new Vector2(0f, 1f);
            root.anchorMax = new Vector2(1f, 1f);
            root.pivot = new Vector2(0.5f, 1f);
            root.anchoredPosition = Vector2.zero;
            if (_writtenLabel != null) _writtenLabel.SetKey(UiTextKeys.ScoreWritten);
            if (_practicalLabel != null) _practicalLabel.SetKey(UiTextKeys.ScorePractical);
            if (_writtenNumber != null) _writtenNumber.Invalidate();
            if (_practicalNumber != null) _practicalNumber.Invalidate();
            Relayout();
        }

        private void Relayout()
        {
            if (Theme == null || _row == null || _writtenNumber == null || _practicalNumber == null) return;
            var values = Theme.Scores;
            _writtenNumber.Show(_written, true, values);
            _practicalNumber.Show(_practical, false, values);

            int x = 0;
            int height = 0;
            x = PlaceGroup(_writtenLabel, _writtenNumber, x, values, values.Offsets(true), ref height);
            x += values.GroupGap;
            x = PlaceGroup(_practicalLabel, _practicalNumber, x, values, values.Offsets(false), ref height);
            _row.sizeDelta = new Vector2(x, height);
            ((RectTransform)transform).sizeDelta = new Vector2(0f, height + values.TopMargin);
            PlaceRow(values);
        }

        // 글자와 숫자를 숫자 줄 가운데 높이에 맞춰 나란히 놓고, 다음 x를 돌려준다.
        // 손 조정 값(묶음·글자·숫자)은 그린 위치에만 더하고, 줄 폭·가운데 계산은 자동 배치 그대로 둔다.
        private static int PlaceGroup(ThemedText label, ScoreNumberView number, int x, ScoreValues values, ScoreGroupOffsets offsets, ref int height)
        {
            int scale = values.NumberScale;
            var text = label.GetComponent<TMP_Text>();
            int labelWidth = Mathf.CeilToInt(text.GetPreferredValues(text.text).x);
            int labelHeight = Mathf.CeilToInt(text.fontSize * (text.font != null ? text.font.faceInfo.lineHeight / text.font.faceInfo.pointSize : 1.4f));
            int digitsHeight = number.DigitsSize.y * scale;

            var group = offsets.Group;
            int labelY = (digitsHeight - labelHeight) / 2;
            var labelShift = group + offsets.Label;
            PixelLayout.PlaceTopLeft((RectTransform)label.transform, x + labelShift.x, labelY + labelShift.y, labelWidth, labelHeight);
            x += labelWidth + values.LabelGap;

            var numberShift = group + offsets.Number;
            var rect = number.Rect;
            PixelLayout.PlaceTopLeft(rect, x + numberShift.x, numberShift.y, 0, 0);
            rect.localScale = new Vector3(scale, scale, 1f);
            x += number.DigitsSize.x * scale;

            height = Mathf.Max(height, Mathf.Max(Mathf.Max(0, labelY) + labelHeight, number.TotalHeight * scale));
            return x;
        }

        private void PlaceRow(ScoreValues values)
        {
            var size = Vector2Int.RoundToInt(_row.sizeDelta);
            var root = (RectTransform)transform;
            _row.anchorMin = _row.anchorMax = _row.pivot = new Vector2(0f, 1f);
            _row.anchoredPosition = new Vector2(Mathf.FloorToInt((root.rect.width - size.x) / 2f), -values.TopMargin);
        }
    }
}
