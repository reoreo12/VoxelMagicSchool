// 굴 조명 스위치: 오브젝트를 켜면 씬의 태양·환경광을 어둡게 하고 바닥 빛 그림을 보여 횃불이 두드러지게 하고, 끄면 켜기 전 값으로 되돌린다.
// 에디터에서도 동작한다(ExecuteAlways). 켜기 전 값은 이 컴포넌트에 저장되므로 켠 채로 씬을 저장해도 끄면 원래대로 돌아간다.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace VoxelMagic.Gameplay
{
    [ExecuteAlways]
    public sealed class CaveLightingSwitch : MonoBehaviour
    {
        [Header("굴 조명 값")]
        [SerializeField] private Light _sun;
        [SerializeField, Min(0f)] private float _sunIntensity = 0.05f;
        [SerializeField] private Color _ambientColor = new Color(0.07f, 0.06f, 0.10f);
        [SerializeField, Range(0f, 1f)] private float _reflectionIntensity = 0.1f;
        [Tooltip("굴 조명일 때만 보이는 바닥 빛 그림")]
        [SerializeField] private List<Renderer> _floorGlows = new List<Renderer>();

        // 켜기 전 값 (씬에 함께 저장됨)
        [SerializeField, HideInInspector] private bool _saved;
        [SerializeField, HideInInspector] private AmbientMode _origAmbientMode;
        [SerializeField, HideInInspector] private Color _origAmbientLight;
        [SerializeField, HideInInspector] private Color _origSky;
        [SerializeField, HideInInspector] private Color _origEquator;
        [SerializeField, HideInInspector] private Color _origGround;
        [SerializeField, HideInInspector] private float _origAmbientIntensity;
        [SerializeField, HideInInspector] private float _origReflection;
        [SerializeField, HideInInspector] private float _origSunIntensity;

        public bool IsApplied => _saved;

        private void OnEnable()
        {
            if (_sun == null) _sun = RenderSettings.sun;
            if (!_saved) SaveOriginal();
            Apply();
        }

        private void OnDisable() => Restore();

        // 켠 상태에서 Inspector 값을 바꾸면 바로 반영.
        private void OnValidate()
        {
            if (isActiveAndEnabled && _saved) Apply();
        }

        private void SaveOriginal()
        {
            _origAmbientMode = RenderSettings.ambientMode;
            _origAmbientLight = RenderSettings.ambientLight;
            _origSky = RenderSettings.ambientSkyColor;
            _origEquator = RenderSettings.ambientEquatorColor;
            _origGround = RenderSettings.ambientGroundColor;
            _origAmbientIntensity = RenderSettings.ambientIntensity;
            _origReflection = RenderSettings.reflectionIntensity;
            _origSunIntensity = _sun != null ? _sun.intensity : 0f;
            _saved = true;
        }

        private void Apply()
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = _ambientColor;
            RenderSettings.reflectionIntensity = _reflectionIntensity;
            if (_sun != null) _sun.intensity = _sunIntensity;
            SetGlows(true);
        }

        private void SetGlows(bool on)
        {
            foreach (var glow in _floorGlows)
                if (glow != null) glow.enabled = on;
        }

        private void Restore()
        {
            SetGlows(false);
            if (!_saved) return;
            RenderSettings.ambientMode = _origAmbientMode;
            RenderSettings.ambientLight = _origAmbientLight;
            RenderSettings.ambientSkyColor = _origSky;
            RenderSettings.ambientEquatorColor = _origEquator;
            RenderSettings.ambientGroundColor = _origGround;
            RenderSettings.ambientIntensity = _origAmbientIntensity;
            RenderSettings.reflectionIntensity = _origReflection;
            if (_sun != null) _sun.intensity = _origSunIntensity;
            _saved = false;
        }
    }
}
