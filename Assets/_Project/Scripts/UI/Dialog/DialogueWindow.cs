// 초상화 대사창(UI_SPEC 6장): 화면 아래 가운데, 패널 위에 왼쪽 열(초상화 창 + 이름표)을 4px 겹쳐 그린다. 타이핑 → 전체 표시 → 다음 대사.
// Space/E(화면 스택 Submit)나 대사창 클릭으로 진행하고, 같은 프레임에 두 번 들어온 입력은 한 번만 처리한다. ESC·바깥 클릭으로는 닫히지 않는다.
// 마지막 대사 뒤 DialogueChannel.Finish로 결과를 반영하고, 그 콜백이 다음 대사·선택지를 열지 않았으면 닫는다. 선택지(6-4)는 질문 한 줄 뒤 글자 버튼으로. 연출은 unscaled time.
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class DialogueWindow : UiScreen, IGameContextBinder, IPointerClickHandler
    {
        [SerializeField] private UiTheme _theme;
        [SerializeField, Tooltip("대사창 묶음(그림 기준 1배 좌표, 배수만큼 키움)")] private RectTransform _frame;
        [SerializeField] private Image _panel;
        [SerializeField, Tooltip("초상화 뒤 단색 배경 (테마 Portrait Backing Color)")] private Image _portraitBacking;
        [SerializeField] private Image _portrait;
        [SerializeField] private Image _leftColumn;
        [SerializeField] private ThemedText _name;
        [SerializeField, Tooltip("작은 이름 글씨를 1px 겹쳐 굵게 보이게 하는 사본")] private ThemedText _nameBold;
        [SerializeField] private ThemedText _text;
        [SerializeField] private Image _nextArrow;
        [SerializeField, Tooltip("선택지 한 줄 원본 (꺼 둔 채, 필요한 만큼 복제)")] private DialogueChoiceRow _choiceRowTemplate;

        private enum Phase { Typing, Complete }

        private DialogueChannel _channel;
        private ScreenStack _screens;
        private IReadOnlyList<DialogueLine> _lines;
        private int _index;
        private Phase _phase;
        private float _visible;
        private int _total;
        private int _lastInputFrame = -1;
        private float _openTime;
        private float _arrowTime;
        private Vector2Int _framePosition;
        private int _previewExpression; // 개발용 표정 넘기기: 0 = 기본, 1~ = 화자의 표정 목록 순서
        private readonly List<DialogueChoiceRow> _rows = new List<DialogueChoiceRow>();
        private IReadOnlyList<DialogueOption> _options = System.Array.Empty<DialogueOption>();
        private int _selected;
        private bool _choicesShown;

        private bool Asking => _options.Count > 0;

        public void Bind(GameContext context)
        {
            _channel = context.Dialogue;
            _screens = context.Screens;
            _channel.RegisterPresenter();
            _channel.Started += OnStarted;
            _channel.PreviewExpressionRequested += OnPreviewExpression;
            _screens.Submitted += OnSubmit;
            _screens.CancelIgnored += OnCancelIgnored;
            _screens.Navigated += OnNavigated;
        }

        private void OnDestroy()
        {
            if (_channel != null)
            {
                _channel.Started -= OnStarted;
                _channel.PreviewExpressionRequested -= OnPreviewExpression;
            }
            if (_screens != null)
            {
                _screens.Submitted -= OnSubmit;
                _screens.CancelIgnored -= OnCancelIgnored;
                _screens.Navigated -= OnNavigated;
            }
        }

        private void OnStarted()
        {
            if (this == null) { OnDestroy(); return; } // 꺼진 채 씬이 내려간 구독자
            _lines = _channel.Lines;
            _options = _channel.Options;
            _index = 0;
            _lastInputFrame = Time.frameCount; // 대사를 연 그 입력(E)이 바로 진행으로 이어지지 않게
            // 이전 대사·선택지에서 이어지면 창은 열린 그대로 다음 내용만 보여 준다.
            if (IsShown)
            {
                LayoutText();
                ShowLine();
            }
            else _screens.Open(GameScreen.Dialogue);
        }

        protected override void OnShown()
        {
            Layout();
            _openTime = 0f;
            GetComponent<CanvasGroup>().alpha = 0f; // 첫 프레임부터 페이드로 시작
            ShowLine();
        }

        private void OnSubmit()
        {
            if (this == null) { OnDestroy(); return; }
            if (IsShown && _screens.Top == GameScreen.Dialogue) Advance();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // 선택지가 떠 있을 때 패널 빈 곳 클릭은 무시한다(선택지 줄을 눌러야 고름).
            if (eventData.button == PointerEventData.InputButton.Left && !_choicesShown) Advance();
        }

        // ESC: 대사창은 닫히지 않는다. 선택지에 "그만두기·돌아가기"가 정해져 있으면 그것을 고른다.
        private void OnCancelIgnored()
        {
            if (this == null) { OnDestroy(); return; }
            if (!IsShown || _screens.Top != GameScreen.Dialogue || !Asking || _channel.CancelOption < 0) return;
            Choose(_channel.CancelOption);
        }

        // W/S·↑/↓: 선택지 사이를 한 칸씩 (끝에서 처음으로 돈다).
        private void OnNavigated(int step)
        {
            if (this == null) { OnDestroy(); return; }
            if (!IsShown || !_choicesShown || _screens.Top != GameScreen.Dialogue) return;
            int count = _options.Count;
            Select(((_selected + step) % count + count) % count);
        }

        private void Select(int index)
        {
            _selected = index;
            for (int i = 0; i < _rows.Count; i++) _rows[i].SetSelected(i == index);
        }

        private void Choose(int index)
        {
            if (!Asking || index < 0 || index >= _options.Count) return;
            _lastInputFrame = Time.frameCount;
            HideChoices();
            _lines = null;
            _options = System.Array.Empty<DialogueOption>();
            _channel.Choose(index); // 콜백이 다음 대사·선택지를 열면 OnStarted로 이어진다
            if (!_channel.IsPlaying) _screens.Close(GameScreen.Dialogue);
        }

        private void OnRowHovered(DialogueChoiceRow row)
        {
            if (_choicesShown) Select(row.Index);
        }

        private void OnRowClicked(DialogueChoiceRow row)
        {
            if (_choicesShown && Time.frameCount != _lastInputFrame) Choose(row.Index);
        }

        // 개발용(치트 키): 지금 화자의 기본 → 표정 목록 순서로 초상화만 바꿔 본다. 다음 대사로 넘어가면 그 줄의 표정으로 돌아간다.
        private void OnPreviewExpression(int step)
        {
            if (this == null) { OnDestroy(); return; }
            if (!IsShown || _lines == null || _index >= _lines.Count) return;
            var speaker = _lines[_index].Speaker;
            if (speaker == null) return;
            int count = speaker.ExpressionCount + 1;
            _previewExpression = ((_previewExpression + step) % count + count) % count;
            string key = _previewExpression == 0 ? null : speaker.ExpressionKey(_previewExpression - 1);
            var portrait = speaker.Portrait(key);
            _portrait.sprite = portrait != null ? portrait : _theme.Dialogue.PlaceholderPortrait;
            _portrait.enabled = _portrait.sprite != null;
            Debug.Log($"[치트] 초상화 표정: {(key ?? "기본")} ({_previewExpression + 1}/{count})");
        }

        private static int ExpressionIndex(SpeakerProfile speaker, string expression)
        {
            if (speaker == null || string.IsNullOrEmpty(expression)) return 0;
            for (int i = 0; i < speaker.ExpressionCount; i++)
                if (speaker.ExpressionKey(i) == expression) return i + 1;
            return 0;
        }

        // 타이핑 중이면 전체 표시, 전체 표시면 다음 대사, 마지막이면 닫고 결과 반영.
        private void Advance()
        {
            if (_lines == null || Time.frameCount == _lastInputFrame) return;
            _lastInputFrame = Time.frameCount;
            if (_phase == Phase.Typing)
            {
                Complete();
                return;
            }
            if (Asking)
            {
                Choose(_selected); // Space/E = 지금 고른 선택지
                return;
            }
            _index++;
            if (_index < _lines.Count)
            {
                ShowLine();
                return;
            }
            _lines = null;
            _channel.Finish(); // 결과 반영. 콜백이 다음 대사·선택지를 열면 창은 그대로 이어진다
            if (!_channel.IsPlaying) _screens.Close(GameScreen.Dialogue);
        }

        private void ShowLine()
        {
            if (_lines == null || _index >= _lines.Count) return;
            var values = _theme.Dialogue;
            var line = _lines[_index];

            var portrait = line.Speaker != null ? line.Speaker.Portrait(line.Expression) : null;
            _portrait.sprite = portrait != null ? portrait : values.PlaceholderPortrait;
            _portrait.enabled = _portrait.sprite != null;
            _previewExpression = ExpressionIndex(line.Speaker, line.Expression);
            FitName(line.Speaker != null ? line.Speaker.DisplayName : string.Empty, values);

            HideChoices();
            var area = Asking ? values.ChoicePromptArea : values.TextArea;
            int maxLines = Asking ? 1 : values.MaxLines;
            var label = _text.GetComponent<TMP_Text>();
            string wrapped = UiTextWrap.ByWords(label, line.Text ?? string.Empty, area.width);
            _text.SetText(wrapped);
            label.ForceMeshUpdate();
            if (label.textInfo.lineCount > maxLines)
                Debug.LogWarning($"[UI] {(Asking ? "선택지 질문" : "대사")}이 {maxLines}줄을 넘습니다({label.textInfo.lineCount}줄): {line.Text}");
            _total = label.textInfo.characterCount;
            _visible = 0f;
            label.maxVisibleCharacters = 0;
            _phase = Phase.Typing;
            _nextArrow.enabled = false;
        }

        private void Complete()
        {
            _phase = Phase.Complete;
            _text.GetComponent<TMP_Text>().maxVisibleCharacters = _total;
            if (Asking)
            {
                _nextArrow.enabled = false;
                ShowChoices();
                return;
            }
            _arrowTime = 0f;
            _nextArrow.enabled = _nextArrow.sprite != null;
            PlaceArrow(0);
        }

        // 질문을 다 보여 준 뒤 선택지 줄을 놓는다 (패널 기준 1배 px). 첫 줄을 고른 상태로 시작.
        private void ShowChoices()
        {
            var values = _theme.Dialogue;
            if (_options.Count > values.MaxChoices)
                Debug.LogWarning($"[UI] 선택지가 {values.MaxChoices}줄을 넘습니다({_options.Count}개). 패널 밖으로 나갈 수 있습니다.");
            int panelX = PanelX(values);
            var prompt = values.ChoicePromptArea;
            for (int i = 0; i < _options.Count; i++)
            {
                var row = Row(i);
                row.Index = i;
                row.gameObject.SetActive(true);
                var option = _options[i];
                var rect = new RectInt(panelX + prompt.x, values.ChoiceTop + i * values.ChoiceRowHeight, prompt.width, values.ChoiceRowHeight);
                row.Setup(values, option.Label, option.Detail, option.Dimmed, rect);
            }
            _choicesShown = true;
            Select(0);
        }

        private void HideChoices()
        {
            _choicesShown = false;
            foreach (var row in _rows)
            {
                row.SetSelected(false);
                row.gameObject.SetActive(false);
            }
        }

        private DialogueChoiceRow Row(int index)
        {
            while (_rows.Count <= index)
            {
                var row = Instantiate(_choiceRowTemplate, _choiceRowTemplate.transform.parent);
                row.name = $"Choice{_rows.Count}";
                row.Hovered += OnRowHovered;
                row.Clicked += OnRowClicked;
                _rows.Add(row);
            }
            return _rows[index];
        }

        private void Update()
        {
            if (!IsShown || _theme == null) return;
            var values = _theme.Dialogue;
            float dt = Time.unscaledDeltaTime;

            // 열기: 아래에서 OpenRise px 올라오며 페이드 (정수 px).
            if (_openTime < values.OpenSeconds)
            {
                _openTime += dt;
                float t = values.OpenSeconds > 0f ? Mathf.Clamp01(_openTime / values.OpenSeconds) : 1f;
                GetComponent<CanvasGroup>().alpha = t;
                int rise = Mathf.RoundToInt(values.OpenRise * (1f - t) * values.Scale);
                _frame.anchoredPosition = new Vector2(_framePosition.x, _framePosition.y - rise);
            }

            if (_phase == Phase.Typing)
            {
                _visible += values.CharsPerSecond * dt;
                int shown = Mathf.Min(_total, Mathf.FloorToInt(_visible));
                _text.GetComponent<TMP_Text>().maxVisibleCharacters = shown;
                if (shown >= _total) Complete();
            }
            else if (_nextArrow.enabled)
            {
                // 전체 표시 뒤 ▼를 1px씩 끊어서 위아래로.
                _arrowTime += dt;
                PlaceArrow(Mathf.FloorToInt(_arrowTime / values.ArrowStepSeconds) % 2);
            }
        }

        // 묶음 배치: 화면 아래 가운데(왼쪽 끝 기준 정수 px), 안은 그림 기준 1배 좌표.
        private void Layout()
        {
            var values = _theme.Dialogue;
            var root = (RectTransform)transform;
            PixelLayout.Stretch(root);
            var column = Size(values.LeftColumn, values.LeftColumnSize);
            var panel = Size(values.Panel, values.PanelSize);
            int panelX = column.x - values.Overlap;
            int width = panelX + panel.x, height = Mathf.Max(column.y, panel.y);
            float scale = values.Scale;

            int screenWidth = Mathf.RoundToInt(root.rect.width);
            _framePosition = new Vector2Int(Mathf.FloorToInt((screenWidth - width * scale) / 2f), values.BottomMargin);
            PixelLayout.PlaceBottomLeft(_frame, _framePosition.x, _framePosition.y, width, height);
            _frame.localScale = new Vector3(scale, scale, 1f);

            // 그리는 순서(자식 순서): 패널 → 초상화 배경 → 초상화 → 왼쪽 열 → 이름 → 대사 → ▼.
            SetImage(_panel, values.Panel, panelX, 0, panel, raycast: true);
            if (_portraitBacking != null)
            {
                _portraitBacking.sprite = null;
                _portraitBacking.color = values.PortraitBackingColor;
                _portraitBacking.raycastTarget = false;
                PixelLayout.PlaceTopLeft(_portraitBacking.rectTransform, values.PortraitPosition.x, values.PortraitPosition.y, values.PortraitSize.x, values.PortraitSize.y);
            }
            SetImage(_portrait, _portrait.sprite, values.PortraitPosition.x, values.PortraitPosition.y, values.PortraitSize, raycast: false);
            SetImage(_leftColumn, values.LeftColumn, 0, 0, column, raycast: true);
            LayoutText();
            if (_choiceRowTemplate != null) _choiceRowTemplate.gameObject.SetActive(false);
            var arrow = Size(values.NextArrow, values.ArrowSize);
            _nextArrow.sprite = values.NextArrow;
            _nextArrow.raycastTarget = false;
            _nextArrow.rectTransform.sizeDelta = arrow;
        }

        // 본문 칸: 대사면 3줄 칸, 선택지면 질문 한 줄 칸.
        private void LayoutText()
        {
            var values = _theme.Dialogue;
            var area = Asking ? values.ChoicePromptArea : values.TextArea;
            PixelLayout.PlaceTopLeft((RectTransform)_text.transform, PanelX(values) + area.x, area.y, area.width, area.height);
        }

        private static int PanelX(DialogueValues values) => Size(values.LeftColumn, values.LeftColumnSize).x - values.Overlap;

        private void PlaceArrow(int bob)
        {
            var values = _theme.Dialogue;
            var column = Size(values.LeftColumn, values.LeftColumnSize);
            int panelX = column.x - values.Overlap;
            var arrow = Size(values.NextArrow, values.ArrowSize);
            PixelLayout.PlaceTopLeft(_nextArrow.rectTransform, panelX + values.ArrowPosition.x, values.ArrowPosition.y + bob, arrow.x, arrow.y);
        }

        // 이름표: 큰 글씨부터 차례로 넣어 보고 이름표 폭(여백 제외)에 들어가는 첫 크기를 쓴다. 굵은체가 없는 작은 글씨는 1px 겹쳐 굵게.
        // 마지막 크기로도 넘치면 … 으로 자른다. 글자 모양 기준으로 이름표 가운데(정수 px).
        private void FitName(string name, DialogueValues values)
        {
            var plate = values.NamePlate;
            int maxWidth = plate.width - values.NamePadding * 2;
            var label = _name.GetComponent<TMP_Text>();
            UiTextStyle chosen = values.NameStyles.Count > 0 ? values.NameStyles[0] : UiTextStyle.BodyBold;
            bool fits = false;
            foreach (var style in values.NameStyles)
            {
                chosen = style;
                _name.SetStyle(style);
                if (label.GetPreferredValues(name).x <= maxWidth + 0.5f) { fits = true; break; } // 0.01px 같은 반올림 오차는 무시
            }
            label.overflowMode = fits ? TextOverflowModes.Overflow : TextOverflowModes.Ellipsis;
            _name.SetText(name);

            var styleValues = _theme.TextStyle(chosen);
            bool pixelBold = styleValues != null && !styleValues.Bold;
            _nameBold.gameObject.SetActive(pixelBold);
            if (pixelBold)
            {
                _nameBold.SetStyle(chosen);
                _nameBold.SetText(name);
                _nameBold.GetComponent<TMP_Text>().overflowMode = label.overflowMode;
            }

            var rect = (RectTransform)_name.transform;
            PixelLayout.PlaceTopLeft(rect, plate.x + values.NamePadding, plate.y, maxWidth, plate.height);
            label.alignment = TextAlignmentOptions.Center;
            label.ForceMeshUpdate();
            var bounds = label.textBounds;
            if (bounds.size.x > 0f && TryInkRows(label, out float inkTop, out float inkBottom))
            {
                // 세로는 textBounds(줄 높이 포함)가 아니라 실제 글자 모양 위·아래로 가운데 맞춤.
                int w = Mathf.RoundToInt(bounds.size.x) + (pixelBold ? 1 : 0), h = Mathf.RoundToInt(inkTop - inkBottom);
                int left = plate.x + (plate.width - w) / 2, top = plate.y + (plate.height - h) / 2;
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x + (left - (plate.x + values.NamePadding + bounds.min.x)),
                    rect.anchoredPosition.y - (top - (plate.y - inkTop)));
            }
            if (pixelBold)
            {
                var copy = (RectTransform)_nameBold.transform;
                copy.anchorMin = rect.anchorMin; copy.anchorMax = rect.anchorMax; copy.pivot = rect.pivot;
                copy.sizeDelta = rect.sizeDelta;
                copy.anchoredPosition = rect.anchoredPosition + new Vector2(1f, 0f);
                _nameBold.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;
            }
        }

        // 보이는 글자들의 글리프 모양 맨 위·맨 아래 (글자 칸 기준 y, 위가 +).
        private static bool TryInkRows(TMP_Text label, out float top, out float bottom)
        {
            top = float.MinValue;
            bottom = float.MaxValue;
            var info = label.textInfo;
            for (int i = 0; i < info.characterCount; i++)
            {
                var ch = info.characterInfo[i];
                if (!ch.isVisible || ch.textElement == null || ch.textElement.glyph == null) continue;
                var metrics = ch.textElement.glyph.metrics;
                float glyphTop = ch.baseLine + metrics.horizontalBearingY * ch.scale;
                top = Mathf.Max(top, glyphTop);
                bottom = Mathf.Min(bottom, glyphTop - metrics.height * ch.scale);
            }
            return top > bottom;
        }

        private static void SetImage(Image image, Sprite sprite, int x, int y, Vector2Int size, bool raycast)
        {
            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.enabled = sprite != null;
            image.raycastTarget = raycast;
            PixelLayout.PlaceTopLeft(image.rectTransform, x, y, size.x, size.y);
        }

        private static Vector2Int Size(Sprite sprite, Vector2Int fallback) =>
            sprite != null ? new Vector2Int(Mathf.RoundToInt(sprite.rect.width), Mathf.RoundToInt(sprite.rect.height)) : fallback;
    }
}
