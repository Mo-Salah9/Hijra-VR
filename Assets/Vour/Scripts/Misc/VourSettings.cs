using UnityEngine;
#if VOUR_SETTINGS
using Unity.XR.CoreUtils;
#endif

namespace CrizGames.Vour
{
#if VOUR_SETTINGS
    [ScriptableSettingsPath("Assets/Vour/Settings")]
    public class VourSettings : ScriptableSettings<VourSettings>
    {
#else
    public class VourSettings : ScriptableObject
    {
        public static readonly VourSettings Instance;
#endif
        [Header("Default Placeables")]
        public GameObject defaultTeleportPoint;
        public GameObject defaultInfoPoint;
        public GameObject defaultInfoPanel;
        public GameObject defaultVideoPoint;
        public GameObject defaultVideoPanel;
        
        [Header("Info Panels")]
        public GameObject infoPanelTextOnly;
        public GameObject infoPanelLeftImage;
        public GameObject infoPanelRightImage;
        
        [Header("Misc")]
        public GameObject locationManagerPrefab;
        public GameObject playerPrefab;
        public GameObject locationPrefab;
        
        [Header("Video Settings")]
        [Tooltip("Should video locations connected to the current location be preloaded?")]
        public bool enableVideoPreloading = true;
        
        [Tooltip("The loading text displayed when it is not overriden by a Teleport Point")]
        public string defaultLoadingText = "Loading";

        [Header("Editor Settings")]
        [Tooltip("With video preview enabled, a video location displays a frame of the video. " +
                 "When disabled, the location displays only a gray color. " +
                 "Video preview must be installed for this feature to work. ")]
        public bool enableVideoPreview = true;
    }
}