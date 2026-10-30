// 의뢰 게시판 창. ScreenState가 게시판을 열면 보이고, 수락 버튼은 RequestTracker에 수락을 요청한다.
using System.Collections.Generic;
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

        private readonly List<RequestEntryView> _entries = new List<RequestEntryView>();
        private RequestTracker _tracker;
        private ScreenState _screen;

        public void Bind(GameContext context)
        {
            _tracker = context.Requests;
            _screen = context.Screen;

            foreach (var request in _tracker.Catalog)
            {
                var entry = Instantiate(_entryPrefab, _listRoot);
                entry.Setup(request, OnAccept);
                _entries.Add(entry);
            }
            _closeButton.onClick.AddListener(() => _screen.Close(GameWindow.RequestBoard));

            _screen.WindowChanged += OnWindowChanged;
            _tracker.ActiveChanged += _ => RefreshEntries();
            context.Inventory.SlotChanged += _ => { if (IsVisible) RefreshEntries(); };
            SetVisible(false);
        }

        private void OnDestroy()
        {
            if (_screen != null) _screen.WindowChanged -= OnWindowChanged;
        }

        private void OnWindowChanged(GameWindow window)
        {
            bool open = window == GameWindow.RequestBoard;
            if (open) RefreshEntries();
            SetVisible(open);
        }

        private void OnAccept(RequestDefinition request)
        {
            if (_tracker.Accept(request) == AcceptResult.Success)
                _screen.Close(GameWindow.RequestBoard);
            else
                RefreshEntries();
        }

        private void RefreshEntries()
        {
            foreach (var entry in _entries)
                entry.Refresh(_tracker.GetStatus(entry.Request), _tracker.CanAccept(entry.Request));
        }
    }
}
