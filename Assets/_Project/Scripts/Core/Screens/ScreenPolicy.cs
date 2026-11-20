// 팝업 화면 종류와 화면마다의 공통 동작(시간 정지 단계, ESC로 닫힘, 뒤쪽 입력 막기, 플레이어 조작 막기).
// 기본값은 UI_SPEC 5-2·7-4·8-2·10-5·13장의 결정을 따른다.
namespace VoxelMagic.Core
{
    public enum GameScreen
    {
        None,
        Book,
        Inventory,
        RequestBoard,
        RequestDetail,
        CreatureDialog
    }

    public readonly struct ScreenPolicy
    {
        public ScreenPolicy(PauseLevel pause, bool closesOnCancel, bool blocksInputBehind, bool blocksGameplay)
        {
            Pause = pause;
            ClosesOnCancel = closesOnCancel;
            BlocksInputBehind = blocksInputBehind;
            BlocksGameplay = blocksGameplay;
        }

        public PauseLevel Pause { get; }
        /// <summary>ESC를 누르면 이 화면이 맨 위일 때 닫힌다.</summary>
        public bool ClosesOnCancel { get; }
        /// <summary>열려 있는 동안 이 화면 뒤쪽 UI(HUD 버튼 등)는 클릭되지 않는다.</summary>
        public bool BlocksInputBehind { get; }
        /// <summary>열려 있는 동안 플레이어 이동·상호작용을 막는다.</summary>
        public bool BlocksGameplay { get; }

        public static ScreenPolicy DefaultFor(GameScreen screen)
        {
            switch (screen)
            {
                case GameScreen.CreatureDialog:
                    return new ScreenPolicy(PauseLevel.GameClock, closesOnCancel: true, blocksInputBehind: false, blocksGameplay: true);
                case GameScreen.None:
                    return default;
                default: // 책, 가방, 의뢰 게시판, 자세히 패널: 전체 정지 + 입력 막기
                    return new ScreenPolicy(PauseLevel.Full, closesOnCancel: true, blocksInputBehind: true, blocksGameplay: true);
            }
        }
    }
}
