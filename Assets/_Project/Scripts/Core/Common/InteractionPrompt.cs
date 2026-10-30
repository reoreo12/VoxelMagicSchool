// 상호작용 안내 한 건(키, 행동, 가능 여부, 불가 이유, 표시 위치). Gameplay가 만들고 UI가 그린다.
using System;
using UnityEngine;

namespace VoxelMagic.Core
{
    public readonly struct InteractionPrompt : IEquatable<InteractionPrompt>
    {
        public static readonly InteractionPrompt None = default;

        public string KeyLabel { get; }
        public string ActionLabel { get; }
        public bool IsAvailable { get; }
        public string BlockedReason { get; }
        public Vector3 WorldAnchor { get; }

        public bool IsVisible => !string.IsNullOrEmpty(ActionLabel) || !string.IsNullOrEmpty(BlockedReason);

        private InteractionPrompt(string actionLabel, bool isAvailable, string blockedReason, Vector3 worldAnchor)
        {
            KeyLabel = GameTexts.InteractKey;
            ActionLabel = actionLabel;
            IsAvailable = isAvailable;
            BlockedReason = blockedReason;
            WorldAnchor = worldAnchor;
        }

        public static InteractionPrompt Available(string actionLabel, Vector3 worldAnchor)
            => new InteractionPrompt(actionLabel, true, null, worldAnchor);

        public static InteractionPrompt Blocked(string actionLabel, string reason, Vector3 worldAnchor)
            => new InteractionPrompt(actionLabel, false, reason, worldAnchor);

        public bool Equals(InteractionPrompt other)
            => ActionLabel == other.ActionLabel && IsAvailable == other.IsAvailable
               && BlockedReason == other.BlockedReason && WorldAnchor == other.WorldAnchor;

        public override bool Equals(object obj) => obj is InteractionPrompt other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(ActionLabel, IsAvailable, BlockedReason, WorldAnchor);
    }
}
