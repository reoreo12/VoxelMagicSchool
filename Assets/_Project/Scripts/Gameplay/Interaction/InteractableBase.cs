// IInteractable의 공통 구현: Core 연결 받기, 충돌체 기준 거리 계산, 안내 문구 위치.
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public abstract class InteractableBase : MonoBehaviour, IInteractable, IGameContextBinder
    {
        [Tooltip("거리 계산에 쓸 영역. 비우면 오브젝트 위치를 쓴다")]
        [SerializeField] private Collider _area;
        [Tooltip("안내 문구를 띄울 위치. 비우면 오브젝트 위쪽")]
        [SerializeField] private Transform _promptAnchor;
        [SerializeField] private float _defaultAnchorHeight = 2f;

        protected GameContext Context { get; private set; }

        public virtual bool CanBeTargeted => Context != null && isActiveAndEnabled;

        protected Vector3 PromptAnchor => _promptAnchor != null
            ? _promptAnchor.position
            : transform.position + Vector3.up * _defaultAnchorHeight;

        public void Bind(GameContext context)
        {
            Context = context;
            OnBound();
        }

        public virtual Vector3 ClosestPoint(Vector3 from)
            => _area != null ? _area.ClosestPoint(from) : transform.position;

        public abstract InteractionPrompt GetPrompt(in InteractionContext context);
        public abstract void Interact(in InteractionContext context);

        protected virtual void OnBound() { }
    }
}
