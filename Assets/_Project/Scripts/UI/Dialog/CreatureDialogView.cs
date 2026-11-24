// 옛 프로토타입 크리처 대사 창. 새 대사창(DialogueWindow, UI_SPEC 6장)으로 바뀌어 더는 아무것도 하지 않는다(숨김만).
// 씬에 남은 컴포넌트가 새 대사 화면을 건드리지 않도록 비워 두었다. 옛 프로토타입 UI를 정리할 때 씬과 함께 지운다.
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class CreatureDialogView : UIPanel, IGameContextBinder
    {
        [SerializeField] private TMP_Text _speaker;
        [SerializeField] private TMP_Text _line;
        [SerializeField] private TMP_Text _hint;
        [SerializeField] private Button _nextButton;

        public void Bind(GameContext context) => SetVisible(false);
    }
}
