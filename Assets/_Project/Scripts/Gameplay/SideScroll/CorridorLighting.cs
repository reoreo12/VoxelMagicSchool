// 복도 낮/밤 연출: 창밖 배경 세트, 복도 타일 밝기·벽이 조명 받는 배율(전역 셰이더 값), 해 세기·주변광(3D 캐릭터),
// 밤 바닥 빛 그림과 천장 조명(칩비를 비춤) 켜고 끄기를 상태에 맞춘다.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class CorridorLighting : MonoBehaviour, ISideScrollBinder
    {
        private static readonly int CorridorAmbientId = Shader.PropertyToID("_CorridorAmbient");
        private static readonly int LampOnWallId = Shader.PropertyToID("_CorridorLampOnWall");

        [SerializeField] private ParallaxBackdrop _backdrop;
        [SerializeField] private Light _sun;
        [Tooltip("천장 조명 (칩비를 비춤)")]
        [SerializeField] private List<Light> _lamps = new List<Light>();
        [Tooltip("바닥 빛 그림 (조명 아님)")]
        [SerializeField] private List<SpriteRenderer> _glows = new List<SpriteRenderer>();

        private TimeOfDayState _state;
        private CorridorConfig _config;
        private float _daySunIntensity;
        private SphericalHarmonicsL2 _dayAmbientProbe;

        public void Bind(SideScrollContext context)
        {
            _config = context.Config;
            _state = context.TimeOfDay;
            _daySunIntensity = _sun != null ? _sun.intensity : 0f;
            _dayAmbientProbe = RenderSettings.ambientProbe;
            _state.Changed += Apply;
            Apply();
        }

        private void OnDestroy()
        {
            if (_state == null) return;
            _state.Changed -= Apply;
            // 에디터에서 플레이를 멈춘 뒤에도 낮 화면이 남도록 되돌린다.
            Shader.SetGlobalColor(CorridorAmbientId, _config.DayCorridorAmbient);
            RenderSettings.ambientProbe = _dayAmbientProbe;
        }

        private void Apply()
        {
            bool night = _state.IsNight;
            var set = night ? _config.NightBackdrop : _config.DayBackdrop;
            if (_backdrop != null && set != null) _backdrop.SetBackdrop(set);

            Shader.SetGlobalColor(CorridorAmbientId, night ? _config.NightCorridorAmbient : _config.DayCorridorAmbient);
            Shader.SetGlobalFloat(LampOnWallId, _config.LampOnWall);
            if (_sun != null) _sun.intensity = _daySunIntensity * (night ? _config.NightSunScale : 1f);
            RenderSettings.ambientProbe = night ? _dayAmbientProbe * _config.NightAmbientScale : _dayAmbientProbe;

            foreach (var lamp in _lamps)
                if (lamp != null) lamp.enabled = night;
            foreach (var glow in _glows)
                if (glow != null) glow.enabled = night;
        }
    }
}
