// 장착물 데이터: 프리팹, 붙을 소켓, 소켓 기준 놓는 위치·회전(물건의 어디를 쥐는지). 아이템이 선택적으로 참조한다.
using UnityEngine;

namespace VoxelMagic.Core
{
    [CreateAssetMenu(menuName = "VoxelMagic/Characters/Attachment", fileName = "Attachment_")]
    public sealed class AttachmentDefinition : ScriptableObject
    {
        [SerializeField] private string _displayName;
        [SerializeField] private GameObject _prefab;
        [SerializeField] private SocketId _socket;
        [Tooltip("소켓 기준 장착물 위치 (예: 지팡이 아래쪽 1/4을 쥐려면 지팡이를 앞으로 민다)")]
        [SerializeField] private Vector3 _holdPosition;
        [Tooltip("소켓 기준 장착물 회전 (오일러 각)")]
        [SerializeField] private Vector3 _holdRotation;

        public string DisplayName => string.IsNullOrEmpty(_displayName) ? name : _displayName;
        public GameObject Prefab => _prefab;
        public SocketId Socket => _socket;
        public Vector3 HoldPosition => _holdPosition;
        public Quaternion HoldRotation => Quaternion.Euler(_holdRotation);
    }
}
