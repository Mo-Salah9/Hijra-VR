using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UnityEngine.Video;
using UnityEngine.Networking;
#if VOUR_FFMPEG
using FFMpegCore;
using FFMpegCore.Pipes;
#endif

namespace CrizGames.Vour.Editor
{
    [InitializeOnLoad]
    public static class VideoPreview
    {
        private static readonly HashSet<string> CurrentSnapshotProcesses = new();

        private static readonly Dictionary<string, List<Action<Texture2D>>> QueuedCallbacks = new();
        
        private static readonly Dictionary<string, Texture2D> CachedTextures = new();

        private static readonly VideoPreviewNotificationOverlay NotificationOverlay = new();

        static VideoPreview()
        {
            VideoPreviewRequests.OnVideoPreviewRequested += OnOnVideoPreviewRequested;
            
#if VOUR_FFMPEG
            GlobalFFOptions.Current.BinaryFolder = FFmpegInstaller.FFmpegBinaryFolder;
#endif
        }

        private static void OnOnVideoPreviewRequested(object sender, VideoPreviewRequests.VideoPreviewRequestEventArgs e)
            => GetVideoPreview(e.videoLocationType, e.video, e.streamingAssetsVidPath, e.videoURL, e.callback);

        public static void GetVideoPreview(this Location location, Action<Texture2D> callback)
            => GetVideoPreview(location.videoLocationType, location.video, location.streamingAssetsVidPath, location.videoURL, callback);

        public static void GetVideoPreview(this VideoPoint point, Action<Texture2D> callback)
            => GetVideoPreview(point.videoLocationType, point.video, point.streamingAssetsVidPath, point.videoURL, callback);

        public static void GetVideoPreview(this VideoPanel panel, Action<Texture2D> callback)
            => GetVideoPreview(panel.videoLocationType, panel.video, panel.streamingAssetsVidPath, panel.videoURL, callback);

#if VOUR_FFMPEG
        private static async void GetVideoPreview(VideoLocationType videoLocationType, VideoClip video, string streamingAssetsVidPath, string videoURL, Action<Texture2D> callback)
        {
            if (!FFmpegInstaller.IsSupported || !FFmpegInstaller.IsFFmpegInstalled || !VourSettings.Instance.enableVideoPreview)
            {
                callback(EditorTools.GrayTextureOpaque);
                return;
            }
            
            if (!TryGetVideoPath(out var videoPath))
            {
                callback(EditorTools.GrayTextureOpaque);
                return;
            }
            
            var texturePath = GetTexturePath();
            
            // Wait for snapshot already running for this texture
            if (IsSnapshotAlreadyRunning(texturePath))
            {
                lock (QueuedCallbacks)
                {
                    if (QueuedCallbacks.TryGetValue(texturePath, out var callbacksList))
                        callbacksList.Add(callback);
                    else
                        QueuedCallbacks.Add(texturePath, new List<Action<Texture2D>> { callback });
                }
                
                return;
            }
            
            // Load from cached texture file
            if (File.Exists(texturePath))
            {
                var texture = LoadTextureCached(texturePath);
                callback(texture);
            }
            // Create snapshot process
            else
            {
                // Snapshot texture
                var texture = await SnapshotTexture2D(videoLocationType, videoPath, texturePath);
                
                if (texture == null)
                    texture = EditorTools.GrayTextureOpaque;
                
                callback(texture);

                // Execute all queued callbacks for the same texture
                lock (QueuedCallbacks)
                {
                    if (QueuedCallbacks.TryGetValue(texturePath, out var callbacksList))
                    {
                        foreach (var queuedCallback in callbacksList)
                            queuedCallback(texture);

                        QueuedCallbacks.Remove(texturePath);
                    }
                }
            }
            return;
            
            bool TryGetVideoPath(out string videoPath)
            {
                var isUrlVideo = videoLocationType == VideoLocationType.URL;
            
                videoPath = GetVideoPath(videoLocationType, video, streamingAssetsVidPath, videoURL);

                return !string.IsNullOrEmpty(videoPath) && (isUrlVideo || File.Exists(videoPath));
            }
            
            string GetTexturePath()
            {
                var fileName = videoLocationType switch
                {
                    VideoLocationType.URL => UnityWebRequest.EscapeURL(videoPath),
                    _ => UnityWebRequest.EscapeURL(Path.GetRelativePath(Application.dataPath, videoPath))
                };
                
                return Path.Combine(Application.temporaryCachePath, $"videopreview-{fileName}.jpg");
            }
        }
#else
        // Removed async in method definition so that there won't be a compiler warning
        private static void GetVideoPreview(VideoLocationType videoLocationType, VideoClip video, string streamingAssetsVidPath, string videoURL, Action<Texture2D> callback)
        {
            callback(EditorTools.GrayTextureOpaque);
            return;
        }
#endif

#if VOUR_FFMPEG
        private static bool IsSnapshotAlreadyRunning(string texturePath)
        {
            lock (CurrentSnapshotProcesses)
                return CurrentSnapshotProcesses.Contains(texturePath);
        }

