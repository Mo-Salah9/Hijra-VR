using UnityEditor;
using UnityEngine;

namespace CrizGames.Vour.Editor
{
    [CustomEditor(typeof(TeleportPoint))]
    [CanEditMultipleObjects]
    public class TeleportPointEditor : UnityEditor.Editor
    {
        private const string EventsFoldoutPrefKey = "VourTeleportPointEventFoldout";
        
        private SerializedProperty teleportType;
        private SerializedProperty targetLocation;
        private SerializedProperty resetPlayerRotation;
        private SerializedProperty onTeleport;

        private bool showEvents;

        /// <summary>
        /// OnEnable
        /// </summary>
        private void OnEnable()
        {
            teleportType = serializedObject.FindProperty(nameof(TeleportPoint.teleportType));
            targetLocation = serializedObject.FindProperty(nameof(TeleportPoint.targetLocation));
            resetPlayerRotation = serializedObject.FindProperty(nameof(TeleportPoint.resetPlayerRotation));
            onTeleport = serializedObject.FindProperty(nameof(TeleportPoint.onTeleport));

            showEvents = EditorPrefs.GetBool(EventsFoldoutPrefKey, false);
        }

        /// <summary>
        /// OnInspectorGUI
        /// </summary>
        public override void OnInspectorGUI()
        {
            TeleportPoint p = (TeleportPoint)target;

            EditorGUILayout.PropertyField(teleportType, new GUIContent("Teleport Type"));

            if (p.teleportType == TeleportPoint.TeleportType.SwitchLocation)
            {
                EditorGUILayout.PropertyField(targetLocation, new GUIContent("Target Location"));
                
                if (p.targetLocation == null)
                    EditorGUILayout.HelpBox("Target location is not assigned.", MessageType.Error);
            }
            
            EditorGUILayout.PropertyField(resetPlayerRotation, new GUIContent("Reset Player Rotation"));
            
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
                EditorGUILayout.PropertyField(onTeleport);
                onTeleport.AddTooltip();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}