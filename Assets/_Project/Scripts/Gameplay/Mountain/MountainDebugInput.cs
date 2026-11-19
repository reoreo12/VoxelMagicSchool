// 뒷산 테스트용 디버그 키 (새 하루 = 재료 다시 배치, 의뢰 교체, 시간대 전환, 입질 즉시, 마법진 성공/실패 강제). 키는 Inspector에서 바꾸고, 화면 구석 안내 목록에 등록한다.
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
        [SerializeField] private InputAction _forceBite = new InputAction("ForceBite", InputActionType.Button, "<Keyboard>/f4");
        [SerializeField] private InputAction _forceSuccess = new InputAction("ForceSuccess", InputActionType.Button, "<Keyboard>/f5");
        [SerializeField] private InputAction _forceFail = new InputAction("ForceFail", InputActionType.Button, "<Keyboard>/f6");
        [Tooltip("의뢰 교체 키가 차례로 돌릴 의뢰 (완료한 의뢰는 건너뜀)")]
        [SerializeField] private List<RequestDefinition> _swapRequests = new List<RequestDefinition>();

        private MountainContext _context;

        public void Bind(MountainContext context)
        {
            _context = context;
            context.Debug.Register(_newDay.GetBindingDisplayString(), MountainTexts.DebugRespawn);
            context.Debug.Register(_swapRequest.GetBindingDisplayString(), MountainTexts.DebugSwapRequest);
            context.Debug.Register(_toggleTime.GetBindingDisplayString(), MountainTexts.DebugToggleTime);
            context.Debug.Register(_forceBite.GetBindingDisplayString(), MountainTexts.DebugForceBite);
            context.Debug.Register(_forceSuccess.GetBindingDisplayString(), MountainTexts.DebugForceSuccess);
            context.Debug.Register(_forceFail.GetBindingDisplayString(), MountainTexts.DebugForceFail);
            context.Debug.Register(MountainTexts.DebugDiscardKey, MountainTexts.DebugDiscard);
        }

        private void OnEnable()
        {
            Hook(_newDay, OnNewDay, true);
            Hook(_swapRequest, OnSwapRequest, true);
            Hook(_toggleTime, OnToggleTime, true);
            Hook(_forceBite, OnForceBite, true);
            Hook(_forceSuccess, OnForceSuccess, true);
            Hook(_forceFail, OnForceFail, true);
        }

        private void OnDisable()
        {
            Hook(_newDay, OnNewDay, false);
            Hook(_swapRequest, OnSwapRequest, false);
            Hook(_toggleTime, OnToggleTime, false);
            Hook(_forceBite, OnForceBite, false);
            Hook(_forceSuccess, OnForceSuccess, false);
            Hook(_forceFail, OnForceFail, false);
        }

        private static void Hook(InputAction action, System.Action<InputAction.CallbackContext> handler, bool on)
        {
            if (on)
            {
                action.performed += handler;
                action.Enable();
            }
            else
            {
                action.performed -= handler;
                action.Disable();
            }
        }

        private void OnNewDay(InputAction.CallbackContext _) => _context?.Gathering.Respawn();
        private void OnToggleTime(InputAction.CallbackContext _) => _context?.Time.Toggle();
        private void OnSwapRequest(InputAction.CallbackContext _) => SwapRequest();
        private void OnForceBite(InputAction.CallbackContext _) => _context?.Capture.SpawnBites();
        private void OnForceSuccess(InputAction.CallbackContext _) => Force(ForcedCaptureResult.Success);
        private void OnForceFail(InputAction.CallbackContext _) => Force(ForcedCaptureResult.Fail);

        private void Force(ForcedCaptureResult result)
        {
            if (_context == null) return;
            _context.Capture.ForceNext(result);
            _context.Debug.NotifyChanged();
        }

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
