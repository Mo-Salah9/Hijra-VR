using UnityEngine;
using UnityEngine.UI;

namespace CrizGames.Vour
{
    public class VideoPlayButton : MonoBehaviour
    {
        [SerializeField] private Sprite playIcon;
        [SerializeField] private Sprite pauseIcon;
        [SerializeField] private Sprite replayIcon;

        private VideoController _controller;
        private Image _image;
        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _image = GetComponent<Image>();
            _controller = GetComponentInParent<VideoController>(true);

            _button.onClick.AddListener(_controller.TogglePlayingState);

            _controller.OnPlayStateChanged.AddListener(OnPlayStateChanged);
        }

        private void OnPlayStateChanged(bool playing)
        {
            if (!playing && _controller.hasVideoEnded)
                _image.sprite = replayIcon;
            else
                _image.sprite = playing ? pauseIcon : playIcon;
        }
    }
}