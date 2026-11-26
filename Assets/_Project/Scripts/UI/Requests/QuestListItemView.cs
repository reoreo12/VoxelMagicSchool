// 의뢰 목록 항목 하나(UI_SPEC 10-2): 제목 한 줄 + 요구 아이템마다 한 줄 "대파 1/2". 완료(납품 가능)면 글자색을 바꾼다.
// 목록이 풀에서 재사용하므로 같은 항목이면 글자를 다시 만들지 않는다. 마우스를 올리면 옅은 바탕 + 강조색, 누르면 목록에 알린다(책이 열림).
using System;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class QuestListItemView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private ThemedText _title;
        [SerializeField, Tooltip("1px 오른쪽에 겹쳐 그려 굵게 보이게 하는 제목 사본")] private ThemedText _titleBold;
        [SerializeField] private ThemedText _requirements;

        private static readonly StringBuilder Lines = new StringBuilder();
        private AcceptedRequestEntry _entry;
        private UiTheme _theme;
        private bool _hovered;
        private bool _pressed;

        public event Action<AcceptedRequestEntry> Clicked;

        public RectTransform Rect => (RectTransform)transform;
        public AcceptedRequestEntry Entry => _entry;

        /// <summary>항목 높이 = 위아래 여백 없이 제목 줄 + 아이템 줄 수 × 줄 높이.</summary>
        public static int HeightOf(AcceptedRequestEntry entry, int titleLine, int requirementLine) =>
            titleLine + entry.Requirements.Count * requirementLine;

        public void Show(AcceptedRequestEntry entry, UiTheme theme, int width, int titleLine, int requirementLine)
        {
            var values = theme.QuestList;
            _theme = theme;
            SetStyle(_title, values.TitleStyle);
            SetStyle(_titleBold, values.TitleStyle);
            SetStyle(_requirements, values.RequirementStyle);
            if (_entry != entry)
            {
                _entry = entry;
                string titleFormat = theme.Texts != null ? theme.Texts.Get(UiTextKeys.QuestItemTitle) : "{0}";
                string title = string.Format(titleFormat, entry.Title);
                _title.SetText(title);
                if (_titleBold != null) _titleBold.SetText(title);
                Lines.Clear();
                string format = theme.Texts != null ? theme.Texts.Get(UiTextKeys.QuestRequirement) : "{0} {1}/{2}";
                for (int i = 0; i < entry.Requirements.Count; i++)
                {
                    var r = entry.Requirements[i];
                    if (i > 0) Lines.Append('\n');
                    Lines.AppendFormat(format, r.Item != null ? r.Item.DisplayName : "?", Mathf.Min(r.Current, r.Required), r.Required);
                }
                _requirements.SetText(Lines.ToString());
            }
            ApplyState();
            if (_titleBold != null)
            {
                if (_titleBold.gameObject.activeSelf != values.TitlePixelBold) _titleBold.gameObject.SetActive(values.TitlePixelBold);
                PixelLayout.PlaceTopLeft((RectTransform)_titleBold.transform, 1, 0, Mathf.Max(1, width - 1), titleLine);
            }

            int height = HeightOf(entry, titleLine, requirementLine);
            Rect.sizeDelta = new Vector2(width, height);
            PixelLayout.PlaceTopLeft((RectTransform)_title.transform, 0, 0, Mathf.Max(1, width - 1), titleLine);
            int indent = values.RequirementIndent;
            PixelLayout.PlaceTopLeft((RectTransform)_requirements.transform, indent, titleLine, Mathf.Max(1, width - indent),
                Mathf.Max(1, entry.Requirements.Count * requirementLine));
        }

        // 테마에서 글자 스타일을 바꾸면 따른다. 같으면 다시 입히지 않는다(스크롤마다 불림).
        private static void SetStyle(ThemedText text, UiTextStyle style)
        {
            if (text != null && text.Style != style) text.SetStyle(style);
        }

        // 글자색: 누름 > 올림 > 완료 > 기본. 바탕: 올렸을 때만 옅은 색(평소엔 투명한 누르기 영역).
        private void ApplyState()
        {
            if (_theme == null || _entry == null) return;
            var values = _theme.QuestList;
            bool highlight = _hovered || _pressed;
            bool use = highlight || _entry.IsComplete;
            var color = _pressed ? values.PressedColor : _hovered ? values.HoverColor : values.CompletedColor;
            _title.SetColorOverride(use, color);
            if (_titleBold != null) _titleBold.SetColorOverride(use, color);
            _requirements.SetColorOverride(use, color);
            var hit = GetComponent<Image>();
            if (hit != null) hit.color = highlight ? values.HoverBacking : new Color(1f, 1f, 1f, 0f);
        }

        public void OnPointerEnter(PointerEventData eventData) { _hovered = true; ApplyState(); }
        public void OnPointerExit(PointerEventData eventData) { _hovered = false; _pressed = false; ApplyState(); }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            _pressed = true;
            ApplyState();
        }

        public void OnPointerUp(PointerEventData eventData) { _pressed = false; ApplyState(); }

        private void OnDisable() { _hovered = false; _pressed = false; }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_entry != null && eventData.button == PointerEventData.InputButton.Left) Clicked?.Invoke(_entry);
        }
    }
}
