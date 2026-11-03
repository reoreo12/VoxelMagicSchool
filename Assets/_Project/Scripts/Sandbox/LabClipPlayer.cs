// Animator Controller 없이 Playable로 아무 클립이나 바로 재생한다. 두 클립을 섞어 전환하고,
// 반복 On/Off·재생 속도·일시정지·시간 이동·프레임 단위 이동을 직접 계산한다 (임포트 설정은 바꾸지 않음).
using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace VoxelMagic.Sandbox
{
    public sealed class LabClipPlayer : IDisposable
    {
        private PlayableGraph _graph;
        private AnimationMixerPlayable _mixer;
        private AnimationClipPlayable _current;
        private AnimationClipPlayable _previous;
        private float _blendDuration;
        private float _blendElapsed;

        public LabClipPlayer(Animator animator)
        {
            _graph = PlayableGraph.Create("AnimationLab");
            _graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
            var output = AnimationPlayableOutput.Create(_graph, "Lab", animator);
            _mixer = AnimationMixerPlayable.Create(_graph, 2);
            output.SetSourcePlayable(_mixer);
            _graph.Play();
        }

        public AnimationClip Clip { get; private set; }
        public bool Loop { get; set; } = true;
        public float Speed { get; set; } = 1f;
        public bool Paused { get; set; }
        public float Time => _current.IsValid() ? (float)_current.GetTime() : 0f;
        public float Length => Clip != null ? Clip.length : 0f;
        public bool IsBlending => _previous.IsValid();
        public float BlendProgress => IsBlending && _blendDuration > 0f ? Mathf.Clamp01(_blendElapsed / _blendDuration) : 1f;

        /// <summary>지금 클립에서 blend초 동안 섞어 새 클립으로 넘어간다. 섞는 중이던 이전 클립은 버린다.</summary>
        public void Play(AnimationClip clip, float blend)
        {
            DropPrevious();
            if (_current.IsValid())
            {
                _graph.Disconnect(_mixer, 0);
                _previous = _current;
                _graph.Connect(_previous, 0, _mixer, 1);
            }
            _current = AnimationClipPlayable.Create(_graph, clip);
            _current.SetApplyFootIK(true);
            _graph.Connect(_current, 0, _mixer, 0);

            Clip = clip;
            _blendElapsed = 0f;
            _blendDuration = blend;
            if (blend <= 0f) DropPrevious();
            Evaluate();
        }

        public void Tick(float deltaTime)
        {
            if (!_current.IsValid()) return;
            if (!Paused)
            {
                float step = deltaTime * Speed;
                Advance(_current, step);
                if (_previous.IsValid())
                {
                    Advance(_previous, step);
                    _blendElapsed += deltaTime;
                    if (_blendElapsed >= _blendDuration) DropPrevious();
                }
            }
            Evaluate();
        }

        /// <summary>시간 이동. 섞는 중이면 섞기를 끝내고 지금 클립만 남긴다.</summary>
        public void Scrub(float time)
        {
            if (!_current.IsValid()) return;
            DropPrevious();
            _current.SetTime(Wrap(time, Length));
            Evaluate();
        }

        public void StepFrames(int frames, float frameRate)
        {
            Paused = true;
            Scrub(Time + frames / frameRate);
        }

        public void Dispose()
        {
            if (_graph.IsValid()) _graph.Destroy();
        }

        private void Advance(AnimationClipPlayable playable, float step)
        {
            float length = playable.GetAnimationClip().length;
            playable.SetTime(Wrap((float)playable.GetTime() + step, length));
        }

        // 반복이면 길이로 감싸고, 아니면 끝에서 멈춘다.
        private float Wrap(float time, float length)
        {
            if (length <= 0f) return 0f;
            if (Loop) return Mathf.Repeat(time, length);
            return Mathf.Clamp(time, 0f, length);
        }

        private void DropPrevious()
        {
            if (!_previous.IsValid()) return;
            _graph.Disconnect(_mixer, 1);
            _previous.Destroy();
        }

        private void Evaluate()
        {
            float weight = BlendProgress;
            _mixer.SetInputWeight(0, _previous.IsValid() ? weight : 1f);
            _mixer.SetInputWeight(1, _previous.IsValid() ? 1f - weight : 0f);
            _graph.Evaluate(0f);
        }
    }
}
