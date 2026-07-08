using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

namespace CrizGames.Vour.Editor
{
#if UNITY_6000_0_OR_NEWER
    [Overlay(defaultDisplay = true)]
    public class VideoPreviewNotificationOverlay : Overlay
#else
    public class VideoPreviewNotificationOverlay
#endif
    {
        private Label _label;
        private string _text;
        
#if UNITY_6000_0_OR_NEWER
        public VideoPreviewNotificationOverlay()
        {
            displayName = "Video Preview Notification Overlay";
        }
        
        public override VisualElement CreatePanelContent()
        {
            var root = new VisualElement();
            root.style.backgroundColor = Color.clear;
            
            _label = new Label(_text);
            root.Add(_label);
            
            return root;
        }

        public override void OnCreated()
        {
            var root = rootVisualElement;
            
            var header = root.Q(className: "overlay-header");
            header.style.display = DisplayStyle.None;
            
            var overlay = root.Q("unity-overlay");
            overlay.style.backgroundColor = Color.clear;
        }
#endif

        public void SetCurrentLoadingVideo(string videoName)
        {
            _text = $"Loading video preview for \"{videoName}\"...";

            if (_label != null)
                _label.text = _text;
        }

        public void NoVideoLoading()
        {
            _text = "No video is currently loading.";

            if (_label != null)
                _label.text = _text;
        }

        public void Show()
        {
#if UNITY_6000_0_OR_NEWER
            SceneView.AddOverlayToActiveView(this);
#endif
        }

        public void Hide()
        {
#if UNITY_6000_0_OR_NEWER
            SceneView.RemoveOverlayFromActiveView(this);
#endif
        }
    }
}
