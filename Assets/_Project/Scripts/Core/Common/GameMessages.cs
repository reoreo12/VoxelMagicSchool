// 획득 정보, 짧은 알림, 크리처 대사를 UI로 보내는 이벤트 통로.
using System;
using System.Collections.Generic;

namespace VoxelMagic.Core
{
    public readonly struct AcquiredInfo
    {
        public ItemDefinition Item { get; }
        public float SizeCm { get; }

        public AcquiredInfo(ItemDefinition item, float sizeCm)
        {
            Item = item;
            SizeCm = sizeCm;
        }
    }

    public sealed class GameMessages
    {
        public event Action<AcquiredInfo> ItemAcquired;
        public event Action AcquiredDismissed;
        public event Action<string> ToastRequested;
        public event Action<IReadOnlyList<string>> CreatureSpeechRequested;

        public void ShowAcquired(AcquiredInfo info) => ItemAcquired?.Invoke(info);
        public void DismissAcquired() => AcquiredDismissed?.Invoke();
        public void ShowToast(string message) => ToastRequested?.Invoke(message);
        public void ShowCreatureSpeech(IReadOnlyList<string> lines) => CreatureSpeechRequested?.Invoke(lines);
    }
}
