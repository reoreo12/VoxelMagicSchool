// 초상화가 있는 대사(크리처와의 대화·수업, UI_SPEC 6장)를 재생하는 창구. Gameplay가 대사 줄과 "끝나면 할 일"을 넘기고, UI 대사창이 보여 준 뒤 Finish를 부른다.
// 대사의 결과(아이템 소모·의뢰 완료·보상)는 onFinished 안에서만 반영한다: 도중에 강제 종료되면 대사를 시작하기 전 상태 그대로 남는다(6-1, 사용자 확정).
using System;
using System.Collections.Generic;

namespace VoxelMagic.Core
{
    /// <summary>대사 한 줄: 화자, 표정 키(비우면 기본 초상화), 본문.</summary>
    public readonly struct DialogueLine
    {
        public DialogueLine(SpeakerProfile speaker, string text, string expression = null)
        {
            Speaker = speaker;
            Text = text;
            Expression = expression;
        }

        public SpeakerProfile Speaker { get; }
        public string Text { get; }
        public string Expression { get; }
    }

    /// <summary>대사창 안 선택지 하나: 글자, 오른쪽 작은 글자(보상 등, 비워도 됨), 흐리게(지금은 안 되는 것, 그래도 고를 수 있음).</summary>
    public readonly struct DialogueOption
    {
        public DialogueOption(string label, string detail = null, bool dimmed = false)
        {
            Label = label;
            Detail = detail;
            Dimmed = dimmed;
        }

        public string Label { get; }
        public string Detail { get; }
        public bool Dimmed { get; }
    }

    public sealed class DialogueChannel
    {
        private static readonly DialogueOption[] NoOptions = Array.Empty<DialogueOption>();

        private Action _onFinished;
        private Action _onAborted;
        private Action<int> _onChosen;

        /// <summary>대사를 보여 줄 UI가 이 씬에 있는지 (없으면 Gameplay가 결과를 바로 반영한다).</summary>
        public bool HasPresenter { get; private set; }
        public bool IsPlaying { get; private set; }
        public IReadOnlyList<DialogueLine> Lines { get; private set; } = Array.Empty<DialogueLine>();
        /// <summary>선택지 (Ask일 때만, 대사만 있을 때는 빈 목록). 질문은 Lines[0].</summary>
        public IReadOnlyList<DialogueOption> Options { get; private set; } = NoOptions;
        /// <summary>ESC로 고를 선택지 (없으면 −1, ESC 무시).</summary>
        public int CancelOption { get; private set; } = -1;
        public bool IsAsking => IsPlaying && Options.Count > 0;

        /// <summary>새 대사가 시작될 때 (UI 대사창이 받아 화면을 연다).</summary>
        public event Action Started;

        /// <summary>개발용: 대사창에 떠 있는 초상화를 화자의 다음(+1)·이전(−1) 표정으로 바꿔 보라는 요청 (치트 키 → UI).</summary>
        public event Action<int> PreviewExpressionRequested;

        public void RegisterPresenter() => HasPresenter = true;

        /// <summary>개발용 치트 키가 부른다. 재생 중일 때만 대사창에 알린다.</summary>
        public void RequestPreviewExpression(int step)
        {
            if (IsPlaying) PreviewExpressionRequested?.Invoke(step);
        }

        /// <summary>대사를 시작한다. 이미 재생 중이거나 줄이 없으면 false(이때 콜백은 부르지 않는다). 도중에 버려지면 onAborted.</summary>
        public bool Play(IReadOnlyList<DialogueLine> lines, Action onFinished, Action onAborted = null)
        {
            if (IsPlaying || lines == null || lines.Count == 0) return false;
            Lines = lines;
            _onFinished = onFinished;
            _onAborted = onAborted;
            IsPlaying = true;
            Started?.Invoke();
            return true;
        }

        /// <summary>
        /// 질문 한 줄 뒤에 선택지를 보여 준다(같은 대사창 안). 고르면 onChosen(번호). cancelOption은 ESC로 고를 번호(−1이면 ESC 무시).
        /// 이전 대사의 끝 콜백 안에서 부르면 대사창이 닫히지 않고 이어진다. 이미 재생 중이거나 선택지가 없으면 false.
        /// </summary>
        public bool Ask(DialogueLine prompt, IReadOnlyList<DialogueOption> options, Action<int> onChosen, int cancelOption = -1, Action onAborted = null)
        {
            if (IsPlaying || options == null || options.Count == 0) return false;
            Lines = new[] { prompt };
            Options = options;
            CancelOption = cancelOption >= 0 && cancelOption < options.Count ? cancelOption : -1;
            _onChosen = onChosen;
            _onAborted = onAborted;
            IsPlaying = true;
            Started?.Invoke();
            return true;
        }

        /// <summary>UI가 선택지를 고르면 부른다. 콜백 안에서 다음 대사·선택지를 이어 열 수 있다.</summary>
        public void Choose(int index)
        {
            if (!IsAsking || index < 0 || index >= Options.Count) return;
            var chosen = _onChosen;
            Clear();
            chosen?.Invoke(index);
        }

        /// <summary>마지막 대사까지 본 뒤 UI가 부른다. 여기서 대사 결과를 한 번에 반영한다.</summary>
        public void Finish()
        {
            if (!IsPlaying || IsAsking) return;
            var done = _onFinished;
            Clear();
            done?.Invoke();
        }

        /// <summary>결과 없이 대사를 버린다(씬이 내려갈 때 등).</summary>
        public void Abort()
        {
            if (!IsPlaying) return;
            var aborted = _onAborted;
            Clear();
            aborted?.Invoke();
        }

        private void Clear()
        {
            IsPlaying = false;
            _onFinished = null;
            _onAborted = null;
            _onChosen = null;
            Lines = Array.Empty<DialogueLine>();
            Options = NoOptions;
            CancelOption = -1;
        }
    }
}
