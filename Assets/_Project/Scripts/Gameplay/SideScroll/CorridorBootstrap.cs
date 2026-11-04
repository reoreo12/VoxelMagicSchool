// 복도 씬의 조립 지점. 복도를 먼저 만들어 범위를 구한 뒤 SideScrollContext를 만들어 ISideScrollBinder들에게 나눠 준다.
using System.Collections.Generic;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    [DefaultExecutionOrder(-1000)]
    public sealed class CorridorBootstrap : MonoBehaviour
    {
        [Header("설정")]
        [SerializeField] private CorridorConfig _config;
        [SerializeField] private CharacterMoveConfig _moveConfig;

        [Header("연결")]
        [SerializeField] private CorridorBuilder _corridor;
        [Tooltip("이 아래의 모든 ISideScrollBinder에 컨텍스트를 연결한다")]
        [SerializeField] private List<GameObject> _bindRoots = new List<GameObject>();

        public SideScrollContext Context { get; private set; }

        private void Awake()
        {
            _corridor.Build();
            Context = SideScrollContext.Create(_config, _moveConfig, _corridor.Bounds);
            foreach (var root in _bindRoots)
            {
                if (root == null) continue;
                foreach (var binder in root.GetComponentsInChildren<ISideScrollBinder>(true))
                    binder.Bind(Context);
            }
        }
    }
}
