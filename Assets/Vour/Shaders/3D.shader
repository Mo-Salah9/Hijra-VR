Shader "Vour/Flat 3D"
{
    Properties
    {
        [MainTexture] [NoScaleOffset] _MainTex ("Texture", 2D) = "grey" {}
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
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 layout3DScaleAndOffset : TEXCOORD2;
                // With fog since the shader will be used in world space, the 360_3D shader acts like a skybox and therefore doesn't need fog
                UNITY_FOG_COORDS(1) 
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            int _Layout;
            
            uniform int _CustomEyeIndex;

            v2f vert (appdata input)
            {
                v2f output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                UNITY_TRANSFER_FOG(output, output.vertex);
                
                int eyeIndex = _CustomEyeIndex == 0 ? unity_StereoEyeIndex : _CustomEyeIndex;
                
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

            fixed4 frag (v2f input) : SV_Target
            {
                const float2 uv = (input.uv + input.layout3DScaleAndOffset.xy) * input.layout3DScaleAndOffset.zw;
                fixed4 color = tex2D(_MainTex, uv);
                UNITY_APPLY_FOG(input.fogCoord, color);
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
            #pragma multi_compile_fog

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Unlit.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float fogCoord : TEXCOORD1;
                float4 layout3DScaleAndOffset : TEXCOORD2;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            uniform int _CustomEyeIndex;

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            int _Layout;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);

                output.positionCS = vertexInput.positionCS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);

                // With fog since the shader will be used in world space, the 360_3D shader acts like a skybox and therefore doesn't need fog
                #if defined(_FOG_FRAGMENT)
                output.fogCoord = vertexInput.positionVS.z;
                #else
                output.fogCoord = ComputeFogFactor(vertexInput.positionCS.z);
                #endif

                int eyeIndex = _CustomEyeIndex == 0 ? unity_StereoEyeIndex : _CustomEyeIndex;

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

            InputData InitializeInputData()
            {
                InputData inputData = (InputData)0;
                inputData.positionWS = float3(0, 0, 0);
                inputData.normalWS = half3(0, 0, 1);
                inputData.viewDirectionWS = half3(0, 0, 1);
                inputData.shadowCoord = 0;
                inputData.fogCoord = 0;
                inputData.vertexLighting = half3(0, 0, 0);
                inputData.bakedGI = half3(0, 0, 0);
                inputData.normalizedScreenSpaceUV = 0;
                inputData.shadowMask = half4(1, 1, 1, 1);
                return inputData;
            }
            
            SurfaceData InitializeSurfaceData(half4 color)
            {
                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = color.rgb;
                surfaceData.alpha = color.a;
                surfaceData.emission = 0;
                surfaceData.metallic = 0;
                surfaceData.occlusion = 1;
                surfaceData.smoothness = 1;
                surfaceData.specular = 0;
                surfaceData.clearCoatMask = 0;
                surfaceData.clearCoatSmoothness = 1;
                surfaceData.normalTS = half3(0, 0, 1);
                return surfaceData;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                const float2 uv = (input.uv + input.layout3DScaleAndOffset.xy) * input.layout3DScaleAndOffset.zw;
                const half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

                const InputData inputData = InitializeInputData();
                const SurfaceData surfaceData = InitializeSurfaceData(color);

                #if defined(_FOG_FRAGMENT)
                #if (defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2))
                float viewZ = -input.fogCoord;
                float nearToFarZ = max(viewZ - _ProjectionParams.y, 0);
                half fogFactor = ComputeFogFactorZ0ToFar(nearToFarZ);
                #else
                half fogFactor = 0;
                #endif
                #else
                half fogFactor = input.fogCoord;
                #endif

                half4 finalColor = UniversalFragmentUnlit(inputData, surfaceData);
                finalColor.rgb = MixFog(finalColor.rgb, fogFactor);
                return finalColor;
            }
            ENDHLSL
        }
    }
}