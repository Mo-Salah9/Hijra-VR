using UnityEngine;
#if VOUR_XRI
using UnityEngine.XR.Interaction.Toolkit.Inputs;
#endif

namespace CrizGames.Vour
{
    public class VRPlayer : PlayerBase
    {
        [SerializeField] protected GameObject gazeInteractor;

        public override void Init()
        {
            if (initialized)
                return;
            
            base.Init();
            
#if VOUR_XRI
            // Subscribe to currentInputMode changes
            // and immediately call the callback to set correct gaze pointer state at startup
            XRInputModalityManager.currentInputMode.SubscribeAndUpdate(OnInputModeChanged);
#endif
        }

        protected virtual void Update()
        {
            if (centerCamera)
                transform.position -= cam.transform.position;
        }

        public override void ResetRotation(Vector3 newEulerAngles)
        {
            transform.eulerAngles = Vector3.up * -cam.transform.localEulerAngles.y + newEulerAngles;
        }
        
#if VOUR_XRI
        protected virtual void OnInputModeChanged(XRInputModalityManager.InputMode newInputMode)
        {
            EnableGaze(newInputMode == XRInputModalityManager.InputMode.None);
        }
#endif
        
        protected void EnableGaze(bool gazeEnabled)
        {
            gazeInteractor.SetActive(gazeEnabled);
        }
    }
}