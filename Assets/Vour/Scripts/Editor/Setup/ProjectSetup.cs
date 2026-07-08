using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace CrizGames.Vour.Editor
{
    [InitializeOnLoad]
    public class ProjectSetup
    {
        static ProjectSetup()
        {
            EditorApplication.update -= OpenSetupIfNeeded;
            EditorApplication.update += OpenSetupIfNeeded;
            
            CreateTags("PopupPanel");
            CreateSortingLayers(("Overlay", 406017675));

            AddShaderCollectionToPreloadedShaders();

            SetVourWebGLTemplate();
            
            SetActiveInputHandler_NewSystem();
        }

        private static void OpenSetupIfNeeded()
        {
            if (VourPackageManager.IsRebuildingCache || EditorTools.IsEditorInPlayMode())
                return;

            EditorApplication.update -= OpenSetupIfNeeded;

            // Show setup window 
            if (!VourPackageManager.AllPackagesInstalled(SetupWindow.RequiredPackages))
            {
                SetupWindow.ShowWindow();
            }
            // Ask to install FFmpeg again when binaries were deleted through removing the Library folder
            else if (FFmpegInstaller.OnlyBinariesMissing)
            {
                if (EditorUtility.DisplayDialog("Enable Feature: Video Preview",
                        "Video preview is enabled, but some files are missing. Do you want to download them again?",
                        "Yes", "No"))
                {
                    FFmpegInstaller.InstallFFmpeg();
                }
                else
                {
                    FFmpegInstaller.UninstallFFmpeg();
                }
            }
        }

        private static void SetVourWebGLTemplate()
        {
            // If WebGL template is currently the default, change to Vour's template
            if (PlayerSettings.WebGL.template != "APPLICATION:Default")
                return;

            // Set Vour template
            PlayerSettings.WebGL.template = "PROJECT:Vour";

            // Set template value so that it doesn't cause a build error.
            (string name, string value)[] tags =
            {
                ("DESCRIPTION", ""), 
                ("SHOW_START_BUTTON", "true")
            };
            AddEntriesToArrayProperty("ProjectSettings/ProjectSettings.asset", "m_TemplateCustomTags", tags, AddTag);
            return;

            // Add a tag with a default value.
            // PlayerSettings.SetTemplateCustomValue() doesn't work reliably, so it is done manually.
            bool AddTag(SerializedProperty tagsArray, (string name, string value) tag, int numTags)
            {
                // Return if tag already exists
                for (int i = 0; i < numTags; i++) {
                    var existingTag = tagsArray.GetArrayElementAtIndex(i);
                    if (existingTag.FindPropertyRelative("first").stringValue == tag.name) 
                        return false;
                }

                // Add tag
                tagsArray.InsertArrayElementAtIndex(numTags);
                var newTag = tagsArray.GetArrayElementAtIndex(numTags);
                newTag.FindPropertyRelative("first").stringValue = tag.name;
                newTag.FindPropertyRelative("second").stringValue = tag.value;
                return true;
            }
        }
        
        /// <summary>
        /// Add "VourShaderCollection" to the "Preloaded Shaders" array in the graphics settings.
        /// </summary>
        private static void AddShaderCollectionToPreloadedShaders()
        {
            var collectionAssetGuids = AssetDatabase.FindAssets("t:ShaderVariantCollection");
            var collectionAssetPath = collectionAssetGuids
                .Select(AssetDatabase.GUIDToAssetPath)
                .FirstOrDefault(path => Path.GetFileNameWithoutExtension(path) == "VourShaderCollection");
            
            if (string.IsNullOrEmpty(collectionAssetPath))
                return;
            
            var collection = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>(collectionAssetPath);
            
            var settings = new SerializedObject(GraphicsSettings.GetGraphicsSettings());
            var preloadedShaders = settings.FindProperty("m_PreloadedShaders");
            
            // Check if the collection is already in the array
            var hasCollection = false;
            for (int i = 0; i < preloadedShaders.arraySize; ++i)
            {
                var arrayElem = preloadedShaders.GetArrayElementAtIndex(i);
                if (collection == arrayElem.objectReferenceValue)
                {
                    hasCollection = true;
                    break;
                }
            }

            // Add the collection to the array
            if (!hasCollection)
            {
                var arrayIndex = preloadedShaders.arraySize;
                preloadedShaders.InsertArrayElementAtIndex(arrayIndex);
                var arrayElem = preloadedShaders.GetArrayElementAtIndex(arrayIndex);
                arrayElem.objectReferenceValue = collection;

                settings.ApplyModifiedProperties();
            }
        }
        
        private static void CreateTags(params string[] tags) {
            AddEntriesToArrayProperty("ProjectSettings/TagManager.asset", "tags", tags, AddTag);
            return;

            bool AddTag(SerializedProperty tagsArray, string tag, int numTags)
            {
                // Return if tag already exists
                for (int i = 0; i < numTags; i++) {
                    var existingTag = tagsArray.GetArrayElementAtIndex(i);
                    if (existingTag.stringValue == tag) 
                        return false;
                }

                // Add tag
                tagsArray.InsertArrayElementAtIndex(numTags);
                tagsArray.GetArrayElementAtIndex(numTags).stringValue = tag;
                return true;
            }
        }
        
        private static void CreateSortingLayers(params (string name, int id)[] sortingLayers)
        {
            AddEntriesToArrayProperty("ProjectSettings/TagManager.asset", "m_SortingLayers", sortingLayers, AddSortingLayer);
            return;
            
            bool AddSortingLayer(SerializedProperty sortingLayersArray, (string name, int id) sortingLayer, int numSortingLayers)
            {
                // Return if tag already exists
                for (int i = 0; i < numSortingLayers; i++) {
                    var existingSortingLayer = sortingLayersArray.GetArrayElementAtIndex(i).FindPropertyRelative("name");
                    if (existingSortingLayer.stringValue == sortingLayer.name) 
                        return false;
                }

                // Add sorting layer
                sortingLayersArray.InsertArrayElementAtIndex(numSortingLayers);
                var newSortingLayer = sortingLayersArray.GetArrayElementAtIndex(numSortingLayers);
                newSortingLayer.FindPropertyRelative("name").stringValue = sortingLayer.name;
                newSortingLayer.FindPropertyRelative("uniqueID").intValue = sortingLayer.id;
                return true;
            }
        }

        private static void AddEntriesToArrayProperty<T>(string assetPath, string property, IEnumerable<T> newEntries, Func<SerializedProperty, T, int, bool> addAction)
        {
            var propertyArray = GetPropertyOrNull(assetPath, property);
            if (propertyArray == null)
            {
                Debug.LogError($"Cannot find '{property}' in '{assetPath}'");
                return;
            }

            var propertyChanged = false;

            foreach (var entry in newEntries)
            {
                if (addAction(propertyArray, entry, propertyArray.arraySize))
                    propertyChanged = true;
            }

            if (propertyChanged)
            {
                propertyArray.serializedObject.ApplyModifiedProperties();
                propertyArray.serializedObject.Update();
            }
        }

        /// <summary>
        /// Set new input system if the old input manager is currently selected.
        /// </summary>
        // Code derived from EditorPlayerSettingHelper in com.unity.inputsystem
        private static void SetActiveInputHandler_NewSystem()
        {
            //const int oldInputManager = 0;
            const int newInputSystem = 1;
            const int inputBoth = 2;
            const string inputHandlerProperty = "activeInputHandler";
            
            var property = GetPropertyOrNull("ProjectSettings/ProjectSettings.asset", inputHandlerProperty);
            if (property != null)
            {
                var setting = property.intValue;
                if (setting is newInputSystem or inputBoth) // Tolerate "Both" setting
                    return;
                
                property.intValue = newInputSystem;
                property.serializedObject.ApplyModifiedProperties();
            }
            else
            {
                Debug.LogError($"Cannot find '{inputHandlerProperty}' in player settings");
            }
        }
        
        private static SerializedProperty GetPropertyOrNull(string assetPath, string name)
        {
            var asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
            if (asset == null)
                return null;
            var serializedObject = new SerializedObject(asset);
            return serializedObject.FindProperty(name);
        }
    }
}