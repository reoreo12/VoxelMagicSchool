// 캐릭터 애니메이션 설정: 상태별 팔·다리 각도, 한 주기 시간, 몸통 오르내림, 달리기 기울기, 전환 섞기 시간.
// 클립 값은 메뉴(VoxelMagic/복도/캐릭터 애니메이션 다시 만들기)로 다시 굽고, 섞기 시간은 플레이 중 바로 반영된다.
using System;
using UnityEngine;

namespace VoxelMagic.Core
{
    [Serializable]
    public struct LimbMotion
    {
        [Tooltip("팔·다리 흔들림 최대 각도 (±도)")]
        [SerializeField] private float _swingDegrees;
        [Tooltip("몸통 위아래 움직임 (유닛)")]
        [SerializeField] private float _bob;
        [Tooltip("한 주기 시간 (초)")]
        [SerializeField, Min(0.05f)] private float _cycleSeconds;
        [Tooltip("몸통 앞으로 기울기 (도)")]
        [SerializeField] private float _leanDegrees;

        public LimbMotion(float swingDegrees, float bob, float cycleSeconds, float leanDegrees)
        {
            _swingDegrees = swingDegrees;
            _bob = bob;
            _cycleSeconds = cycleSeconds;
            _leanDegrees = leanDegrees;
        }

        public float SwingDegrees => _swingDegrees;
        public float Bob => _bob;
        public float CycleSeconds => _cycleSeconds;
        public float LeanDegrees => _leanDegrees;
    }

    [CreateAssetMenu(menuName = "VoxelMagic/SideScroll/Character Animation Config", fileName = "CharacterAnimationConfig")]
    public sealed class CharacterAnimationConfig : ScriptableObject
    {
        [Tooltip("Idle: 팔은 거의 정지, 몸통만 숨쉬기")]
        [SerializeField] private LimbMotion _idle = new LimbMotion(2f, 0.012f, 2.4f, 0f);
        [SerializeField] private LimbMotion _walk = new LimbMotion(25f, 0.02f, 0.8f, 0f);
        [SerializeField] private LimbMotion _run = new LimbMotion(45f, 0.04f, 0.5f, 10f);
        [Tooltip("상태가 바뀔 때 섞어서 넘어가는 시간 (초)")]
        [SerializeField, Min(0f)] private float _blendSeconds = 0.12f;
        [Tooltip("클립 한 주기에 찍는 키 수")]
        [SerializeField, Min(4)] private int _keysPerCycle = 16;

        public LimbMotion Idle => _idle;
        public LimbMotion Walk => _walk;
        public LimbMotion Run => _run;
        public float BlendSeconds => _blendSeconds;
        public int KeysPerCycle => _keysPerCycle;

        public LimbMotion For(CharacterStateId state) =>
            state == CharacterStateId.Run ? _run : state == CharacterStateId.Walk ? _walk : _idle;
    }
}
