// 책 모양 배치(UI_SPEC 5-1): 화면 가운데 왼쪽 페이지 + 책등 + 오른쪽 페이지, 그 뒤 오른쪽 가장자리에 사이드 탭을 위에서부터 쌓는다.
// 탭을 바꾸면 페이지를 넘긴다(5-3): 멀리 있는 탭은 한 장씩 이어서 넘기고, 넘기는 동안 들어온 입력은 마지막 탭만 목표로 남긴다.
// 탭 위에서 휠을 굴리면 이전(위)·다음(아래) 탭으로 옮긴다(사용자 요청). 책을 열 때는 넘기지 않고 바로 보인다.
using UnityEngine;
using UnityEngine.UI;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class BookView : ThemedWidget, IGameContextBinder
    {
        [SerializeField] private Image _leftPage;
        [SerializeField] private Image _spine;
        [SerializeField] private Image _rightPage;
        [SerializeField, Tooltip("위에서 아래 순서 (책 그림보다 먼저 = 뒤에 그림)")] private BookTabView[] _tabs = new BookTabView[0];
        [SerializeField] private BookPage[] _pages = new BookPage[0];
        [SerializeField, Tooltip("페이지 위에 그리는 넘김 연출")] private BookPageTurner _turner;

        private BookState _book;
        private BookTab _shown;      // 지금 펼쳐 보이는 탭(넘기는 중에는 출발한 탭)
        private BookTab _tabMarked;  // 사이드 탭에서 선택 그림을 보이는 탭

        public void Bind(GameContext context)
        {
            _book = context.Book;
            _book.TabChanged += OnTabChanged;
            foreach (var tab in _tabs)
            {
                tab.Clicked -= OnTabClicked;
                tab.Clicked += OnTabClicked;
                tab.Scrolled -= OnTabScrolled;
                tab.Scrolled += OnTabScrolled;
            }
            Refresh();
        }

        private void OnDestroy()
        {
            if (_book != null) _book.TabChanged -= OnTabChanged;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (_turner != null) _turner.Stop();
        }

        private BookTab Target => _book != null ? _book.Tab : BookTab.Requests;

        private void OnTabClicked(BookTab tab) => _book?.ChooseTab(tab);

        // 휠 위(+) = 위 탭(이전), 아래(−) = 아래 탭(다음). 양 끝에서 멈춘다.
        private void OnTabScrolled(int delta)
        {
            if (_book == null || delta == 0) return;
            int index = IndexOf(Target) + (delta > 0 ? -1 : 1);
            if (index < 0 || index >= _tabs.Length) return;
            _book.ChooseTab(_tabs[index].Tab);
        }

        private void OnTabChanged(BookTab _)
        {
            if (!isActiveAndEnabled || _turner == null || Theme == null) { Refresh(); return; }
            if (!_turner.IsPlaying) StepToward();
        }

        /// <summary>책이 열릴 때: 넘기지 않고 지금 탭을 바로 보인다.</summary>
        public void Refresh()
        {
            if (Theme == null) return;
            if (_turner != null) _turner.Stop();
            _shown = Target;
            _tabMarked = Target;
            PlaceTabs(Theme.Book);
            foreach (var page in _pages)
            {
                bool show = page.Tab == _shown;
                page.SetHalves(show, show);
                if (show) page.OnOpened();
            }
        }

        // 목표 탭 쪽으로 한 장 넘긴다. 끝나면 목표가 또 바뀌었을 수 있으니 다시 부른다.
        private void StepToward()
        {
            if (_shown == Target) return;
            int from = IndexOf(_shown), to = IndexOf(Target);
            if (from < 0 || to < 0) { Refresh(); return; }
            bool forward = to > from;
            var nextTab = _tabs[from + (forward ? 1 : -1)].Tab;
            var a = PageOf(_shown);
            var b = PageOf(nextTab);
            int last = Theme.Book.TurnFrames - 1;

            _turner.Play(Theme.Book, forward, k =>
            {
                if (k == 0)
                {
                    // 출발한 쪽 내용은 넘어가는 종이 아래에서 새 탭으로 바뀐다. 탭 선택 그림도 이때 바꾼다.
                    _tabMarked = nextTab;
                    PlaceTabs(Theme.Book);
                    if (a != null) a.SetHalves(forward, !forward);
                    if (b != null) { b.SetHalves(!forward, forward); b.OnOpened(); }
                }
                if (k == last)
                {
                    // 도착한 쪽 내용도 거의 다 덮였을 때 바꾼다.
                    if (a != null && a != b) a.SetHalves(false, false);
                    if (b != null) b.SetHalves(true, true);
                }
            }, () =>
            {
                _shown = nextTab;
                StepToward();
            });
        }

        private int IndexOf(BookTab tab)
        {
            for (int i = 0; i < _tabs.Length; i++) if (_tabs[i].Tab == tab) return i;
            return -1;
        }

        private BookPage PageOf(BookTab tab)
        {
            foreach (var page in _pages) if (page.Tab == tab) return page;
            return null;
        }

        private void OnRectTransformDimensionsChange()
        {
            if (Theme != null && isActiveAndEnabled) Apply(Theme);
        }

        protected override void Apply(UiTheme theme)
        {
            var values = theme.Book;
            if (_leftPage == null || _rightPage == null || _spine == null) return;
            var page = values.PageSize;
            int spine = values.SpineWidth;
            int width = page.x * 2 + spine;

            // 부모(팝업 층 전체)의 가운데. 가운데 앵커 대신 왼쪽 위 기준 정수 px.
            var root = (RectTransform)transform;
            var parent = (RectTransform)root.parent;
            int x = parent != null ? Mathf.FloorToInt((parent.rect.width - width) / 2f) : 0;
            int y = parent != null ? Mathf.FloorToInt((parent.rect.height - page.y) / 2f) : 0;
            PixelLayout.PlaceTopLeft(root, x, y, width, page.y);

            SetSliced(_leftPage, values.PageLeft, 0, page);
            SetSliced(_spine, values.Spine, page.x, new Vector2Int(spine, page.y));
            SetSliced(_rightPage, values.PageRight, page.x + spine, page);

            int inset = values.ContentInset;
            var left = new RectInt(inset, inset, page.x - inset * 2, page.y - inset * 2);
            var right = new RectInt(page.x + spine + inset, inset, page.x - inset * 2, page.y - inset * 2);
            foreach (var p in _pages) p.Place(left, right);
            if (_turner != null) PixelLayout.Stretch((RectTransform)_turner.transform);
            Refresh();
        }

        private void PlaceTabs(BookValues values)
        {
            int width = values.PageSize.x * 2 + values.SpineWidth;
            for (int i = 0; i < _tabs.Length; i++)
            {
                var tab = _tabs[i];
                tab.Show(values.Tab(tab.Tab), tab.Tab == _tabMarked, width - values.TabTuck, values.FirstTabY + i * values.TabStep, values);
            }
        }

        private static void SetSliced(Image image, Sprite sprite, int x, Vector2Int size)
        {
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.enabled = sprite != null;
            image.raycastTarget = true; // 책 위 클릭이 뒤의 입력 막기 판(바깥 클릭 = 닫기)으로 새지 않게
            PixelLayout.PlaceTopLeft(image.rectTransform, x, 0, size.x, size.y);
        }
    }
}
