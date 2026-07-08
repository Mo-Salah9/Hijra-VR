Shader "Vour/TeleportBlink"
{
    Properties
    {
        _Alpha("Alpha", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags { "Queue" = "Overlay+100" "RenderType" = "Transparent" "IgnoreProjector" = "True" }

        ZTest Off
        ZWrite Off
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma multi_compile_instancing
            
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float _Alpha;

            v2f vert(appdata input)
            {
                v2f output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.pos = UnityObjectToClipPos(input.vertex);
                return output;
            }

            float4 frag() : SV_Target
            {
                return float4(0, 0, 0, _Alpha);
            }
            ENDCG
        }
    }

    SubShader
    {
        PackageRequirements 
        {
            "com.unity.render-pipelines.universal"
        }
        
        Tags { "Queue"="Geometry" "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM
            #pragma multi_compile_instancing

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
            float _Alpha;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half4 frag() : SV_Target
            {
                return half4(0, 0, 0, _Alpha);
            }
            ENDHLSL
        }
    }
}