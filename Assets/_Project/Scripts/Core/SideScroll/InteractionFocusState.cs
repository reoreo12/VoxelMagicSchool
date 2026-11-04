// 복도에서 지금 상호작용할 수 있는 대상의 안내 문구. 상호작용 감지(Gameplay)가 바꾸고, 안내 UI가 이벤트로 받아 표시한다.
using System;

namespace VoxelMagic.Core
{
    public sealed class InteractionFocusState
    {
        public event Action Changed;

        public InteractionPrompt Prompt { get; private set; } = InteractionPrompt.None;
        public bool HasTarget => Prompt.IsVisible;

        public void Set(InteractionPrompt prompt)
        {
            if (Prompt.Equals(prompt)) return;
            Prompt = prompt;
            Changed?.Invoke();
        }

        public void Clear() => Set(InteractionPrompt.None);
    }
}
