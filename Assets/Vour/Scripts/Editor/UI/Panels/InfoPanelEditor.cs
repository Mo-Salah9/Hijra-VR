using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CrizGames.Vour.Editor
{
    [CustomEditor(typeof(InfoPanel))]
    public class InfoPanelEditor : PanelEditor
    {
        private SerializedProperty title;
        private SerializedProperty image;
        private SerializedProperty text;
        private SerializedProperty displayType;
        private SerializedProperty layout3D;
        
        private static string[] _displayTypes = {"2D", "3D"};
        private static GUIContent[] _displayTypesContent = _displayTypes.Select(x => new GUIContent(x)).ToArray();
        
        protected override void OnEnable()
        {
            base.OnEnable();

            title = serializedObject.FindProperty(nameof(InfoPanel.title));
            image = serializedObject.FindProperty(nameof(InfoPanel.image));
            text = serializedObject.FindProperty(nameof(InfoPanel.text));
            displayType = serializedObject.FindProperty(nameof(InfoPanel.displayType));
            layout3D = serializedObject.FindProperty(nameof(InfoPanel.layout3D));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var info = (InfoPanel)target;

            if (inPrefabView)
            {
                DrawDefaultInspector();
                return;
            }

            // Vour panel UI
            EditorGUI.BeginChangeCheck();
            
            EditorGUI.BeginDisabledGroup(inPlayMode);
            
            EditorGUILayout.PropertyField(title, new GUIContent("Title"));
            EditorGUILayout.PropertyField(image, new GUIContent("Image"));
            
            if (info.image != null)
            {
                // Display type
                DisplayTypeProperty("Display Type");
                if (info.displayType == PanelDisplayType._3D)
                    EditorGUILayout.PropertyField(layout3D, new GUIContent("3D Layout"));
                
                // Image Position (left/right)
                var prevPanelType = info.panelType;
                var newPanelType = (InfoPanelImageType)EditorGUILayout.EnumPopup("Image Position", info.panelType);
                if (newPanelType != prevPanelType)
                {
                    Undo.RecordObject(info, $"Updated Info Panel ({info.name})");
                    PrefabUtility.RecordPrefabInstancePropertyModifications(info);
                    info.panelType = newPanelType;
                }
            }
            
            EditorGUILayout.PropertyField(text, new GUIContent("Text"));
            
            // Bugfix: There must be some text rendering after the "text" property,
            // otherwise a second label is rendered in the top left of the inspector.
            EditorGUILayout.LabelField("", GUILayout.Height(0));
            
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();

            if (inPlayMode)
            {
                EditorGUI.EndChangeCheck();
                return;
            }
            
            // Get Vour panel prefab
            var prefab = (info.image == null) switch
            {
                true => VourSettings.Instance.infoPanelTextOnly,
                false => info.panelType switch
                {
                    InfoPanelImageType.LeftImage => VourSettings.Instance.infoPanelLeftImage,
                    InfoPanelImageType.RightImage => VourSettings.Instance.infoPanelRightImage,
                    _ => null
                }
            };
            
            var panelContainer = info.panelContainer;
            var panel = panelContainer.childCount > 0 ? panelContainer.GetChild(0).gameObject : null;
            
            // Delete old panel if the panel type changed
            if (panel != null && panel.name != prefab.name)
                DestroyImmediate(panel);

            // Create panel if there isn't one yet
            if (panel == null)
            {
                panel = PrefabUtility.InstantiatePrefab(prefab, panelContainer) as GameObject;
                EditorUtility.SetDirty(panel);
                PrefabUtility.RecordPrefabInstancePropertyModifications(panel);
                
                UpdatePanel(); // Set the initial text in case EndChangeCheck() detects nothing
            }
            
            // Update panel
            {
                if (!panel.activeSelf)
                    panel.SetActive(true);

                if (EditorGUI.EndChangeCheck())
                    UpdatePanel();
            }
        }

        protected override void UpdatePanel()
        {
            UpdateInfoPanel(target as InfoPanel);
        }

        public static void UpdateInfoPanel(Panel panel)
        {
            var panelVariant = panel.panel.GetComponent<InfoPanelVariant>();
            
            // Notify objects that they will change
            Undo.RecordObject(panelVariant.title, $"Updated Info Title ({panel.name})");
            PrefabUtility.RecordPrefabInstancePropertyModifications(panelVariant.title);
            
            Undo.RecordObject(panelVariant.text, $"Updated Info Text ({panel.name})");
            PrefabUtility.RecordPrefabInstancePropertyModifications(panelVariant.text);

            if (panelVariant.image != null)
            {
                Undo.RecordObject(panelVariant.image, $"Updated Info Image ({panel.name})");
                PrefabUtility.RecordPrefabInstancePropertyModifications(panelVariant.image);
            }
            
            // Update text & image
            panel.InitPanel();
        }

        private void DisplayTypeProperty(string strLabel)
        {
            var rect = EditorGUILayout.GetControlRect(true);
            var label = EditorGUI.BeginProperty(rect, new GUIContent(strLabel), displayType);
            
            EditorGUI.BeginChangeCheck();
            var newIdx = EditorGUI.Popup(rect, label, displayType.enumValueIndex, _displayTypesContent);
            if (EditorGUI.EndChangeCheck())
                displayType.enumValueIndex = newIdx;
            
            EditorGUI.EndProperty();
        }
    }
}