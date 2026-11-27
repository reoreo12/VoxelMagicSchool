// 획득 창의 모델 촬영 무대: 게임 화면에 안 보이는 먼 곳에 재료 모델을 놓고, 전용 직교 카메라가 모델만 작은 렌더 텍스처(도트)에 그린다.
// 모델은 툰 셰이더 머티리얼로 바꿔 끼우고 세로축으로 계속 돌린다(시간 정지와 무관). 보일 때만 카메라를 켠다.
using System.Collections.Generic;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class AcquireModelStage : MonoBehaviour
    {
        private readonly Dictionary<Material, Material> _toonMaterials = new Dictionary<Material, Material>();
        private AcquireValues _values;
        private Camera _camera;
        private Transform _pivot;
        private GameObject _model;
        private RenderTexture _texture;

        public RenderTexture Texture => _texture;

        public static AcquireModelStage Create(AcquireValues values)
        {
            var go = new GameObject("AcquireModelStage");
            var stage = go.AddComponent<AcquireModelStage>();
            stage.Init(values);
            return stage;
        }

        private void Init(AcquireValues values)
        {
            _values = values;
            transform.position = values.StagePosition;
            _pivot = new GameObject("Pivot").transform;
            _pivot.SetParent(transform, false);

            int size = values.CellSize;
            _texture = new RenderTexture(size, size, 24, RenderTextureFormat.ARGB32) { name = "AcquireModel", filterMode = FilterMode.Point, antiAliasing = 1 };
            _texture.Create();

            var cameraGo = new GameObject("Camera");
            cameraGo.transform.SetParent(transform, false);
            _camera = cameraGo.AddComponent<Camera>();
            _camera.orthographic = true;
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            _camera.targetTexture = _texture;
            _camera.allowHDR = false;
            _camera.allowMSAA = false;
            _camera.depth = -100;
            _camera.enabled = false;
        }

        /// <summary>이 아이템의 모델을 놓고 카메라를 켠다. 모델이 없는 아이템이면 false(아이콘으로 대신).</summary>
        public bool Show(ItemDefinition item)
        {
            Clear();
            _model = BuildModel(item);
            if (_model == null) return false;

            _model.transform.SetParent(_pivot, false);
            foreach (var behaviour in _model.GetComponentsInChildren<MonoBehaviour>(true)) behaviour.enabled = false;
            foreach (var collider in _model.GetComponentsInChildren<Collider>(true)) collider.enabled = false;
            var renderers = _model.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) { Clear(); return false; }

            // 모델 경계 가운데를 회전축(Pivot)에 맞추고, 경계 구가 칸에 들어오게 직교 크기를 정한다.
            _pivot.localRotation = Quaternion.identity;
            var bounds = renderers[0].bounds;
            foreach (var r in renderers)
            {
                bounds.Encapsulate(r.bounds);
                r.sharedMaterials = ToToon(r.sharedMaterials);
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                r.receiveShadows = false;
            }
            _model.transform.position -= bounds.center - _pivot.position;
            float radius = Mathf.Max(0.01f, bounds.extents.magnitude);

            var rotation = Quaternion.Euler(_values.CameraPitch, 0f, 0f);
            float distance = radius * 4f + 1f;
            _camera.transform.SetPositionAndRotation(_pivot.position - rotation * Vector3.forward * distance, rotation);
            _camera.orthographicSize = radius * _values.Fit;
            _camera.nearClipPlane = 0.01f;
            _camera.farClipPlane = distance + radius * 2f;
            _camera.enabled = true;
            return true;
        }

        public void Clear()
        {
            if (_camera != null) _camera.enabled = false;
            if (_model != null) { _model.SetActive(false); Destroy(_model); }
            _model = null;
        }

        private void Update()
        {
            if (_model != null) _pivot.Rotate(Vector3.up, _values.SpinDegreesPerSecond * Time.unscaledDeltaTime, Space.World);
        }

        // 획득용 모델이 있으면 그것, 없으면 땅에 놓인 FBX 메시·머티리얼. 둘 다 없으면(큐브 자리 등) null.
        private static GameObject BuildModel(ItemDefinition item)
        {
            if (!(item is GatherMaterialDefinition gather)) return null;
            if (gather.AcquireModel != null) return Instantiate(gather.AcquireModel);
            if (gather.WorldMesh == null || gather.WorldMaterial == null) return null;
            var go = new GameObject(gather.name, typeof(MeshFilter), typeof(MeshRenderer));
            go.GetComponent<MeshFilter>().sharedMesh = gather.WorldMesh;
            go.GetComponent<MeshRenderer>().sharedMaterial = gather.WorldMaterial;
            go.transform.localRotation = gather.WorldRotation;
            return go;
        }

        // 원래 머티리얼의 색·텍스처를 그대로 쓰는 툰 머티리얼로 바꾼다(머티리얼마다 한 번만 만든다).
        private Material[] ToToon(Material[] source)
        {
            var result = new Material[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                var original = source[i];
                if (original == null || _values.ToonShader == null) { result[i] = original; continue; }
                if (!_toonMaterials.TryGetValue(original, out var toon))
                {
                    toon = new Material(_values.ToonShader) { name = original.name + " (Toon)" };
                    if (original.HasProperty("_BaseMap")) toon.SetTexture("_BaseMap", original.GetTexture("_BaseMap"));
                    else if (original.HasProperty("_MainTex")) toon.SetTexture("_BaseMap", original.GetTexture("_MainTex"));
                    if (original.HasProperty("_BaseColor")) toon.SetColor("_BaseColor", original.GetColor("_BaseColor"));
                    else if (original.HasProperty("_Color")) toon.SetColor("_BaseColor", original.GetColor("_Color"));
                    _toonMaterials.Add(original, toon);
                }
                ApplyToonValues(toon);
                result[i] = toon;
            }
            return result;
        }

        private void ApplyToonValues(Material toon)
        {
            var light = _values.LightDirection;
            toon.SetVector("_ToonLightDir", new Vector4(light.x, light.y, light.z, 0f));
            toon.SetVector("_ToonThresholds", new Vector4(_values.Thresholds.x, _values.Thresholds.y, 0f, 0f));
            toon.SetVector("_ToonLevels", new Vector4(_values.Levels.x, _values.Levels.y, _values.Levels.z, 0f));
        }

        private void OnDestroy()
        {
            Clear();
            foreach (var m in _toonMaterials.Values) Destroy(m);
            _toonMaterials.Clear();
            if (_texture != null) { _texture.Release(); Destroy(_texture); }
        }
    }
}
