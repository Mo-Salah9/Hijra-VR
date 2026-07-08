using UnityEngine;
using UnityEditor;

namespace CrizGames.Vour.Editor
{
    public abstract class PanelEditor : UnityEditor.Editor
    {
        protected bool inPlayMode;
        protected bool inPrefabView;

        protected virtual void OnEnable()
        {
            var panel = (Panel)target;
            
            inPlayMode = EditorTools.IsEditorInPlayMode();
            inPrefabView = EditorTools.IsInPrefabView(panel.gameObject);

            Undo.undoRedoPerformed -= UpdatePanel;
            Undo.undoRedoPerformed += UpdatePanel;
        }

        protected virtual void OnDisable()
        {
            Undo.undoRedoPerformed -= UpdatePanel;
        }
        
        protected abstract void UpdatePanel();
    }
}
