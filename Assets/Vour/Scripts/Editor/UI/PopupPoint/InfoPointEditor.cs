using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CrizGames.Vour.Editor
{
    [CustomEditor(typeof(InfoPoint))]
    public class InfoPointEditor : PopupPointEditor
    {
        private SerializedProperty useCustomPanel;
        private SerializedProperty customPanelObject;
        private SerializedProperty title;
        private SerializedProperty image;
        private SerializedProperty text;
        private SerializedProperty rotateTowardsPlayer;
        private SerializedProperty displayType;
        private SerializedProperty layout3D;
        
        private static string[] _displayTypes = {"2D", "3D"};
        private static GUIContent[] _displayTypesContent = _displayTypes.Select(x => new GUIContent(x)).ToArray();

        protected override void OnEnable()
        {
            base.OnEnable();
            
            useCustomPanel = serializedObject.FindProperty(nameof(InfoPoint.useCustomPanel));
            customPanelObject = serializedObject.FindProperty(nameof(InfoPoint.customPanelObject));
            title = serializedObject.FindProperty(nameof(InfoPoint.title));
            image = serializedObject.FindProperty(nameof(InfoPoint.image));
            text = serializedObject.FindProperty(nameof(InfoPoint.text));
            rotateTowardsPlayer = serializedObject.FindProperty(nameof(InfoPoint.rotateTowardsPlayer));
            displayType = serializedObject.FindProperty(nameof(InfoPoint.displayType));
            layout3D = serializedObject.FindProperty(nameof(InfoPoint.layout3D));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var point = (InfoPoint)target;
            
            var panelContainer = point.panelContainer;
            var panel = panelContainer.childCount > 0 ? panelContainer.GetChild(0).gameObject : null;

            if (inPrefabView)
            {
                DrawDefaultInspector();
                return;
            }
            
            EditorGUI.BeginDisabledGroup(inPlayMode);

            // Custom panel
            EditorGUILayout.PropertyField(useCustomPanel, new GUIContent("Use Custom Panel"));
            if (point.useCustomPanel)
            {
                EditorGUILayout.PropertyField(customPanelObject, new GUIContent("Custom Panel Object"));

                if (point.customPanelObject == null)
                    EditorGUILayout.HelpBox("You need to set Custom Panel Object in order for it to work in play mode!", MessageType.Warning);

                else if (!point.customPanelObject.activeSelf)
                    point.customPanelObject.SetActive(true);

                EditorGUILayout.PropertyField(rotateTowardsPlayer, new GUIContent("Rotate Towards Player"));
                rotateTowardsPlayer.AddTooltip();
                
                serializedObject.ApplyModifiedProperties();
                
                // Disable instantiated Vour panel
                if (panel != null)
                    panel.SetActive(false);
                
                EditorGUI.EndDisabledGroup();
                return;
            }
            else if (point.customPanelObject != null && point.customPanelObject.activeSelf)
            {
                point.customPanelObject.SetActive(false);
            }

            // Vour panel UI
            EditorGUI.BeginChangeCheck();
            
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(title, new GUIContent("Title"));
            EditorGUILayout.PropertyField(image, new GUIContent("Image"));

            if (point.image != null)
            {
                // Display type
                DisplayTypeProperty("Display Type");
                if (point.displayType == PanelDisplayType._3D)
                    EditorGUILayout.PropertyField(layout3D, new GUIContent("3D Layout"));
                
                // Image Position (left/right)
                var prevPanelType = point.panelType;
                var newPanelType = (InfoPanelImageType)EditorGUILayout.EnumPopup("Image Position", point.panelType);
                if (newPanelType != prevPanelType)
                {
                    Undo.RecordObject(point, $"Updated Info Point Panel ({point.name})");
                    PrefabUtility.RecordPrefabInstancePropertyModifications(point);
                    point.panelType = newPanelType;
                }
            }

            EditorGUILayout.PropertyField(text, new GUIContent("Text"));

            EditorGUILayout.Space();
            
            EditorGUILayout.PropertyField(rotateTowardsPlayer, new GUIContent("Rotate Panel Towards Player"));

            EditorGUI.EndDisabledGroup();
            
            serializedObject.ApplyModifiedProperties();

            if (inPlayMode)
            {
                EditorGUI.EndChangeCheck();
                return;
            }
            
            // Get Vour panel prefab
            var prefab = (point.image == null) switch
            {
                true => VourSettings.Instance.infoPanelTextOnly,
                false => point.panelType switch
                {
                    InfoPanelImageType.LeftImage => VourSettings.Instance.infoPanelLeftImage,
                    InfoPanelImageType.RightImage => VourSettings.Instance.infoPanelRightImage,
                    _ => null
                }
            };
            
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

        protected override void NotifyRotationsWillChange()
        {
            base.NotifyRotationsWillChange();

            var point = (InfoPoint)target;
            
            if (point.customPanelObject != null)
            {
                var t = point.customPanelObject.transform;
                Undo.RecordObject(t, $"Updated custom panel rotation ({point.name})");
                PrefabUtility.RecordPrefabInstancePropertyModifications(t);
            }
        }

        protected override void UpdatePanel()
        {
            var point = (InfoPoint)target;
            InfoPanelEditor.UpdateInfoPanel(point);
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