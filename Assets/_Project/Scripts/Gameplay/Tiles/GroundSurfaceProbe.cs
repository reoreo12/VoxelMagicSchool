// 확인용: 플레이 중 이 오브젝트 발밑의 바닥 종류를 화면 왼쪽 위에 표시한다 (타일 맵 에디터 씬의 치비에 붙음). 게임 기능 아님.
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class GroundSurfaceProbe : MonoBehaviour
    {
        [SerializeField] private Vector2 _labelPosition = new Vector2(12f, 12f);
        [SerializeField] private Vector2 _labelSize = new Vector2(420f, 28f);
        [SerializeField, Min(8)] private int _fontSize = 18;

        private GUIStyle _style;

        public string CurrentText { get; private set; } = "";

        private void Update()
        {
            var position = transform.position;
            CurrentText = GroundSurfaceQuery.TryGet(position, out GroundSurface surface)
                ? $"발밑 바닥: {surface.DisplayName}  ({position.x:0.0}, {position.z:0.0})"
                : $"발밑 바닥: 없음  ({position.x:0.0}, {position.z:0.0})";
        }

        private void OnGUI()
        {
            if (_style == null) _style = new GUIStyle(GUI.skin.box) { fontSize = _fontSize, alignment = TextAnchor.MiddleLeft };
            GUI.Box(new Rect(_labelPosition, _labelSize), CurrentText, _style);
        }
    }
}
