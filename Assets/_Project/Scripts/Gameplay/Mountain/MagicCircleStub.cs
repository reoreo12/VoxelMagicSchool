// 마법진 그리기 임시 대역. 실제 그리기 대신 잠시 기다렸다가, 성공 확률에 따라 동물 요구 정확도 이상/미만의 정확도를 돌려준다.
// 실제 마법진이 생기면 IMagicCircleDrawer를 구현한 컴포넌트로 이것을 바꿔 끼운다.
using System;
using System.Collections;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class MagicCircleStub : MonoBehaviour, IMagicCircleDrawer, IMountainContextBinder
    {
        [Tooltip("그리는 척 기다리는 시간 (초, 가정)")]
        [SerializeField, Min(0f)] private float _drawSeconds = 1f;
        [Tooltip("잘 그릴 확률 (가정 0.85). 이 확률로 요구 정확도 이상, 나머지는 미만을 돌려준다 → 가끔만 놓친다")]
        [SerializeField, Range(0f, 1f)] private float _successChance = 0.85f;

        private MountainContext _context;

        public void Bind(MountainContext context) => _context = context;

        public void Begin(MagicCircleRequest request, Action<float> onFinished)
        {
            _context?.Game.Messages.ShowToast(MountainTexts.Drawing);
            StartCoroutine(Draw(request, onFinished));
        }

        private IEnumerator Draw(MagicCircleRequest request, Action<float> onFinished)
        {
            if (_drawSeconds > 0f) yield return new WaitForSeconds(_drawSeconds);
            onFinished(RollAccuracy(request.Target.RequiredAccuracy));
        }

        private float RollAccuracy(float required)
        {
            IRandom random = _context?.Game.Random;
            float Roll(float min, float max) => random != null ? random.Range(min, max) : UnityEngine.Random.Range(min, max);
            bool good = Roll(0f, 1f) < _successChance;
            return good ? Roll(required, 1f) : Roll(0f, Mathf.Max(0f, required - 0.01f));
        }
    }
}
