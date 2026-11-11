// 뒷산 테스트용 디버그 키 (새 하루 = 재료 다시 배치, 의뢰 교체, 시간대 전환). 키는 Inspector에서 바꾸고, 화면 구석 안내 목록에 등록한다.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class MountainDebugInput : MonoBehaviour, IMountainContextBinder
    {
        [SerializeField] private InputAction _newDay = new InputAction("NewDay", InputActionType.Button, "<Keyboard>/f1");
        [SerializeField] private InputAction _swapRequest = new InputAction("SwapRequest", InputActionType.Button, "<Keyboard>/f2");
        [SerializeField] private InputAction _toggleTime = new InputAction("ToggleTime", InputActionType.Button, "<Keyboard>/f3");
        [Tooltip("의뢰 교체 키가 차례로 돌릴 의뢰 (완료한 의뢰는 건너뜀)")]
        [SerializeField] private List<RequestDefinition> _swapRequests = new List<RequestDefinition>();

        private MountainContext _context;

        public void Bind(MountainContext context)
        {
            _context = context;
            context.Debug.Register(_newDay.GetBindingDisplayString(), MountainTexts.DebugRespawn);
            context.Debug.Register(_swapRequest.GetBindingDisplayString(), MountainTexts.DebugSwapRequest);
            context.Debug.Register(_toggleTime.GetBindingDisplayString(), MountainTexts.DebugToggleTime);
            context.Debug.Register(MountainTexts.DebugDiscardKey, MountainTexts.DebugDiscard);
        }

        private void OnEnable()
        {
            _newDay.performed += OnNewDay;
            _swapRequest.performed += OnSwapRequest;
            _toggleTime.performed += OnToggleTime;
            _newDay.Enable();
            _swapRequest.Enable();
            _toggleTime.Enable();
        }

        private void OnDisable()
        {
            _newDay.performed -= OnNewDay;
            _swapRequest.performed -= OnSwapRequest;
            _toggleTime.performed -= OnToggleTime;
            _newDay.Disable();
            _swapRequest.Disable();
            _toggleTime.Disable();
        }

        private void OnNewDay(InputAction.CallbackContext _) => _context?.Gathering.Respawn();
        private void OnToggleTime(InputAction.CallbackContext _) => _context?.Time.Toggle();
        private void OnSwapRequest(InputAction.CallbackContext _) => SwapRequest();

        /// <summary>진행 중 의뢰를 내려놓고 목록의 다음(완료 안 한) 의뢰를 받는다.</summary>
        public void SwapRequest()
        {
            if (_context == null || _swapRequests.Count == 0) return;
            var tracker = _context.Game.Requests;
            int start = tracker.HasActive ? _swapRequests.IndexOf(tracker.Active) : -1;
            tracker.Abandon();
            for (int step = 1; step <= _swapRequests.Count; step++)
            {
                var next = _swapRequests[(start + step + _swapRequests.Count) % _swapRequests.Count];
                if (next == null || tracker.GetStatus(next) == RequestStatus.Completed) continue;
                if (tracker.Accept(next) == AcceptResult.Success)
                {
                    _context.Game.Messages.ShowToast(string.Format(MountainTexts.RequestSwappedFormat, next.Title));
                    return;
                }
            }
            _context.Game.Messages.ShowToast(MountainTexts.NoRequestToSwap);
        }
    }
}
