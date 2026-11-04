// 상호작용 안내("E / 심기")를 대상 근처 화면 위치에 띄운다. 할 수 없으면 이유를 흐린 색으로 보여 준다.
using TMPro;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class InteractionPromptView : MonoBehaviour, IGameContextBinder
    {
        [SerializeField] private RectTransform _root;
        [SerializeField] private TMP_Text _keyText;
        [SerializeField] private TMP_Text _actionText;
        [SerializeField] private Camera _worldCamera;
        [SerializeField] private Vector2 _screenOffset = new Vector2(0f, 20f);
        [SerializeField] private Color _availableColor = Color.white;
        [SerializeField] private Color _blockedColor = new Color(1f, 1f, 1f, 0.5f);

        private PromptChannel _channel;

        public void Bind(GameContext context)
        {
            _channel = context.Prompt;
            _channel.Changed += Show;
            Show(_channel.Current);
        }

        private void OnDestroy()
        {
            if (_channel != null) _channel.Changed -= Show;
        }

        private void Show(InteractionPrompt prompt)
        {
            _root.gameObject.SetActive(prompt.IsVisible);
            if (!prompt.IsVisible) return;

            _keyText.text = prompt.KeyLabel;
            _actionText.text = prompt.IsAvailable ? prompt.ActionLabel : prompt.BlockedReason;
            var color = prompt.IsAvailable ? _availableColor : _blockedColor;
            _keyText.color = color;
            _actionText.color = color;
            Reposition();
        }

        private void LateUpdate()
        {
            if (_root.gameObject.activeSelf) Reposition();
        }

        private void Reposition()
        {
            if (_worldCamera == null || _channel == null) return;
            Vector3 screen = _worldCamera.WorldToScreenPoint(_channel.Current.WorldAnchor);
            _root.position = new Vector3(screen.x + _screenOffset.x, screen.y + _screenOffset.y, 0f);
        }
    }
}
