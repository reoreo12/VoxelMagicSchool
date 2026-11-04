// 현재 상호작용 안내를 보관하고, 바뀔 때만 이벤트로 알린다.
using System;

namespace VoxelMagic.Core
{
    public sealed class PromptChannel
    {
        public event Action<InteractionPrompt> Changed;

        public InteractionPrompt Current { get; private set; }

        public void Set(InteractionPrompt prompt)
        {
            if (Current.Equals(prompt)) return;
            Current = prompt;
            Changed?.Invoke(prompt);
        }

        public void Clear() => Set(InteractionPrompt.None);
    }
}
