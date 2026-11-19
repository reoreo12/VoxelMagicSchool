// 캐릭터 몸의 소켓 이름표 에셋 (오른손·왼손·등…). 에셋 참조로 비교하므로 문자열 오타가 없고, 코드 수정 없이 소켓을 늘린다.
using UnityEngine;

namespace VoxelMagic.Core
{
    [CreateAssetMenu(menuName = "VoxelMagic/Characters/Socket Id", fileName = "Socket_")]
    public sealed class SocketId : ScriptableObject
    {
        [SerializeField] private string _displayName;

        public string DisplayName => string.IsNullOrEmpty(_displayName) ? name : _displayName;
    }
}
