// 팝업 화면 공통(UI_SPEC 4-A-2). 화면은 한 번 만들어 두고 켜고 끄며, 열기·닫기는 UiScreenRouter가 화면 스택을 따라 부른다.
// 시간 정지·ESC·입력 막기 여부는 Core의 ScreenPolicy가 정하고, 화면은 보이기와 내용 갱신만 맡는다.
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public interface IUiScreen
    {
        GameScreen Id { get; }
        RectTransform Root { get; }
        bool IsShown { get; }
        void Show();
        void Hide();
    }

    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UiScreen : MonoBehaviour, IUiScreen
    {
        [SerializeField] private GameScreen _id;

        private CanvasGroup _group;

        public GameScreen Id => _id;
        public RectTransform Root => (RectTransform)transform;
        public bool IsShown { get; private set; }

        private CanvasGroup Group => _group != null ? _group : _group = GetComponent<CanvasGroup>();

        public void Show()
        {
            if (IsShown) return;
            SetShown(true);
            OnShown();
        }

        public void Hide()
        {
            if (!IsShown && gameObject.activeSelf == false) return;
            SetShown(false);
            OnHidden();
        }

        /// <summary>처음에 숨긴 상태로 맞춘다(라우터가 연결할 때 부름).</summary>
        public void HideImmediate() => SetShown(false);

        protected virtual void OnShown() { }
        protected virtual void OnHidden() { }

        private void SetShown(bool shown)
        {
            IsShown = shown;
            Group.alpha = shown ? 1f : 0f;
            Group.interactable = shown;
            Group.blocksRaycasts = shown;
            // 숨긴 화면은 캔버스 리빌드 대상에서 빠지도록 끈다(4-A-5).
            gameObject.SetActive(shown);
        }
    }
}
