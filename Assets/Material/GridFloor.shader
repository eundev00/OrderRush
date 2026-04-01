Shader "OrderRush/GridFloor"
{
    Properties
    {
        _CellSize  ("Cell Size",  Float) = 1.0
        _LineWidth ("Line Width", Float) = 0.03
        _LineColor ("Line Color", Color) = (1,1,1,1)
        _BaseColor ("Base Color", Color) = (0.15,0.15,0.15,1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Name "GridFloor"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float  _CellSize;
            float  _LineWidth;
            float4 _LineColor;
            float4 _BaseColor;

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float2 grid  = frac(float2(i.positionWS.x, i.positionWS.z) / _CellSize + 0.5);
                float  lineX = step(1.0 - _LineWidth, grid.x);
                float  lineY = step(1.0 - _LineWidth, grid.y);
                float  isLine = saturate(lineX + lineY);
                return lerp(_BaseColor, _LineColor, isLine);
            }
            ENDHLSL
        }
    }
}