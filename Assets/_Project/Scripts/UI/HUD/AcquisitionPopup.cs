// 뽑거나 낚은 아이템의 이름·설명·크기를 화면 위쪽에 잠시 보여 준다. 정해진 시간이 지나거나 닫기 요청이 오면 사라진다.
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class AcquisitionPopup : UIPanel, IGameContextBinder
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _description;
        [SerializeField] private TMP_Text _size;

        private GameMessages _messages;
        private float _duration;
        private float _timer;

        public void Bind(GameContext context)
        {
            _messages = context.Messages;
            _duration = context.Balance.AcquiredInfoSeconds;
            _messages.ItemAcquired += OnAcquired;
            _messages.AcquiredDismissed += Hide;
            SetVisible(false);
        }

        private void OnDestroy()
        {
            if (_messages == null) return;
            _messages.ItemAcquired -= OnAcquired;
            _messages.AcquiredDismissed -= Hide;
        }

        private void OnAcquired(AcquiredInfo info)
        {
            _icon.color = info.Item.IconColor;
            _title.text = string.Format(GameTexts.AcquiredFormat, info.Item.DisplayName);
            _description.text = info.Item.Description;
            _size.text = string.Format(GameTexts.SizeFormat, info.SizeCm);
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
