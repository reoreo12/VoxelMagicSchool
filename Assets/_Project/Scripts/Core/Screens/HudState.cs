// 팝업이 아닌 HUD의 보기 상태. 지금은 의뢰 목록 드롭다운의 펼침/접힘만 둔다(화살표 버튼과 Q 키가 같이 쓴다).
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
    }
}
