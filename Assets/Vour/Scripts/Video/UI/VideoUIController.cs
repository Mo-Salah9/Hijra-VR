using UnityEngine;
using UnityEngine.Video;

namespace CrizGames.Vour
{
    [RequireComponent(typeof(VideoController))]
    public class VideoUIController : MonoBehaviour
    {
        [SerializeField] private GameObject audioVolume;
        [SerializeField] private GameObject loopButton;
        [Space]
        [SerializeField] private GameObject uiContainerOverride;

        private GameObject UIContainer => uiContainerOverride != null ? uiContainerOverride : gameObject;

        private VideoController _controller;

        protected virtual void Awake()
        {
            _controller = GetComponent<VideoController>();
        }

        public void EnableUI(VideoPlayer videoPlayer, bool enableAudioVolume, bool enableLoopButton)
        {
            UIContainer.SetActive(true);
            audioVolume.SetActive(enableAudioVolume);
            loopButton.SetActive(enableLoopButton);

            if(_controller == null)
                _controller = GetComponent<VideoController>();

            if (Application.isPlaying)
                _controller.Init(videoPlayer);
        }

        public void DisableUI()
        {
            if (UIContainer != null) // UIContainer can be null when scene unloads and it is already destroyed
                UIContainer.SetActive(false);
        }
    }
}