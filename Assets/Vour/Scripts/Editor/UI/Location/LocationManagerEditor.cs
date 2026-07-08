using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CrizGames.Vour.Editor
{
    [CustomEditor(typeof(LocationManager))]
    public class LocationManagerEditor : UnityEditor.Editor
    {
        private const string EventsFoldoutPrefKey = "VourLocationManagerEventsFoldout";

        private SerializedProperty startLocation;
        private SerializedProperty onEnter;
        private SerializedProperty onExit;
        
        private bool showEvents;

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnInitializeAndReloadScripts()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            
            EditorSceneManager.sceneOpened -= ActivateStartLocationOnSceneOpen;
            EditorSceneManager.sceneOpened += ActivateStartLocationOnSceneOpen;

            AddActivateLocationEvent();
        }

        private static void ActivateStartLocationOnSceneOpen(Scene scene, OpenSceneMode mode)
        {
            var manager = LocationManager.GetManager();
            if (manager == null)
                return;
            
            manager.DeactivateLocations();
            manager.DeactivateLocationViews();
            
            if (manager.startLocation != null)
                ActivateLocation(manager, manager.startLocation);
        }

        private static void AddActivateLocationEvent()
        {
            Selection.selectionChanged -= ActivateLocationIfChildSelected;
            Selection.selectionChanged += ActivateLocationIfChildSelected;

            // Execute delayed after domain reload so that when a video location was selected, 
            // its video preview will reload
            EditorApplication.delayCall += ActivateLocationIfChildSelected;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange stateChange)
        {
            switch (stateChange)
            {
                case PlayModeStateChange.ExitingEditMode:
                    ExitingEditMode();
                    break;
                case PlayModeStateChange.EnteredEditMode:
                    EnteredEditMode();
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                case PlayModeStateChange.ExitingPlayMode:
                default:
                    break;
            }
        }

        private static void ExitingEditMode()
        {
            var manager = LocationManager.GetManager();
            
            // Disable all locations
            manager.DeactivateLocationViews();
            EditorUtility.SetDirty(manager);
            
            // Before entering play mode, video UI must be disabled at startup.
            manager.videoUI.DisableUI();
            EditorUtility.SetDirty(manager.videoUI);
            
            EditorSceneManager.SaveScene(manager.gameObject.scene);
        }

        private static void EnteredEditMode()
        {
            // Because of ExitingEditMode(), all locations would be disabled, regardless if one is selected or not,
            // so a check for a selected location is needed when entering edit mode again.
            EditorApplication.delayCall += ActivateLocationIfChildSelected;
        }

        /// <summary>
        /// Activate location if it or a child of it is selected
        /// </summary>
        private static void ActivateLocationIfChildSelected()
        {
            if (EditorTools.IsEditorInPlayMode())
                return;

            var manager = LocationManager.GetManager();
            if (manager == null)
                return;

            var selection = Selection.activeTransform;
            
            // Leave last location active if none selected
            if (selection == null)
                return;

            // Don't do anything if there is no selection or object is in prefab view
            if (EditorTools.IsInPrefabView(selection.gameObject))
                return;
            
            // Search for a parent with a location script
            var parentLocation = selection.GetComponentInParent<Location>(true);
            
            // Activate location
            if (parentLocation != null)
                ActivateLocation(manager, parentLocation);
        }

        private static void ActivateLocation(LocationManager manager, Location location)
        {
            LocationEditor.EnableLocation(manager, location);
            location.SetData();

            if (manager.GetLocationView(location) is not VideoLocation videoLocation)
                return;
            
            location.GetVideoPreview(previewTexture =>
            {
                // Cancel if the location was destroyed while the preview texture was fetched
                if (location == null)
                    return;
                
                if (videoLocation != null && location.isActiveAndEnabled)
                    videoLocation.SetTexture(previewTexture);
            });
        }

        public void OnEnable()
        {
            startLocation = serializedObject.FindProperty(nameof(LocationManager.startLocation));
            onEnter = serializedObject.FindProperty(nameof(LocationManager.onLocationEnter));
            onExit = serializedObject.FindProperty(nameof(LocationManager.onLocationExit));
            
            showEvents = EditorPrefs.GetBool(EventsFoldoutPrefKey, false);
        }

        public override void OnInspectorGUI()
        {
            var manager = (LocationManager)target;

            // If prefab view or something else
            var locationManagerObj = manager.gameObject;
            if (EditorTools.IsInPrefabView(locationManagerObj))
            {
                DrawDefaultInspector();
                return;
            }

            // Start Location
            EditorGUILayout.PropertyField(startLocation, new GUIContent("Start Location"));

            // HelpBox if no start location assigned
            if (manager.startLocation == null)
                EditorGUILayout.HelpBox("Start Location is not assigned.", MessageType.Error);
            
            EditorGUILayout.Space();

            // Events Foldout
            var newShowEvents = EditorGUILayout.Foldout(showEvents, "Events");
            if (newShowEvents != showEvents)
            {
                EditorPrefs.SetBool(EventsFoldoutPrefKey, newShowEvents);
                showEvents = newShowEvents;
            }
            
            // Show Events
            if (showEvents)
            {
                EditorGUILayout.PropertyField(onEnter);
                EditorGUILayout.PropertyField(onExit);
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}