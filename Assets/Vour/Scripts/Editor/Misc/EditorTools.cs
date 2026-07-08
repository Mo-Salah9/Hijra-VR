using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using UnityEditor.SceneManagement;
#if VOUR_XRPLUGINMANAGEMENT
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine.XR.Management;
#endif

namespace CrizGames.Vour.Editor
{
    public static class EditorTools
    {
        /// <summary>
        /// Opaque gray texture. <see cref="Texture2D.grayTexture"/> is also gray in the alpha channel. 
        /// </summary>
        public static Texture2D GrayTextureOpaque
        {
            get
            {
                if (_grayTexture != null)
                    return _grayTexture;
                
                _grayTexture = new Texture2D(1, 1, TextureFormat.RGB24, false);
                _grayTexture.SetPixel(0, 0, new Color(0.5f, 0.5f, 0.5f, 1));
                _grayTexture.Apply();
                return _grayTexture;
            }
        }
        
        private static Texture2D _grayTexture;
        
        public static bool IsEditorInPlayMode()
        {
            return EditorApplication.isPlayingOrWillChangePlaymode ||
                   EditorApplication.isPlaying ||
                   EditorApplication.isPaused;
        }
        
        public static bool IsInPrefabView(GameObject obj)
        {
            return !obj.scene.IsValid() || PrefabStageUtility.GetCurrentPrefabStage() != null;
        }

        public static void AddTooltip(this SerializedProperty property)
        {
            GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", property.tooltip));
        }
        
#if VOUR_XRPLUGINMANAGEMENT
        // Recreated from XRGeneralSettingsPerBuildTarget.GetOrCreate because it is internal
        public static XRGeneralSettingsPerBuildTarget GetOrCreateXRSettingsForBuildTarget()
        {
            EditorBuildSettings.TryGetConfigObject<XRGeneralSettingsPerBuildTarget>(XRGeneralSettings.k_SettingsKey, out var generalSettings);
            if (generalSettings == null)
            {
                string searchText = "t:XRGeneralSettings";
                string[] assets = AssetDatabase.FindAssets(searchText);
                if (assets.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(assets[0]);
                    generalSettings = AssetDatabase.LoadAssetAtPath(path, typeof(XRGeneralSettingsPerBuildTarget)) as XRGeneralSettingsPerBuildTarget;
                }
            }

            if (generalSettings == null)
            {
                generalSettings = ScriptableObject.CreateInstance(typeof(XRGeneralSettingsPerBuildTarget)) as XRGeneralSettingsPerBuildTarget;
                string assetPath = GetAssetPathForComponents(new[]{"XR"});
                if (!string.IsNullOrEmpty(assetPath))
                {
                    assetPath = Path.Combine(assetPath, "XRGeneralSettings.asset");
                    AssetDatabase.CreateAsset(generalSettings, assetPath);
                }
            }

            EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, generalSettings, true);

            return generalSettings;
        }
        
        // Recreated from XRManagement EditorUtilities.GetAssetPathForComponents because it is internal
        private static string GetAssetPathForComponents(string[] pathComponents, string root = "Assets")
        {
            if (pathComponents.Length <= 0)
                return null;

            string path = root;
            foreach( var pc in pathComponents)
            {
                string subFolder = Path.Combine(path, pc);
                bool shouldCreate = true;
                foreach (var f in AssetDatabase.GetSubFolders(path))
                {
                    if (String.Compare(Path.GetFullPath(f), Path.GetFullPath(subFolder), StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        shouldCreate = false;
                        break;
                    }
                }

                if (shouldCreate)
                    AssetDatabase.CreateFolder(path, pc);
                path = subFolder;
            }

            return path;
        }
#endif

        public static bool IsXRLoaderEnabled(string loaderName, BuildTargetGroup group) =>
#if VOUR_XRPLUGINMANAGEMENT
            XRPackageMetadataStore.IsLoaderAssigned(loaderName, group);
#else
            false;
#endif

        public static bool SetXRLoader(string loaderName, BuildTargetGroup group, bool enabled)
        {
#if VOUR_XRPLUGINMANAGEMENT
            var target = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(group);

            if (target == null)
                return false;
            
            if (target.AssignedSettings == null)
            {
                var instance = ScriptableObject.CreateInstance<XRManagerSettings>();
                target.AssignedSettings = instance;
                EditorUtility.SetDirty(target);
            }

            bool loaderEnabled = IsXRLoaderEnabled(loaderName, group);

            if (!loaderEnabled && enabled)
            {
                if (!XRPackageMetadataStore.AssignLoader(target.AssignedSettings, loaderName, group))
                {
                    Debug.LogError($"Unable to assign loader {loaderName} for build target {group}. " +
                                   $"Please try to enable it manually by checking the loader in Project Settings > XR Plug-in Management > {group} > Plug-in Providers.");
                }
            }
            else if(loaderEnabled && !enabled)
            {
                if (!XRPackageMetadataStore.RemoveLoader(target.AssignedSettings, loaderName, group))
                {
                    Debug.LogError($"Unable to remove loader {loaderName} for build target {group}. " +
                                   $"Please try to enable it manually by unchecking the loader in Project Settings > XR Plug-in Management > {group} > Plug-in Providers.");
                }
            }

            return true;
#else
            return false;
#endif
        }
    }
}