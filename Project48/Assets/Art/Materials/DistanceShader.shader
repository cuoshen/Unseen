Shader "Custom/DistanceShader"
{
    Properties
    {
        _Origin("Origin", Vector) = (0,0,0)
        _BaseColor("Base Color", color) = (1,1,1,1)
        _Grey("Grey", color) = (0.5,0.5,0.5,1)
        _DistanceMultiplier("Distance Multiplier", Float) = 0.01
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalRenderPipeline"}
        LOD 100

        Pass
        {
            Name "Distant2Origin"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl" 
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 vertex : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION; 
                float4 worldPos: TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float4 _Grey;
            float4 _Origin;
            float _DistanceMultiplier;
            CBUFFER_END

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.vertex.xyz);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float distanceToOrigin = distance(_Origin.xyz, i.worldPos.xyz);
                float4 distColor = lerp(_BaseColor, _Grey, saturate(distanceToOrigin * _DistanceMultiplier));
                distColor.a = 0.5;
                return distColor;
            }
            ENDHLSL
        }
    }
}
