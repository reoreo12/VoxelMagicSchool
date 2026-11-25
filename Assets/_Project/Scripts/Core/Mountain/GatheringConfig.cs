// 뒷산 채집 설정(QUEST_TIMER 3-2): 서식지 종류마다 채집 자리 수(8)와 하루 시작 때 켜지는 자리 수(4), 자리마다 희귀도 확률(60/30/10), 나올 수 있는 재료 전부.
// 서식지 종류별 슬롯 수·배치 모양은 씬에 자리를 만드는 에디터 도구(VoxelMagic/뒷산/채집 밸런스)가 쓴다. 종류마다 씬 슬롯 합계가 자리 수 이상이어야 한다.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelMagic.Core
{
    [Serializable]
    public sealed class HabitatBalance
    {
        [SerializeField] private GatherHabitatKind _kind;
        [Tooltip("서식지 오브젝트 하나의 슬롯(재료 자리) 수. 바꾸면 에디터 창에서 '슬롯 수 적용'")]
        [SerializeField, Min(1)] private int _slotCount = 3;
        [Tooltip("슬롯 기본 배치 모양 (서식지 로컬, +Z = 길 쪽). 앞에서부터 슬롯 수만큼 쓴다")]
        [SerializeField] private List<Vector3> _slotLayout = new List<Vector3>();

        public HabitatBalance(GatherHabitatKind kind, int slotCount, IEnumerable<Vector3> layout)
        {
            _kind = kind;
            _slotCount = slotCount;
            _slotLayout = new List<Vector3>(layout ?? Array.Empty<Vector3>());
        }

        public GatherHabitatKind Kind => _kind;
        public int SlotCount => Mathf.Max(1, _slotCount);
        public IReadOnlyList<Vector3> SlotLayout => _slotLayout;
    }

    [CreateAssetMenu(menuName = "VoxelMagic/Config/Gathering", fileName = "GatheringConfig")]
    public sealed class GatheringConfig : ScriptableObject
    {
        [Tooltip("서식지 종류마다 채집 자리 수 (그 종류 서식지 오브젝트들의 슬롯을 차례로 센 앞쪽 N개)")]
        [SerializeField, Min(1)] private int _spotsPerKind = 8;
        [Tooltip("하루 시작 때 서식지 종류마다 켜지는 자리 수")]
        [SerializeField, Min(0)] private int _activeSpotsPerKind = 4;
        [Tooltip("켜진 자리마다 재료 희귀도 확률 (흔함·보통·희귀)")]
        [SerializeField] private RarityTable _rarity = RarityTable.Default;
        [Tooltip("뒷산에 나올 수 있는 채집 재료 전부 (서식지·희귀도는 재료 데이터에서)")]
        [SerializeField] private List<GatherMaterialDefinition> _materials = new List<GatherMaterialDefinition>();
        [Tooltip("서식지 종류별 씬 배치 (슬롯 수·모양). 목록에 없는 종류는 슬롯 3")]
        [SerializeField] private List<HabitatBalance> _habitats = new List<HabitatBalance>();

        private static readonly HabitatBalance Fallback = new HabitatBalance(default, 3, null);

        public int SpotsPerKind => Mathf.Max(1, _spotsPerKind);
        public int ActiveSpotsPerKind => Mathf.Clamp(_activeSpotsPerKind, 0, SpotsPerKind);
        public RarityTable Rarity => _rarity;
        public IReadOnlyList<GatherMaterialDefinition> Materials => _materials;
        public IReadOnlyList<HabitatBalance> Habitats => _habitats;

        public HabitatBalance For(GatherHabitatKind kind)
        {
            foreach (var balance in _habitats)
                if (balance != null && balance.Kind == kind) return balance;
            return Fallback;
        }

        /// <summary>에디터 도구·테스트에서 목록을 통째로 채운다.</summary>
        public void SetHabitats(IEnumerable<HabitatBalance> habitats) => _habitats = new List<HabitatBalance>(habitats);

        public void Set(int spotsPerKind, int activeSpotsPerKind, RarityTable rarity, IEnumerable<GatherMaterialDefinition> materials)
        {
            _spotsPerKind = spotsPerKind;
            _activeSpotsPerKind = activeSpotsPerKind;
            _rarity = rarity;
            _materials = new List<GatherMaterialDefinition>(materials);
        }
    }
}
