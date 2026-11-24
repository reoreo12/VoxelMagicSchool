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

    public sealed class DialogueChannel
    {
        private Action _onFinished;

        /// <summary>대사를 보여 줄 UI가 이 씬에 있는지 (없으면 Gameplay가 결과를 바로 반영한다).</summary>
        public bool HasPresenter { get; private set; }
        public bool IsPlaying { get; private set; }
        public IReadOnlyList<DialogueLine> Lines { get; private set; } = Array.Empty<DialogueLine>();

        /// <summary>새 대사가 시작될 때 (UI 대사창이 받아 화면을 연다).</summary>
        public event Action Started;

        public void RegisterPresenter() => HasPresenter = true;

        /// <summary>대사를 시작한다. 이미 재생 중이거나 줄이 없으면 false(이때 onFinished는 부르지 않는다).</summary>
        public bool Play(IReadOnlyList<DialogueLine> lines, Action onFinished)
        {
            if (IsPlaying || lines == null || lines.Count == 0) return false;
            Lines = lines;
            _onFinished = onFinished;
            IsPlaying = true;
            Started?.Invoke();
            return true;
        }

        /// <summary>마지막 대사까지 본 뒤 UI가 부른다. 여기서 대사 결과를 한 번에 반영한다.</summary>
        public void Finish()
        {
            if (!IsPlaying) return;
            var done = _onFinished;
            Clear();
            done?.Invoke();
        }

        /// <summary>결과 없이 대사를 버린다(씬이 내려갈 때 등).</summary>
        public void Abort() => Clear();

        private void Clear()
        {
            IsPlaying = false;
            _onFinished = null;
            Lines = Array.Empty<DialogueLine>();
        }
    }
}