        private static Texture2D LoadTextureCached(string texturePath)
        {
            if (CachedTextures.TryGetValue(texturePath, out var texture))
            {
                if (texture == null)
                    CachedTextures.Remove(texturePath);
                else
                    return texture;
            }
            
            texture = new Texture2D(1, 1, TextureFormat.RGB24, false);
            texture.LoadImage(File.ReadAllBytes(texturePath), true);
            
            CachedTextures.Add(texturePath, texture);
            
            return texture;
        }

        private static async Task<Texture2D> SnapshotTexture2D(VideoLocationType videoLocationType, string videoPath, string texturePath)
        {
            lock (CurrentSnapshotProcesses)
            {
                CurrentSnapshotProcesses.Add(texturePath);

                // Video loading notification
                if (CurrentSnapshotProcesses.Count == 1)
                    NotificationOverlay.Show();
                NotificationOverlay.SetCurrentLoadingVideo(Path.GetFileNameWithoutExtension(videoPath));
            }
            
            // Analyze video and cancel if an exception was thrown
            var (success, mediaInfo, videoSize) = await TryAnalyzeVideo(videoLocationType, videoPath);
            if (!success)
                return null;
            
            // Set video size to max 2048px height to prevent too long loading times
            var maxVideoWidth = (int)(2048 * (float)videoSize.Width / videoSize.Height);
            videoSize = new Size(Mathf.Min(videoSize.Width, maxVideoWidth), Mathf.Min(videoSize.Height, 2048));
            
            var bytes = await SnapshotToBytesAsync(videoPath, mediaInfo, videoSize, TimeSpan.FromSeconds(2));

            var texture = CreateTexture2D(texturePath, bytes);

            lock (CurrentSnapshotProcesses)
            {
                CurrentSnapshotProcesses.Remove(texturePath);

                // Video loading notification
                if (CurrentSnapshotProcesses.Count == 0)
                {
                    NotificationOverlay.NoVideoLoading();
                    NotificationOverlay.Hide();
                }
            }

            return texture;
        }

        private static async Task<(bool success, IMediaAnalysis mediaInfo, Size videoSize)> TryAnalyzeVideo(VideoLocationType videoLocationType, string path)
        {
            try
            {
                var mediaInfo = videoLocationType switch
                {
                    // Try to convert to URI and analyze remote video
                    VideoLocationType.URL => Uri.TryCreate(path, UriKind.Absolute, out var videoUri) ? await FFProbe.AnalyseAsync(videoUri) : null,
                    // Analyze local video file
                    _ => await FFProbe.AnalyseAsync(path)
                };

                var videoStream = mediaInfo?.PrimaryVideoStream;

                var videoSize = default(Size);
                
                // Cancel if there is no video stream (or no mediaInfo)
                if (videoStream == null)
                {
                    return (false, mediaInfo, videoSize);
                }
            
                videoSize = new Size(videoStream.Width, videoStream.Height);

                return (true, mediaInfo, videoSize);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return (false, null, default);
            }
        }

        private static Texture2D CreateTexture2D(string texturePath, byte[] data)
        {
            var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            texture.LoadImage(data);
                
            // Save raw texture data with metadata to temp cache for later reuse
            if (texture != null)
            {
                File.WriteAllBytes(texturePath, texture.EncodeToJPG());
            }
                
            return texture;
        }

        private static string GetVideoPath(VideoLocationType videoLocationType, VideoClip video, string streamingAssetsVidPath, string videoURL) => videoLocationType switch
        {
            VideoLocationType.Local when video != null => Path.Combine(Application.dataPath, "..", video.originalPath),
            VideoLocationType.StreamingAssets => Path.Combine(Application.streamingAssetsPath, streamingAssetsVidPath),
            VideoLocationType.URL => videoURL,
            _ => null
        };
        
        private static async Task<byte[]> SnapshotToBytesAsync(string path, IMediaAnalysis mediaInfo, Size size, TimeSpan captureTime)
        {
            var videoStream = mediaInfo.PrimaryVideoStream;
            if (videoStream == null)
                return null;
            
            Texture.allowThreadedTextureCreation = true; // For a potential small speedup
            
            var (arguments, outputOptions) = SnapshotArgumentBuilder.BuildSnapshotArguments(path, mediaInfo, size, captureTime);
            try
            {
                using var ms = new MemoryStream();
            
                await arguments
                    .OutputToPipe(new StreamPipeSink(ms), options => outputOptions(options.ForceFormat("rawvideo")))
                    .ProcessAsynchronously();

                return ms.ToArray();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return null;
            }
        }
#endif
    }
}