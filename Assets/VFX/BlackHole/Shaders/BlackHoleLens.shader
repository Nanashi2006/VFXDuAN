Shader "VFXDuAN/BlackHole/Lens"
{
    Properties
    {
        _Distortion("Distortion", Range(0, 0.2)) = 0.055
        _LensRadius("Lens Radius", Range(0.05, 1.0)) = 0.48
        _Falloff("Falloff", Range(0.5, 8.0)) = 3.2
        _Darkness("Center Darkness", Range(0, 1)) = 0.55
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "Queue"="Transparent+50"
        }

        Pass
        {
            Name "BlackHoleLens"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float _Distortion;
                float _LensRadius;
                float _Falloff;
                float _Darkness;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                o.uv = input.uv;
                o.screenPos = ComputeScreenPos(o.positionCS);
                return o;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 local = (input.uv - 0.5) * 2.0;
                float r = length(local);
                float inside = 1.0 - smoothstep(_LensRadius, 1.0, r);
                float edge = pow(saturate(1.0 - r), _Falloff);

                float2 screenUV = input.screenPos.xy / max(input.screenPos.w, 0.0001);
                float2 dir = normalize(local + 1e-5);
                float strength = _Distortion * inside * (0.25 + edge * 1.75);
                float2 warpedUV = screenUV - dir * strength;

                half3 background = SampleSceneColor(warpedUV);
                float darkMask = saturate(1.0 - r / max(_LensRadius, 0.001));
                background *= 1.0 - darkMask * _Darkness;

                return half4(background, inside);
            }
            ENDHLSL
        }
    }
}
