// 애니메이션 클립의 Animation Event를 받아 C# 이벤트로 바꾸는 부품. Animator는 같은 오브젝트(모델)의 함수만 부를 수 있어서 모델에 붙인다.
// 클립 이벤트 함수 이름은 OnActionEvent, 문자열 값이 이벤트 이름 (CastRelease, WaterApply …). ClipImportPlan에서 넣는다.
using System;
using UnityEngine;

namespace VoxelMagic.Gameplay
{
    public sealed class CharacterAnimationEvents : MonoBehaviour
    {
        public const string FunctionName = nameof(OnActionEvent);
        public const string CastRelease = "CastRelease";
        public const string WaterApply = "WaterApply";

        /// <summary>이벤트 이름 (예: CastRelease).</summary>
        public event Action<string> Fired;

        // Animator가 클립 이벤트로 부른다 (이름을 바꾸면 클립 이벤트도 바꿔야 함).
        public void OnActionEvent(string id) => Fired?.Invoke(id);
    }
}
