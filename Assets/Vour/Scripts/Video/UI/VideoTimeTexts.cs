using UnityEngine;
using TMPro;

namespace CrizGames.Vour
{
    public class VideoTimeTexts : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI currentTimeText;
        [SerializeField] private TextMeshProUGUI finalTimeText;

        private VideoController _controller;

        private bool _gotTimes = false;

        private void Start()
        {
            _controller = GetComponentInParent<VideoController>(true);

            _controller.OnInit.AddListener(() => _gotTimes = false);
        }

        private void LateUpdate()
        {
            // Wait while times are not available (video with URL)
            if (!_gotTimes && !float.IsNaN(_controller.videoTimeLength))
            {
                finalTimeText.text = SecondsToTimeString(_controller.videoTimeLength);
                _gotTimes = true;
            }

            if (_gotTimes)
                currentTimeText.text = SecondsToTimeString(_controller.currentVideoTime);
        }

        private string SecondsToTimeString(float seconds)
        {
            if (float.IsNaN(seconds))
                return "0:00";

            var format = @"m\:ss";
            if (seconds >= 600)
                format = @"mm\:ss";
            else if (seconds >= 3600)
                format = @"hh\:mm\:ss";
            
            return System.TimeSpan.FromSeconds(seconds).ToString(format);
        }
    }
}