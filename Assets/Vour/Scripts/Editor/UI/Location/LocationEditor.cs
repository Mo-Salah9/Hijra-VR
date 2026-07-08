using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CrizGames.Vour.Editor
{
    [CustomEditor(typeof(Location))]
    public class LocationEditor : UnityEditor.Editor
    {
        private const string EventsFoldoutPrefKey = "VourLocationEventsFoldout";
        
        private static string[] _mediaTypes = {"2D", "3D", "180", "180 3D", "360", "360 3D"};
        private static GUIContent[] _mediaTypesContent = _mediaTypes.Select(x => new GUIContent(x)).ToArray();

        private static Location currentLocation;
        
        private SerializedProperty locationType;
        private SerializedProperty mediaType;
        private SerializedProperty texture;
        private SerializedProperty video;
        private SerializedProperty videoURL;
        private SerializedProperty streamingAssetsVidPath;
        private SerializedProperty loadingTextOverride;
        private SerializedProperty videoLocationType;
        private SerializedProperty scaleToFullscreen;
        private SerializedProperty lockCamera;
        private SerializedProperty loopVideo;
        private SerializedProperty videoVolume;
        private SerializedProperty videoUI;
        private SerializedProperty videoUIAudioVolume;
        private SerializedProperty videoUILoopButton;
        private SerializedProperty scene;
        private SerializedProperty _3DLayout;
        private SerializedProperty rotOffset;
        private SerializedProperty onEnter;
        private SerializedProperty onExit;
        
        private bool inPlayMode;

        private bool showEvents;
        
        private void OnEnable()
        {
            locationType = serializedObject.FindProperty(nameof(Location.locationType));
            mediaType = serializedObject.FindProperty(nameof(Location.displayType));
            texture = serializedObject.FindProperty(nameof(Location.texture));
            video = serializedObject.FindProperty(nameof(Location.video));
            videoURL = serializedObject.FindProperty(nameof(Location.videoURL));
            streamingAssetsVidPath = serializedObject.FindProperty(nameof(Location.streamingAssetsVidPath));
            loadingTextOverride = serializedObject.FindProperty(nameof(Location.loadingTextOverride));
            videoLocationType = serializedObject.FindProperty(nameof(Location.videoLocationType));
            scaleToFullscreen = serializedObject.FindProperty(nameof(Location.scaleToFullscreen));
            lockCamera = serializedObject.FindProperty(nameof(Location.lockCamera));
            loopVideo = serializedObject.FindProperty(nameof(Location.loopVideo));
            videoVolume = serializedObject.FindProperty(nameof(Location.videoVolume));
            videoUI = serializedObject.FindProperty(nameof(Location.videoUI));
            videoUIAudioVolume = serializedObject.FindProperty(nameof(Location.videoUIAudioVolume));
            videoUILoopButton = serializedObject.FindProperty(nameof(Location.videoUILoopButton));
            scene = serializedObject.FindProperty(nameof(Location.scene));
            _3DLayout = serializedObject.FindProperty(nameof(Location.layout3D));
            rotOffset = serializedObject.FindProperty(nameof(Location.rotOffset));
            onEnter = serializedObject.FindProperty(nameof(Location.onEnter));
            onExit = serializedObject.FindProperty(nameof(Location.onExit));
            
            inPlayMode = EditorTools.IsEditorInPlayMode();

            showEvents = EditorPrefs.GetBool(EventsFoldoutPrefKey, false);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var location = (Location)target;
            var manager = LocationManager.GetManager();

            if (manager == null)
            {
                EditorGUILayout.HelpBox("There is no Location Manager! You need to have one in your scene!", MessageType.Error);
                if (GUILayout.Button("Add Location Manager to Scene"))
                    MenuEntries.AddLocationManager();
                return;
            }

            EditorGUI.BeginChangeCheck();
            
            DrawLocationProperties(location);
            
            serializedObject.ApplyModifiedProperties();

            var propertiesChanged = EditorGUI.EndChangeCheck();
            
            // Display media in scene
            if (propertiesChanged)
            {
                // Only display changes in play mode when the selected location is also the active one
                if (inPlayMode && !location.gameObject.activeSelf)
                    return;
                
                EnableLocation(manager, location);
                location.SetData();
                
                if (!inPlayMode)
                    GetVideoPreview(manager, location);
                
                // If location.lockCamera could have changed in play mode and is enabled,
                // reset player rotation and update DesktopPlayer.canMoveCam via OnNewLocation()
                if (inPlayMode && !location.displayType.Is360() && PlayerBase.Instance is DesktopPlayer player)
                {
                    // See DesktopPlayer.OnNewLocation()
                    player.canMoveCam = !location.lockCamera || location.displayType.Is360();
                    
                    // Reset rotation in case lockCamera was enabled
                    if (location.lockCamera)
                        player.ResetRotation();
                }
            }

            // Don't show add buttons in play mode
            if (inPlayMode)
                return;
            
            // Add teleport point & info point buttons
            if (location.locationType != LocationType.Scene)
            {
                void InstantiateObj(GameObject obj)
                {
                    Selection.activeGameObject = (GameObject)PrefabUtility.InstantiatePrefab(obj, location.transform);
                }

                EditorGUILayout.Space();

                // Teleport point button
                if (GUILayout.Button("Add Teleport Point"))
                    InstantiateObj(VourSettings.Instance.defaultTeleportPoint);

                // Place buttons next to each other
                GUILayout.BeginHorizontal();

                // Info point button
                if (GUILayout.Button("Add Info Point"))
                    InstantiateObj(VourSettings.Instance.defaultInfoPoint);

                // Info panel button
                if (GUILayout.Button("Add Info Panel"))
                    InstantiateObj(VourSettings.Instance.defaultInfoPanel);

                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();

                // Video point button
                if (GUILayout.Button("Add Video Point"))
                    InstantiateObj(VourSettings.Instance.defaultVideoPoint);

                // Video panel button
                if (GUILayout.Button("Add Video Panel"))
                    InstantiateObj(VourSettings.Instance.defaultVideoPanel);

                GUILayout.EndHorizontal();
            }
        }

        private void GetVideoPreview(LocationManager manager, Location location)
        {
            if (manager.GetLocationView(location) is not VideoLocation videoLocation)
                return;
            
            location.GetVideoPreview(previewTexture =>
            {
                if (currentLocation != location)
                    return;
                
                videoLocation.SetTexture(previewTexture);
            });
        }

        private void DisplayTypeProperty(string strLabel)
        {
            var rect = EditorGUILayout.GetControlRect(true);
            var label = EditorGUI.BeginProperty(rect, new GUIContent(strLabel), mediaType);
            
            EditorGUI.BeginChangeCheck();
            var newIdx = EditorGUI.Popup(rect, label, mediaType.enumValueIndex, _mediaTypesContent);
            if (EditorGUI.EndChangeCheck())
                mediaType.enumValueIndex = newIdx;
            
            EditorGUI.EndProperty();
        }

        private void DrawLocationProperties(Location loc)
        {
            // Location Type
            EditorGUI.BeginDisabledGroup(inPlayMode);
            EditorGUILayout.PropertyField(locationType, new GUIContent("Location Type"));
            EditorGUI.EndDisabledGroup();
            
            // 3D Only
            bool show3DLayout = false;
            switch (loc.displayType)
            {
                case LocationDisplayType._3D:
                case LocationDisplayType._180_3D:
                case LocationDisplayType._3603D:
                    show3DLayout = true;
                    break;
            }

            // IMAGE or VIDEO
            if (loc.locationType.IsImage() || loc.locationType.IsVideo())
            {
                DisplayTypeProperty("Display Type");
                
                if (show3DLayout)
                    EditorGUILayout.PropertyField(_3DLayout, new GUIContent("3D Layout"));
            }

            // IMAGE
            if (loc.locationType.IsImage())
            {
                EditorGUILayout.PropertyField(texture, new GUIContent("Texture"));
            }
            // VIDEO
            else if (loc.locationType.IsVideo())
            {
                EditorGUI.BeginDisabledGroup(inPlayMode);
                
                EditorGUILayout.PropertyField(videoLocationType, new GUIContent("Video Type"));

                switch (loc.videoLocationType)
                {
                    case VideoLocationType.Local:
                        EditorGUILayout.PropertyField(video, new GUIContent("Video"));
                        break;

                    case VideoLocationType.StreamingAssets:
                        EditorGUILayout.PropertyField(streamingAssetsVidPath, new GUIContent("Video Streaming Assets Path"));
                        break;

                    case VideoLocationType.URL:
                        EditorGUILayout.PropertyField(videoURL, new GUIContent("Video URL"));
                        break;
                }
                
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.PropertyField(loopVideo, new GUIContent("Loop Video"));

                EditorGUILayout.PropertyField(videoVolume, new GUIContent("Volume"));

                EditorGUILayout.PropertyField(videoUI, new GUIContent("Enable Video UI"));
                if (loc.videoUI)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(videoUIAudioVolume, new GUIContent("Show Audio Button"));
                    EditorGUILayout.PropertyField(videoUILoopButton, new GUIContent("Show Loop Button"));
                    EditorGUI.indentLevel--;
                }
                
                EditorGUILayout.PropertyField(loadingTextOverride, new GUIContent("Loading Text Override"));
            }
            // SCENE
            else if (loc.locationType == LocationType.Scene)
            {
                EditorGUILayout.PropertyField(scene, new GUIContent("Scene"));
            }

            // 180/360 Only
            if (loc.displayType.Is180() || loc.displayType.Is360())
            {
                EditorGUILayout.PropertyField(rotOffset, new GUIContent("Rotation"));
            }
            // Not 180/360 only
            else if (loc.locationType != LocationType.Scene)
            {
                if (loc.locationType != LocationType.Empty)
                {
                    EditorGUILayout.PropertyField(scaleToFullscreen, new GUIContent("Scale to Fullscreen"));
                    scaleToFullscreen.AddTooltip();
                }
                EditorGUILayout.PropertyField(lockCamera, new GUIContent("Lock Camera (Non-VR Only)"));
            }
            
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
        }

        /// <summary>
        /// Enables the location and location view but doesn't update its current set data
        /// </summary>
        public static void EnableLocation(LocationManager manager, Location location)
        {
            // Deactivate all because the last active location view could be active but unknown (due to domain reloads etc.)
            manager.DeactivateLocationViews();
            manager.DeactivateLocationsExcept(location); // Keep this location active in case it was already active

            // Show this location object
            if (!location.gameObject.activeSelf)
                location.gameObject.SetActive(true);
            
            // Show corresponding location view
            manager.SwitchCurrentLocationView(location);
            
            currentLocation = location;
            
            if (Application.isPlaying)
                return;
            
            // Show video UI in editor mode
            if (location.videoUI)
            {
                manager.SetVideoUI(PlayerMode.Desktop);
                manager.videoUI.EnableUI(null, location.videoUIAudioVolume, location.videoUILoopButton);
            }
            else
            {
                manager.videoUI.DisableUI();
            }
        }

        /// <summary>
        /// Disables the location and location view
        /// </summary>
        public static void DisableLocation(LocationManager manager, Location location)
        {
            manager.DeactivateLocationViews();
            
            // Disable video UI in case it was active before
            manager.videoUI.DisableUI();
            
            if (location != null && location.gameObject.activeSelf)
                location.gameObject.SetActive(false);
        }
    }
}