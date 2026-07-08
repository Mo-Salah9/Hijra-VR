using UnityEditor;
using UnityEngine;

namespace CrizGames.Vour.Editor
{
    [CustomEditor(typeof(Player))]
    public class PlayerEditor : UnityEditor.Editor
    {
        private SerializedProperty centerCam;

        public void OnEnable()
        {
            centerCam = serializedObject.FindProperty(nameof(Player.CenterCamera));
        }

        public override void OnInspectorGUI()
        {
            // No custom inspector if in prefab mode
            var playerGo = ((Player)target).gameObject;
            if (EditorTools.IsInPrefabView(playerGo))
            {
                DrawDefaultInspector();
                return;
            }

            EditorGUILayout.PropertyField(centerCam, new GUIContent("Center Camera"));
            serializedObject.ApplyModifiedProperties();
        }
    }
}