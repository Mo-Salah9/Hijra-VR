using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CrizGames.Vour
{
    public class VideoVolumeControl : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Sprite audioOnIcon;
        [SerializeField] private Sprite audioOffIcon;
        [Space]
        [SerializeField] private float sliderHeight = 200f;
        [Space]
        [SerializeField] private float smoothTime = 0.1f;
        
        private VideoController _controller;
        private Image _image;
        private Button _button;
        private CanvasGroup _audioSliderGroup;
        private RectTransform _audioSliderContainerT;
        private Slider _audioSlider;

        private float _targetAlpha, _alphaVel;
        private float _targetSliderHeight, _sliderHeightVel;

        private bool _pointerHovering = false;

        private void Start()
        {
            _button = GetComponentInChildren<Button>();
            _image = _button.GetComponent<Image>();
            _controller = GetComponentInParent<VideoController>(true);
            _audioSliderGroup = GetComponentInChildren<CanvasGroup>();
            _audioSliderContainerT = _audioSliderGroup.GetComponent<RectTransform>();
            _audioSlider = _audioSliderGroup.GetComponentInChildren<Slider>();

            _button.onClick.AddListener(ToggleAudioMute);

            _controller.OnVideoLoaded.AddListener(OnVideoLoaded);
            _controller.OnAudioVolumeChanged.AddListener(_audioSlider.SetValueWithoutNotify);
            _controller.OnAudioMuteStateChanged.AddListener(OnAudioMuteStateChanged);

            _audioSlider.onValueChanged.AddListener(_controller.SetAudioVolume);

            SetSliderVisibility(0f);
            _targetSliderHeight = sliderHeight;
        }

        private void Update()
        {
            if (Mathf.Approximately(_audioSliderGroup.alpha, _targetAlpha)) 
                return;
            
            SetSliderVisibility(Mathf.SmoothDamp(_audioSliderGroup.alpha, _targetAlpha, ref _alphaVel, smoothTime));

            var currentSliderHeight = Mathf.SmoothDamp(_audioSliderContainerT.sizeDelta.y, _targetSliderHeight, ref _sliderHeightVel, smoothTime);
            _audioSliderContainerT.sizeDelta = new Vector2(_audioSliderContainerT.sizeDelta.x, currentSliderHeight);
        }

        private void OnVideoLoaded()
        {
            _button.interactable = _controller.hasAudio;
            _audioSliderGroup.gameObject.SetActive(_button.interactable);

            _audioSlider.SetValueWithoutNotify(_controller.audioVolume);

            if (!_button.interactable)
                Debug.Log("Video has no audio tracks. Volume control was disabled.");
        }

        private void ToggleAudioMute()
        {
            _controller.SetAudioMute(!_controller.isAudioMuted);
        }

        private void SetSliderVisibility(float alpha)
        {
            _audioSliderGroup.alpha = alpha;
            _audioSliderGroup.interactable = alpha > 0.5f;
            _audioSliderGroup.blocksRaycasts = alpha > 0.5f;
        }

        private void OnAudioMuteStateChanged(bool muted)
        {
            _image.sprite = muted ? audioOffIcon : audioOnIcon;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _pointerHovering = true;
            _targetAlpha = 1f;
            _targetSliderHeight = sliderHeight;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _pointerHovering = false;
            Invoke(nameof(DelayedPointerExit), 0.4f);
        }

        private void DelayedPointerExit()
        {
            // Pointer not at slider or sth.
            if (!_pointerHovering)
            {
                _targetAlpha = 0f;
                _targetSliderHeight = 0f;
            }
        }
    }
}