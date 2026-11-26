// 의뢰 게시판 화면(UI_Root 팝업 층): 낡은 종이 창에 오늘 게시판 의뢰를 칸마다 보여 주고, 수락·씨앗 다시 받기는 RequestLog에 요청한다.
// 게시판 전체 안내(낮·닫힌 날·빈 게시판·칸 가득·씨앗 결과)는 아래 한 줄, 없으면 "받은 의뢰 n/m". 창 높이는 의뢰 수에 맞추고 화면 가운데 정수 px.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class RequestBoardScreen : UiScreen, IGameContextBinder
    {
        [SerializeField] private UiTheme _theme;
        [SerializeField] private RectTransform _window;
        [SerializeField] private ThemedText _title;
        [SerializeField] private ThemedButton _close;
        [SerializeField] private Image _headerLine;
        [SerializeField] private RectTransform _list;
        [SerializeField] private RequestBoardEntryView _entryTemplate;
        [SerializeField] private Image _footerLine;
        [SerializeField] private BookMenuButton _refill;
        [SerializeField] private ThemedText _info;

        private readonly List<RequestBoardEntryView> _entries = new List<RequestBoardEntryView>();
        private readonly List<Image> _dividers = new List<Image>();
        private RequestLog _log;
        private RequestBoardState _board;
        private ScreenStack _screens;
        private string _notice;

        public void Bind(GameContext context)
        {
            _log = context.Requests;
            _board = context.Board;
            _screens = context.Screens;
            _close.Button.onClick.RemoveAllListeners();
            _close.Button.onClick.AddListener(() => _screens.Close(GameScreen.RequestBoard));
            _refill.Clicked -= OnRefill;
            _refill.Clicked += OnRefill;
            if (_entryTemplate != null) _entryTemplate.gameObject.SetActive(false);
            // 의뢰·게시판은 씬을 넘어 유지되므로 OnDestroy에서 꼭 구독을 푼다.
            _log.AcceptedListChanged += OnChanged;
            if (_board != null) _board.OffersChanged += OnChanged;
        }

        private void OnDestroy()
        {
            if (_log != null) _log.AcceptedListChanged -= OnChanged;
            if (_board != null) _board.OffersChanged -= OnChanged;
        }

        private void OnChanged()
        {
            if (this == null) { OnDestroy(); return; } // 꺼진 채 씬이 내려가 OnDestroy가 불리지 않은 구독자
            if (IsShown) Layout();
        }

        protected override void OnShown()
        {
            _notice = null;
            Layout();
        }

        private void OnAccept(RequestDefinition request)
        {
            var result = _log.Accept(request);
            _notice = result switch
            {
                AcceptResult.Success => null,
                AcceptResult.SlotsFull => GameTexts.RequestSlotsFullShort, // 긴 설명은 아래 한 줄에 들어가지 않는다
                _ => RequestEntryView.Reason(result, _log.MaxAccepted)
            };
            Layout();
        }

        private void OnRefill()
        {
            if (_log.RefillableSeeds().Count == 0) _notice = GameTexts.SeedRefillNone;
            else
            {
                int given = _log.GiveRefillSeeds();
                _notice = given > 0 ? string.Format(GameTexts.SeedRefillGivenFormat, given) : GameTexts.SeedRefillNoRoom;
            }
            Layout();
        }

        private void Layout()
        {
            if (_theme == null || _log == null) return;
            var values = _theme.RequestBoard;
            var offers = _board != null ? _board.Offers : (IReadOnlyList<RequestDefinition>)new RequestDefinition[0];
            int pad = values.Padding, gap = values.LineGap, width = values.Width, inner = width - pad * 2;
            int titleLine = Line(values.TitleStyle), bodyLine = Line(values.BodyStyle);
            int footer = Mathf.Max(values.RefillSize.y, bodyLine);
            var access = _log.BoardAccess();
            bool boardWide = access != QuestAccess.Open;

            // 의뢰 칸부터 채워 높이를 안다(대사가 길면 그 칸만 높아진다). 빈 게시판도 한 칸 높이는 비워 둔다.
            while (_entries.Count < offers.Count)
            {
                var entry = Instantiate(_entryTemplate, _list);
                entry.name = $"Entry{_entries.Count + 1}";
                entry.AcceptClicked += OnAccept;
                _entries.Add(entry);
            }
            int listHeight = 0;
            for (int i = 0; i < _entries.Count; i++)
            {
                bool used = i < offers.Count;
                if (_entries[i].gameObject.activeSelf != used) _entries[i].gameObject.SetActive(used);
                if (!used) continue;
                if (i > 0)
                {
                    if (_dividers.Count < i) _dividers.Add(NewDivider());
                    SetLine(_dividers[i - 1], 0, listHeight + values.EntryGap / 2, inner);
                    _dividers[i - 1].gameObject.SetActive(true);
                    listHeight += values.EntryGap;
                }
                int h = _entries[i].Show(offers[i], _log.RewardOf(offers[i]), _log.CanAccept(offers[i]), boardWide, _theme, inner);
                PixelLayout.PlaceTopLeft(_entries[i].Rect, 0, listHeight, inner, h);
                listHeight += h;
            }
            for (int i = Mathf.Max(0, offers.Count - 1); i < _dividers.Count; i++) _dividers[i].gameObject.SetActive(false);
            if (offers.Count == 0) listHeight = values.EntryHeight;
            int height = pad + titleLine + gap + 1 + gap + listHeight + gap + 1 + gap + footer + pad;

            var parent = Vector2Int.RoundToInt(Root.rect.size);
            PixelLayout.PlaceTopLeft(_window, PixelLayout.CenterOffset(parent.x, width), PixelLayout.CenterOffset(parent.y, height), width, height);

            // 머리: 제목 가운데 + 오른쪽 위 X + 구분선
            _title.SetStyle(values.TitleStyle);
            _title.SetText(GameTexts.BoardTitle);
            PixelLayout.PlaceTopLeft((RectTransform)_title.transform, pad, pad, inner, titleLine);
            var closeSize = Vector2Int.RoundToInt(((RectTransform)_close.transform).sizeDelta);
            PixelLayout.PlaceTopLeft((RectTransform)_close.transform, width - pad - closeSize.x, pad + PixelLayout.CenterOffset(titleLine, closeSize.y), 0, 0);
            int y = pad + titleLine + gap;
            SetLine(_headerLine, pad, y, inner);
            y += 1 + gap;
            PixelLayout.PlaceTopLeft(_list, pad, y, inner, listHeight);
            y += listHeight + gap;
            SetLine(_footerLine, pad, y, inner);
            y += 1 + gap;

            // 아래: 씨앗 다시 받기 + 안내(있으면 경고색, 없으면 받은 의뢰 수)
            var refill = values.RefillSize;
            _refill.Show(GameTexts.SeedRefill, _theme.Book, pad, y + (footer - refill.y) / 2, refill.x, refill.y);
            _refill.SetInteractable(_log.AcceptedCount > 0, values.MutedColor);
            string notice = Notice(access, offers.Count);
            _info.SetStyle(values.BodyStyle);
            _info.SetText(notice ?? string.Format(GameTexts.BoardSlotsFormat, _log.AcceptedCount, _log.MaxAccepted));
            _info.SetColorOverride(notice != null, values.WarnColor);
            int infoX = pad + refill.x + 8, infoWidth = width - pad - infoX;
            // 안내가 칸보다 길면 작은 글자(8px)로 줄인다.
            var label = _info.GetComponent<TMPro.TMP_Text>();
            if (label.GetPreferredValues(label.text).x > infoWidth) _info.SetStyle(values.SmallStyle);
            int infoLine = Line(_info.Style);
            PixelLayout.PlaceTopLeft((RectTransform)_info.transform, infoX, y + PixelLayout.CenterOffset(footer, infoLine), infoWidth, infoLine);
        }

        private string Notice(QuestAccess access, int offers)
        {
            if (!string.IsNullOrEmpty(_notice)) return _notice;
            if (access == QuestAccess.Closed) return GameTexts.RequestBoardClosed;
            if (access == QuestAccess.NotNight) return GameTexts.RequestNotNight;
            if (offers == 0) return GameTexts.BoardEmpty;
            return null;
        }

        private Image NewDivider()
        {
            var go = new GameObject("Divider", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(_list, false);
            return go.GetComponent<Image>();
        }

        private void SetLine(Image image, int x, int y, int width)
        {
            image.sprite = null;
            image.color = _theme.Book.LineColor;
            image.raycastTarget = false;
            PixelLayout.PlaceTopLeft(image.rectTransform, x, y, width, 1);
        }

        private int Line(UiTextStyle id)
        {
            var style = _theme.TextStyle(id);
            return style != null ? style.LineHeightFrom(_theme.Fonts) : 16;
        }
    }
}
