// 화면 구석에 조작법을 항상 표시한다. 문구는 GameTexts.ControlsHint에서 가져온다.
using TMPro;
using UnityEngine;

namespace VoxelMagic.UI
{
    public sealed class ControlsHintView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;

        private void Awake() => _text.text = Core.GameTexts.ControlsHint;
    }
}
