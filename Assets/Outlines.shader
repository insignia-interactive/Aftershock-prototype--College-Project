Shader "Unlit/BackFaceOutlines"
{
    Properties{
        _Thickness("Thickness", Float) = 1 //the amount to extrude the outline mesh
        _Color("Color", Color) = (1, 1, 1, 1) //the outline colour

    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "Outlines"
            Cull Front

            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x

            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "BackFaceOutlines.hlsl"

            ENDHLSL
        }
    }
}
