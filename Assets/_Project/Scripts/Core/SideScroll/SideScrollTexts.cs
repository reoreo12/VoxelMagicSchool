// 복도 씬 화면 문구(한국어). 프로토타입 1의 GameTexts와 따로 둔다.
namespace VoxelMagic.Core
{
    public static class SideScrollTexts
    {
        public const string ControlsHint = "A / ← 왼쪽으로 걷기\nD / → 오른쪽으로 걷기\nShift 누르는 동안 달리기\nP 도트 렌더링 켜기/끄기";
        public const string StateFormat = "상태: {0}";
        public const string PixelToggle = "도트 렌더링 (P)";
        public const string PixelOn = "도트 {0}배 · {1}×{2}";
        public const string PixelOff = "도트 {0}배 (꺼짐)";

        public static string StateName(CharacterStateId state) => state switch
        {
            CharacterStateId.Idle => "서 있기",
            CharacterStateId.Walk => "걷기",
            CharacterStateId.Run => "달리기",
            _ => state.ToString()
        };
    }
}
