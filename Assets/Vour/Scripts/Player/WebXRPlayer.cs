using UnityEngine;
#if VOUR_WEBXR
using WebXR;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
#endif

namespace CrizGames.Vour
{
    public class WebXRPlayer : VRPlayer
    {
        private WebXRDesktopPlayer desktopPlayer;

#if VOUR_WEBXR
        private WebXRState xrState = WebXRState.NORMAL;

        private XRInputModalityManager.InputMode lastInputMode;

        public override void Init()
        {
            if (initialized)
                return;
            
            desktopPlayer = GetComponent<WebXRDesktopPlayer>();
            desktopPlayer.centerCamera = centerCamera;
            desktopPlayer.Init(); // Init if it wasn't yet
            desktopPlayer.SetCenterCam(centerCamera);

            if (WebXRManager.Instance.subsystem != null)
                xrState = WebXRManager.Instance.XRState;

            // Do SwitchXRState before Init() so that lastInputMode will be set correctly in Init() by SubscribeAndUpdate() in VRPlayer.cs
            // This ensures that gaze pointer will not be activated at startup when there actually are controllers/hands available
            SwitchXRState();
            
            base.Init();
        }

        private void OnEnable()
        {
            WebXRManager.OnXRChange += OnXRChange;
        }

        private void OnDisable()
        {
            WebXRManager.OnXRChange -= OnXRChange;
        }

        private void SwitchXRState()
        {
            var manager = LocationManager.GetManager();
            var playerMode = xrState == WebXRState.VR ? PlayerMode.VR : PlayerMode.Desktop;
            manager.SetTransitionUI(playerMode);
            manager.SetVideoUI(playerMode);
            
            switch (xrState)
            {
                case WebXRState.VR:
                    OnInputModeChanged(lastInputMode);
                    desktopPlayer.enabled = false;
                    break;

                case WebXRState.NORMAL:
                    EnableGaze(false);
                    desktopPlayer.enabled = true;
                    desktopPlayer.SetCenterCam(desktopPlayer.centerCamera);
                    
                    // When returning to desktop from VR, reset rotation if desktop cam is locked
                    if (!desktopPlayer.canMoveCam)
                    {
                        // Delay one frame because the TrackedPoseDriver sometimes would interfere with setting the rotation
                        Invoke(nameof(ResetRotation), 0);
                    }
                    break;
                
                case WebXRState.AR:
                    Debug.LogError("AR is not supported by Vour.");
                    break;
            }
        }

        private void OnXRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
        {
            xrState = state;
            SwitchXRState();
        }
        
        protected override void OnInputModeChanged(XRInputModalityManager.InputMode newInputMode)
        {
            if (xrState == WebXRState.NORMAL)
                return;
            
            base.OnInputModeChanged(newInputMode);
            lastInputMode = newInputMode;
        }

        public override void SetCenterCam(bool center)
        {
            if (xrState == WebXRState.VR)
                base.SetCenterCam(center);
            else
                desktopPlayer.SetCenterCam(center);
        }
        
        public override void OnNewLocation(Location newLocation)
        {
            base.OnNewLocation(newLocation);

            desktopPlayer.OnNewLocation(newLocation);
        }
        
        public override void ResetRotation(Vector3 newEulerAngles)
        {
            cam.transform.parent.eulerAngles = Vector3.up * -cam.transform.localEulerAngles.y + newEulerAngles;
            
            desktopPlayer.ResetRotation(cam.transform.eulerAngles);
        }
#endif
    }
}
