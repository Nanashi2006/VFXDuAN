Shader "VFXDuAN/BlackHole/Disk"
{
    Properties
    {
        [HDR]_InnerColor("Inner Color", Color) = (3.2,0.45,0.08,1)
        [HDR]_OuterColor("Outer Color", Color) = (0.35,0.02,2.4,1)
        _InnerRadius("Inner Radius", Range(0, 0.9)) = 0.24
        _OuterRadius("Outer Radius", Range(0.1, 1.2)) = 0.98
        _SpiralArms("Spiral Arms", Range(1, 14)) = 7
        _Twist("Twist", Range(0, 40)) = 22
        _Speed("Rotation Speed", Range(-4, 4)) = 0.85
        _Sharpness("Spiral Sharpness", Range(0.5, 8)) = 3.2
        _Intensity("Intensity", Range(0, 8)) = 2.4
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }

        Pass
        {
            Name "BlackHoleDisk"
            Tags { "LightMode"="UniversalForward" }

            Blend One One
            Cull Off
            ZWrite Off
            ZTest LEqual

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
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _InnerColor;
                half4 _OuterColor;
                float _InnerRadius;
                float _OuterRadius;
                float _SpiralArms;
                float _Twist;
                float _Speed;
                float _Sharpness;
                float _Intensity;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 p = (input.uv - 0.5) * 2.0;
                float r = length(p);
                float a = atan2(p.y, p.x);
                float t = _Time.y * _Speed;

                float innerMask = smoothstep(_InnerRadius, _InnerRadius + 0.055, r);
                float outerMask = 1.0 - smoothstep(_OuterRadius - 0.10, _OuterRadius, r);
                float ringMask = innerMask * outerMask;

                float spiralA = 0.5 + 0.5 * sin(a * _SpiralArms + r * _Twist - t * 6.0);
                float spiralB = 0.5 + 0.5 * sin(a * (_SpiralArms * 0.53) - r * (_Twist * 1.37) + t * 3.1);
                float turbulence = 0.5 + 0.5 * sin((p.x - p.y) * 34.0 + sin(a * 6.0 + t) * 3.0 + t * 4.0);

                float spiral = saturate(spiralA * 0.72 + spiralB * 0.38);
                spiral = pow(spiral, _Sharpness);

                float middle = (_InnerRadius + _OuterRadius) * 0.5;
                float halfWidth = max((_OuterRadius - _InnerRadius) * 0.58, 0.001);
                float radialGlow = saturate(1.0 - abs(r - middle) / halfWidth);
                radialGlow = pow(radialGlow, 0.7);

                float intensity = ringMask * radialGlow * (spiral * 0.82 + turbulence * 0.18);
                float colorT = saturate((r - _InnerRadius) / max(_OuterRadius - _InnerRadius, 0.001));
                half3 color = lerp(_InnerColor.rgb, _OuterColor.rgb, colorT) * intensity * _Intensity;

                return half4(color, intensity);
            }
            ENDHLSL
        }
    }
}
