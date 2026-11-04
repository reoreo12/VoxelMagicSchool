// 횡스크롤 캐릭터 루트. 매 프레임 입력 → 상태 머신 → 위치 적용 순서로 돌린다.
// 위치(Y·Z 고정)와 회전(Y축 ±90°만)을 매 프레임 코드로 덮어써서 다른 값이 끼어들 틈을 없앤다.
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class SideScrollCharacter : MonoBehaviour, ISideScrollBinder
    {
        [SerializeField] private SideScrollInputReader _input;

        private CharacterStateMachine _machine;
        private CharacterMoveConfig _config;

        public void Bind(SideScrollContext context)
        {
            _machine = context.Character;
            _config = context.MoveConfig;
            Apply();
        }

        private void Update()
        {
            if (_machine == null) return;
            _machine.Update(_input.Read(), Time.deltaTime);
            Apply();
        }

        private void Apply()
        {
            float yaw = _machine.Facing == Facing.Right ? _config.FacingYaw : -_config.FacingYaw;
            transform.SetPositionAndRotation(new Vector3(_machine.Motor.X, _config.GroundY, _config.Z),
                Quaternion.Euler(0f, yaw, 0f));
        }
    }
}
