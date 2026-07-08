#if VOUR_URP
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_6000_0_OR_NEWER
using UnityEngine.Rendering.RenderGraphModule;
#endif
using UnityEngine.Rendering.Universal;

namespace CrizGames.Vour
{
    /// <summary>
    /// Sets the custom stereo eye index for Vour's 3D shaders
    /// because WebGL for some reason doesn't support the "unity_StereoEyeIndex" property in shaders.
    /// </summary>
    public class WebXRStereoEyeRendererFeature : ScriptableRendererFeature
    {
        private class WebXRStereoEyeRenderPass : ScriptableRenderPass
        {
            private static readonly int CustomEyeIndexProperty = Shader.PropertyToID("_CustomEyeIndex");

#if UNITY_6000_0_OR_NEWER
            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                // Set custom stereo eye index for 3D shaders
                Shader.SetGlobalInt(CustomEyeIndexProperty, frameData.Get<UniversalCameraData>().xr.multipassId);
            }
#else
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                // Set custom stereo eye index for 3D shaders
                Shader.SetGlobalInt(CustomEyeIndexProperty, renderingData.cameraData.xr.multipassId);
            }
#endif
        }

        private WebXRStereoEyeRenderPass _renderPass;

        public override void Create()
        {
            _renderPass = new WebXRStereoEyeRenderPass
            {
                renderPassEvent = RenderPassEvent.BeforeRendering
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
#if (VOUR_WEBXR && UNITY_WEBGL) || UNITY_EDITOR
            renderer.EnqueuePass(_renderPass);
#endif
        }
    }
}
#endif