// 낚시 미니게임 화면. 세로 바 안의 물고기·잡기 바 위치와 잡기 게이지를 FishingMinigame 값대로 그린다.
using UnityEngine;
using UnityEngine.UI;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class FishingMinigameView : UIPanel, IGameContextBinder
    {
        [SerializeField] private RectTransform _catchBar;
        [SerializeField] private Image _catchBarImage;
        [SerializeField] private RectTransform _fishIcon;
        [SerializeField] private Image _fishImage;
        [SerializeField] private RectTransform _gaugeFill;
        [SerializeField] private Image _gaugeImage;
        [SerializeField] private Color _barOnFish = new Color(0.4f, 1f, 0.45f, 0.85f);
        [SerializeField] private Color _barOffFish = new Color(0.4f, 1f, 0.45f, 0.4f);
        [SerializeField] private Color _gaugeLow = new Color(1f, 0.35f, 0.25f);
        [SerializeField] private Color _gaugeHigh = new Color(0.4f, 1f, 0.45f);

        private FishingSession _session;

        public void Bind(GameContext context)
        {
            _session = context.Fishing;
            _session.StateChanged += OnStateChanged;
            SetVisible(false);
        }

        private void OnDestroy()
        {
            if (_session != null) _session.StateChanged -= OnStateChanged;
        }

        private void OnStateChanged(FishingState state)
        {
            bool playing = state == FishingState.Minigame && _session.Minigame != null;
            if (playing) _fishImage.color = _session.Minigame.Fish.IconColor;
            SetVisible(playing);
            if (playing) Redraw(_session.Minigame);
        }

        private void LateUpdate()
        {
            if (IsVisible && _session.Minigame != null) Redraw(_session.Minigame);
        }

        private void Redraw(FishingMinigame game)
        {
            float half = game.BarSize * 0.5f;
            _catchBar.anchorMin = new Vector2(0f, game.BarCenter - half);
            _catchBar.anchorMax = new Vector2(1f, game.BarCenter + half);
            _catchBar.offsetMin = _catchBar.offsetMax = Vector2.zero;
            _catchBarImage.color = game.IsFishInBar ? _barOnFish : _barOffFish;

            _fishIcon.anchorMin = _fishIcon.anchorMax = new Vector2(0.5f, game.FishPosition);

            _gaugeFill.anchorMax = new Vector2(1f, game.Gauge);
            _gaugeFill.offsetMax = Vector2.zero;
            _gaugeImage.color = Color.Lerp(_gaugeLow, _gaugeHigh, game.Gauge);
        }
    }
}
