// 아이템 분류: 희귀도(필기 점수·출현 확률 기준)와 출처(어디서 얻는 재료인지). 포획은 보류 중이라 자리만 둔다.
namespace VoxelMagic.Core
{
    public enum ItemRarity
    {
        /// <summary>흔함</summary>
        Common,
        /// <summary>보통</summary>
        Uncommon,
        /// <summary>희귀</summary>
        Rare
    }

    public enum ItemSource
    {
        /// <summary>재료가 아님 (씨앗·도구)</summary>
        None,
        Farming,
        Fishing,
        Gathering,
        /// <summary>포획 (보류: 의뢰 재료로 쓰지 않음)</summary>
        Capture
    }

    public static class ItemTypeNames
    {
        public static string Of(ItemRarity rarity) => rarity switch
        {
            ItemRarity.Rare => "희귀",
            ItemRarity.Uncommon => "보통",
            _ => "흔함"
        };

        public static string Of(ItemSource source) => source switch
        {
            ItemSource.Farming => "농작",
            ItemSource.Fishing => "낚시",
            ItemSource.Gathering => "채집",
            ItemSource.Capture => "포획",
            _ => "-"
        };
    }
}
