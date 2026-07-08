using System.IO;
using UnityEditor;
using UnityEngine;

namespace CrizGames.Vour.Editor
{
    [CustomPropertyDrawer(typeof(StreamingAssetsPathAttribute))]
    public class StreamingAssetsPathDrawer : PropertyDrawer
    {
        private static readonly GUIStyle ErrorStyle = "CN EntryErrorIconSmall";

        private bool _shouldOpenFileDialog;
        private bool _shouldSetNewValue;
        private string _newValue;
        private string _lastValue;
        private bool _isValidPath;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            // New value and GUI.changed must be set in the GUI cycle,
            // otherwise the change won't be recognised and handled correctly
            if (_shouldSetNewValue)
            {
                property.stringValue = _newValue;
                GUI.changed = true;

                _shouldSetNewValue = false;
            }
            
            // When string value changed
            if (property.stringValue != _lastValue)
            {
                // Check if the path is valid
                _isValidPath = File.Exists(Path.Combine(Application.streamingAssetsPath, property.stringValue));
                _lastValue = property.stringValue;
            }
            
            var fileButtonPosition = new Rect(position.x + position.width - 25, position.y, 25, position.height);

            // Button on the right to open a file picker
            if (GUI.Button(fileButtonPosition, "..."))
            {
                _shouldOpenFileDialog = true;
                
                // Delay the file dialog to avoid GUI layout issues
                EditorApplication.delayCall += OpenFileDialog;
            }

            var errorIconWidth = _isValidPath ? 0 : ErrorStyle.fixedWidth + 4;
            var errorIconPosition = new Rect(fileButtonPosition.x - errorIconWidth, position.y, errorIconWidth, position.height);
            if (!_isValidPath)
                DisplayErrorIcon(errorIconPosition, string.IsNullOrEmpty(property.stringValue));
            
            var textFieldPosition = new Rect(position.x, position.y, position.width - 30 - errorIconPosition.width, position.height);
            
            // Text field
            EditorGUI.PropertyField(textFieldPosition, property, label);
            
            EditorGUI.EndProperty();
        }

        private void OpenFileDialog()
        {
            if (!_shouldOpenFileDialog)
                return;
            
            var streamingAssetsPathAttribute = attribute as StreamingAssetsPathAttribute;
            
            var path = EditorUtility.OpenFilePanel("Select File", Application.streamingAssetsPath, streamingAssetsPathAttribute!.fileExtensionFilter);
            if (!string.IsNullOrEmpty(path))
            {
                _newValue = path.Replace(Application.streamingAssetsPath + "/", "");
                _shouldSetNewValue = true;
            }
            
            _shouldOpenFileDialog = false;
        }
        
        private void DisplayErrorIcon(Rect position, bool isEmpty)
        {
            var warningRect = new Rect(position);
            warningRect.width = ErrorStyle.fixedWidth + 4;
            
            var errorTooltip = new GUIContent("", "File at specified path not found.");
            if (isEmpty)
                errorTooltip.tooltip = "Path is empty.";

            position.xMin = warningRect.xMax;
            GUI.Label(warningRect, errorTooltip, ErrorStyle);
        }
    }
}