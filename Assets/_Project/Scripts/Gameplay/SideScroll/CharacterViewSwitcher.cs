// 복도 캐릭터의 2D/3D 전환: CharacterViewState에 따라 3D 모델의 렌더러와 2D 스프라이트를 켜고 끈다.
// 3D는 렌더러만 끄므로 Animator는 계속 돌아, 3D로 돌아오면 자세가 바로 이어진다. 2D 버전이 없으면 항상 3D.
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class CharacterViewSwitcher : MonoBehaviour, ISideScrollBinder
    {
        [SerializeField] private GameObject _model;
        [SerializeField] private GameObject _sprite;

        private CharacterViewState _state;
        private Renderer[] _modelRenderers;

        public void Bind(SideScrollContext context)
        {
            _state = context.CharacterView;
            _modelRenderers = _model.GetComponentsInChildren<Renderer>(true);
            _state.Changed += Apply;
            Apply();
        }

        private void OnDestroy()
        {
            if (_state != null) _state.Changed -= Apply;
        }

        private void Apply()
        {
            bool show2D = _state.Is2D && _sprite != null;
            foreach (var renderer in _modelRenderers) renderer.enabled = !show2D;
            if (_sprite != null) _sprite.SetActive(show2D);
        }
    }
}
