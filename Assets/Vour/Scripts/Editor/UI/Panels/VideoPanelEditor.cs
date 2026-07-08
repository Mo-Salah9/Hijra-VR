using UnityEditor;
using UnityEngine;

namespace CrizGames.Vour.Editor
{
    [CustomEditor(typeof(VideoPanel))]
    public class VideoPanelEditor : PanelEditor
    {
        private SerializedProperty video;
        private SerializedProperty videoURL;
        private SerializedProperty streamingAssetsVidPath;
        private SerializedProperty videoType;
        private SerializedProperty playAtStart;
        private SerializedProperty loopVideo;
        private SerializedProperty videoVolume;
        private SerializedProperty videoUI;
        private SerializedProperty videoUIAudioVolume;
        private SerializedProperty videoUILoopButton;
        
        protected override void OnEnable()
        {
            base.OnEnable();

            video = serializedObject.FindProperty(nameof(VideoPanel.video));
            videoURL = serializedObject.FindProperty(nameof(VideoPanel.videoURL));
            streamingAssetsVidPath = serializedObject.FindProperty(nameof(VideoPanel.streamingAssetsVidPath));
            videoType = serializedObject.FindProperty(nameof(VideoPanel.videoLocationType));
            playAtStart = serializedObject.FindProperty(nameof(VideoPanel.playAtStart));
            loopVideo = serializedObject.FindProperty(nameof(VideoPanel.loopVideo));
            videoVolume = serializedObject.FindProperty(nameof(VideoPanel.videoVolume));
            videoUI = serializedObject.FindProperty(nameof(VideoPanel.videoUI));
            videoUIAudioVolume = serializedObject.FindProperty(nameof(VideoPanel.videoUIAudioVolume));
            videoUILoopButton = serializedObject.FindProperty(nameof(VideoPanel.videoUILoopButton));
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var panel = (VideoPanel)target;

            if (inPrefabView)
            {
                DrawDefaultInspector();
                return;
            }

            EditorGUI.BeginChangeCheck();
            
            EditorGUI.BeginDisabledGroup(inPlayMode);
            DrawProperties(panel);
            EditorGUI.EndDisabledGroup();

            var propertiesChanged = EditorGUI.EndChangeCheck();

            serializedObject.ApplyModifiedProperties();
            
            if (propertiesChanged && !inPlayMode)
            {
                UpdatePanel();
            }
        }
        
        private void GetVideoPreview(VideoPanel panel)
        {
            panel.GetVideoPreview(previewTexture =>
            {
                if (panel == null)
                    return;

                panel.SetTexture(previewTexture);
            });
        }

        private void DrawProperties(VideoPanel panel)
        {
            EditorGUILayout.PropertyField(videoType, new GUIContent("Video Type"));

            switch (panel.videoLocationType)
            {
                case VideoLocationType.Local:
                    EditorGUILayout.PropertyField(video, new GUIContent("Video"));
                    break;

                case VideoLocationType.StreamingAssets:
                    EditorGUILayout.PropertyField(streamingAssetsVidPath, new GUIContent("Streaming Assets Video Path"));
                    break;

                case VideoLocationType.URL:
                    EditorGUILayout.PropertyField(videoURL, new GUIContent("Video URL"));
                    break;
            }

            EditorGUILayout.PropertyField(playAtStart, new GUIContent("Play at Start"));

            EditorGUILayout.PropertyField(loopVideo, new GUIContent("Loop Video"));

            EditorGUILayout.PropertyField(videoVolume, new GUIContent("Volume"));

            EditorGUILayout.PropertyField(videoUI, new GUIContent("Enable Video UI"));
            if (videoUI.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(videoUIAudioVolume, new GUIContent("Show Audio Button"));
                EditorGUILayout.PropertyField(videoUILoopButton, new GUIContent("Show Loop Button"));
                EditorGUI.indentLevel--;
            }
        }

        protected override void UpdatePanel()
        {
            var panel = (VideoPanel)target;
                
            GetVideoPreview(panel);
            
            var ui = panel.panel.GetComponent<VideoUIController>();
            
            if (panel.videoUI)
                ui.EnableUI(null, panel.videoUIAudioVolume, panel.videoUILoopButton);
            else
                ui.DisableUI();
        }
    }
}