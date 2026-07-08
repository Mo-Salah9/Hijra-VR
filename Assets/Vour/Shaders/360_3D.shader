Shader "Vour/360 3D"
{
    Properties
    {
        [MainTexture] [NoScaleOffset] _MainTex ("Texture", 2D) = "grey" {}
        [Enum(360 Degrees, 0, 180 Degrees, 1)] _ImageType("Image Type", Float) = 0
        [Enum(None, 0, Side by Side, 1, Over Under, 2)] _Layout("3D Layout", Float) = 0
    }
    
    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }

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
                float4 vertex : SV_POSITION;
                float3 uv : TEXCOORD0;
                float2 image180ScaleAndCutoff : TEXCOORD1;
                float4 layout3DScaleAndOffset : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            int _ImageType;
            int _Layout;
            
            uniform int _CustomEyeIndex;

            v2f vert (appdata input)
            {
                v2f output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = input.vertex.xyz;

                int eyeIndex = _CustomEyeIndex == 0 ? unity_StereoEyeIndex : _CustomEyeIndex;
                
                // Set horizontal scale and cutoff for 180 (vs 360) image type
                // 360 degree
                if (_ImageType == 0)
                    output.image180ScaleAndCutoff = float2(1.0, 1.0);
                // 180 degree
                else
                    output.image180ScaleAndCutoff = float2(2.0, 0.5);
                
                // Set scale and offset for 3D layouts
                // No 3D layout
                if (_Layout == 0)
                    output.layout3DScaleAndOffset = float4(0, 0, 1, 1);
                // Side-by-Side 3D layout
                else if (_Layout == 1)
                    output.layout3DScaleAndOffset = float4(eyeIndex, 0, 0.5, 1);
                // Over-Under 3D layout
                else
                    output.layout3DScaleAndOffset = float4(0, 1 - eyeIndex, 1, 0.5);
                
                return output;
            }

            inline float2 ToRadialCoords(float3 coords)
            {
                float3 normalizedCoords = normalize(coords);
                float latitude = acos(normalizedCoords.y);
                float longitude = atan2(normalizedCoords.z, normalizedCoords.x);
                const float2 sphereCoords = float2(longitude, latitude) * float2(0.5 / UNITY_PI, 1.0 / UNITY_PI);
                return float2(0.5, 1.0) - sphereCoords;
            }

            fixed4 frag (v2f input) : SV_Target
            {
                float2 uv = ToRadialCoords(input.uv);
                if (uv.x > input.image180ScaleAndCutoff[1])
                    return fixed4(0, 0, 0, 1);
                uv.x = fmod(uv.x * input.image180ScaleAndCutoff[0], 1);
                uv = (uv + input.layout3DScaleAndOffset.xy) * input.layout3DScaleAndOffset.zw;
                
                fixed4 color = tex2D(_MainTex, uv);
                return color;
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
                float3 uv : TEXCOORD0;
                float2 image180ScaleAndCutoff : TEXCOORD1;
                float4 layout3DScaleAndOffset : TEXCOORD2;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            uniform int _CustomEyeIndex;

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            int _ImageType;
            int _Layout;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.positionOS.xyz;

                int eyeIndex = _CustomEyeIndex == 0 ? unity_StereoEyeIndex : _CustomEyeIndex;
                
                // Set horizontal scale and cutoff for 180 (vs 360) image type
                // 360 degree
                if (_ImageType == 0)
                    output.image180ScaleAndCutoff = float2(1.0, 1.0);
                // 180 degree
                else
                    output.image180ScaleAndCutoff = float2(2.0, 0.5);

                // Set scale and offset for 3D layouts
                // No 3D layout
                if (_Layout == 0)
                    output.layout3DScaleAndOffset = float4(0, 0, 1, 1);
                // Side-by-Side 3D layout
                else if (_Layout == 1)
                    output.layout3DScaleAndOffset = float4(eyeIndex, 0, 0.5, 1);
                // Over-Under 3D layout
                else
                    output.layout3DScaleAndOffset = float4(0, 1 - eyeIndex, 1, 0.5);

                return output;
            }

            inline float2 ToRadialCoords(float3 coords)
            {
                float3 normalizedCoords = normalize(coords);
                float latitude = acos(normalizedCoords.y);
                float longitude = atan2(normalizedCoords.z, normalizedCoords.x);
                const float2 sphereCoords = float2(longitude, latitude) * float2(0.5 / PI, 1.0 / PI);
                return float2(0.5, 1.0) - sphereCoords;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float2 uv = ToRadialCoords(input.uv);
                if (uv.x > input.image180ScaleAndCutoff[1])
                    return half4(0, 0, 0, 1);
                uv.x = fmod(uv.x * input.image180ScaleAndCutoff[0], 1);
                uv = (uv + input.layout3DScaleAndOffset.xy) * input.layout3DScaleAndOffset.zw;
                
                const half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                return color;
            }
            ENDHLSL
        }
    }
}
