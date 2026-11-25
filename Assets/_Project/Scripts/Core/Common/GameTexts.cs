// 화면에 보이는 한국어 문구 모음. 문구를 고칠 때 이 파일만 보면 된다.
namespace VoxelMagic.Core
{
    public static class GameTexts
    {
        public const string InteractKey = "E";

        public const string ControlsHint =
            "WASD / 방향키  이동\n" +
            "E  상호작용\n" +
            "1~5 / 클릭  인벤토리 선택\n" +
            "스크롤  낚시 바 이동\n" +
            "Q  의뢰 진행 접기/펼치기\n" +
            "Esc  창 닫기";

        /// <summary>개발 빌드에서만 조작법 아래에 붙는 치트 안내 (퀵슬롯 우클릭 버리기).</summary>
        public const string CheatDiscardHint = "우클릭  인벤토리 칸 통째로 버리기 (치트)";

        public const string ViewRequests = "의뢰 보기";
        public const string Plant = "심기";
        public const string Water = "물 주기";
        public const string Harvest = "뽑기";
        public const string CastLine = "낚싯줄 던지기";
        public const string Deliver = "전달하기";
        public const string Talk = "말 걸기";

        public const string NotEnoughCells = "칸이 부족해요";
        public const string AlreadyPlanted = "이미 심어져 있어요";
        public const string NeedWandToWater = "지팡이로 물을 주자";
        public const string AlreadyWatered = "이미 물을 줬어요";
        public const string NeedWandToFish = "지팡이를 들고 낚시하자";
        public const string InventoryFull = "가방이 가득 찼어요";
        public const string DiscardedFormat = "{0} {1}개를 버렸다";
        public const string NoActiveRequest = "게시판에서 의뢰를 받아 오자";
        public const string MissingItems = "의뢰 물품이 부족해요";

        public const string RequestSlotsFullFormat = "의뢰는 {0}개까지 받을 수 있어요. 크리처에게 전달하거나 포기해서 칸을 비우자";
        public const string RequestSlotsFullShort = "받을 칸이 없어요";
        public const string RequestNotNight = "게시판은 밤에만 열려요";
        public const string RequestBoardClosed = "이제 게시판에 새 의뢰가 올라오지 않아요";
        public const string RequestNotOnBoard = "오늘 게시판에 없는 의뢰예요";
        public const string BoardEmpty = "오늘 남은 의뢰가 없어요";
        public const string BoardSlotsFormat = "받은 의뢰 {0}/{1}";
        public const string SeedRefill = "씨앗 다시 받기";
        public const string SeedRefillGivenFormat = "씨앗 {0}개를 다시 받았다";
        public const string SeedRefillNone = "더 받을 씨앗이 없어요";
        public const string SeedRefillNoRoom = "씨앗을 넣을 칸이 부족해요";
        public const string CreatureNotNight = "밤에 굴로 오면 의뢰를 받을게";
        public const string CreatureTooLate = "이제 늦었어. 의뢰는 더 받을 수 없어";
        public const string DeliveryChoiceTitle = "어떤 의뢰를 전달할까?";
        public const string AbandonButton = "포기";
        public const string AbandonConfirmFormat = "'{0}' 의뢰를 포기할까요?\n받은 씨앗은 그대로 남아요.";
        public const string AbandonedFormat = "'{0}' 의뢰를 포기했다";
        public const string Yes = "예";
        public const string No = "아니요";
        public const string RequestCompleted = "완료";
        public const string RequestNoRoomForSeeds = "씨앗을 넣을 칸이 부족해요";

        public const string BoardTitle = "의뢰 게시판";
        public const string Accept = "수락";
        public const string Accepted = "진행 중";
        public const string Close = "닫기";
        public const string NeedsFormat = "모아 올 것: {0}";
        public const string RewardFormat = "보상: 필기 +{0}";
        public const string TrackerDone = "■";
        public const string TrackerTodo = "□";
        public const string TrackerExpanded = "▼";
        public const string TrackerCollapsed = "▶";
        public const string TrackerCompleted = "완료!";

        public const string PracticalLabel = "실기";
        public const string WrittenLabel = "필기";
        public const string ScoreFormat = "{0} {1}/{2}";

        public const string CreatureName = "'그것'";
        public const string DialogNext = "E / 클릭: 다음";
        public const string DialogClose = "E / 클릭: 닫기";
        public const string MaterialNoteFormat = "[{0}] {1}";

        public const string FishingHint ="스크롤로 바를 움직여\n물고기를 잡아라!";

        public const string AcquiredFormat = "{0} 획득!";
        public const string SizeFormat = "크기 {0:0.#}cm";

        public const string BiteMissed = "입질을 놓쳤다…";
        public const string FishEscaped = "물고기가 도망갔다…";
        public const string FishReleasedInventoryFull = "가방이 가득 차서 놓아줬다";
    }
}
