// 작물 하나의 모양. 새싹 ↔ 다 자란 모양을 바꾸고, 자랄 때 짧게 커지는 연출을 한다.
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class CropView : MonoBehaviour
    {
        private const float GrowStartScale = 0.3f;

        private GameObject _visual;
        private float _growDuration;
        private float _growTimer;

        public void Show(FarmPlot plot, float growAnimationSeconds, bool animate)
        {
            if (_visual != null) Destroy(_visual);
            var prefab = plot.IsGrown ? plot.Seed.GrownPrefab : plot.Seed.SproutPrefab;
            if (prefab == null) return;

            _visual = Instantiate(prefab, transform);
            _visual.transform.localPosition = Vector3.zero;
            _growDuration = growAnimationSeconds;
            _growTimer = animate && growAnimationSeconds > 0f ? growAnimationSeconds : 0f;
            ApplyGrowScale();
        }

        private void Update()
        {
            if (_growTimer <= 0f) return;
            _growTimer -= Time.deltaTime;
            ApplyGrowScale();
        }

        private void ApplyGrowScale()
        {
            if (_visual == null) return;
            float t = _growDuration > 0f ? 1f - Mathf.Clamp01(_growTimer / _growDuration) : 1f;
            float eased = 1f - (1f - t) * (1f - t);
            _visual.transform.localScale = Vector3.one * Mathf.Lerp(GrowStartScale, 1f, eased);
        }
    }
}
