// 화면 정중앙에 "산토끼 포획!"·"놓쳤다" 같은 큰 문구를 잠시 띄운다. 표시 시간은 Inspector에서 정한다.
using TMPro;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class CenterBannerView : UIPanel, IMountainContextBinder
    {
        [SerializeField] private TMP_Text _text;
        [Tooltip("띄워 두는 시간 (초, 가정)")]
        [SerializeField, Min(0.1f)] private float _seconds = 1.5f;

        private MountainMessages _messages;
        private float _timer;

        public void Bind(MountainContext context)
        {
            _messages = context.Messages;
            _messages.BannerRequested += OnBanner;
            SetVisible(false);
        }

        private void OnDestroy()
        {
            if (_messages != null) _messages.BannerRequested -= OnBanner;
        }

        private void OnBanner(string text)
        {
            _text.text = text;
            _timer = _seconds;
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
