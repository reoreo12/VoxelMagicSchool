// 복도 캐릭터를 2D 스프라이트로 보일지 3D 아바타로 보일지. 입력(V 키)과 화면 토글이 같은 상태를 바꾸고, 바뀌면 이벤트로 알린다.
using System;

namespace VoxelMagic.Core
{
    public enum CharacterViewMode
    {
        ThreeD,
        TwoD
    }

    public sealed class CharacterViewState
    {
        public CharacterViewState(CharacterViewMode mode) => Mode = mode;

        public event Action Changed;

        public CharacterViewMode Mode { get; private set; }
        public bool Is2D => Mode == CharacterViewMode.TwoD;

        public void Set(CharacterViewMode mode)
        {
            if (Mode == mode) return;
            Mode = mode;
            Changed?.Invoke();
        }

        public void Set2D(bool is2D) => Set(is2D ? CharacterViewMode.TwoD : CharacterViewMode.ThreeD);

        public void Toggle() => Set2D(!Is2D);
    }
}
