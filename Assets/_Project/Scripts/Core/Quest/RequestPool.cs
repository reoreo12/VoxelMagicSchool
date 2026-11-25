// 게시판에 올라올 수 있는 의뢰 전부. 새 의뢰는 에셋을 만들어 여기에 넣기만 하면 된다(난이도는 의뢰 데이터에서).
using System.Collections.Generic;
using UnityEngine;

namespace VoxelMagic.Core
{
    [CreateAssetMenu(menuName = "VoxelMagic/Quest/Request Pool", fileName = "RequestPool")]
    public sealed class RequestPool : ScriptableObject
    {
        [SerializeField] private List<RequestDefinition> _requests = new List<RequestDefinition>();

        public IReadOnlyList<RequestDefinition> Requests => _requests;

        public void Set(IEnumerable<RequestDefinition> requests) => _requests = new List<RequestDefinition>(requests);
    }
}
