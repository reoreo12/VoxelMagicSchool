// 디버그용: 캐릭터의 현재 상태 이름을 화면 구석에 표시한다. CorridorConfig.ShowStateDebug로 끈다.
using TMPro;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class StateDebugView : MonoBehaviour, ISideScrollBinder
    {
        [SerializeField] private TMP_Text _text;

        private CharacterStateMachine _machine;

        public void Bind(SideScrollContext context)
        {
            if (!context.Config.ShowStateDebug)
            {
                gameObject.SetActive(false);
                return;
            }
            _machine = context.Character;
            _machine.StateChanged += OnStateChanged;
            Show(_machine.CurrentId);
        }

        private void OnDestroy()
        {
            if (_machine != null) _machine.StateChanged -= OnStateChanged;
        }

        private void OnStateChanged(CharacterStateId previous, CharacterStateId next) => Show(next);

        private void Show(CharacterStateId state) =>
            _text.text = string.Format(SideScrollTexts.StateFormat, SideScrollTexts.StateName(state));
    }
}
