// 예/아니요 확인 창 요청 창구. 게임 규칙·UI 어디서든 Ask로 묻고, UI의 확인 창이 Asked를 받아 띄운 뒤 답을 Answer로 돌려준다.
using System;

namespace VoxelMagic.Core
{
    public sealed class ConfirmChannel
    {
        private Action _onYes;
        private Action _onNo;

        /// <summary>(질문 문구, 예 버튼 문구, 아니요 버튼 문구)</summary>
        public event Action<string, string, string> Asked;

        public bool IsAsking { get; private set; }
        public string Message { get; private set; }

        /// <summary>확인 창을 띄운다. 띄울 창이 없으면(구독자 없음) false, 이미 묻는 중이어도 false.</summary>
        public bool Ask(string message, Action onYes, Action onNo = null, string yesLabel = null, string noLabel = null)
        {
            if (IsAsking || Asked == null) return false;
            IsAsking = true;
            Message = message;
            _onYes = onYes;
            _onNo = onNo;
            Asked.Invoke(message, yesLabel, noLabel);
            return true;
        }

        /// <summary>확인 창이 닫힐 때 답을 돌려준다 (닫기·ESC = 아니요).</summary>
        public void Answer(bool yes)
        {
            if (!IsAsking) return;
            var callback = yes ? _onYes : _onNo;
            IsAsking = false;
            _onYes = _onNo = null;
            callback?.Invoke();
        }
    }
}
