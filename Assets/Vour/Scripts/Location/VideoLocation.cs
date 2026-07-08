using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

namespace CrizGames.Vour
{
    public class VideoLocation : MediaLocation
    {
        private RenderTexture _renderTexture;
		
        private bool _isRetryingPlayback;

        public override void Init()
        {
            base.Init();
            
            _isRetryingPlayback = false;
			
            gameObject.SetActive(true);

            if (!Application.isPlaying) 
                return;
            
            if (location.videoUI)
            {
                MainVideoUIController.Instance.EnableUI(location.videoPlayer, location.videoUIAudioVolume, location.videoUILoopButton);
            }
            else
            {
                MainVideoUIController.Instance.DisableUI();
            }

            location.videoPlayer.errorReceived -= RetryPlayback;
            location.videoPlayer.errorReceived += RetryPlayback;
            
            if (!location.videoPlayer.isPlaying)
                location.videoPlayer.Prepare();
            
            // Set isLooping again in case Location.loopVideo property changed in play mode
            location.videoPlayer.isLooping = location.loopVideo;
            
            // Set volume before SetupLoadedVideo() in case Location.volume property changed in play mode
            SetVideoVolume(); 
        }

        public void OnDisable()
        {
            if (location == null || location.videoPlayer == null)
                return;

            if (_renderTexture != null)
            {
                _renderTexture.DiscardContents();
                _renderTexture.Release();
                _renderTexture = null;
            }

            if (location.videoUI)
                MainVideoUIController.Instance.DisableUI();
        }

        public override void UpdateLocation()
        {
#if UNITY_EDITOR
            // Must be done for the video to play without sound when volume is set to 0.
            // For some reason without this double initialization of the video player,
            // it will play with full volume on first frame even when set to 0.
            if (Application.isPlaying && !location.videoPlayer.isPlaying)
            {
                location.CreateVideoPlayer();
                location.videoPlayer.Play();
            }
#endif
            // Set loading texture while video is loading
            if (Application.isPlaying)
            {
                if (!location.videoPlayer.isPrepared)
                {
                    SetTexture(Texture2D.grayTexture);
                    LocationManager.GetManager().ShowLoadingUI(true);

                    // When video loaded
                    location.videoPlayer.prepareCompleted += SetupLoadedVideo;
                }
                else
                {
                    // When video is already loaded
                    SetupLoadedVideo(location.videoPlayer);
                }
            }
            else
            {
                SetTexture(Texture2D.grayTexture);
            }

            GetVideoSize((width, height) => UpdateSize(new Vector2(width, height)));
        }

        private void SetupLoadedVideo(VideoPlayer videoPlayer)
        {
            videoPlayer.SetupRenderTexture(ref _renderTexture);
            videoPlayer.targetTexture = _renderTexture;

            SetTexture(_renderTexture);
            
            if (!location.videoPlayer.isPlaying)
                location.videoPlayer.Play();
            
            SetVideoVolume();
            
            LocationManager.GetManager().ShowLoadingUI(false);
            
            // Setup video UI volume
            if (location.videoUI)
            {
                var videoController = MainVideoUIController.Instance.videoController;
                videoController.SetAudioVolume(location.videoVolume);
            }
            
            IsReady = true;
            _isRetryingPlayback = false;
        }

        private void RetryPlayback(VideoPlayer source, string message)
        {
            if (_isRetryingPlayback)
                return;
            
            _isRetryingPlayback = true;
            Debug.Log($"Video playback failed for {source.name}. Retrying...\n{message}");
            
            source.Stop();
            source.Play();
        }

        private void GetVideoSize(UnityAction<int, int> callback)
        {
            VideoUtils.GetVideoSize(location.videoLocationType, location, video, location.videoPlayer, callback, callbackTextureSize => location.videoPlayer.prepareCompleted += (_) => callbackTextureSize());
        }

        private void SetVideoVolume()
        {
            if (location.videoPlayer == null)
                return;

            for (ushort i = 0; i < location.videoPlayer.audioTrackCount; i++)
                location.videoPlayer.SetDirectAudioVolume(i, location.videoVolume);
        }
    }
}