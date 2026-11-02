// 복도 씬 화면 구석에 조작법을 항상 표시한다. 문구는 SideScrollTexts.ControlsHint.
using TMPro;
using UnityEngine;

namespace VoxelMagic.UI
{
    public sealed class SideScrollHintView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;

        private void Awake() => _text.text = Core.SideScrollTexts.ControlsHint;
    }
}
