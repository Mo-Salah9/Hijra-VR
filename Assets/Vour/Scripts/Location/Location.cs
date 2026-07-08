using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

namespace CrizGames.Vour
{
    public enum LocationType
    {
        Empty,
        Image,
        Video,
        Scene
    }

    public enum LocationDisplayType
    {
        _2D,
        _3D,
        _180,
        _180_3D,
        _360,
        _3603D,
    }

    public enum VideoLocationType
    {
        Local,
        StreamingAssets,
        URL
    }

    public enum Layout3D
    {
        OverUnder,
        SideBySide
    }
    
    public class Location : MonoBehaviour
    {
        public LocationType locationType;
        public LocationDisplayType displayType;
        public Layout3D layout3D;
        public Texture2D texture;
        public VideoPlayer videoPlayer;
        public VideoClip video;
        public string videoURL;
        [StreamingAssetsPath(Utils.VideoFileExtensionFilter)]
        public string streamingAssetsVidPath;
        
        [Tooltip("Set your own loading text to be displayed when teleporting to this video location.")]
        public string loadingTextOverride;
        public VideoLocationType videoLocationType;

        [Tooltip("Scale the image/video to fullscreen height.")]
        public bool scaleToFullscreen = false;

        public bool lockCamera = false;
        
        public bool loopVideo = true;
        public bool videoUI = false;
        public bool videoUIAudioVolume = true;
        public bool videoUILoopButton = true;
        [Range(0f, 1f)] public float videoVolume = 1f;

        public SceneReference scene;

        public Vector3 rotOffset;

        public UnityEvent onEnter;
        public UnityEvent onExit;

        private TeleportPoint[] _teleportPoints;

        // Cache LocationManager because in OnDisable the manager can sometimes be not found when exiting the game
        // which would cause an error log to appear
        private LocationManager Manager => _manager ??= LocationManager.GetManager();
        private LocationManager _manager;

        /// <summary>
        /// Init is called from LocationManager because the GameObject of Location is inactive
        /// </summary>
        public void Init()
        {
            // Create a video player object for the video location
            if (locationType.IsVideo())
                this.CreateVideoPlayer();

            _teleportPoints = GetComponentsInChildren<TeleportPoint>(true);
        }

        /// <summary>
        /// Set data of this location to the corresponding LocationBase
        /// </summary>
        public void SetData()
        {
            LoadingUI.LoadingTextOverride = loadingTextOverride;
            Manager.SetDataToLocationView(this);
        }

        /// <summary>
        /// Preload video locations linked to this location via teleport points
        /// </summary>
        public void PreloadLinkedVideos()
        {
            if (!VourSettings.Instance.enableVideoPreloading)
                return;
            
            foreach (var t in _teleportPoints)
            {
                if (t.targetLocation != null && t.targetLocation.locationType.IsVideo() && !t.targetLocation.videoPlayer.isPrepared)
                    t.targetLocation.videoPlayer.Prepare();
            }
        }
        
        /// <summary>
        /// Unloads all video locations the player didn't jump to
        /// </summary>
        public void UnloadLinkedVideos(Location nextLocation)
        {
            // No need to check if it is disabled
            if (!VourSettings.Instance.enableVideoPreloading)
            {
                if (locationType.IsVideo()) 
                    videoPlayer.Stop();
                return;
            }
            
            // Unload all connected video locations that are not nextLocation
            foreach (var t in _teleportPoints)
            {
                if (t.targetLocation != null && t.targetLocation.locationType.IsVideo() && t.targetLocation != nextLocation)
                    t.targetLocation.videoPlayer.Stop();
            }

            if (!locationType.IsVideo()) 
                return;
            
            var nextHasLinkToThisLocation = nextLocation._teleportPoints.Any(t => t.targetLocation == this);
            // Don't unload video if connected
            if (nextHasLinkToThisLocation)
            {
                videoPlayer.Pause();
                videoPlayer.time = 0;
            }
            // Unload video if not connected
            else
                videoPlayer.Stop();
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        private void OnEnable()
        {
            onEnter?.Invoke();
            
            if (Manager != null)
                Manager.onLocationEnter?.Invoke(this);
        }

        private void OnDisable()
        {
            onExit?.Invoke();
            
            if (Manager != null)
                Manager.onLocationExit?.Invoke(this);
        }

        private void OnApplicationQuit()
        {
            if (videoPlayer != null && videoPlayer.isPlaying)
                videoPlayer.Stop();
        }
    }
}