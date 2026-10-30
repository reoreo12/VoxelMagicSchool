// 화면 고정 패널의 공통 열기/닫기. CanvasGroup으로 보이기·클릭 가능 여부를 함께 바꾼다.
using UnityEngine;

namespace VoxelMagic.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIPanel : MonoBehaviour
    {
        private CanvasGroup _group;

        public bool IsVisible { get; private set; }

        private CanvasGroup Group => _group != null ? _group : _group = GetComponent<CanvasGroup>();

        public void Show() => SetVisible(true);
        public void Hide() => SetVisible(false);

        protected void SetVisible(bool visible)
        {
            IsVisible = visible;
            Group.alpha = visible ? 1f : 0f;
            Group.interactable = visible;
            Group.blocksRaycasts = visible;
            OnVisibilityChanged(visible);
        }

        protected virtual void OnVisibilityChanged(bool visible) { }
    }
}
