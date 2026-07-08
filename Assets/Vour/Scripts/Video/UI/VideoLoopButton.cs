using UnityEngine;
using UnityEngine.UI;

namespace CrizGames.Vour
{
    public class VideoLoopButton : MonoBehaviour
    {
        [SerializeField] private Sprite loopOnIcon;
        [SerializeField] private Sprite loopOffIcon;

        private VideoController _controller;
        private Image _image;
        private Button _button;

        private void Start()
        {
            _image = GetComponent<Image>();
            _button = GetComponent<Button>();
            _controller = GetComponentInParent<VideoController>(true);

            _button.onClick.AddListener(ToggleLoop);
            
            // Set correct value on startup and when the user changed it in play mode
            _controller.OnInit.AddListener(() => SetLoopIcon(_controller.isLooping));
        }

        private void OnEnable()
        {
            if (_controller != null)
                SetLoopIcon(_controller.isLooping);
        }

        private void ToggleLoop()
        {
            _controller.SetLooping(!_controller.isLooping);
            SetLoopIcon(_controller.isLooping);
        }

        private void SetLoopIcon(bool on)
        {
            _image.sprite = on ? loopOnIcon : loopOffIcon;
        }
    }
}