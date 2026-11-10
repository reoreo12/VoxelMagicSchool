// 보이지 않는 이동 트리거. 플레이어가 상자 범위에 들어오는 순간 지정한 씬의 도착 지점으로 이동을 요청한다.
// 물리 이벤트 대신 범위 검사라 CharacterController(쿼터뷰)와 코드 이동(횡스크롤) 모두에 쓴다.
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class SceneTransitionTrigger : MonoBehaviour
    {
        [Tooltip("Build Settings 씬 목록에 있는 씬 이름")]
        [SerializeField] private string _sceneName;
        [Tooltip("도착할 씬의 SceneSpawnPoint ID")]
        [SerializeField] private string _arrivalSpawnId;
        [SerializeField] private SceneTransitionDirector _director;
        [SerializeField] private Transform _player;
        [Tooltip("플레이어 발에서 이 높이(m)의 점이 상자 안이면 들어온 것으로 본다 (발이 상자 바닥과 딱 맞아도 잡히게)")]
        [SerializeField, Min(0f)] private float _probeHeight = 0.2f;

        private BoxCollider _box;
        private bool _inside;

        public string SceneName => _sceneName;

        private void Awake()
        {
            _box = GetComponent<BoxCollider>();
            _box.isTrigger = true;
        }

        // 도착하자마자 안에 서 있으면 한 번 나갔다 들어와야 작동.
        private void Start() => _inside = Contains();

        private void Update()
        {
            if (_player == null || _director == null || SceneTravel.IsTraveling) return;
            bool now = Contains();
            if (now && !_inside) _director.TravelTo(_sceneName, _arrivalSpawnId);
            _inside = now;
        }

        private bool Contains() => _player != null && _box.bounds.Contains(_player.position + Vector3.up * _probeHeight);
    }
}
