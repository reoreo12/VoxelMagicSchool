// 씬 이동용 전체 화면 페이드. 픽셀 계단 원형 셰이더(PixelCircleFade)에 진행도·중심을 넘기고, 가리는 동안 마우스 클릭도 막는다.
using UnityEngine;
using UnityEngine.UI;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class PixelCircleFadeView : MonoBehaviour, IScreenFade
    {
        private static readonly int CoverageId = Shader.PropertyToID("_Coverage");
        private static readonly int CenterId = Shader.PropertyToID("_Center");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int BlockSizeId = Shader.PropertyToID("_BlockSize");
        private static readonly int DitherBlocksId = Shader.PropertyToID("_DitherBlocks");

        [SerializeField] private Graphic _graphic;
        [SerializeField] private Canvas _canvas;

        private Material _material;

        public void Configure(SceneTransitionConfig config)
        {
            if (_material == null)
            {
                _material = new Material(_graphic.material);
                _graphic.material = _material;
            }
            if (config == null) return;
            _material.SetColor(ColorId, config.Color);
            _material.SetFloat(BlockSizeId, config.BlockSize);
            _material.SetFloat(DitherBlocksId, config.DitherBlocks);
            if (_canvas != null) _canvas.sortingOrder = config.SortingOrder;
        }

        public void SetFade(float coverage, Vector2 center)
        {
            if (_material == null) Configure(null);
            _material.SetFloat(CoverageId, coverage);
            _material.SetVector(CenterId, center);
            _graphic.enabled = coverage > 0.0001f;
        }

        private void OnDestroy()
        {
            if (_material != null) Destroy(_material);
        }
    }
}
