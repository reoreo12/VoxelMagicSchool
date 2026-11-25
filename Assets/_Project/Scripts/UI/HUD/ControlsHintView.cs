// 화면 구석에 조작법을 항상 표시한다. 문구는 GameTexts.ControlsHint에서 가져오고, 씬마다 따로 쓸 문구가 있으면 그것을 쓴다.
// 에디터·개발 빌드에서는 맨 아래에 치트 안내(퀵슬롯 우클릭 버리기) 한 줄을 덧붙인다.
using TMPro;
using UnityEngine;

namespace VoxelMagic.UI
{
    public sealed class ControlsHintView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [Tooltip("비우면 기본 조작법(GameTexts.ControlsHint). 그 씬에서 할 수 있는 조작만 보여 줄 때 채운다")]
        [SerializeField, TextArea(3, 10)] private string _sceneText;

        private void Awake()
        {
            _text.text = string.IsNullOrEmpty(_sceneText) ? Core.GameTexts.ControlsHint : _sceneText;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _text.text += "\n" + Core.GameTexts.CheatDiscardHint; // 퀵슬롯 우클릭 버리기는 치트라 개발 빌드에서만 안내
#endif
        }
    }
}
