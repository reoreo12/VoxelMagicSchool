// 애니메이션 랩 화면 문구(한국어).
namespace VoxelMagic.Sandbox
{
    public static class LabTexts
    {
        public const string Play = "재생";
        public const string Pause = "일시정지";
        public const string TimeFormat = "{0:0.00} / {1:0.00}초   ·   {2} / {3}프레임";
        public const string SpeedFormat = "재생 속도 {0:0.00}배";
        public const string BlendFormat = "섞기 {0:0.00}초";
        public const string AbFormat = "A↔B 간격 {0:0.0}초";
        public const string AbPair = "A↔B: {0}  ↔  {1}";
        public const string AbNeedTwo = "A↔B: 클립 두 개를 차례로 재생하면 그 둘을 오갑니다";
        public const string NowPlaying = "재생 중: {0}";
        public const string Blending = "   (섞는 중 {0:0}%)";
        public const string NoClip = "재생 중인 클립 없음";
        public const string ClipEntry = "{0}   <color=#9aa>{1:0.00}초 · {2}</color>";
        public const string UnusableEntry = "<color=#888>[Generic] {0} · {1}   이 아바타에 쓸 수 없음</color>";
        public const string GenericEntry = "<color=#e9a>[Generic]</color> {0}   <color=#9aa>눌러서 Humanoid로 임포트</color>";
        public const string ClipCount = "애니메이션 {0}개 (Humanoid {1}개)   ·   Assets/_Project/Animations";
        public const string AvatarInfo = "{0}\n키 {1:0.00}  ·  Humanoid {2}  ·  매핑된 뼈 {3}개\n상태 클립: Idle {4} / Walk {5} / Run {6}";
        public const string Yes = "예";
        public const string No = "아니요";
        public const string None = "없음";
        public const string NoAvatar = "Data/Avatars에 아바타가 없습니다. 메뉴 VoxelMagic/아바타/1로 모델을 준비하세요.";
        public const string NotHuman = "{0}: Humanoid 아바타가 아니라 재생할 수 없습니다 (메뉴 1을 실행하세요)";
        public const string StateMissing = "{0} 상태에 연결된 클립이 없습니다 (AvatarSetup에서 연결)";
        public const string Imported = "{0}: Humanoid로 임포트했습니다";
        public const string ImportFailed = "{0}: Humanoid로 임포트하지 못했습니다";
        public const string ModelsAdded = "새 모델 {0}개를 준비해 목록에 추가했습니다: {1}";
        public const string NoBridge = "파일 검색은 에디터에서 플레이할 때만 됩니다";
        public const string MeterWaiting = "디딤발 속도: 걷기·달리기 클립을 재생하면 잽니다";
        public const string MeterFormat = "디딤발 속도 {0:0.00} m/s (재생 속도 1 기준)";
        public const string SuggestWalk = "\n걷기로 쓰면: 이동 {0:0.0} m/s → 재생 속도 {1:0.00}배 (AvatarSetup의 Walk 속도)";
        public const string SuggestRun = "\n달리기로 쓰면: 이동 {0:0.0} m/s → 재생 속도 {1:0.00}배 (AvatarSetup의 Run 속도)";
        public const string HipFormat = "\n골반 앞뒤 이동 폭 {0:0.000} m  {1}";
        public const string InPlace = "(제자리)";
        public const string NotInPlace = "<color=#e9a>(앞으로 이동함 - In Place 아님)</color>";
        public const string CameraHelp = "우클릭 드래그 회전 · 휠 확대 · 가운데 드래그 이동\nSpace 재생/일시정지 · ←/→ 한 프레임 · 1/2/3 Idle/Walk/Run · L 반복 · P 도트";
        public const string PixelToggle = "도트 렌더링 (P)";
        public const string PixelOn = "도트 {0}배 · {1}×{2}";
        public const string PixelOff = "도트 {0}배 (꺼짐)";
    }
}
