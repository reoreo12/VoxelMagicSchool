// 도트 렌더링: 켜면 카메라를 화면의 1/픽셀 크기 해상도 텍스처에 그리고, Point 필터로 화면 가득 확대해 보여 준다.
// 화면 크기가 바뀌면 텍스처를 다시 만든다. 프로젝트 설정·URP 렌더러는 건드리지 않는다 (복도·애니메이션 랩 공용).
using UnityEngine;
using UnityEngine.UI;

namespace VoxelMagic.UI
{
    [RequireComponent(typeof(Camera))]
    public sealed class PixelRenderView : MonoBehaviour
    {
        private const int DepthBits = 24;

        [Tooltip("화면 맨 뒤에 깔린 전체 화면 RawImage (마우스 입력을 막지 않게 Raycast Target 끔)")]
        [SerializeField] private RawImage _output;

        private Camera _camera;
        private RenderTexture _texture;
        private int _pixelSize = 4;

        public bool IsOn { get; private set; }
        public int PixelSize => _pixelSize;
        public Vector2Int Resolution => new Vector2Int(Mathf.Max(1, Screen.width / _pixelSize), Mathf.Max(1, Screen.height / _pixelSize));

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _output.enabled = false;
        }

        public void SetOn(bool on)
        {
            IsOn = on;
            Apply();
        }

        public void SetPixelSize(int size)
        {
            _pixelSize = Mathf.Max(1, size);
            if (IsOn) Apply();
        }

        private void LateUpdate()
        {
            var size = Resolution;
            if (IsOn && (_texture == null || _texture.width != size.x || _texture.height != size.y)) Apply();
        }

        private void OnDestroy()
        {
            if (_camera != null) _camera.targetTexture = null;
            Release();
        }

        private void Apply()
        {
            _camera.targetTexture = null;
            Release();
            if (!IsOn)
            {
                _output.enabled = false;
                return;
            }

            var size = Resolution;
            _texture = new RenderTexture(size.x, size.y, DepthBits) { name = "PixelRenderView", filterMode = FilterMode.Point };
            _texture.Create();
            _camera.targetTexture = _texture;
            _output.texture = _texture;
            _output.enabled = true;
        }

        private void Release()
        {
            if (_texture == null) return;
            _texture.Release();
            Destroy(_texture);
            _texture = null;
        }
    }
}
