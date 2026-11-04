// 복도 타일용 스프라이트 셰이더: 그림 × (전역 복도 밝기 색 + 3D 추가 조명들 × 전역 벽 배율). 해(방향 조명)는 쓰지 않아 낮 화면이 조명 없는 스프라이트와 같다.
// 벽은 카메라를 보는 평면이라 법선을 -Z로 고정한다. 스포트 조명 쿠키도 반영한다.
Shader "VoxelMagic/Sprite Lit 3D"
{
    Properties
    {
        [MainTexture] _MainTex ("Sprite", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _FORWARD_PLUS
            #pragma multi_compile_fragment _ _LIGHT_COOKIES

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            // CorridorLighting이 낮/밤에 맞춰 바꾸는 전역 값 (설정하기 전에는 흰색으로 본다)
            half4 _CorridorAmbient;
            // 벽이 추가 조명을 받는 배율 (설정 전·0이면 안 받음)
            half _CorridorLampOnWall;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
                float3 positionWS : TEXCOORD1;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                // Unity 6 스프라이트는 좌우 반전·색을 정점 대신 렌더러 값(unity_SpriteProps·unity_SpriteColor)으로 넘긴다.
                input.positionOS.xyz = UnityFlipSprite(input.positionOS.xyz, unity_SpriteProps.xy);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.uv = input.uv;
                output.color = input.color * unity_SpriteColor;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * input.color;
                half3 ambient = any(_CorridorAmbient.rgb) ? _CorridorAmbient.rgb : half3(1, 1, 1);
                half3 light = 0;

            #if defined(_ADDITIONAL_LIGHTS)
                const half3 normalWS = half3(0, 0, -1);
                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.normalWS = normalWS;
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);

                uint lightCount = GetAdditionalLightsCount();
            #if USE_FORWARD_PLUS
                for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
                {
                    Light directional = GetAdditionalLight(lightIndex, input.positionWS, half4(1, 1, 1, 1));
                    light += directional.color * directional.distanceAttenuation * saturate(dot(normalWS, directional.direction));
                }
            #endif
                LIGHT_LOOP_BEGIN(lightCount)
                    Light added = GetAdditionalLight(lightIndex, input.positionWS, half4(1, 1, 1, 1));
                    light += added.color * added.distanceAttenuation * saturate(dot(normalWS, added.direction));
                LIGHT_LOOP_END
            #endif

                return half4(albedo.rgb * (ambient + light * _CorridorLampOnWall), albedo.a);
            }
            ENDHLSL
        }
    }
}
