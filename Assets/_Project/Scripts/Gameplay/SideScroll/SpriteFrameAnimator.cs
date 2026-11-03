// 2D 스프라이트 캐릭터 재생: 상태 머신의 StateChanged를 받아 그 상태의 프레임 묶음을 fps대로 넘긴다 (섞기 없음, 처음 프레임부터).
// 캐릭터 루트는 Y ±90°로 돌지만 스프라이트는 항상 카메라 정면을 보게 회전을 고정하고, 왼쪽을 볼 때 좌우 반전한다 (그림은 오른쪽을 봄).
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class SpriteFrameAnimator : MonoBehaviour, ISideScrollBinder
    {
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private SpriteCharacterSet _set;

        private CharacterStateMachine _machine;
        private SpriteClip _clip;
        private float _time;

        public void Bind(SideScrollContext context)
        {
            _machine = context.Character;
            _machine.StateChanged += OnStateChanged;
            Play(_machine.CurrentId);
        }

        private void OnDestroy()
        {
            if (_machine != null) _machine.StateChanged -= OnStateChanged;
        }

        private void OnStateChanged(CharacterStateId previous, CharacterStateId next) => Play(next);

        private void Play(CharacterStateId state)
        {
            _clip = _set.ClipFor(state);
            _time = 0f;
            ShowFrame();
        }

        private void Update()
        {
            if (_machine == null || _clip.IsEmpty) return;
            _time += Time.deltaTime;
            ShowFrame();
        }

        private void LateUpdate()
        {
            if (_machine == null) return;
            transform.rotation = Quaternion.identity;
            _renderer.flipX = _machine.Facing == Facing.Left;
        }

        private void ShowFrame()
        {
            if (_clip.IsEmpty) return;
            int frame = SpriteClipMath.FrameAt(_time, _clip.Fps, _clip.Frames.Length, _clip.Loop);
            _renderer.sprite = _clip.Frames[frame];
        }
    }
}
