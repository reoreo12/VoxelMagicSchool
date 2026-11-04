// 복셀 모델용 조명 그림자 보정. 복셀 메시는 면마다 정점이 따로라, URP 기본 Normal Bias가 면을 안쪽으로 밀면 면 사이가 벌어져
// 그림자에 구멍이 난다. 씬 조명만 Normal Bias 0으로 바꾼다 (프로젝트 URP 설정은 그대로). 씬 생성 도구들이 함께 쓴다.
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace VoxelMagic.EditorTools
{
    public static class VoxelShadowSettings
    {
        // 2026-11-03 비교: 0.5(기본) → 0 에서 아바타 그림자 구멍이 메워지고, 빛 받는 면에 새 얼룩은 생기지 않음 (CHANGELOG)
        private const float DepthBias = 0.1f;
        private const float NormalBias = 0f;

        public static void Apply(Light light)
        {
            var data = light.GetUniversalAdditionalLightData();
            data.usePipelineSettings = false;
            light.shadowBias = DepthBias;
            light.shadowNormalBias = NormalBias;
        }
    }
}
