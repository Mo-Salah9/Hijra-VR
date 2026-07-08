using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace CrizGames.Vour
{
    public static class Utils
    {
        // https://docs.unity3d.com/6000.0/Documentation/Manual/VideoSources-FileCompatibility.html
        // https://developer.android.com/media/platform/supported-formats#video-codecs
        public const string VideoFileExtensionFilter = "mp4,mov,vp8,asf,avi,dv,m4v,mpg,mpeg,ogv,webm,wmv,mkv";
        
        /// <summary>
        /// Is some kind of VR system active?
        /// </summary>
        public static bool InVR()
        {
#if VOUR_XR
            var displaySubsystems = new List<XRDisplaySubsystem>();
            SubsystemManager.GetSubsystems(displaySubsystems);
            // If there are xr displays detected = VR is on
            return displaySubsystems.Count > 0;
#else
            return false;
#endif
        }
        
        /// <summary>
        /// Get player's current mode (desktop or VR).
        /// </summary>
        public static PlayerMode GetCurrentPlayerMode()
            => InVR() ? PlayerMode.VR : PlayerMode.Desktop;

        /// <summary>
        /// Is it some kind of image location?
        /// </summary>
        public static bool IsImage(this LocationType type) => type == LocationType.Image;

        /// <summary>
        /// Is it some kind of video location?
        /// </summary>
        public static bool IsVideo(this LocationType type) => type == LocationType.Video;

        /// <summary>
        /// Is it some kind of 360 location?
        /// </summary>
        public static bool Is360(this LocationDisplayType type)
        {
            switch (type)
            {
                case LocationDisplayType._360:
                case LocationDisplayType._3603D:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Is it some kind of 180 location?
        /// </summary>
        public static bool Is180(this LocationDisplayType type)
        {
            switch (type)
            {
                case LocationDisplayType._180:
                case LocationDisplayType._180_3D:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Is it some kind of 2D location?
        /// </summary>
        public static bool Is2D(this LocationDisplayType type)
        {
            switch (type)
            {
                case LocationDisplayType._2D:
                case LocationDisplayType._180:
                case LocationDisplayType._360:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Is it some kind of 3D location?
        /// </summary>
        public static bool Is3D(this LocationDisplayType type)
        {
            switch (type)
            {
                case LocationDisplayType._3D:
                case LocationDisplayType._180_3D:
                case LocationDisplayType._3603D:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Find child of transform by tag
        /// </summary>
        public static Transform FindChildByTag(this Transform parent, string tag)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.CompareTag(tag))
                    return child;
            }
            return null;
        }
    }
}