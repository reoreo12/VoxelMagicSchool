// 의뢰 완료 알림 창(UI_SPEC 10-6, 13장): 조건을 다 채운 순간 화면 왼쪽 아래에 "의뢰 완료: 제목"을 잠시 보이고 페이드로 사라진다.
// 여러 건은 순서대로 하나씩. X로 지금 알림을 바로 닫는다. 의뢰 바를 꺼도 알림은 뜬다(사용자 결정). 시간 정지와 무관하게 unscaled time.
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class QuestToastView : ThemedWidget, IGameContextBinder
    {
        [SerializeField] private ThemedPanel _background;
        [SerializeField] private ThemedText _text;
        [SerializeField] private ThemedButton _close;

        private readonly Queue<string> _queue = new Queue<string>();
        private IAcceptedRequestList _requests;
        private CanvasGroup _group;
        private bool _showing;
        private float _elapsed;

        private CanvasGroup Group => _group != null ? _group : _group = GetComponent<CanvasGroup>();

        public void Bind(GameContext context)
        {
            _requests = context.Requests;
            _requests.RequestBecameComplete += OnCompleted;
            if (_close != null)
            {
                _close.Button.onClick.RemoveListener(Dismiss);
                _close.Button.onClick.AddListener(Dismiss);
            }
            Hide();
        }

        private void OnDestroy()
        {
            if (_requests != null) _requests.RequestBecameComplete -= OnCompleted;
        }

        private void OnCompleted(AcceptedRequestEntry entry) => Enqueue(entry.Title);

        /// <summary>알림 한 건을 줄에 넣는다. 보이는 알림이 없으면 바로 띄운다.</summary>
        public void Enqueue(string title)
        {
            _queue.Enqueue(title);
            if (!_showing) ShowNext();
        }

        private void ShowNext()
        {
            if (_queue.Count == 0 || Theme == null) { Hide(); return; }
            string title = _queue.Dequeue();
            string format = Theme.Texts != null ? Theme.Texts.Get(UiTextKeys.QuestToast) : "{0}";
            _text.SetText(string.Format(format, title));
            Layout(Theme);
            _showing = true;
            _elapsed = 0f;
            Group.alpha = 1f;
            Group.blocksRaycasts = true;
            SetChildrenActive(true);
        }

        /// <summary>X 버튼: 지금 알림을 바로 닫고 다음 알림으로.</summary>
        public void Dismiss() => ShowNext();

        private void Update()
        {
            if (!_showing || Theme == null) return;
            var values = Theme.QuestToast;
            _elapsed += Time.unscaledDeltaTime;
            if (_elapsed < values.ShowSeconds) return;
            float fade = values.FadeSeconds > 0f ? (_elapsed - values.ShowSeconds) / values.FadeSeconds : 1f;
            if (fade >= 1f) { ShowNext(); return; }
            Group.alpha = 1f - fade;
        }

        private void Hide()
        {
            _showing = false;
            Group.alpha = 0f;
            Group.blocksRaycasts = false;
            SetChildrenActive(false);
        }

        private void SetChildrenActive(bool active)
        {
            if (_background != null && _background.gameObject.activeSelf != active) _background.gameObject.SetActive(active);
            if (_text != null && _text.gameObject.activeSelf != active) _text.gameObject.SetActive(active);
            if (_close != null && _close.gameObject.activeSelf != active) _close.gameObject.SetActive(active);
        }

        protected override void Apply(UiTheme theme)
        {
            if (_text != null) _text.SetStyle(theme.QuestToast.TextStyle);
            Layout(theme);
            if (!Application.isPlaying) Hide();
        }

        // 글자 폭에 맞춘 창: [여백][글자][간격][X][여백], 높이는 글자 줄과 X 중 큰 값 + 여백.
        private void Layout(UiTheme theme)
        {
            if (_background == null || _text == null || _close == null) return;
            var values = theme.QuestToast;
            var label = _text.GetComponent<TMP_Text>();
            var style = theme.TextStyle(values.TextStyle);
            int line = style != null ? style.LineHeightFrom(theme.Fonts) : 16;
            int textWidth = Mathf.CeilToInt(label.GetPreferredValues(label.text).x);
            _close.ApplyTheme();
            var closeSize = Vector2Int.RoundToInt(((RectTransform)_close.transform).sizeDelta);
            int inner = Mathf.Max(line, closeSize.y);
            int width = values.Padding.x * 2 + textWidth + values.CloseGap + closeSize.x;
            int height = values.Padding.y * 2 + inner;

            PixelLayout.PlaceBottomLeft((RectTransform)transform, values.Margin, values.Margin, width, height);
            PixelLayout.Stretch((RectTransform)_background.transform);
            PixelLayout.PlaceTopLeft((RectTransform)_text.transform, values.Padding.x, values.Padding.y + PixelLayout.CenterOffset(inner, line), textWidth, line);
            PixelLayout.PlaceTopLeft((RectTransform)_close.transform, values.Padding.x + textWidth + values.CloseGap,
                values.Padding.y + PixelLayout.CenterOffset(inner, closeSize.y), 0, 0);
        }
    }
}
