// 가장 가까운 상호작용 대상의 안내를 PromptChannel에 쓰고, E 입력을 우선순위(창 → 낚시 → 대상)대로 처리한다.
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class PlayerInteractor : MonoBehaviour, IGameContextBinder
    {
        [SerializeField] private GameInputReader _input;

        private GameContext _context;
        private InteractableRegistry _registry;

        public IInteractable Current { get; private set; }

        public void Initialize(InteractableRegistry registry) => _registry = registry;

        public void Bind(GameContext context) => _context = context;

        private void OnEnable() => _input.InteractPressed += OnInteractPressed;
        private void OnDisable() => _input.InteractPressed -= OnInteractPressed;

        private void Update()
        {
            if (_context == null || _registry == null) return;

            if (_context.Screen.IsModalOpen || _context.Fishing.IsBusy)
            {
                Current = null;
                _context.Prompt.Clear();
                return;
            }

            var interaction = BuildContext();
            Current = _registry.FindNearest(transform.position, _context.PlayerConfig.InteractRadius);
            _context.Prompt.Set(Current != null ? Current.GetPrompt(interaction) : InteractionPrompt.None);
        }

        private void OnInteractPressed()
        {
            if (_context == null) return;

            if (_context.Screen.IsModalOpen)
            {
                _context.Screen.Submit();
                return;
            }
            if (_context.Fishing.IsBusy)
            {
                _context.Fishing.PressInteract();
                return;
            }
            if (Current == null || !Current.CanBeTargeted) return;

            var interaction = BuildContext();
            if (Current.GetPrompt(interaction).IsAvailable) Current.Interact(interaction);
        }

        private InteractionContext BuildContext()
            => new InteractionContext(_context.Inventory.SelectedItem, transform.position, transform.forward);
    }
}
