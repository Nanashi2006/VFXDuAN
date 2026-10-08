Shader "VFXDuAN/BlackHole/Core"
{
    Properties
    {
        _CoreColor("Core Color", Color) = (0,0,0,1)
        [HDR]_EdgeColor("Edge Color", Color) = (0.35,0.02,1.5,1)
        _FresnelPower("Fresnel Power", Range(0.5, 10)) = 3.0
        _EdgeIntensity("Edge Intensity", Range(0, 8)) = 2.5
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
            "Queue"="Geometry+50"
        }

        Pass
        {
            Name "BlackHoleCore"
            Tags { "LightMode"="UniversalForward" }

            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _CoreColor;
                half4 _EdgeColor;
                float _FresnelPower;
                float _EdgeIntensity;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs pos = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = pos.positionCS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(pos.positionWS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 n = normalize(input.normalWS);
                float3 v = normalize(input.viewDirWS);
                float fresnel = pow(saturate(1.0 - dot(n, v)), _FresnelPower);

                half3 color = _CoreColor.rgb + _EdgeColor.rgb * fresnel * _EdgeIntensity;
                return half4(color, 1.0);
            }
            ENDHLSL
        }
    }
}
