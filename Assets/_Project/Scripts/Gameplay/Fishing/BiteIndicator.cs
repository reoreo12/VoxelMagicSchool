// 입질이 오면 주인공 머리 위에 "!"(기본 도형)를 띄우고 통통 튀게 한다.
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class BiteIndicator : MonoBehaviour, IGameContextBinder
    {
        [SerializeField] private GameObject _visual;
        [SerializeField] private float _bounceHeight = 0.12f;
        [SerializeField] private float _bounceSpeed = 10f;

        private FishingSession _session;
        private Vector3 _restPosition;

        public void Bind(GameContext context)
        {
            _session = context.Fishing;
            _restPosition = _visual.transform.localPosition;
            _session.StateChanged += OnStateChanged;
            _visual.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_session != null) _session.StateChanged -= OnStateChanged;
        }

        private void OnStateChanged(FishingState state) => _visual.SetActive(state == FishingState.Biting);

        private void Update()
        {
            if (!_visual.activeSelf) return;
            float bounce = Mathf.Abs(Mathf.Sin(Time.time * _bounceSpeed)) * _bounceHeight;
            _visual.transform.localPosition = _restPosition + Vector3.up * bounce;
        }
    }
}
