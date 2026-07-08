using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

// From XR Interaction Toolkit Sample Assets

namespace CrizGames.Vour.Sample
{
#if UNITY_EDITOR
    [InitializeOnLoad]
    static class RenderPipelineValidation
    {
        static RenderPipelineValidation()
        {
            foreach (var pipelineHandler in GetAllInstances())
                pipelineHandler.AutoRefreshPipelineShaders();
        }

        static List<MaterialPipelineHandler> GetAllInstances()
        {
            var instances = new List<MaterialPipelineHandler>();

            // Find all GUIDs for objects that match the type MaterialPipelineHandler
            var guids = AssetDatabase.FindAssets("t:MaterialPipelineHandler");
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var asset = AssetDatabase.LoadAssetAtPath<MaterialPipelineHandler>(path);
                if (asset != null)
                    instances.Add(asset);
            }

            return instances;
        }
    }
#endif

    /// <summary>
    /// Scriptable object that allows for setting the shader on a material based on the current render pipeline.
    /// Will run automatically OnEnable in the editor to set the shaders on project bootup. Can be refreshed manually with editor button.
    /// This exists because while objects render correctly using shadergraph shaders, others do not and using the standard shader resolves various rendering issues.
    /// </summary>
    //[CreateAssetMenu(fileName = "MaterialPipelineHandler", menuName = "Vour/MaterialPipelineHandler", order = 0)]
    public class MaterialPipelineHandler : ScriptableObject
    {
        [SerializeField] private List<Material> materials;

        [Tooltip("If true, the shaders will be refreshed automatically when the editor opens and when this scriptable object instance is enabled.")]
        [SerializeField] private bool autoRefreshShaders = true;

#if UNITY_EDITOR
        void OnEnable()
        {
            if (Application.isPlaying)
                return;
            AutoRefreshPipelineShaders();
        }
#endif

        public void AutoRefreshPipelineShaders()
        {
            if (autoRefreshShaders)
                SetPipelineShaders();
        }

        /// <summary>
        /// Applies the appropriate shader to the materials based on the current render pipeline.
        /// </summary>
        public void SetPipelineShaders()
        {
            if (materials == null)
                return;

            var isBuiltinRenderPipeline = GraphicsSettings.currentRenderPipeline == null;

            foreach (var material in materials)
            {
                if (material == null)
                    continue;

                // Find the appropriate shaders based on the toggle
                var birpShader = Shader.Find("Standard");
                var srpShader = Shader.Find("Universal Render Pipeline/Lit");

                // Determine current shader for comparison
                var currentShader = material.shader;

                // Update shader for the current render pipeline only if necessary
                if (isBuiltinRenderPipeline && birpShader != null && currentShader != birpShader)
                {
                    material.shader = birpShader;
                    MarkMaterialModified(material);
                }
                else if (!isBuiltinRenderPipeline && srpShader != null && currentShader != srpShader)
                {
                    material.shader = srpShader;
                    MarkMaterialModified(material);
                }
            }
        }

        static void MarkMaterialModified(Material material)
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(material);
#endif
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Custom editor MaterialPipelineHandler
    /// </summary>
    [CustomEditor(typeof(MaterialPipelineHandler)), CanEditMultipleObjects]
    public class MaterialPipelineHandlerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // Draw the "Refresh Shaders" button
            if (GUILayout.Button("Refresh Shaders"))
            {
                foreach (var t in targets)
                {
                    var handler = (MaterialPipelineHandler)t;
                    handler.SetPipelineShaders();
                }
            }
        }
    }
#endif
}
