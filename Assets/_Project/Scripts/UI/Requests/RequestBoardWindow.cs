// 의뢰 게시판 창(임시 UI). 화면 스택이 게시판을 열면 오늘 게시판(RequestBoardState)의 의뢰를 보여 주고, 수락은 RequestLog에 요청한다.
// 못 받는 이유(칸이 꽉 참·씨앗 자리·밤에만·닫힌 날)와 "씨앗 다시 받기" 버튼을 함께 둔다. 수락한 의뢰는 게시판에서 빠진다.
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class RequestBoardWindow : UIPanel, IGameContextBinder
    {
        [SerializeField] private RectTransform _listRoot;
        [SerializeField] private RequestEntryView _entryPrefab;
        [SerializeField] private Button _closeButton;
        [Tooltip("비우면 닫기 버튼을 복제해 왼쪽 아래에 만든다")]
        [SerializeField] private Button _refillButton;
        [Tooltip("받은 의뢰 수·못 받는 이유·씨앗 결과 안내. 비우면 닫기 버튼 글자를 복제해 만든다")]
        [SerializeField] private TMP_Text _info;

        private readonly List<RequestEntryView> _entries = new List<RequestEntryView>();
        private RequestLog _log;
        private RequestBoardState _board;
        private ScreenStack _screens;
        private Inventory _inventory;
        private string _notice;

        public void Bind(GameContext context)
        {
            _log = context.Requests;
            _board = context.Board;
            _screens = context.Screens;
            _inventory = context.Inventory;
            EnsureExtras();

            _closeButton.onClick.AddListener(() => _screens.Close(GameScreen.RequestBoard));
            _refillButton.onClick.AddListener(OnRefill);
            _screens.Opened += OnOpened;
            _screens.Closed += OnClosed;
            // 의뢰·게시판·인벤토리는 씬을 넘어 유지되므로 OnDestroy에서 꼭 구독을 푼다.
            _log.AcceptedListChanged += OnChanged;
            if (_board != null) _board.OffersChanged += OnChanged;
            SetVisible(false);
        }

        private void OnDestroy()
        {
            if (_screens != null)
            {
                _screens.Opened -= OnOpened;
                _screens.Closed -= OnClosed;
            }
            if (_log != null) _log.AcceptedListChanged -= OnChanged;
            if (_board != null) _board.OffersChanged -= OnChanged;
        }

        private void OnChanged()
        {
            if (this == null) { OnDestroy(); return; } // 꺼진 채 씬이 내려가 OnDestroy가 불리지 않은 구독자: 여기서 구독을 푼다
            if (IsVisible) Rebuild();
        }

        private void OnOpened(GameScreen screen)
        {
            if (screen != GameScreen.RequestBoard) return;
            _notice = null;
            Rebuild();
            SetVisible(true);
        }

        private void OnClosed(GameScreen screen)
        {
            if (screen == GameScreen.RequestBoard) SetVisible(false);
        }

        private void OnAccept(RequestDefinition request)
        {
            var result = _log.Accept(request);
            _notice = result == AcceptResult.Success ? null : RequestEntryView.Reason(result, _log.MaxAccepted);
            Rebuild();
        }

        private void OnRefill()
        {
            var seeds = _log.RefillableSeeds();
            if (seeds.Count == 0) _notice = GameTexts.SeedRefillNone;
            else
            {
                int given = _log.GiveRefillSeeds();
                _notice = given > 0 ? string.Format(GameTexts.SeedRefillGivenFormat, given) : GameTexts.SeedRefillNoRoom;
            }
            Rebuild();
        }

        // 의뢰가 많아야 3개라 열 때마다 줄을 다시 만든다.
        private void Rebuild()
        {
            var offers = _board != null ? _board.Offers : (IReadOnlyList<RequestDefinition>)new RequestDefinition[0];
            while (_entries.Count < offers.Count) _entries.Add(Instantiate(_entryPrefab, _listRoot));
            for (int i = 0; i < _entries.Count; i++)
            {
                bool used = i < offers.Count;
                _entries[i].gameObject.SetActive(used);
                if (!used) continue;
                _entries[i].Setup(offers[i], _log.RewardOf(offers[i]), OnAccept);
                _entries[i].Refresh(_log.CanAccept(offers[i]), _log.MaxAccepted);
            }

            string line = string.Format(GameTexts.BoardSlotsFormat, _log.AcceptedCount, _log.MaxAccepted);
            var access = _log.BoardAccess();
            if (access == QuestAccess.Closed) line += "\n" + GameTexts.RequestBoardClosed;
            else if (access == QuestAccess.NotNight) line += "\n" + GameTexts.RequestNotNight;
            else if (offers.Count == 0) line += "\n" + GameTexts.BoardEmpty;
            else if (_log.IsFull) line += "\n" + string.Format(GameTexts.RequestSlotsFullFormat, _log.MaxAccepted);
            if (!string.IsNullOrEmpty(_notice)) line += "\n" + _notice;
            _info.text = line;
            _refillButton.interactable = _log.AcceptedCount > 0;
        }

        // 예전 씬의 게시판 창에는 씨앗 버튼·안내 글자가 없어서 닫기 버튼을 복제해 만든다(임시 UI).
        private void EnsureExtras()
        {
            if (_refillButton == null)
            {
                _refillButton = Instantiate(_closeButton, _closeButton.transform.parent);
                _refillButton.name = "RefillSeedsButton";
                var rt = (RectTransform)_refillButton.transform;
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 0f);
                rt.anchoredPosition = new Vector2(20f, 20f);
                rt.sizeDelta = new Vector2(220f, rt.sizeDelta.y);
                _refillButton.onClick = new Button.ButtonClickedEvent();
                var label = _refillButton.GetComponentInChildren<TMP_Text>(true);
                if (label != null) label.text = GameTexts.SeedRefill;
            }
            if (_info == null)
            {
                var source = _closeButton.GetComponentInChildren<TMP_Text>(true);
                _info = Instantiate(source, _closeButton.transform.parent);
                _info.name = "BoardInfo";
                var rt = _info.rectTransform;
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0f);
                rt.anchoredPosition = new Vector2(0f, 16f);
                rt.sizeDelta = new Vector2(440f, 64f);
                _info.fontSize = Mathf.Max(16f, source.fontSize * 0.75f);
                _info.alignment = TextAlignmentOptions.Bottom;
                _info.enableWordWrapping = true;
                _info.raycastTarget = false;
                _info.color = TempUiBuilder.Text; // 닫기 버튼 글자색은 어두운 창 배경에 묻힌다
            }
        }
    }
}
