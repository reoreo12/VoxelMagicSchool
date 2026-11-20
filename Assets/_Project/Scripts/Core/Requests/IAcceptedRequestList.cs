// 수락한 의뢰 목록을 UI가 읽는 창구(UI_SPEC 4-A-3, 10장). UI는 의뢰 규칙 대신 이 인터페이스만 본다.
// 지금은 개수만 둔다. 목록 항목(제목·진행·완료·수락 순서)은 의뢰 드롭다운 목록을 만들 때 넓힌다.
using System;

namespace VoxelMagic.Core
{
    public interface IAcceptedRequestList
    {
        /// <summary>지금 수락해 진행 중인 의뢰 수.</summary>
        int AcceptedCount { get; }

        /// <summary>수락·완료·포기로 목록이 바뀔 때.</summary>
        event Action AcceptedListChanged;
    }
}
