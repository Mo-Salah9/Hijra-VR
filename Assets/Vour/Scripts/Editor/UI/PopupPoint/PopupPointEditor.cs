using UnityEngine;
using UnityEditor;

namespace CrizGames.Vour.Editor
{
    public abstract class PopupPointEditor : PanelEditor
    {
        private Vector3 lastPos;

        protected override void OnEnable()
        {
            base.OnEnable();
            
            var point = (PopupPoint)target;
            
            lastPos = point.transform.position;
        }

        protected virtual void OnSceneGUI()
        {
            if (inPlayMode || inPrefabView)
                return;
            
            var point = (PopupPoint)target;
            
            // Update rotation when point is being moved
            if (point.rotateTowardsPlayer && point.transform.position != lastPos)
            {
                point.RotateTowardsPlayer();
                
                lastPos = point.transform.position;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (inPlayMode || inPrefabView)
                return;

            var point = (PopupPoint)target;
            if (point == null)
                return;
            
            NotifyRotationsWillChange();
            point.RotateTowardsPlayer();
        }

        protected virtual void NotifyRotationsWillChange()
        {
            var point = (PopupPoint)target;
            
            // Notify objects that will be changed because of RotateTowardsPlayer()
            var t = point.transform;
            Undo.RecordObject(t, $"Updated Popup Point rotation ({point.name})");
            PrefabUtility.RecordPrefabInstancePropertyModifications(t);

            t = point.panelContainer;
            Undo.RecordObject(t, $"Updated panel container rotation ({point.name})");
            PrefabUtility.RecordPrefabInstancePropertyModifications(t);

            t = point.panel;
            Undo.RecordObject(t, $"Updated panel rotation ({point.name})");
            PrefabUtility.RecordPrefabInstancePropertyModifications(t);
        }
    }
}