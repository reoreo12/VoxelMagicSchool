// 복도 캐릭터 위치(발에서 설정한 높이)가 겹친 트리거 중 IInteractable을 지금 대상으로 두고 안내 상태에 알린다.
// 캐릭터는 Rigidbody 없이 움직이므로 트리거 이벤트 대신 매 프레임 겹침 검사를 한다. E 입력 때 대상을 실행한다.
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    // 캐릭터가 이번 프레임 위치로 옮겨진 뒤에 검사한다 (한 프레임 늦게 안내가 뜨지 않게).
    [DefaultExecutionOrder(100)]
    public sealed class SideScrollInteractor : MonoBehaviour, ISideScrollBinder
    {
        private const int MaxHits = 8;
        private const float ProbeRadius = 0.01f;

        [SerializeField] private SideScrollInputReader _input;
        [SerializeField] private Transform _character;

        private readonly Collider[] _hits = new Collider[MaxHits];
        private InteractionFocusState _focus;
        private CorridorConfig _config;
        private IInteractable _target;

        public void Bind(SideScrollContext context)
        {
            _focus = context.Interaction;
            _config = context.Config;
        }

        private void OnEnable() => _input.InteractPressed += OnInteract;
        private void OnDisable() => _input.InteractPressed -= OnInteract;

        private void Update()
        {
            if (_focus == null || _character == null) return;
            _target = FindTarget();
            _focus.Set(_target != null ? _target.GetPrompt(CreateContext()) : InteractionPrompt.None);
        }

        private IInteractable FindTarget()
        {
            var probe = _character.position + Vector3.up * _config.InteractProbeHeight;
            int count = Physics.OverlapSphereNonAlloc(probe, ProbeRadius, _hits, ~0, QueryTriggerInteraction.Collide);
            for (int i = 0; i < count; i++)
            {
                if (_hits[i].transform.IsChildOf(_character)) continue;
                if (_hits[i].TryGetComponent<IInteractable>(out var interactable) && interactable.CanBeTargeted)
                    return interactable;
            }
            return null;
        }

        private void OnInteract()
        {
            if (_target != null && _target.CanBeTargeted) _target.Interact(CreateContext());
        }

        // 복도에서는 들고 있는 아이템이 없다.
        private InteractionContext CreateContext() =>
            new InteractionContext(null, _character.position, _character.forward);
    }
}
