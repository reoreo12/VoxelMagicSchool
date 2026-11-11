// 캐릭터 동작 이름표. 공용 Animator Controller의 상태 이름과 같고, AvatarSetup이 아바타별 클립을 연결한다.
// 새 동작: 여기에 값을 추가 → AvatarSetup 칸 → 메뉴 VoxelMagic/아바타/공용 Controller 다시 만들기.
namespace VoxelMagic.Core
{
    public enum CharacterActionId
    {
        /// <summary>낚시 던지기 (끝나면 Controller가 FishWait로)</summary>
        FishCast,
        /// <summary>입질 기다리기 (반복)</summary>
        FishWait,
        /// <summary>입질 후 올리기 (FishReelBack과 왕복 반복)</summary>
        FishReel,
        /// <summary>올리기 왕복의 돌아오는 쪽 (FishReel을 뒤집은 클립)</summary>
        FishReelBack,
        /// <summary>낚아채기 (던지기를 뒤집은 클립, 끝나면 Idle)</summary>
        FishHook,
        /// <summary>지팡이로 물 주기 (끝나면 Idle)</summary>
        Water
    }
}
