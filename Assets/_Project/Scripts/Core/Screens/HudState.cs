// 팝업이 아닌 HUD의 보기 상태: 의뢰 목록 드롭다운의 펼침/접힘(화살표 버튼과 Q 키가 같이 씀),
// 마우스가 휠을 쓰는 HUD(퀵슬롯) 위에 있는지(그때 휠은 카메라 줌 대신 HUD가 쓴다).
using System;

namespace VoxelMagic.Core
{
    public sealed class HudState
    {
        public event Action<bool> QuestListToggled;

        // 처음에는 펼친 상태(기존 프로토타입 동작 유지). 시작 상태는 UI_SPEC에 정해지지 않았다.
        public bool IsQuestListExpanded { get; private set; } = true;

        public void SetQuestListExpanded(bool expanded)
        {
            if (IsQuestListExpanded == expanded) return;
            IsQuestListExpanded = expanded;
            QuestListToggled?.Invoke(expanded);
        }

        public void ToggleQuestList() => SetQuestListExpanded(!IsQuestListExpanded);

        /// <summary>마우스가 휠을 받는 HUD 위에 있으면 true. 게임 입력은 이때 휠로 카메라를 줌하지 않는다.</summary>
        public bool IsPointerOverScrollHud { get; private set; }

        public void SetPointerOverScrollHud(bool over) => IsPointerOverScrollHud = over;

        /// <summary>의뢰 드롭다운 바(와 목록)를 화면에 보일지. 바의 X로 끄고 책 의뢰 탭의 토글로 다시 켠다(10-1).</summary>
        public bool IsQuestBarVisible { get; private set; } = true;

        public event Action<bool> QuestBarVisibilityChanged;

        public void SetQuestBarVisible(bool visible)
        {
            if (IsQuestBarVisible == visible) return;
            IsQuestBarVisible = visible;
            QuestBarVisibilityChanged?.Invoke(visible);
        }
    }
}
