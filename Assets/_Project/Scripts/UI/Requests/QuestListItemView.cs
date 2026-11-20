// 의뢰 목록 항목 하나(UI_SPEC 10-2): 제목 한 줄 + 요구 아이템마다 한 줄 "대파 1/2". 완료(납품 가능)면 글자색을 바꾼다.
// 목록이 풀에서 재사용하므로 같은 항목이면 글자를 다시 만들지 않는다. 누르면 목록에 알린다(자세히 패널 연결 자리).
using System;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class QuestListItemView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private ThemedText _title;
        [SerializeField, Tooltip("1px 오른쪽에 겹쳐 그려 굵게 보이게 하는 제목 사본")] private ThemedText _titleBold;
        [SerializeField] private ThemedText _requirements;

        private static readonly StringBuilder Lines = new StringBuilder();
        private AcceptedRequestEntry _entry;

        public event Action<AcceptedRequestEntry> Clicked;

        public RectTransform Rect => (RectTransform)transform;
        public AcceptedRequestEntry Entry => _entry;

        /// <summary>항목 높이 = 위아래 여백 없이 제목 줄 + 아이템 줄 수 × 줄 높이.</summary>
        public static int HeightOf(AcceptedRequestEntry entry, int titleLine, int requirementLine) =>
            titleLine + entry.Requirements.Count * requirementLine;

        public void Show(AcceptedRequestEntry entry, UiTheme theme, int width, int titleLine, int requirementLine)
        {
            var values = theme.QuestList;
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
            _title.SetColorOverride(entry.IsComplete, values.CompletedColor);
            if (_titleBold != null)
            {
                _titleBold.SetColorOverride(entry.IsComplete, values.CompletedColor);
                if (_titleBold.gameObject.activeSelf != values.TitlePixelBold) _titleBold.gameObject.SetActive(values.TitlePixelBold);
                PixelLayout.PlaceTopLeft((RectTransform)_titleBold.transform, 1, 0, Mathf.Max(1, width - 1), titleLine);
            }
            _requirements.SetColorOverride(entry.IsComplete, values.CompletedColor);

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

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_entry != null && eventData.button == PointerEventData.InputButton.Left) Clicked?.Invoke(_entry);
        }
    }
}
