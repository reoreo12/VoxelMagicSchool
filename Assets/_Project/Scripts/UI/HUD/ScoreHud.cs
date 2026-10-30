// 화면 상단 시험 점수(실기·필기) 표시. ScoreBook 이벤트로 목표값을 받고, 숫자와 채움 바가 설정된 속도로 부드럽게 차오른다.
using System;
using TMPro;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class ScoreHud : MonoBehaviour, IGameContextBinder
    {
        [Serializable]
        private sealed class Row
        {
            public TMP_Text Text;
            public RectTransform Fill;
            [NonSerialized] public float Shown;
            [NonSerialized] public int Target;
        }

        [SerializeField] private Row _practical = new Row();
        [SerializeField] private Row _written = new Row();

        private ScoreBook _score;
        private float _fillSpeed;

        public void Bind(GameContext context)
        {
            _score = context.Score;
            _fillSpeed = context.Balance.ScoreFillSpeed;
            _score.ScoreChanged += OnScoreChanged;
            _practical.Shown = _practical.Target = _score.Practical;
            _written.Shown = _written.Target = _score.Written;
            Draw(_practical, GameTexts.PracticalLabel);
            Draw(_written, GameTexts.WrittenLabel);
        }

        private void OnDestroy()
        {
            if (_score != null) _score.ScoreChanged -= OnScoreChanged;
        }

        private void OnScoreChanged(ScoreKind kind, int previous, int current)
        {
            (kind == ScoreKind.Practical ? _practical : _written).Target = current;
        }

        private void Update()
        {
            if (_score == null) return;
            Step(_practical, GameTexts.PracticalLabel);
            Step(_written, GameTexts.WrittenLabel);
        }

        private void Step(Row row, string label)
        {
            if (Mathf.Approximately(row.Shown, row.Target)) return;
            row.Shown = Mathf.MoveTowards(row.Shown, row.Target, _fillSpeed * Time.deltaTime);
            Draw(row, label);
        }

        private void Draw(Row row, string label)
        {
            row.Text.text = string.Format(GameTexts.ScoreFormat, label, Mathf.RoundToInt(row.Shown), _score.MaxScore);
            row.Fill.anchorMax = new Vector2(row.Shown / _score.MaxScore, 1f);
            row.Fill.offsetMax = Vector2.zero;
        }
    }
}
