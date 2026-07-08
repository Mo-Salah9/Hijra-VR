using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace CrizGames.Vour
{
    public class VideoSlider : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {

        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform fillArea;
        [SerializeField] private RectTransform handle;
        private RectTransform _handleParent;
        [Space]
        [SerializeField] private float handleHoverSize = 30f;
        private float _handleNormalSize;
        [Space]
        [SerializeField] private float tweenSmoothing = 0.05f;
        [SerializeField] private float normalPadding = 6f;
        [SerializeField] private float hoverPadding = 0f;
        
        private float _currentPadding, _targetPadding, _paddingTweenVel;
        private float _currentHandleSize, _targetHandleSize, _handleTweenVel;

        private VideoController _controller;
        private Slider _slider;

        private void Start()
        {
            _slider = GetComponent<Slider>();
            _controller = GetComponentInParent<VideoController>(true);
            _handleParent = handle.parent.GetComponent<RectTransform>();

            _controller.OnVideoProgress.AddListener(_slider.SetValueWithoutNotify);
            _slider.onValueChanged.AddListener(_controller.SetVideoTime);

            // Set start values
            _currentPadding = _targetPadding = normalPadding;
            SetPadding(normalPadding);
            SetHandleSize(_handleParent.rect.height - normalPadding * 2f);
            _handleNormalSize = handle.rect.height;
        }

        private void Update()
        {
            if (Mathf.Approximately(_currentPadding, _targetPadding)) 
                return;
            
            _currentPadding = Mathf.SmoothDamp(_currentPadding, _targetPadding, ref _paddingTweenVel, tweenSmoothing);
            _currentHandleSize = Mathf.SmoothDamp(_currentHandleSize, _targetHandleSize, ref _handleTweenVel, tweenSmoothing);
            SetPadding(_currentPadding);
            SetHandleSize(_currentHandleSize);
        }

        private void SetPadding(float yPadding)
        {
            background.offsetMin = new Vector2(background.offsetMin.x, yPadding);
            background.offsetMax = new Vector2(background.offsetMax.x, -yPadding);
            fillArea.offsetMin = new Vector2(fillArea.offsetMin.x, yPadding);
            fillArea.offsetMax = new Vector2(fillArea.offsetMax.x, -yPadding);
        }

        private void SetHandleSize(float size)
        {
            float yPadding = (_handleParent.rect.height - size) / 2f;
            handle.offsetMin = new Vector2(handle.offsetMin.x, yPadding);
            handle.offsetMax = new Vector2(handle.offsetMax.x, -yPadding);
            handle.sizeDelta = new Vector2(handle.rect.height, handle.sizeDelta.y);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _targetPadding = hoverPadding;
            _targetHandleSize = handleHoverSize;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _targetPadding = normalPadding;
            _targetHandleSize = _handleNormalSize;
        }

        private void OnValidate()
        {
            if (handle == null)
                return;

            _handleParent = handle.parent.GetComponent<RectTransform>();
            SetPadding(normalPadding);
            SetHandleSize(_handleParent.rect.height - normalPadding * 2f);
        }
    }
}