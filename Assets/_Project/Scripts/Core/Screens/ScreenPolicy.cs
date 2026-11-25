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
        /// <summary>초상화 대사창 (크리처 대화·수업, UI_SPEC 6장)</summary>
        Dialogue,
        /// <summary>크리처에게 전달할 의뢰 고르기 (QUEST_TIMER 7-7, 임시 UI)</summary>
        DeliveryChoice,
        /// <summary>예/아니요 확인 창 (포기 등, 임시 UI)</summary>
        Confirm
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
                case GameScreen.Dialogue:
                    // 게임 시계만 멈춤(캐릭터 애니메이션은 계속, 13장·사용자 확인 2026-11-23). ESC·바깥 클릭으로 닫히지 않음(6-1).
                    return new ScreenPolicy(PauseLevel.GameClock, closesOnCancel: false, blocksInputBehind: false, blocksGameplay: true);
                case GameScreen.DeliveryChoice:
                    // 대화의 일부라 대화와 같게 게임 시계만 멈춤(QUEST_TIMER 7-7). ESC = 고르지 않고 대화 끝.
                    return new ScreenPolicy(PauseLevel.GameClock, closesOnCancel: true, blocksInputBehind: true, blocksGameplay: true);
                case GameScreen.None:
                    return default;
                default: // 책, 가방, 의뢰 게시판, 자세히 패널: 전체 정지 + 입력 막기
                    return new ScreenPolicy(PauseLevel.Full, closesOnCancel: true, blocksInputBehind: true, blocksGameplay: true);
            }
        }
    }
}
