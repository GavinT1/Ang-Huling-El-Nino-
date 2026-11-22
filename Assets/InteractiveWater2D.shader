Shader "Custom/InteractiveWater2D"
{
    Properties
    {
        _TopColor ("Surface Color", Color) = (0.35, 0.88, 0.96, 0.78)
        _BottomColor ("Deep Color", Color) = (0.08, 0.42, 0.58, 0.92)
        _FoamColor ("Surface Foam Line Color", Color) = (0.85, 0.98, 1.0, 0.95)
        _FoamThickness ("Foam Line Thickness", Range(0.005, 0.15)) = 0.035
        _ShimmerSpeed ("Shimmer Speed", Float) = 1.6
        _ShimmerScale ("Shimmer Scale", Float) = 14.0
        _ShimmerStrength ("Shimmer Strength", Range(0, 0.4)) = 0.14
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _TopColor;
                float4 _BottomColor;
                float4 _FoamColor;
                float _FoamThickness;
                float _ShimmerSpeed;
                float _ShimmerScale;
                float _ShimmerStrength;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Vertical gradient from deep water (uv.y = 0) to surface (uv.y = 1)
                half4 col = lerp(_BottomColor, _TopColor, pow(IN.uv.y, 1.4));

                // Subtle moving caustic / specular shimmer near the upper half
                float wave1 = sin(IN.worldPos.x * _ShimmerScale + _Time.y * _ShimmerSpeed);
                float wave2 = cos(IN.worldPos.x * (_ShimmerScale * 0.65) - _Time.y * (_ShimmerSpeed * 1.3) + IN.uv.y * 6.0);
                float shimmer = saturate((wave1 * wave2) * 0.5 + 0.5);
                col.rgb += shimmer * _ShimmerStrength * IN.uv.y;

                // Crisp glossy foam highlight right along the animated wave top edge
                float foamMask = smoothstep(1.0 - _FoamThickness, 1.0 - (_FoamThickness * 0.25), IN.uv.y);
                col = lerp(col, _FoamColor, foamMask);

                return col;
            }
            ENDHLSL
        }
    }
}