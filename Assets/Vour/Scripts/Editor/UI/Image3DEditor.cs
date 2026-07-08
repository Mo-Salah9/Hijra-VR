using UnityEditor;
using UnityEditor.UI;

namespace CrizGames.Vour.Editor
{
    [CustomEditor(typeof(Image3D))]
    public class Image3DEditor : ImageEditor
    {
        private SerializedProperty displayType;
        private SerializedProperty layout3D;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            
            displayType = serializedObject.FindProperty("_displayType");
            layout3D = serializedObject.FindProperty("_layout3D");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var img = (Image3D)target;
            
            serializedObject.Update();
            
            EditorGUILayout.PropertyField(displayType);
            if (img.displayType == PanelDisplayType._3D)
                EditorGUILayout.PropertyField(layout3D);

            serializedObject.ApplyModifiedProperties();
        }
        
    }
}