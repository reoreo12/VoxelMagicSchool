// 획득 창(UI_Root HudAnimated 층): 뽑거나 낚은 아이템을 화면 위 가운데 밝은 종이 창에 잠시 보인다. 왼쪽은 도는 재료 모델(툰 + 도트 외곽선),
// 모델이 없는 아이템은 아이콘을 크게. 오른쪽은 "○○ 획득!"과 설명. 크기는 데이터·규칙에 남기고 화면에는 쓰지 않는다(사용자 결정 2026-11-27).
// 움직여서 닫기 요청이 와도 최소 표시 시간은 보이고 닫는다. 보이는 중에 새로 획득하면 바로 새 아이템으로 바뀐다(시간도 처음부터).
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class AcquireView : MonoBehaviour, IGameContextBinder
    {
        [SerializeField] private UiTheme _theme;
        [SerializeField] private ThemedPanel _panel;
        [SerializeField] private RawImage _model;
        [SerializeField] private Image _icon;
        [SerializeField] private ThemedText _title;
        [SerializeField] private ThemedText _description;

        private GameMessages _messages;
        private AcquireModelStage _stage;
        private Material _outline;
        private CanvasGroup _group;
        private float _duration;
        private float _minimum;
        private float _elapsed;
        private bool _dismissRequested;
        private bool _visible;

        private RectTransform Rect => (RectTransform)transform;

        public void Bind(GameContext context)
        {
            _messages = context.Messages;
            _duration = context.Balance.AcquiredInfoSeconds;
            _minimum = context.Balance.AcquiredInfoMinSeconds;
            _messages.ItemAcquired += OnAcquired;
            _messages.AcquiredDismissed += OnDismissRequested;
            Hide();
        }

        private void OnDestroy()
        {
            if (_messages != null)
            {
                _messages.ItemAcquired -= OnAcquired;
                _messages.AcquiredDismissed -= OnDismissRequested;
            }
            if (_stage != null) Destroy(_stage.gameObject);
            if (_outline != null) Destroy(_outline);
        }

        private void OnAcquired(AcquiredInfo info)
        {
            if (this == null) { OnDestroy(); return; } // 꺼진 채 씬이 내려가 OnDestroy가 불리지 않은 구독자
            if (_theme == null || info.Item == null) return;
            Layout(info.Item);
            _elapsed = 0f;
            _dismissRequested = false;
            SetVisible(true);
        }

        // 움직이거나 낚시를 다시 던질 때: 최소 시간이 지났으면 바로, 아니면 최소 시간이 차면 닫는다.
        private void OnDismissRequested()
        {
            if (this == null || !_visible) return;
            if (_elapsed >= _minimum) Hide();
            else _dismissRequested = true;
        }

        private void Hide()
        {
            if (this == null) return;
            if (_stage != null) _stage.Clear();
            _dismissRequested = false;
            SetVisible(false);
        }

        private void Update()
        {
            if (!_visible) return;
            _elapsed += Time.deltaTime;
            if (_elapsed >= _duration || (_dismissRequested && _elapsed >= _minimum)) Hide();
        }

        private void Layout(ItemDefinition item)
        {
            var values = _theme.Acquire;
            var size = values.Size;
            var parent = Vector2Int.RoundToInt(((RectTransform)Rect.parent).rect.size);
            PixelLayout.PlaceTopLeft(Rect, PixelLayout.CenterOffset(parent.x, size.x), values.Top, size.x, size.y);
            PixelLayout.Stretch((RectTransform)_panel.transform);

            // 왼쪽 칸: 모델(렌더 텍스처 + 외곽선) 또는 아이콘
            var cell = values.CellPosition;
            int cellSize = values.CellSize;
            if (_stage == null) _stage = AcquireModelStage.Create(values);
            bool hasModel = _stage.Show(item);
            _model.gameObject.SetActive(hasModel);
            _icon.gameObject.SetActive(!hasModel);
            if (hasModel)
            {
                _model.texture = _stage.Texture;
                _model.material = OutlineMaterial(values);
                _model.raycastTarget = false;
                PixelLayout.PlaceTopLeft(_model.rectTransform, cell.x, cell.y, cellSize, cellSize);
            }
            else
            {
                int iconSize = 16 * values.IconScale;
                _icon.sprite = item.Icon;
                _icon.color = item.Icon != null ? Color.white : item.IconColor;
                _icon.preserveAspect = true;
                _icon.raycastTarget = false;
                PixelLayout.PlaceTopLeft(_icon.rectTransform, cell.x + PixelLayout.CenterOffset(cellSize, iconSize), cell.y + PixelLayout.CenterOffset(cellSize, iconSize), iconSize, iconSize);
            }

            // 오른쪽 글자: 제목 + 설명(낱말 단위 줄바꿈)
            int textX = cell.x + cellSize + values.TextGap;
            int textWidth = size.x - textX - values.TextRight;
            int titleLine = Line(values.TitleStyle), bodyLine = Line(values.DescriptionStyle);
            _title.SetStyle(values.TitleStyle);
            _title.SetText(string.Format(GameTexts.AcquiredFormat, item.DisplayName));
            PixelLayout.PlaceTopLeft((RectTransform)_title.transform, textX, values.TitleTop, textWidth, titleLine);
            _description.SetStyle(values.DescriptionStyle);
            var label = _description.GetComponent<TMP_Text>();
            string wrapped = UiTextWrap.ByWords(label, item.Description ?? string.Empty, textWidth);
            int lines = 1;
            foreach (char c in wrapped) if (c == '\n') lines++;
            _description.SetText(wrapped);
            PixelLayout.PlaceTopLeft((RectTransform)_description.transform, textX, values.TitleTop + titleLine + values.DescriptionGap, textWidth, bodyLine * lines);
        }

        private Material OutlineMaterial(AcquireValues values)
        {
            if (values.OutlineShader == null) return null;
            if (_outline == null) _outline = new Material(values.OutlineShader) { name = "AcquireOutline" };
            _outline.SetColor("_OutlineColor", values.OutlineColor);
            _outline.SetFloat("_OutlineWidth", values.OutlineWidth);
            return _outline;
        }

        private void SetVisible(bool visible)
        {
            _visible = visible;
            if (_group == null) _group = GetComponent<CanvasGroup>();
            _group.alpha = visible ? 1f : 0f;
            _group.blocksRaycasts = false;
            _group.interactable = false;
        }

        private int Line(UiTextStyle id)
        {
            var style = _theme.TextStyle(id);
            return style != null ? style.LineHeightFrom(_theme.Fonts) : 16;
        }
    }
}
