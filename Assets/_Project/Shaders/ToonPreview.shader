// 획득 창에서 도는 재료 모델용 툰 셰이더(URP): 원래 색(_BaseMap × _BaseColor)에 고정 빛 방향의 N·L을 3단(밝음·중간·그늘)으로 끊어 곱한다.
// 장면 조명을 쓰지 않아 낮·밤과 상관없이 같은 모습이다. 빛 방향·단계 값은 머티리얼 속성(코드가 테마 값으로 채움).
Shader "VoxelMagic/ToonPreview"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _ToonLightDir ("Light Direction (world, toward light)", Vector) = (-0.45, 0.75, -0.5, 0)
        _ToonThresholds ("Thresholds (bright, mid)", Vector) = (0.45, 0.05, 0, 0)
        _ToonLevels ("Levels (bright, mid, shade)", Vector) = (1, 0.82, 0.64, 0)
        _Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }
        Pass
        {
            Name "ToonForward"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back
            ZWrite On

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                float4 _ToonLightDir;
                float4 _ToonThresholds;
                float4 _ToonLevels;
                half _Cutoff;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
                clip(color.a - _Cutoff);
                float ndl = dot(normalize(input.normalWS), normalize(_ToonLightDir.xyz));
                half level = ndl > _ToonThresholds.x ? _ToonLevels.x : (ndl > _ToonThresholds.y ? _ToonLevels.y : _ToonLevels.z);
                return half4(color.rgb * level, 1);
            }
            ENDHLSL
        }
    }
}
