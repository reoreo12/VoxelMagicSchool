// 책(메뉴) 화면의 탭 상태(UI_SPEC 5-2, 13장): 지금 펼친 탭, 직접 고른 마지막 탭, 의뢰 탭에서 먼저 보여 줄 의뢰.
// 책 아이콘은 마지막 탭으로, ESC는 메뉴 탭으로, HUD 의뢰 목록 항목은 의뢰 탭 + 그 의뢰로 연다. 화면을 여닫는 것은 ScreenStack이 한다.
using System;

namespace VoxelMagic.Core
{
    /// <summary>책의 사이드 탭. 순서 = 위에서 아래 (사용자 결정 2026-11-20).</summary>
    public enum BookTab
    {
        Requests,
        Spells,
        MagicCircles,
        Settings,
        Menu
    }

    public sealed class BookState
    {
        /// <summary>지금 펼친 탭.</summary>
        public BookTab Tab { get; private set; } = BookTab.Requests;

        /// <summary>탭을 직접 눌러 옮긴 마지막 탭. 책 아이콘으로 열면 여기로 간다. 처음에는 맨 앞 탭(의뢰 목록).</summary>
        public BookTab LastChosenTab { get; private set; } = BookTab.Requests;

        /// <summary>의뢰 탭을 열 때 먼저 고를 의뢰 (없으면 목록 맨 위).</summary>
        public RequestDefinition FocusRequest { get; private set; }

        public event Action<BookTab> TabChanged;
        public event Action<RequestDefinition> FocusChanged;

        /// <summary>책 아이콘: 마지막으로 직접 고른 탭.</summary>
        public void PrepareForIcon() => SetTab(LastChosenTab);

        /// <summary>ESC로 열 때: 메뉴 탭. 마지막 탭으로 기억하지 않는다(5-2 [미정], Claude 제안).</summary>
        public void PrepareForMenu() => SetTab(BookTab.Menu);

        /// <summary>HUD 의뢰 목록 항목: 의뢰 탭 + 그 의뢰.</summary>
        public void PrepareForRequest(RequestDefinition request)
        {
            FocusRequest = request;
            FocusChanged?.Invoke(request);
            SetTab(BookTab.Requests);
        }

        /// <summary>책 안에서 탭을 직접 눌렀을 때.</summary>
        public void ChooseTab(BookTab tab)
        {
            LastChosenTab = tab;
            SetTab(tab);
        }

        public void SetFocus(RequestDefinition request)
        {
            if (FocusRequest == request) return;
            FocusRequest = request;
            FocusChanged?.Invoke(request);
        }

        private void SetTab(BookTab tab)
        {
            if (Tab == tab) return;
            Tab = tab;
            TabChanged?.Invoke(tab);
        }
    }
}
