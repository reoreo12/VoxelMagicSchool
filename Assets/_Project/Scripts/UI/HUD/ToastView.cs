// "가방이 가득 찼어요" 같은 짧은 알림을 화면 가운데 잠시 띄운다. 표시 시간은 GameBalanceConfig에서 정한다.
using TMPro;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class ToastView : UIPanel, IGameContextBinder
    {
        [SerializeField] private TMP_Text _text;

        private GameMessages _messages;
        private float _duration;
        private float _timer;

        public void Bind(GameContext context)
        {
            _messages = context.Messages;
            _duration = context.Balance.ToastSeconds;
            _messages.ToastRequested += OnToast;
            SetVisible(false);
        }

        private void OnDestroy()
        {
            if (_messages != null) _messages.ToastRequested -= OnToast;
        }

        private void OnToast(string message)
        {
            _text.text = message;
            _timer = _duration;
            Show();
        }

        private void Update()
        {
            if (!IsVisible) return;
            _timer -= Time.deltaTime;
            if (_timer <= 0f) Hide();
        }
    }
}
