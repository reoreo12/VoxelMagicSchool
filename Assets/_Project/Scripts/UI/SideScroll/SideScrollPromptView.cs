// 복도 화면 아래 가운데의 상호작용 안내 ("E  농장으로 가기"). 대상이 있을 때만 보인다.
using TMPro;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class SideScrollPromptView : MonoBehaviour, ISideScrollBinder
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private TMP_Text _text;

        private InteractionFocusState _focus;

        public void Bind(SideScrollContext context)
        {
            _focus = context.Interaction;
            _focus.Changed += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            if (_focus != null) _focus.Changed -= Refresh;
        }

        private void Refresh()
        {
            var prompt = _focus.Prompt;
            _panel.SetActive(prompt.IsVisible);
            if (prompt.IsVisible) _text.text = string.Format(SideScrollTexts.PromptFormat, prompt.KeyLabel, prompt.ActionLabel);
        }
    }
}
