// 개발용 (에디터·개발 빌드만): F8 프리 카메라가 켜져 있을 때 마우스 왼쪽 클릭으로 연결한 크리처의 반응 동작을 재생한다 (E로 말 건 것과 같음).
// 프리 카메라로 가까이 다가가 크리처 애니메이션을 보기 위한 것. 크리처 굴 씬의 크리처에만 붙여 둔다.
using UnityEngine;
using UnityEngine.InputSystem;

namespace VoxelMagic.Gameplay
{
    public sealed class CreatureReactionClickDebug : MonoBehaviour
    {
        [SerializeField] private CreatureReactions _reactions;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void Awake()
        {
            if (_reactions == null) _reactions = GetComponent<CreatureReactions>();
        }

        private void Update()
        {
            var mouse = Mouse.current;
            if (_reactions == null || mouse == null || !FreeCameraToggle.AnyOn) return;
            if (mouse.leftButton.wasPressedThisFrame) _reactions.PlayRandom();
        }
#endif
    }
}
