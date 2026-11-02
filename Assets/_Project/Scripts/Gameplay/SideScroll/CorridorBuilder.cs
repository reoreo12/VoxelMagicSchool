// CorridorLayout을 읽어 복도 타일을 타일 폭 간격으로 이어 붙이고 복도 범위를 계산한다.
// 플레이 중에는 부트스트랩이 Build()를 부르고, 에디터에서는 씬에 저장되지 않는 미리보기로 보여 준다.
using System.Collections.Generic;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    [ExecuteAlways]
    public sealed class CorridorBuilder : MonoBehaviour
    {
        [SerializeField] private CorridorLayout _layout;
        [SerializeField] private CorridorConfig _config;
        [Tooltip("조명 영향을 받지 않는 스프라이트 머테리얼")]
        [SerializeField] private Material _spriteMaterial;

        private readonly List<GameObject> _tiles = new List<GameObject>();

        public CorridorBounds Bounds { get; private set; }

        private void OnEnable()
        {
            if (!Application.isPlaying) Build();
        }

        private void OnDisable()
        {
            if (!Application.isPlaying) Clear();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying) return;
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null && isActiveAndEnabled) Build();
            };
        }
#endif

        public void Build()
        {
            Clear();
            if (_layout == null || _config == null) return;

            var origin = transform.position;
            float cursor = origin.x;
            float height = 0f;
            for (int i = 0; i < _layout.Tiles.Count; i++)
            {
                var tile = _layout.Tiles[i];
                if (tile.Sprite == null) continue;

                var go = new GameObject($"Tile_{i}");
                if (!Application.isPlaying) go.hideFlags = HideFlags.DontSaveInEditor | HideFlags.NotEditable;
                go.transform.SetParent(transform, false);

                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = tile.Sprite;
                renderer.flipX = tile.FlipX;
                renderer.sortingOrder = _config.CorridorSortingOrder;
                if (_spriteMaterial != null) renderer.sharedMaterial = _spriteMaterial;

                // 반전하면 기준점을 축으로 뒤집히므로, 왼쪽 끝 위치를 반전 여부에 맞춰 구한다.
                var bounds = tile.Sprite.bounds;
                float leftOffset = tile.FlipX ? -bounds.max.x : bounds.min.x;
                go.transform.position = new Vector3(cursor - leftOffset, origin.y, _config.CorridorZ);

                cursor += bounds.size.x;
                height = Mathf.Max(height, bounds.size.y);
                _tiles.Add(go);
            }
            Bounds = new CorridorBounds(origin.x, cursor, height);
        }

        private void Clear()
        {
            foreach (var tile in _tiles)
            {
                if (tile == null) continue;
                if (Application.isPlaying) Destroy(tile);
                else DestroyImmediate(tile);
            }
            _tiles.Clear();
        }
    }
}
