using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;
#if UNITY_EDITOR
using CrizGames.Vour.Editor;
#endif

namespace CrizGames.Vour
{
    public static class VideoUtils
    {
        public static void CreateVideoPlayer(this VideoPanel p)
        {
            CreateVideoPlayer(
                p.name,
                p.video,
                ref p.videoPlayer,
                p.videoURL,
                p.streamingAssetsVidPath,
                p.videoLocationType,
                p.loopVideo,
                p.playAtStart);
        }

        public static void CreateVideoPlayer(this VideoPoint p)
        {
            CreateVideoPlayer(
                p.name,
                p.video,
                ref p.videoPlayer,
                p.videoURL,
                p.streamingAssetsVidPath,
                p.videoLocationType,
                p.loopVideo,
                false);
        }

        public static void CreateVideoPlayer(this Location l)
        {
            CreateVideoPlayer(
                l.name,
                l.video,
                ref l.videoPlayer,
                l.videoURL,
                l.streamingAssetsVidPath,
                l.videoLocationType,
                l.loopVideo,
                false);
        }

        private static void CreateVideoPlayer(
            string objName,
            VideoClip video,
            ref VideoPlayer videoPlayer,
            string videoURL,
            string streamingAssetsVidPath,
            VideoLocationType videoLocationType,
            bool loopVideo,
            bool playAtStart)
        {
            if (videoPlayer == null)
            {
                videoPlayer = new GameObject().AddComponent<VideoPlayer>();
            }
            videoPlayer.playOnAwake = playAtStart;
            videoPlayer.isLooping = loopVideo;

            switch (videoLocationType)
            {
                case VideoLocationType.Local:
                    if (video == null)
                        Debug.LogError(objName + ": Video is not specified!");
                    
                    videoPlayer.source = VideoSource.VideoClip;
                    videoPlayer.clip = video;
                    videoPlayer.name = $"Video Player ({video.name})";
                    break;

                case VideoLocationType.StreamingAssets:
                    videoPlayer.source = VideoSource.Url;
                    videoPlayer.url = Path.Combine(Application.streamingAssetsPath, streamingAssetsVidPath);
                    videoPlayer.name = $"Video Player (StreamingAssets/{streamingAssetsVidPath})";

                    if (string.IsNullOrWhiteSpace(streamingAssetsVidPath))
                        Debug.LogError(objName + ": Streaming assets video path is empty!");
                    break;

                case VideoLocationType.URL:
                    videoPlayer.source = VideoSource.Url;
                    videoPlayer.url = videoURL;
                    videoPlayer.name = $"Video Player ({videoURL})";

                    if (string.IsNullOrWhiteSpace(videoURL))
                        Debug.LogError(objName + ": Video URL is empty!");
                    break;
            }
        }

        public static void GoToFirstFrame(this VideoPlayer videoPlayer)
        {
            videoPlayer.frame = 0;
        }
        
        public static void SetupRenderTexture(this VideoPlayer videoPlayer, ref RenderTexture renderTexture)
        {
            Texture tex = videoPlayer.texture;
            int width = tex.width;
            int height = tex.height;

            // Make new renderTexture if none there yet or size changed
            if (renderTexture == null || width != renderTexture.width || height != renderTexture.height)
            {
                if (renderTexture != null)
                {
                    renderTexture.DiscardContents();
                    renderTexture.Release();
                }
                renderTexture = new RenderTexture(width, height, 1);
                renderTexture.Create();
            }
        }
        
        public static void GetVideoSize(VideoLocationType videoLocationType, object caller, VideoClip video, VideoPlayer videoPlayer, UnityAction<int, int> callback, UnityAction<UnityAction> setVidPreparedCallback)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                // Get size of video clip
                if (video != null && videoLocationType == VideoLocationType.Local)
                    callback.Invoke((int)video.width, (int)video.height);
                // Location video preview size
                else if (caller is Location location)
                    location.RequestVideoPreview(texture => callback.Invoke(texture.width, texture.height));
                // VideoPoint video preview size
                else if (caller is VideoPoint point)
                    point.RequestVideoPreview(texture => callback.Invoke(texture.width, texture.height));
                // VideoPanel video preview size
                else if (caller is VideoPanel panel)
                    panel.RequestVideoPreview(texture => callback.Invoke(texture.width, texture.height));
                return;
            }
#endif
            switch (videoLocationType)
            {
                case VideoLocationType.Local:
                    if (video == null)
                        Debug.LogError("There is no video to play!");

                    callback.Invoke((int)video.width, (int)video.height);
                    break;

                case VideoLocationType.StreamingAssets:
                case VideoLocationType.URL:
                    void CallbackTextureSize()
                    {
                        Texture tex = videoPlayer.texture;
                        callback.Invoke(tex.width, tex.height);
                    }

                    if (videoPlayer.isPrepared)
                        CallbackTextureSize();
                    else
                        setVidPreparedCallback(CallbackTextureSize);
                    break;
            }
        }

        public static void SetupVideoPanel(
            Transform panel, 
            ref VideoController controller, 
            ref VideoUIController uiController,
            VideoPlayer videoPlayer,
            UnityAction<Texture2D> SetTex,
            UnityAction SetupLoadedVideo,
            UnityAction<Vector2> UpdateSize,
            UnityAction<UnityAction<int, int>> GetVideoSize,
            bool videoUI,
            bool videoUIAudioVolume,
            bool videoUILoopButton)
        {
            controller = panel.GetComponent<VideoController>();
            uiController = panel.GetComponent<VideoUIController>();

            // Set loading texture while video is loading
            if (Application.isPlaying)
            {
                if (!videoPlayer.isPrepared)
                {
                    SetTex(Texture2D.grayTexture);

                    // When video loaded
                    controller.OnVideoLoaded.AddListener(SetupLoadedVideo);
                }
                else
                {
                    // When video is already loaded
                    SetupLoadedVideo();
                }
            }

            GetVideoSize((width, height) => UpdateSize(new Vector2(width, height)));

            // Enable and initialize videoController
            panel.gameObject.SetActive(true);
            if (videoUI)
                uiController.EnableUI(videoPlayer, videoUIAudioVolume, videoUILoopButton);
            else
            {
                uiController.DisableUI();
                controller.Init(videoPlayer);
            }
        }
    }
}
