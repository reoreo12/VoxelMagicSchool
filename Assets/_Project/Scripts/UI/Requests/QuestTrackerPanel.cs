// 화면 오른쪽 위 의뢰 진행 토글. 항목별 "대파 1/2"를 보여 주고, Q로 접고 펼치며, 완료되면 잠시 "완료!" 후 사라진다.
using System.Text;
using TMPro;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class QuestTrackerPanel : UIPanel, IGameContextBinder
    {
        [SerializeField] private TMP_Text _header;
        [SerializeField] private TMP_Text _body;
        [SerializeField] private Color _doneColor = new Color(0.55f, 1f, 0.55f);

        private readonly StringBuilder _builder = new StringBuilder();
        private RequestTracker _tracker;
        private ScreenState _screen;
        private float _completeDisplaySeconds;
        private float _hideTimer;
        private string _completedTitle;

        public void Bind(GameContext context)
        {
            _tracker = context.Requests;
            _screen = context.Screen;
            _completeDisplaySeconds = context.Balance.QuestCompleteDisplaySeconds;

            _tracker.ActiveChanged += OnActiveChanged;
            _tracker.ProgressChanged += Redraw;
            _tracker.Completed += OnCompleted;
            _screen.TrackerToggled += _ => Redraw();
            SetVisible(false);
        }

        private void OnActiveChanged(RequestDefinition active)
        {
            if (active == null) return;
            _completedTitle = null;
            _hideTimer = 0f;
            Show();
            Redraw();
        }

        private void OnCompleted(DeliveryReport report)
        {
            _completedTitle = report.Request.Title;
            _hideTimer = _completeDisplaySeconds;
            Redraw();
        }

        private void Update()
        {
            if (_hideTimer <= 0f) return;
            _hideTimer -= Time.deltaTime;
            if (_hideTimer <= 0f && !_tracker.HasActive) Hide();
        }

        private void Redraw()
        {
            if (!IsVisible) return;
            string arrow = _screen.IsTrackerCollapsed ? GameTexts.TrackerCollapsed : GameTexts.TrackerExpanded;

            if (_completedTitle != null && !_tracker.HasActive)
            {
                _header.text = $"{arrow} {_completedTitle}";
                _body.text = _screen.IsTrackerCollapsed ? string.Empty
                    : $"<color=#{ColorUtility.ToHtmlStringRGB(_doneColor)}>{GameTexts.TrackerCompleted}</color>";
                return;
            }
            if (!_tracker.HasActive) return;

            _header.text = $"{arrow} {_tracker.Active.Title}";
            if (_screen.IsTrackerCollapsed)
            {
                _body.text = string.Empty;
                return;
            }

            _builder.Clear();
            foreach (var progress in _tracker.Progress)
            {
                if (progress.IsMet)
                    _builder.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(_doneColor)).Append('>')
                        .Append(GameTexts.TrackerDone).Append(' ').Append(progress).Append("</color>\n");
                else
                    _builder.Append(GameTexts.TrackerTodo).Append(' ').Append(progress).Append('\n');
            }
            _body.text = _builder.ToString().TrimEnd('\n');
        }
    }
}
