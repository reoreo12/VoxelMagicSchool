// 의뢰 게시판. E를 누르면 의뢰 목록 창을 열어 달라고 ScreenState에 요청한다.
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class RequestBoard : InteractableBase
    {
        public override InteractionPrompt GetPrompt(in InteractionContext context)
            => InteractionPrompt.Available(GameTexts.ViewRequests, PromptAnchor);

        public override void Interact(in InteractionContext context)
            => Context.Screen.Open(GameWindow.RequestBoard);
    }
}
