// 2D 스프라이트 캐릭터 한 벌: 상태별 프레임(Sprite)·fps·반복과 목표 키. json에서 메뉴(VoxelMagic/아바타/선택한 2D 스프라이트 준비)로 만든다.
using System;
using UnityEngine;

namespace VoxelMagic.Core
{
    [Serializable]
    public struct SpriteClip
    {
        [SerializeField] private Sprite[] _frames;
        [SerializeField, Min(0.1f)] private float _fps;
        [SerializeField] private bool _loop;

        public SpriteClip(Sprite[] frames, float fps, bool loop)
        {
            _frames = frames;
            _fps = fps;
            _loop = loop;
        }

        public Sprite[] Frames => _frames;
        public float Fps => _fps;
        public bool Loop => _loop;
        public bool IsEmpty => _frames == null || _frames.Length == 0;
    }

    [CreateAssetMenu(menuName = "VoxelMagic/Sprite Character Set", fileName = "SpriteCharacterSet")]
    public sealed class SpriteCharacterSet : ScriptableObject
    {
        [Tooltip("임포트 PPU를 이 키(유닛)에 맞춰 계산한다 (Idle 그림 높이 기준)")]
        [SerializeField, Min(0.1f)] private float _targetHeight = 1.6f;
        [SerializeField] private SpriteClip _idle;
        [SerializeField] private SpriteClip _walk;
        [SerializeField] private SpriteClip _run;

        public float TargetHeight => _targetHeight;

        public SpriteClip ClipFor(CharacterStateId state) =>
            state == CharacterStateId.Run ? _run : state == CharacterStateId.Walk ? _walk : _idle;
    }

    /// <summary>프레임 애니메이션 계산: 재생 시간 → 보여 줄 프레임 번호. 반복이 아니면 마지막 프레임에서 멈춘다.</summary>
    public static class SpriteClipMath
    {
        public static int FrameAt(float time, float fps, int frameCount, bool loop)
        {
            if (frameCount <= 0) return -1;
            int index = (int)Math.Floor(Math.Max(0f, time) * fps);
            return loop ? index % frameCount : Math.Min(index, frameCount - 1);
        }
    }
}
