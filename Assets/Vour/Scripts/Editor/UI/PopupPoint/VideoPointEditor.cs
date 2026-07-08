using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CrizGames.Vour.Editor
{
    [CustomEditor(typeof(VideoPoint))]
    public class VideoPointEditor : PopupPointEditor
    {
        private SerializedProperty video;
        private SerializedProperty videoURL;
        private SerializedProperty streamingAssetsVidPath;
        private SerializedProperty videoType;
        private SerializedProperty loopVideo;
        private SerializedProperty videoVolume;
        private SerializedProperty videoUI;
        private SerializedProperty videoUIAudioVolume;
        private SerializedProperty videoUILoopButton;
        private SerializedProperty rotateTowardsPlayer;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            
            video = serializedObject.FindProperty(nameof(VideoPoint.video));
            videoURL = serializedObject.FindProperty(nameof(VideoPoint.videoURL));
            streamingAssetsVidPath = serializedObject.FindProperty(nameof(VideoPoint.streamingAssetsVidPath));
            videoType = serializedObject.FindProperty(nameof(VideoPoint.videoLocationType));
            loopVideo = serializedObject.FindProperty(nameof(VideoPoint.loopVideo));
            videoVolume = serializedObject.FindProperty(nameof(VideoPoint.videoVolume));
            videoUI = serializedObject.FindProperty(nameof(VideoPoint.videoUI));
            videoUIAudioVolume = serializedObject.FindProperty(nameof(VideoPoint.videoUIAudioVolume));
            videoUILoopButton = serializedObject.FindProperty(nameof(VideoPoint.videoUILoopButton));
            rotateTowardsPlayer = serializedObject.FindProperty(nameof(VideoPoint.rotateTowardsPlayer));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var point = (VideoPoint)target;

            if (inPrefabView)
            {
                DrawDefaultInspector();
                return;
            }

            if (inPlayMode)
                EditorGUI.BeginDisabledGroup(true);

            EditorGUI.BeginChangeCheck();
            
            DrawProperties(point);
            
            var propertiesChanged = EditorGUI.EndChangeCheck();
            
            if (inPlayMode)
                EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
            
            if (propertiesChanged && !inPlayMode)
            {
                UpdatePanel();
            }
        }
        
        public static void GetVideoPreview(VideoPoint point)
        {
            point.GetVideoPreview(previewTexture =>
            {
                if (point == null)
                    return;

                point.SetTexture(previewTexture);
            });
        }

        private void DrawProperties(VideoPoint point)
        {
            EditorGUILayout.PropertyField(videoType, new GUIContent("Video Type"));

            switch (point.videoLocationType)
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

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(rotateTowardsPlayer, new GUIContent("Rotate Towards Player"));
            rotateTowardsPlayer.AddTooltip();
        }

        protected override void UpdatePanel()
        {
            var point = (VideoPoint)target;
                
            GetVideoPreview(point);
            
            var ui = point.panel.GetComponent<VideoUIController>();
            
            if (point.videoUI)
                ui.EnableUI(null, point.videoUIAudioVolume, point.videoUILoopButton);
            else
                ui.DisableUI();
        }
    }
}