// 에디터 씬 뷰에서는 보이고, 플레이가 시작되면 이 오브젝트와 자식의 메시를 숨긴다. 평지 서식지의 Visual처럼 배치 기준용 메시에 붙인다.
// 그 아래에 있는 채집 슬롯의 재료 메시는 채집 상태가 켜고 끄므로 건드리지 않는다(부트스트랩이 먼저 켠 재료가 꺼지던 문제).
using UnityEngine;

namespace VoxelMagic.Gameplay
{
    public class HideOnPlay : MonoBehaviour
    {
        void Awake()
        {
            foreach (var r in GetComponentsInChildren<Renderer>(true))
                if (r.GetComponentInParent<GatherPickup>(true) == null) r.enabled = false;
        }
    }
}
