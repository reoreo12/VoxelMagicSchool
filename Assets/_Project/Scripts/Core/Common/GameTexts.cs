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

        public const string ViewRequests = "의뢰 보기";
        public const string Plant = "심기";
        public const string Water = "물 주기";
        public const string Harvest = "뽑기";
        public const string CastLine = "낚싯줄 던지기";
        public const string Deliver = "전달하기";

        public const string NotEnoughCells = "칸이 부족해요";
        public const string AlreadyPlanted = "이미 심어져 있어요";
        public const string NeedWandToWater = "지팡이로 물을 주자";
        public const string NeedWandToFish = "지팡이를 들고 낚시하자";
        public const string InventoryFull = "가방이 가득 찼어요";
        public const string NoActiveRequest = "게시판에서 의뢰를 받아 오자";
        public const string MissingItems = "의뢰 물품이 부족해요";

        public const string RequestAlreadyActive = "진행 중인 의뢰가 있어요";
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

        public const string BiteMissed = "입질을 놓쳤다…";
        public const string FishEscaped = "물고기가 도망갔다…";
        public const string FishReleasedInventoryFull = "가방이 가득 차서 놓아줬다";
    }
}
