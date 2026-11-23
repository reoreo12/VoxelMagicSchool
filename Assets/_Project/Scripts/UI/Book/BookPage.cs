// 책의 탭 하나에 해당하는 펼친 두 페이지 내용(UI_SPEC 5-2). 책이 왼쪽·오른쪽 내용 칸을 정해 주면 그 안에 그린다.
// 내용이 아직 정해지지 않은 탭(마법 스펠·마법진·환경 설정·메뉴)은 이 기본 페이지(빈 종이)를 쓴다.
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public class BookPage : ThemedWidget
    {
        [SerializeField] private BookTab _tab;
        [SerializeField, Tooltip("왼쪽 페이지 내용 칸")] private RectTransform _left;
        [SerializeField, Tooltip("오른쪽 페이지 내용 칸")] private RectTransform _right;

        public BookTab Tab => _tab;
        protected RectTransform Left => _left;
        protected RectTransform Right => _right;
        protected Vector2Int LeftSize { get; private set; }
        protected Vector2Int RightSize { get; private set; }

        /// <summary>책이 내용 칸(책 왼쪽 위 기준 정수 px)을 정해 준다.</summary>
        public void Place(RectInt left, RectInt right)
        {
            PixelLayout.Stretch((RectTransform)transform);
            LeftSize = left.size;
            RightSize = right.size;
            if (_left != null) PixelLayout.PlaceTopLeft(_left, left.x, left.y, left.width, left.height);
            if (_right != null) PixelLayout.PlaceTopLeft(_right, right.x, right.y, right.width, right.height);
            Relayout();
        }

        /// <summary>왼쪽·오른쪽 페이지 내용을 따로 보이고 숨긴다(페이지 넘김 중 한쪽씩 바뀔 때, 5-3).</summary>
        public void SetHalves(bool left, bool right)
        {
            bool any = left || right;
            if (gameObject.activeSelf != any) gameObject.SetActive(any);
            if (_left != null && _left.gameObject.activeSelf != left) _left.gameObject.SetActive(left);
            if (_right != null && _right.gameObject.activeSelf != right) _right.gameObject.SetActive(right);
        }

        /// <summary>이 탭이 펼쳐질 때 (책을 열거나 탭을 옮길 때).</summary>
        public virtual void OnOpened() { }

        protected override void Apply(UiTheme theme) => Relayout();

        /// <summary>내용 칸 크기나 테마가 바뀌었을 때 안쪽을 다시 놓는다.</summary>
        protected virtual void Relayout() { }
    }
}
