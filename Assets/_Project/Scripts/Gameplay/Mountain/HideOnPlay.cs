// 에디터 씬 뷰에서는 보이고, 플레이가 시작되면 이 오브젝트와 자식의 메시를 숨긴다. 평지 서식지의 Visual처럼 배치 기준용 메시에 붙인다.
using UnityEngine;

namespace VoxelMagic.Gameplay
{
    public class HideOnPlay : MonoBehaviour
    {
        void Awake()
        {
            foreach (var r in GetComponentsInChildren<Renderer>(true))
                r.enabled = false;
        }
    }
}
