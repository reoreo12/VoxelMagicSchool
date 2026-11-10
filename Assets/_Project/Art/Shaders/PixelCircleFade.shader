// 씬 이동용 픽셀 계단 원형 페이드 (UI). 화면을 블록 단위로 잘라 원 밖을 칠하고, 테두리는 4×4 Bayer 디더로 점점 가린다.
// _Coverage 0 = 다 보임, 1 = 다 가림. _Center = 원 중심(뷰포트 0~1). _BlockSize는 1080p 기준 px (해상도에 맞춰 늘고 줆).
Shader "VoxelMagic/UI/PixelCircleFade"
{
    Properties
    {
        _Color ("Color", Color) = (0, 0, 0, 1)
        _Coverage ("Coverage", Range(0, 1)) = 0
        _Center ("Center (viewport)", Vector) = (0.5, 0.5, 0, 0)
        _BlockSize ("Block Size (1080p px)", Float) = 12
        _DitherBlocks ("Dither Width (blocks)", Float) = 3
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane" }
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; float4 screen : TEXCOORD0; };

            fixed4 _Color;
            float _Coverage;
            float4 _Center;
            float _BlockSize;
            float _DitherBlocks;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.screen = ComputeScreenPos(o.pos);
                return o;
            }

            float Bayer4 (float2 cell)
            {
                static const float m[16] = { 0, 8, 2, 10, 12, 4, 14, 6, 3, 11, 1, 9, 15, 7, 13, 5 };
                int x = (int)fmod(abs(cell.x), 4.0);
                int y = (int)fmod(abs(cell.y), 4.0);
                return (m[y * 4 + x] + 0.5) / 16.0;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 screenSize = _ScreenParams.xy;
                float2 px = i.screen.xy / i.screen.w * screenSize;
                float block = max(1.0, floor(_BlockSize * screenSize.y / 1080.0 + 0.5));
                float2 cell = floor(px / block);
                float2 cellCenter = (cell + 0.5) * block;

                float2 center = _Center.xy * screenSize;
                float2 farthest = max(center, screenSize - center);
                float maxRadius = length(farthest) + block;
                float dither = max(_DitherBlocks, 0.001) * block;

                // coverage 0: 원이 화면보다 커서 다 보임 / 1: 원 반지름 < -디더 폭이라 다 가림
                float radius = lerp(maxRadius, -dither, _Coverage);
                float t = saturate((length(cellCenter - center) - radius) / dither);
                float alpha = t > Bayer4(cell) ? 1.0 : 0.0;
                return fixed4(_Color.rgb, alpha * _Color.a);
            }
            ENDCG
        }
    }
}
