// 낚시 설정(대기·입질 시간, 스크롤 감도, 게이지 속도, 낚을 때마다의 희귀도 확률, 연못 물고기 출현 표).
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelMagic.Core
{
    [Serializable]
    public struct FishWeight
    {
        [SerializeField] private FishDefinition _fish;
        [SerializeField, Min(0f)] private float _weight;

        public FishWeight(FishDefinition fish, float weight)
        {
            _fish = fish;
            _weight = weight;
        }

        public FishDefinition Fish => _fish;
        public float Weight => _weight;
    }

    [CreateAssetMenu(menuName = "VoxelMagic/Config/Fishing", fileName = "FishingConfig")]
    public sealed class FishingConfig : ScriptableObject
    {
        [Tooltip("찌를 놓은 뒤 물에 닿기까지 (초)")]
        [SerializeField, Min(0f)] private float _castSeconds = 0.6f;
        [Tooltip("던지기 애니메이션의 '놓는 순간' 신호를 이만큼 기다려도 안 오면 스스로 놓는다 (초)")]
        [SerializeField, Min(0f)] private float _castReleaseTimeoutSeconds = 6f;
        [SerializeField] private FloatRange _biteWaitSeconds = new FloatRange(2f, 6f);
        [SerializeField, Min(0.1f)] private float _biteWindowSeconds = 1f;
        [Tooltip("입질을 놓치면 다시 기다릴지 (끄면 낚시가 끝난다)")]
        [SerializeField] private bool _rewaitOnMissedBite = true;
        [Tooltip("스크롤 한 칸당 잡기 바 이동량 (바 높이 0~1 기준)")]
        [SerializeField, Min(0.001f)] private float _scrollSensitivity = 0.08f;
        [SerializeField, Range(0.05f, 1f)] private float _catchBarSize = 0.25f;
        [SerializeField, Range(0.01f, 0.99f)] private float _gaugeStart = 0.3f;
        [SerializeField, Min(0.01f)] private float _gaugeFillPerSecond = 0.35f;
        [SerializeField, Min(0.01f)] private float _gaugeDrainPerSecond = 0.25f;
        [Tooltip("낚을 때마다 희귀도 확률 (흔함·보통·희귀). 고른 희귀도의 물고기 중에서 아래 가중치로 고른다")]
        [SerializeField] private RarityTable _rarity = RarityTable.Default;
        [Tooltip("연못 물고기와 같은 희귀도 안에서의 가중치")]
        [SerializeField] private List<FishWeight> _pondFish = new List<FishWeight>();

        public float CastSeconds => _castSeconds;
        public float CastReleaseTimeoutSeconds => _castReleaseTimeoutSeconds;
        public FloatRange BiteWaitSeconds => _biteWaitSeconds;
        public float BiteWindowSeconds => _biteWindowSeconds;
        public bool RewaitOnMissedBite => _rewaitOnMissedBite;
        public float ScrollSensitivity => _scrollSensitivity;
        public float CatchBarSize => _catchBarSize;
        public float GaugeStart => _gaugeStart;
        public float GaugeFillPerSecond => _gaugeFillPerSecond;
        public float GaugeDrainPerSecond => _gaugeDrainPerSecond;
        public IReadOnlyList<FishWeight> PondFish => _pondFish;
        public RarityTable Rarity => _rarity;

        /// <summary>희귀도 확률로 희귀도를 고르고(forced가 있으면 그것), 그 희귀도의 연못 물고기 중 가중치로 하나 고른다. 표가 비어 있으면 null.</summary>
        public FishDefinition PickFish(IRandom random, ItemRarity? forced = null)
        {
            var rarity = forced ?? _rarity.Roll(random);
            var fishes = new List<FishDefinition>();
            foreach (var entry in _pondFish)
                if (entry.Fish != null && entry.Weight > 0f && !fishes.Contains(entry.Fish)) fishes.Add(entry.Fish);
            var candidates = RarityTable.CandidatesOf(fishes, rarity);
            if (candidates.Count == 0) return null;

            float total = 0f;
            foreach (var entry in _pondFish)
                if (candidates.Contains(entry.Fish)) total += entry.Weight;
            float roll = random.Range(0f, total);
            FishDefinition last = null;
            foreach (var entry in _pondFish)
            {
                if (!candidates.Contains(entry.Fish) || entry.Weight <= 0f) continue;
                last = entry.Fish;
                if (roll < entry.Weight) return entry.Fish;
                roll -= entry.Weight;
            }
            return last;
        }
    }
}
