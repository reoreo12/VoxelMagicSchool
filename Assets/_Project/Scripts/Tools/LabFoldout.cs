// 랩 패널 접이식 부품: 제목 줄을 누르면 내용을 숨기고 패널 높이를 제목 줄만큼 줄인다 (세로로 쌓인 패널은 아래 패널이 위로 올라온다).
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VoxelMagic.Tools
{
    public sealed class LabFoldout : MonoBehaviour
    {
        [SerializeField] private Button _header;
        [SerializeField] private TMP_Text _headerLabel;
        [SerializeField] private GameObject _content;
        [SerializeField] private string _title;
        [SerializeField, Min(1f)] private float _expandedHeight = 200f;
        [SerializeField, Min(1f)] private float _collapsedHeight = 44f;
        [Tooltip("시작할 때 펼쳐 둘지")]
        [SerializeField] private bool _startExpanded = true;

        private RectTransform _rect;
        private LayoutElement _layout;
        private bool _expanded;

        public bool Expanded => _expanded;

        private void Awake()
        {
            _rect = (RectTransform)transform;
            _layout = GetComponent<LayoutElement>();
            _header.onClick.AddListener(Toggle);
            SetExpanded(_startExpanded);
        }

        public void Toggle() => SetExpanded(!_expanded);

        public void SetExpanded(bool expanded)
        {
            _expanded = expanded;
            _content.SetActive(expanded);
            float height = expanded ? _expandedHeight : _collapsedHeight;
            _rect.sizeDelta = new Vector2(_rect.sizeDelta.x, height);
            if (_layout != null) _layout.preferredHeight = height;
            _headerLabel.text = (expanded ? "-  " : "+  ") + _title;
        }
    }
}
