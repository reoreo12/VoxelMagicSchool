// 이 오브젝트의 그래픽은 일부러 클릭을 받아 뒤쪽 입력을 막는다는 표시. UI 검사기가 "Raycast Target 켜짐" 경고에서 뺀다.
// 눌리면 Clicked로 알린다(팝업 바깥 클릭 = 닫기에 쓴다).
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace VoxelMagic.UI
{
    [DisallowMultipleComponent]
    public sealed class UiRaycastBlocker : MonoBehaviour, IPointerClickHandler
    {
        public event Action Clicked;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) Clicked?.Invoke();
        }
    }
}
