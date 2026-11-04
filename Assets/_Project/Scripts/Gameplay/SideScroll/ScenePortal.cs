// 복도 문 앞의 반투명 원기둥 포털. 트리거 콜라이더 안에서 E를 누르면 지정한 씬을 연다 (IInteractable).
using UnityEngine;
using UnityEngine.SceneManagement;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    [RequireComponent(typeof(CapsuleCollider))]
    public sealed class ScenePortal : MonoBehaviour, IInteractable
    {
        [Tooltip("Build Settings 씬 목록에 있는 씬 이름")]
        [SerializeField] private string _sceneName;
        [SerializeField] private string _actionLabel = SideScrollTexts.PortalToFarm;

        private CapsuleCollider _trigger;

        public bool CanBeTargeted => isActiveAndEnabled && !string.IsNullOrEmpty(_sceneName);

        private void Awake()
        {
            _trigger = GetComponent<CapsuleCollider>();
            _trigger.isTrigger = true;
        }

        public Vector3 ClosestPoint(Vector3 from) => _trigger.ClosestPoint(from);

        public InteractionPrompt GetPrompt(in InteractionContext context) =>
            InteractionPrompt.Available(_actionLabel, transform.position + Vector3.up * _trigger.height);

        public void Interact(in InteractionContext context)
        {
            if (!Application.CanStreamedLevelBeLoaded(_sceneName))
            {
                Debug.LogError($"[ScenePortal] '{_sceneName}' 씬이 Build Settings 씬 목록에 없습니다.");
                return;
            }
            SceneManager.LoadScene(_sceneName);
        }
    }
}
