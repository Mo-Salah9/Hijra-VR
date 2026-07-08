using System;
using UnityEngine;
using UnityEngine.Video;

namespace CrizGames.Vour.Editor
{
    public static class VideoPreviewRequests
    {
        public class VideoPreviewRequestEventArgs : EventArgs
        {
            public VideoLocationType videoLocationType;
            public VideoClip video;
            public string streamingAssetsVidPath;
            public string videoURL;
            public Action<Texture2D> callback;

            public VideoPreviewRequestEventArgs(VideoLocationType videoLocationType, VideoClip video, string streamingAssetsVidPath, string videoURL, Action<Texture2D> callback)
            {
                this.videoLocationType = videoLocationType;
                this.video = video;
                this.streamingAssetsVidPath = streamingAssetsVidPath;
                this.videoURL = videoURL;
                this.callback = callback;
            }
        }
        
        public static event EventHandler<VideoPreviewRequestEventArgs> OnVideoPreviewRequested;
        
        public static void RequestVideoPreview(this Location location, Action<Texture2D> callback)
            => RequestVideoPreview(location.videoLocationType, location.video, location.streamingAssetsVidPath, location.videoURL, callback);
        
        public static void RequestVideoPreview(this VideoPoint point, Action<Texture2D> callback)
            => RequestVideoPreview(point.videoLocationType, point.video, point.streamingAssetsVidPath, point.videoURL, callback);

        public static void RequestVideoPreview(this VideoPanel panel, Action<Texture2D> callback)
            => RequestVideoPreview(panel.videoLocationType, panel.video, panel.streamingAssetsVidPath, panel.videoURL, callback);
        
        public static void RequestVideoPreview(VideoLocationType videoLocationType, VideoClip video, string streamingAssetsVidPath, string videoURL, Action<Texture2D> callback)
        {
            OnVideoPreviewRequested?.Invoke(null, new VideoPreviewRequestEventArgs(videoLocationType, video, streamingAssetsVidPath, videoURL, callback));
        }
    }
}