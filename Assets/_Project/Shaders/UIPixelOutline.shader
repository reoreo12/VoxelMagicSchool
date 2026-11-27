// UI RawImage용 도트 외곽선: 투명한 픽셀 중 _OutlineWidth 텍셀(가로·세로 거리 합, 마름모) 안에 그림이 있으면 외곽선 색으로 칠한다.
// 획득 창의 회전 모델 그림(작은 렌더 텍스처)에 아이템 아이콘과 같은 진갈색 1px 선을 두른다. 두께는 테마 값(1, 2, …).
Shader "VoxelMagic/UI/PixelOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _OutlineColor ("Outline Color", Color) = (0.1686, 0.1137, 0.1333, 1)
        _OutlineWidth ("Outline Width (texels)", Range(0, 4)) = 1
        _AlphaThreshold ("Alpha Threshold", Range(0, 1)) = 0.5

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane" "CanUseSpriteAtlas" = "True" }
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; fixed4 color : COLOR; float2 uv : TEXCOORD0; };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _OutlineColor;
            float _OutlineWidth;
            float _AlphaThreshold;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 텍셀 가운데에서 읽어 도트가 번지지 않게 한다.
                float2 texel = _MainTex_TexelSize.xy;
                float2 center = (floor(i.uv / texel) + 0.5) * texel;
                fixed4 c = tex2D(_MainTex, center);
                if (c.a >= _AlphaThreshold) return fixed4(c.rgb, 1) * i.color;

                int width = (int)round(_OutlineWidth);
                [loop] for (int y = -4; y <= 4; y++)
                {
                    [loop] for (int x = -4; x <= 4; x++)
                    {
                        if (abs(x) + abs(y) > width || (x == 0 && y == 0)) continue;
                        if (tex2D(_MainTex, center + float2(x, y) * texel).a >= _AlphaThreshold)
                            return fixed4(_OutlineColor.rgb, _OutlineColor.a * i.color.a);
                    }
                }
                return fixed4(0, 0, 0, 0);
            }
            ENDCG
        }
    }
}
