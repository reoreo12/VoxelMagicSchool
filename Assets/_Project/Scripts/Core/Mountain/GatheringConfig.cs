// 뒷산 채집 밸런스: 서식지 종류마다 슬롯 수(기본 배치 모양)와 재료 개수(최소~최대, 추가분이 나올 확률)를 정한다.
// 개수·확률은 다시 배치할 때 바로 쓰이고, 슬롯 수는 에디터 창(VoxelMagic/뒷산/채집 밸런스)에서 프리팹·씬에 적용한다.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelMagic.Core
{
    [Serializable]
    public sealed class HabitatBalance
    {
        [SerializeField] private GatherHabitatKind _kind;
        [Tooltip("서식지 하나의 슬롯(재료 자리) 수. 바꾸면 에디터 창에서 '슬롯 수 적용'")]
        [SerializeField, Min(1)] private int _slotCount = 2;
        [Tooltip("다시 배치할 때 반드시 놓는 개수")]
        [SerializeField, Min(0)] private int _minItems;
        [Tooltip("다시 배치할 때 최대 개수 (슬롯 수를 넘으면 슬롯 수까지)")]
        [SerializeField, Min(0)] private int _maxItems = 1;
        [Tooltip("최소를 넘는 한 개 한 개가 나올 확률 (0~1). 최소 0·최대 1이면 '서식지에 재료가 있을 확률'")]
        [SerializeField, Range(0f, 1f)] private float _extraChance = 0.5f;
        [Tooltip("슬롯 기본 배치 모양 (서식지 로컬, +Z = 길 쪽). 앞에서부터 슬롯 수만큼 쓴다")]
        [SerializeField] private List<Vector3> _slotLayout = new List<Vector3>();

        public HabitatBalance(GatherHabitatKind kind, int slotCount, int minItems, int maxItems, float extraChance, IEnumerable<Vector3> layout)
        {
            _kind = kind;
            _slotCount = slotCount;
            _minItems = minItems;
            _maxItems = maxItems;
            _extraChance = extraChance;
            _slotLayout = new List<Vector3>(layout ?? Array.Empty<Vector3>());
        }

        public GatherHabitatKind Kind => _kind;
        public int SlotCount => Mathf.Max(1, _slotCount);
        public int MinItems => Mathf.Max(0, _minItems);
        public int MaxItems => Mathf.Max(MinItems, _maxItems);
        public float ExtraChance => _extraChance;
        public IReadOnlyList<Vector3> SlotLayout => _slotLayout;

        /// <summary>서식지 하나에 기대되는 평균 재료 수 (슬롯 수 제한 반영 전).</summary>
        public float ExpectedItems => MinItems + (MaxItems - MinItems) * ExtraChance;

        /// <summary>이번에 놓을 개수: 최소 + (최대 − 최소)개 각각을 확률로 굴림, 슬롯 수 이하.</summary>
        public int RollCount(int slotCount, IRandom random)
        {
            int count = MinItems;
            for (int i = MinItems; i < MaxItems; i++)
                if (random.Value < ExtraChance) count++;
            return Mathf.Clamp(count, 0, slotCount);
        }
    }

    [CreateAssetMenu(menuName = "VoxelMagic/Config/Gathering", fileName = "GatheringConfig")]
    public sealed class GatheringConfig : ScriptableObject
    {
        [Tooltip("서식지 종류별 밸런스. 목록에 없는 종류는 슬롯 2·0~1개·확률 0.5 (가정)")]
        [SerializeField] private List<HabitatBalance> _habitats = new List<HabitatBalance>();

        private static readonly HabitatBalance Fallback = new HabitatBalance(default, 2, 0, 1, 0.5f, null);

        public IReadOnlyList<HabitatBalance> Habitats => _habitats;

        public HabitatBalance For(GatherHabitatKind kind)
        {
            foreach (var balance in _habitats)
                if (balance != null && balance.Kind == kind) return balance;
            return Fallback;
        }

        /// <summary>에디터 도구·테스트에서 목록을 통째로 채운다.</summary>
        public void SetHabitats(IEnumerable<HabitatBalance> habitats) => _habitats = new List<HabitatBalance>(habitats);
    }
}
