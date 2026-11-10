// 씬 도착 지점. 이동해 들어온 도착 ID가 같으면 부트스트랩보다 먼저 플레이어를 이 자리로 옮긴다 (횡스크롤은 이동 계산기 x도 맞춤).
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    [DefaultExecutionOrder(-1500)]
    public sealed class SceneSpawnPoint : MonoBehaviour, ISideScrollBinder
    {
        [SerializeField] private string _id;
        [SerializeField] private Transform _player;

        private bool _arrived;

        public string Id => _id;

        private void Awake()
        {
            if (_player == null || !SceneTravel.IsTraveling || SceneTravel.ArrivalSpawnId != _id) return;
            _arrived = true;
            var controller = _player.GetComponent<CharacterController>();
            if (controller != null) controller.enabled = false;
            _player.SetPositionAndRotation(transform.position, transform.rotation);
            if (controller != null) controller.enabled = true;
        }

        public void Bind(SideScrollContext context)
        {
            if (_arrived) context.Character.Motor.Teleport(transform.position.x);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.8f);
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.5f, 0.4f);
            Gizmos.DrawLine(transform.position + Vector3.up * 0.5f, transform.position + Vector3.up * 0.5f + transform.forward * 0.8f);
        }
    }
}
