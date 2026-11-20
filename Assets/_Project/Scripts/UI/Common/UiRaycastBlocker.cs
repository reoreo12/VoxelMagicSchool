// 이 오브젝트의 그래픽은 일부러 클릭을 받아 뒤쪽 입력을 막는다는 표시. UI 검사기가 "Raycast Target 켜짐" 경고에서 뺀다.
using UnityEngine;

namespace VoxelMagic.UI
{
    [DisallowMultipleComponent]
    public sealed class UiRaycastBlocker : MonoBehaviour
    {
    }
}
