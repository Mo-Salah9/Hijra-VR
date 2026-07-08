using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace CrizGames.Vour
{
    public class MainVideoUIController : MonoBehaviour
    {
        public static MainVideoUIController Instance;
        
        public VideoUIController uiControllerDesktop;
        public VideoUIController uiControllerVR;
        
        [HideInInspector] public VideoController videoController;

        public void Init()
        {
            Instance = this;

            videoController = GetComponent<VideoController>();

            // Scale up video UI for mobile devices
            if (Application.isMobilePlatform)
            {
                var desktopCanvas = transform.GetChild(0).GetComponent<CanvasScaler>();
                desktopCanvas.referenceResolution /= 1.5f;
            }
        }

        public void EnableUI(VideoPlayer videoPlayer, bool enableAudioVolume, bool enableLoopButton)
        {
            uiControllerDesktop.EnableUI(videoPlayer, enableAudioVolume, enableLoopButton);
            uiControllerVR.EnableUI(videoPlayer, enableAudioVolume, enableLoopButton);
        }

        public void DisableUI()
        {
            uiControllerDesktop.DisableUI();
            uiControllerVR.DisableUI();
        }
    }
}