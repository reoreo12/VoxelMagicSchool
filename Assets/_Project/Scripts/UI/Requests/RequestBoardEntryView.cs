// 의뢰 게시판의 의뢰 한 칸: 왼쪽에 제목·크리처 대사·"모아 올 것" 재료·서식지 힌트, 오른쪽에 수락 버튼·보상·못 받는 이유. 위치는 정수 px.
// 게시판 전체 이유(낮·닫힌 날)는 칸마다 쓰지 않고 창 아래 안내에 한 번만 쓴다.
using System;
using System.Linq;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class RequestBoardEntryView : MonoBehaviour
    {
        [SerializeField] private ThemedText _title;
        [SerializeField] private ThemedText _line;
        [SerializeField] private ThemedText _needsLabel;
        [SerializeField] private ThemedText _needs;
        [SerializeField] private ThemedText _hint;
        [SerializeField] private BookMenuButton _accept;
        [SerializeField] private ThemedText _reward;
        [SerializeField] private ThemedText _reason;

        private RequestDefinition _request;

        public RectTransform Rect => (RectTransform)transform;
        public event Action<RequestDefinition> AcceptClicked;

        private void Awake()
        {
            _accept.Clicked += () => { if (_request != null) AcceptClicked?.Invoke(_request); };
        }

        /// <summary>내용을 채우고 이 칸의 높이를 돌려준다(대사가 길면 낱말 단위로 줄바꿈해 그만큼 높아진다).</summary>
        public int Show(RequestDefinition request, int reward, AcceptResult canAccept, bool boardWideReason, UiTheme theme, int width)
        {
            _request = request;
            var values = theme.RequestBoard;
            int title = Line(theme, values.TitleStyle), body = Line(theme, values.BodyStyle), small = Line(theme, values.SmallStyle);
            int right = values.RightColumn;
            int leftWidth = width - right - 8;

            int y = 0;
            Place(_title, values.TitleStyle, request.Title, 0, y, leftWidth, title);
            y += title + 1;
            if (_line.Style != values.BodyStyle) _line.SetStyle(values.BodyStyle);
            string wrapped = UiTextWrap.ByWords(_line.GetComponent<TMPro.TMP_Text>(), $"“{request.CreatureLine}”", leftWidth);
            int lines = 1;
            foreach (char c in wrapped) if (c == '\n') lines++;
            Place(_line, values.BodyStyle, wrapped, 0, y, leftWidth, body * lines);
            y += body * lines + 1;

            var label = _needsLabel.GetComponent<TMPro.TMP_Text>();
            Place(_needsLabel, values.BodyStyle, GameTexts.NeedsLabel, 0, y, leftWidth, body);
            _needsLabel.SetColorOverride(true, values.MutedColor);
            int labelWidth = Mathf.CeilToInt(label.GetPreferredValues(GameTexts.NeedsLabel).x) + 6;
            string needs = string.Join(", ", request.Requirements.Where(r => r.Item != null).Select(r => $"{r.Item.DisplayName} {r.Amount}"));
            Place(_needs, values.BodyStyle, needs, labelWidth, y, leftWidth - labelWidth, body);
            y += body + 1;
            Place(_hint, values.SmallStyle, Hint(request), 0, y, leftWidth, small);
            _hint.SetColorOverride(true, values.MutedColor);

            // 오른쪽: 수락 버튼 → 보상 → 이유 (버튼 가운데 기준)
            var size = values.AcceptSize;
            int bx = width - right + (right - size.x) / 2;
            bool accepted = canAccept == AcceptResult.AlreadyAccepted;
            _accept.Show(accepted ? GameTexts.Accepted : GameTexts.Accept, theme.Book, bx, 1, size.x, size.y);
            _accept.SetInteractable(canAccept == AcceptResult.Success, values.MutedColor);
            Place(_reward, values.BodyStyle, string.Format(GameTexts.RewardShortWritten, reward), width - right, size.y + 4, right, body);
            _reward.SetColorOverride(true, values.RewardColor);
            string reason = boardWideReason ? string.Empty : ShortReason(canAccept);
            // 이유는 오른쪽 칸보다 길 수 있어 오른쪽 끝에 맞추고 왼쪽으로 넘친다.
            Place(_reason, values.SmallStyle, reason, width - right - 40, size.y + 4 + body + 2, right + 40, small);
            _reason.SetColorOverride(true, values.WarnColor);

            int height = values.EntryHeight + body * (lines - 1);
            Rect.sizeDelta = new Vector2(width, height);
            return height;
        }

        private static string ShortReason(AcceptResult result) => result switch
        {
            AcceptResult.SlotsFull => GameTexts.RequestSlotsFullShort,
            AcceptResult.InventoryFull => GameTexts.RequestNoRoomShort,
            _ => string.Empty
        };

        // 의뢰에 힌트가 없으면 채집 재료의 힌트를 모은다(정확한 위치는 알려 주지 않는다).
        private static string Hint(RequestDefinition request)
        {
            if (!string.IsNullOrEmpty(request.HabitatHint)) return request.HabitatHint;
            return string.Join(" / ", request.Requirements.Select(r => r.Item).OfType<GatherMaterialDefinition>()
                .Select(m => m.HabitatHint).Where(h => !string.IsNullOrEmpty(h)).Distinct());
        }

        private static void Place(ThemedText text, UiTextStyle style, string value, int x, int y, int width, int height)
        {
            if (text.Style != style) text.SetStyle(style);
            text.SetText(value);
            PixelLayout.PlaceTopLeft((RectTransform)text.transform, x, y, Mathf.Max(1, width), height);
        }

        private static int Line(UiTheme theme, UiTextStyle id)
        {
            var style = theme.TextStyle(id);
            return style != null ? style.LineHeightFrom(theme.Fonts) : 16;
        }
    }
}
