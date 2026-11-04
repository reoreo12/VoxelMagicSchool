// 창밖 배경을 패럴랙스 계수만큼 느리게 움직이고, 복사본 몇 장을 화면 밖에서 반대편으로 옮겨 끝없이 잇는다.
// 카메라가 움직인 뒤에 위치를 정하도록 늦게 실행한다. 배경 세트를 바꾸면 낮/밤이 바뀐다.
using System.Collections.Generic;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    [ExecuteAlways, DefaultExecutionOrder(100)]
    public sealed class ParallaxBackdrop : MonoBehaviour
    {
        [SerializeField] private BackdropSet _set;
        [SerializeField] private CorridorConfig _config;
        [SerializeField] private Camera _camera;
        [Tooltip("조명 영향을 받지 않는 스프라이트 머테리얼")]
        [SerializeField] private Material _spriteMaterial;

        private readonly List<SpriteRenderer> _copies = new List<SpriteRenderer>();

        public BackdropSet Set => _set;

        private void OnEnable() => Rebuild();
        private void OnDisable() => Clear();

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying) return;
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null && isActiveAndEnabled) Rebuild();
            };
        }
#endif

        public void SetBackdrop(BackdropSet set)
        {
            _set = set;
            Rebuild();
        }

        private void LateUpdate() => Position();

        private void Rebuild()
        {
            Clear();
            if (_set == null || _set.Sprite == null || _config == null) return;

            for (int i = 0; i < _config.BackdropCopies; i++)
            {
                var go = new GameObject($"Copy_{i}");
                go.hideFlags = HideFlags.DontSaveInEditor | HideFlags.NotEditable;
                go.transform.SetParent(transform, false);
                go.transform.localScale = Vector3.one * _config.BackdropScale;

                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = _set.Sprite;
                renderer.sortingOrder = _config.BackdropSortingOrder;
                if (_spriteMaterial != null) renderer.sharedMaterial = _spriteMaterial;
                _copies.Add(renderer);
            }
            if (_camera != null) _camera.backgroundColor = _set.SkyColor;
            Position();
        }

        private void Position()
        {
            if (_copies.Count == 0 || _camera == null) return;

            float scale = _config.BackdropScale;
            var bounds = _set.Sprite.bounds;
            float width = bounds.size.x * scale;
            float cameraX = _camera.transform.position.x;
            float halfView = _camera.orthographicSize * _camera.aspect;

            float origin = SideScrollMath.ParallaxOriginX(cameraX, _config.BackdropParallax);
            float left = SideScrollMath.FirstLoopLeft(origin, width, cameraX - halfView);
            for (int i = 0; i < _copies.Count; i++)
            {
                float copyLeft = left + i * width;
                _copies[i].transform.position = new Vector3(copyLeft - bounds.min.x * scale,
                    _config.BackdropOffsetY, _config.BackdropZ);
            }
        }

        private void Clear()
        {
            foreach (var copy in _copies)
            {
                if (copy == null) continue;
                if (Application.isPlaying) Destroy(copy.gameObject);
                else DestroyImmediate(copy.gameObject);
            }
            _copies.Clear();
        }
    }
}
