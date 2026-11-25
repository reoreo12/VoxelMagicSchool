// 뒷산 테스트용 디버그 키 (채집 자리 다시 뽑기, 방과후↔밤 시각 이동, 입질 즉시, 마법진 성공/실패 강제). 키는 Inspector에서 바꾸고, 화면 구석 안내 목록에 등록한다.
using UnityEngine;
using UnityEngine.InputSystem;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class MountainDebugInput : MonoBehaviour, IMountainContextBinder
    {
        [SerializeField] private InputAction _newDay = new InputAction("NewDay", InputActionType.Button, "<Keyboard>/f1");
        [SerializeField] private InputAction _toggleTime = new InputAction("ToggleTime", InputActionType.Button, "<Keyboard>/f3");
        [SerializeField] private InputAction _forceBite = new InputAction("ForceBite", InputActionType.Button, "<Keyboard>/f4");
        [SerializeField] private InputAction _forceSuccess = new InputAction("ForceSuccess", InputActionType.Button, "<Keyboard>/f5");
        [SerializeField] private InputAction _forceFail = new InputAction("ForceFail", InputActionType.Button, "<Keyboard>/f6");

        private MountainContext _context;

        public void Bind(MountainContext context)
        {
            _context = context;
            context.Debug.Register(_newDay.GetBindingDisplayString(), MountainTexts.DebugRespawn);
            context.Debug.Register(_toggleTime.GetBindingDisplayString(), MountainTexts.DebugToggleTime);
            context.Debug.Register(_forceBite.GetBindingDisplayString(), MountainTexts.DebugForceBite);
            context.Debug.Register(_forceSuccess.GetBindingDisplayString(), MountainTexts.DebugForceSuccess);
            context.Debug.Register(_forceFail.GetBindingDisplayString(), MountainTexts.DebugForceFail);
        }

        private void OnEnable()
        {
            Hook(_newDay, OnNewDay, true);
            Hook(_toggleTime, OnToggleTime, true);
            Hook(_forceBite, OnForceBite, true);
            Hook(_forceSuccess, OnForceSuccess, true);
            Hook(_forceFail, OnForceFail, true);
        }

        private void OnDisable()
        {
            Hook(_newDay, OnNewDay, false);
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

        // 오늘 채집 자리를 다시 뽑는다 (하루는 넘기지 않음).
        private void OnNewDay(InputAction.CallbackContext _)
        {
            var progress = _context?.Game.Progress;
            if (progress?.GatherStep != null && progress.Clock != null) progress.GatherStep.Reroll(progress.Clock.Day);
        }

        // 시간대는 게임 시계를 따른다: 방과후면 20:00(밤)으로, 밤이면 16:00(방과후)으로 시각을 옮긴다.
        private void OnToggleTime(InputAction.CallbackContext _)
        {
            var clock = _context?.Game.Clock;
            if (clock == null) { _context?.Time.Toggle(); return; }
            clock.JumpTo(clock.Period == TimeOfDay.Night ? clock.Config.DayStartMinute : clock.Config.PeriodStartHour(TimeOfDay.Night) * 60);
        }
        private void OnForceBite(InputAction.CallbackContext _) => _context?.Capture.SpawnBites();
        private void OnForceSuccess(InputAction.CallbackContext _) => Force(ForcedCaptureResult.Success);
        private void OnForceFail(InputAction.CallbackContext _) => Force(ForcedCaptureResult.Fail);

        private void Force(ForcedCaptureResult result)
        {
            if (_context == null) return;
            _context.Capture.ForceNext(result);
            _context.Debug.NotifyChanged();
        }
    }
}
