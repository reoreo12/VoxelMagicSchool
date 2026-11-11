// 마법진 그리기의 계약. 지금은 스텁이 정확도를 돌려주고, 나중에 실제 그리기 화면으로 바꿔 끼운다.
using System;

namespace VoxelMagic.Core
{
    public readonly struct MagicCircleRequest
    {
        public MagicCircleRequest(AnimalDefinition target) => Target = target;

        /// <summary>잡으려는 동물 (도안은 지금 하나).</summary>
        public AnimalDefinition Target { get; }
    }

    public interface IMagicCircleDrawer
    {
        /// <summary>그리기를 시작한다. 끝나면 정확도(0~1)를 onFinished로 돌려준다.</summary>
        void Begin(MagicCircleRequest request, Action<float> onFinished);
    }
}
