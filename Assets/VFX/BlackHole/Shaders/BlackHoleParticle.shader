Shader "VFXDuAN/BlackHole/Particle"
{
    Properties
    {
        [HDR]_Tint("Tint", Color) = (1.6,0.25,0.05,1)
        _Softness("Softness", Range(0.1, 8)) = 2.5
        _Intensity("Intensity", Range(0, 8)) = 2.0
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
            Name "BlackHoleParticle"
            Tags { "LightMode"="UniversalForward" }

            Blend One One
            Cull Off
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _Tint;
                float _Softness;
                float _Intensity;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                o.uv = input.uv;
                o.color = input.color;
                return o;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 p = input.uv * 2.0 - 1.0;
                float r = length(p);
                float mask = pow(saturate(1.0 - r), _Softness);
                half3 col = _Tint.rgb * input.color.rgb * mask * _Intensity;
                return half4(col, mask * input.color.a);
            }
            ENDHLSL
        }
    }
}
