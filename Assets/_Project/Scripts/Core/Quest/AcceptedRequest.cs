// 수락한 의뢰 한 건(QUEST_TIMER 2-3). 고유 번호는 수락 순서와 같아서 먼저 받을수록 작다. 상태는 RequestLog가 바꾼다.
namespace VoxelMagic.Core
{
    public sealed class AcceptedRequest
    {
        internal AcceptedRequest(int id, RequestDefinition request)
        {
            Id = id;
            Request = request;
            State = RequestState.InProgress;
        }

        /// <summary>고유 번호 = 수락 순서 (포기·전달 호출에 쓴다).</summary>
        public int Id { get; }
        public RequestDefinition Request { get; }
        /// <summary>진행 중 / 완료 가능 (칸을 비우면 전달됨·포기).</summary>
        public RequestState State { get; internal set; }
        public bool IsReady => State == RequestState.Ready;
        public string Title => Request != null ? Request.Title : string.Empty;
    }
}
