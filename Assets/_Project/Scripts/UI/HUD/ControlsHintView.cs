// 화면 구석에 조작법을 항상 표시한다. 문구는 GameTexts.ControlsHint에서 가져오고, 씬마다 따로 쓸 문구가 있으면 그것을 쓴다.
using TMPro;
using UnityEngine;

namespace VoxelMagic.UI
{
    public sealed class ControlsHintView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [Tooltip("비우면 기본 조작법(GameTexts.ControlsHint). 그 씬에서 할 수 있는 조작만 보여 줄 때 채운다")]
        [SerializeField, TextArea(3, 10)] private string _sceneText;

        private void Awake() => _text.text = string.IsNullOrEmpty(_sceneText) ? Core.GameTexts.ControlsHint : _sceneText;
    }
}
