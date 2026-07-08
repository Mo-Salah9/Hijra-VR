using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;
#if UNITY_EDITOR
using CrizGames.Vour.Editor;
#endif

namespace CrizGames.Vour
{
    [ExecuteAlways]
    public class VideoPanel : Panel
    {
        [SerializeField] private RawImage videoImg;
        private RenderTexture renderTexture;

        private VideoController controller;
        private VideoUIController uiController;

        public VideoPlayer videoPlayer;
        public VideoClip video;
        public string videoURL;
        [StreamingAssetsPath(Utils.VideoFileExtensionFilter)]
        public string streamingAssetsVidPath;
        public VideoLocationType videoLocationType;
        public bool playAtStart = true;
        public bool loopVideo = true;
        public bool videoUI = true;
        public bool videoUIAudioVolume = true;
        public bool videoUILoopButton = true;
        [Range(0f, 1f)]
        public float videoVolume = 1f;

        private void Awake()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif
            panel.gameObject.SetActive(false);
            
            this.CreateVideoPlayer();
        }

        protected override void Start()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif
            base.Start();
        }

        public override void InitPanel()
        {
            VideoUtils.SetupVideoPanel(
                panel,
                ref controller,
                ref uiController,
                videoPlayer,
                SetTexture,
                SetupLoadedVideo,
                UpdateSize,
                GetVideoSize,
                videoUI,
                videoUIAudioVolume,
                videoUILoopButton);
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (UnityEditor.Selection.activeGameObject != gameObject)
                    this.RequestVideoPreview(SetTexture);
                return;
            }
#endif
            videoPlayer.Prepare();
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif
            if (videoPlayer != null)
                videoPlayer.Stop();

            if (renderTexture != null)
            {
                renderTexture.DiscardContents();
                renderTexture.Release();
                renderTexture = null;
            }
        }

        private void GetVideoSize(UnityAction<int, int> callback)
        {
            VideoUtils.GetVideoSize(videoLocationType, this, video, videoPlayer, callback, callbackTextureSize => controller.OnVideoLoaded.AddListener(callbackTextureSize));
        }

        private void SetupLoadedVideo()
        {
            videoPlayer.SetupRenderTexture(ref renderTexture);

            if (!playAtStart)
            {
                // Display first frame
                videoPlayer.GoToFirstFrame();
                videoPlayer.Pause();
                Graphics.CopyTexture(videoPlayer.texture, renderTexture);
            }

            SetTexture(renderTexture);

            controller.SetAudioVolume(videoVolume);
        }

        public void SetTexture(Texture tex)
        {
            videoImg.texture = tex;

            if (tex.GetType() == typeof(RenderTexture))
                videoPlayer.targetTexture = (RenderTexture)tex;
        }

        protected override Transform GetPanel()
        {
            return transform.GetChild(0);
        }
    }
}