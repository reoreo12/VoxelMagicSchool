// 화면 오른쪽 위 시간대 HUD(UI_SPEC 8장, 13장): 나무 틀 왼쪽 칸에 시간대 그림, 오른쪽 칸에 룰렛과 1시간마다 정해진 각도로 움직이는 화살표, 아래 패널에 날짜.
// 시각은 Core의 IGameTime을 이벤트로만 읽고, 시계가 연결되기 전에는 테마의 미리보기 값을 보인다. 마우스를 올리면 "방과후 16:00" 툴팁.
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class TimeHudView : ThemedWidget, IGameContextBinder, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
    {
        [SerializeField, Tooltip("틀 뒤 받침 (틀 구멍 뒤 틈 가리기)")] private Image _backing;
        [SerializeField] private Image _frame;
        [SerializeField] private Image _roulette;
        [SerializeField] private Image _icon;
        [SerializeField] private Image _arrow;
        [SerializeField] private ThemedText _date;
        [SerializeField] private ThemedText _paused;
        [SerializeField, Tooltip("UI_Root의 Overlay 층 툴팁")] private UiTooltipView _tooltip;

        private IGameTime _time;
        private TimePause _pause;
        private bool _hovering;
        private Vector2 _pointer;
        private int _shownDay = -1;
        private int _shownHour = -1;
        private bool _shownNight;

        public void Bind(GameContext context)
        {
            _time = context.Time;
            _pause = context.Pause;
            if (_time != null) _time.TimeChanged += Refresh;
            if (_pause != null) _pause.Changed += RefreshPaused;
            _shownDay = -1;
            Refresh();
            RefreshPaused();
        }

        private void OnDestroy()
        {
            if (_time != null) _time.TimeChanged -= Refresh;
            if (_pause != null) _pause.Changed -= RefreshPaused;
        }

        private int Day => _time != null ? _time.Day : Theme.Time.PreviewDay;
        private int Hour => _time != null ? _time.Hour : Theme.Time.PreviewHour;
        private bool IsNight => _time != null ? _time.Period == TimeOfDay.Night : Theme.Time.PreviewHour >= Theme.Time.NightStartHour;

        protected override void Apply(UiTheme theme)
        {
            var values = theme.Time;
            var root = (RectTransform)transform;
            if (_frame == null || values.Frame == null) return;

            // 조각 위치는 1배 px로 놓고 묶음 전체를 배수만큼 키운다. 여백은 확대 뒤 화면 기준이라 오른쪽 위 기준점에 그대로 둔다.
            var frameSize = SpriteSize(values.Frame);
            PixelLayout.PlaceTopRight(root, values.Margin, values.Margin, frameSize.x, frameSize.y);
            root.localScale = new Vector3(values.Scale, values.Scale, 1f);

            if (_backing != null)
            {
                var b = values.Backing;
                _backing.sprite = null;
                _backing.color = values.BackingColor;
                _backing.raycastTarget = false;
                _backing.enabled = b.width > 0 && b.height > 0;
                PixelLayout.PlaceTopLeft(_backing.rectTransform, b.x, b.y, b.width, b.height);
            }
            SetSprite(_frame, values.Frame, Vector2Int.zero);
            _frame.raycastTarget = true; // 툴팁용 마우스 올리기
            SetSprite(_roulette, values.Roulette, values.RoulettePosition);

            // 화살표: 꼬리 가운데를 회전 중심으로 두고, 그 점을 틀 안 ArrowPivot에 맞춘다.
            var arrowSize = SpriteSize(values.Arrow);
            _arrow.sprite = values.Arrow;
            _arrow.enabled = values.Arrow != null;
            _arrow.raycastTarget = false;
            var arrowRect = _arrow.rectTransform;
            arrowRect.anchorMin = arrowRect.anchorMax = new Vector2(0f, 1f);
            arrowRect.sizeDelta = arrowSize;
            arrowRect.pivot = arrowSize.x > 0 && arrowSize.y > 0
                ? new Vector2(values.ArrowPivotInSprite.x / (float)arrowSize.x, 1f - values.ArrowPivotInSprite.y / (float)arrowSize.y)
                : new Vector2(0f, 0.5f);
            arrowRect.anchoredPosition = new Vector2(values.ArrowPivot.x, -values.ArrowPivot.y);

            if (_date != null && _date.Style != values.DateStyle) _date.SetStyle(values.DateStyle);
            PlaceText(_date, values.DatePosition, values.DateSize, values.Scale);
            PlaceText(_paused, values.PausedPosition, new Vector2Int(frameSize.x, values.DateSize.y), values.Scale);

            _shownDay = -1; // 위치·스프라이트가 바뀌었으니 다시 그린다
            Refresh();
            RefreshPaused();
        }

        private void Refresh()
        {
            if (Theme == null || _icon == null || _arrow == null) return;
            int day = Day, hour = Hour;
            bool night = IsNight;
            if (day == _shownDay && hour == _shownHour && night == _shownNight) return;
            _shownDay = day;
            _shownHour = hour;
            _shownNight = night;

            var values = Theme.Time;
            SetSprite(_icon, values.Icon(night), values.IconPosition);
            _arrow.rectTransform.localRotation = Quaternion.Euler(0f, 0f, values.ArrowAngle(hour));
            if (_date != null && Theme.Texts != null) _date.SetText(string.Format(Theme.Texts.Get(UiTextKeys.TimeDay), day));
            if (_hovering) ShowTooltip(_pointer); // 시각이 바뀌면 떠 있는 툴팁 글자도 바꾼다
        }

        // 개발 빌드에서만 "멈춰 있음"(8-2). 정식 빌드에서는 숨긴다(기본 가정).
        private void RefreshPaused()
        {
            if (_paused == null) return;
            bool show = Debug.isDebugBuild && Application.isPlaying && _pause != null && _pause.IsClockPaused;
            if (_paused.gameObject.activeSelf != show) _paused.gameObject.SetActive(show);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovering = true;
            _pointer = eventData.position;
            ShowTooltip(_pointer);
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            _pointer = eventData.position;
            if (_hovering) ShowTooltip(_pointer);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovering = false;
            if (_tooltip != null) _tooltip.Hide();
        }

        private void ShowTooltip(Vector2 screenPosition)
        {
            if (_tooltip == null || Theme == null || Theme.Texts == null) return;
            var texts = Theme.Texts;
            string period = texts.Get(IsNight ? UiTextKeys.TimeNight : UiTextKeys.TimeAfterSchool);
            string text = string.Format(texts.Get(UiTextKeys.TimeTooltip), period, (Hour % 24).ToString("00"));
            _tooltip.Show(text, screenPosition, Theme.Time.TooltipOffset);
        }

        private static void SetSprite(Image image, Sprite sprite, Vector2Int position)
        {
            if (image == null) return;
            image.sprite = sprite;
            image.enabled = sprite != null;
            var size = SpriteSize(sprite);
            PixelLayout.PlaceTopLeft(image.rectTransform, position.x, position.y, size.x, size.y);
        }

        // 글자는 다른 HUD 글자와 같은 크기로 보이도록 묶음 배수를 되돌린다(1/배수). 위치는 1배 px, 칸 크기는 배수만큼 넓힌다.
        private static void PlaceText(ThemedText text, Vector2Int position, Vector2Int size, float scale)
        {
            if (text == null) return;
            var rect = (RectTransform)text.transform;
            PixelLayout.PlaceTopLeft(rect, position.x, position.y, Mathf.RoundToInt(size.x * scale), Mathf.RoundToInt(size.y * scale));
            rect.localScale = new Vector3(1f / scale, 1f / scale, 1f);
        }

        private static Vector2Int SpriteSize(Sprite sprite) =>
            sprite != null ? new Vector2Int(Mathf.RoundToInt(sprite.rect.width), Mathf.RoundToInt(sprite.rect.height)) : Vector2Int.zero;
    }
}
