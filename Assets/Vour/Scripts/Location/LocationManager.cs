using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CrizGames.Vour
{
    public class LocationManager : MonoBehaviour
    {
        private static LocationManager instance;
        
        private readonly int _alpha = Shader.PropertyToID("_Alpha");
        
        [SerializeField] private Material blinkMat;
        [SerializeField] private Transform blinkMesh;
        [SerializeField] private Image blinkPanel;
        [Space] 
        [SerializeField] private GameObject transitionUIDesktop;
        [SerializeField] private GameObject transitionUIVR;
        [SerializeField] private GameObject loadingTextDesktop;
        [SerializeField] private GameObject loadingTextVR;
        [Space] 
        public MainVideoUIController videoUI;
        [SerializeField] private GameObject videoUIDesktop;
        [SerializeField] private GameObject videoUIVR;
        [Space] 
        [SerializeField] private LocationView emptyLoc;
        [SerializeField] private LocationView imageLoc;
        [SerializeField] private LocationView image360Loc;
        [SerializeField] private LocationView videoLoc;
        [SerializeField] private LocationView video360Loc;
        [SerializeField] private LocationView sceneLoc;
        [Space]  
        public Location startLocation;
        
        public UnityEvent<Location> onLocationEnter;
        public UnityEvent<Location> onLocationExit;

        private Location[] Locations => _locationsCache ??= FindObjectsByType<Location>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        private Location[] _locationsCache;

        private LocationView _currentLocationView;

        private IEnumerator Start()
        {
            if (startLocation == null)
            {
                Debug.LogError("Start location is not assigned.", this);
                yield break;
            }
            
            if (PlayerBase.Instance == null)
            {
                Debug.LogError("Could not find a player game object. You have to add one to your scene!");
                yield break;
            }

            instance = this;

            SetBlinkState(1f);

            DeactivateLocationViews();
            DeactivateLocations();

            InitAllLocations(out var hasVideoLocations);

            var playerMode = Utils.GetCurrentPlayerMode();
            SetTransitionUI(playerMode);
            SetupVideoUI();

            yield return ActivateStartLocation();
            
            PlayBlinkAnimation(0f);
            yield break;

            void InitAllLocations(out bool foundVideoLocation)
            {
                foundVideoLocation = false;
                foreach (var location in Locations)
                {
                    location.Init();

                    if (location.locationType.IsVideo())
                        foundVideoLocation = true;
                }
            }

            void SetupVideoUI()
            {
                videoUI.Init();
                videoUI.gameObject.SetActive(false);
                
                SetVideoUI(playerMode);
            }

            IEnumerator ActivateStartLocation()
            {
                startLocation.SetData();
                SetLocationViewActive(startLocation, true);
                startLocation.SetActive(true);
                _currentLocationView = GetLocationView(startLocation);
                
                PlayerBase.Instance.OnNewLocation(startLocation);
            
                if (hasVideoLocations)
                    startLocation.PreloadLinkedVideos();

                // Wait until start location is ready
                if(!_currentLocationView.IsReady) 
                    yield return new WaitUntil(() => _currentLocationView.IsReady);
            }
        }

        /// <summary>
        /// Deactivate all locations views.
        /// </summary>
        public void DeactivateLocationViews()
        {
            foreach (var locationView in new [] {emptyLoc, imageLoc, image360Loc, videoLoc, video360Loc, sceneLoc})
                locationView.gameObject.SetActive(false);
        }

        /// <summary>
        /// Deactivate all locations in the scene.
        /// </summary>
        public void DeactivateLocations()
        {
            DeactivateLocationsExcept(null);
        }

        /// <summary>
        /// Deactivate all locations in the scene except the specified location.
        /// </summary>
        public void DeactivateLocationsExcept(Location locationRemainsActive)
        {
            var invalidateCache = false;
            
            foreach (var location in Locations)
            {
                // location can become null when it got deleted
                if (location == null)
                    invalidateCache = true;
                else if (location != locationRemainsActive)
                    location.SetActive(false);
            }

            if (invalidateCache)
                _locationsCache = null;
        }

        /// <summary>
        /// Get the corresponding location view for the specified location based on the location type.
        /// </summary>
        public LocationView GetLocationView(Location location) => location.locationType switch
        {
            LocationType.Image => location.displayType switch
            {
                LocationDisplayType._2D or LocationDisplayType._3D => imageLoc,
                _ => image360Loc
            },
            LocationType.Video => location.displayType switch
            {
                LocationDisplayType._2D or LocationDisplayType._3D => videoLoc,
                _ => video360Loc
            },
            LocationType.Scene => sceneLoc,
            _ => emptyLoc
        };
        
        /// <summary>
        /// Disable the old location view and activate the new one for the specified location.
        /// </summary>
        public void SwitchCurrentLocationView(Location newLocation)
        {
            var newView = GetLocationView(newLocation);
            
            // Disable current/old location view
            if (_currentLocationView != null)
                SetLocationViewActive(_currentLocationView, false);
            SetLocationViewActive(newView, true);
            
            // Call OnNewLocation when the new location is not the same as the old one.
            // The new location can be the same as the old one when location properties are changed in play mode.
            if (_currentLocationView != newView && PlayerBase.Instance != null)
                PlayerBase.Instance.OnNewLocation(newLocation);
            
            _currentLocationView = newView;
        }

        /// <summary>
        /// Transfer data of the specified location to the corresponding location view.
        /// </summary>
        public void SetDataToLocationView(Location location)
            => GetLocationView(location).SetData(location);

        /// <summary>
        /// Set the location view of the specified location active or inactive.
        /// </summary>
        private void SetLocationViewActive(Location location, bool active)
            => SetLocationViewActive(GetLocationView(location), active);

        /// <summary>
        /// Set the location view active or inactive.
        /// </summary>
        private void SetLocationViewActive(LocationView locationView, bool active)
            => locationView.gameObject.SetActive(active);


        /// <summary>
        /// Set VR or desktop video UI active.
        /// </summary>
        public void SetVideoUI(PlayerMode playerMode)
        {
            videoUIDesktop.SetActive(playerMode == PlayerMode.Desktop);
            videoUIVR.SetActive(playerMode == PlayerMode.VR);
        }

        /// <summary>
        /// Set VR or desktop transition UI active.
        /// </summary>
        public void SetTransitionUI(PlayerMode playerMode)
        {
            transitionUIDesktop.SetActive(playerMode == PlayerMode.Desktop);
            transitionUIVR.SetActive(playerMode == PlayerMode.VR);
        }
        
        /// <summary>
        /// Enables or disables the loading text from the transition UI for Desktop and VR.
        /// Because the correct transition UI got set active on startup, only the correct text will be displayed.
        /// </summary>
        public void ShowLoadingUI(bool show)
        {
            loadingTextDesktop.SetActive(show);
            loadingTextVR.SetActive(show);
        }

        /// <summary>
        /// Play a blink animation which fades to the targetAlpha.
        /// Used for location transitions/teleportation.
        /// </summary>
        /// <param name="targetAlpha">0 for transparent, 1 for black</param>
        /// <returns>You can use the coroutine to wait for the animation to finish.</returns>
        public Coroutine PlayBlinkAnimation(float targetAlpha)
        {
            // Stop previous animation if there is one
            StopCoroutine(nameof(DoPlayBlinkAnimation));
            
            // Start actual animation and return its coroutine
            return StartCoroutine(DoPlayBlinkAnimation(targetAlpha));
        }

        /// <summary>
        /// The actual blink animation code
        /// </summary>
        private IEnumerator DoPlayBlinkAnimation(float targetAlpha)
        {
            // Move blink mesh to player in case the player moved
            if (PlayerBase.Instance != null)
                blinkMesh.position = PlayerBase.Instance.transform.position;

            const float animDuration = 0.1f;
            var time = 0f;
            
            var startAlpha = blinkMat.GetFloat(_alpha);
            var currentColor = new Color(0, 0, 0, startAlpha);
            
            while (time < animDuration)
            {
                var alpha = currentColor.a = Mathf.Lerp(startAlpha, targetAlpha, time / animDuration);
                blinkMat.SetFloat(_alpha, alpha);
                blinkPanel.color = currentColor;

                time += Time.deltaTime;
                yield return null;
            }

            SetBlinkState(targetAlpha);
        }

        private void SetBlinkState(float alpha)
        {
            blinkMat.SetFloat(_alpha, alpha);
            blinkPanel.color = new Color(0, 0, 0, alpha);
        }

        private void OnApplicationQuit()
        {
            blinkMat.SetFloat(_alpha, 0);
        }

        public static LocationManager GetManager()
        {
            if (instance != null)
                return instance;

            var manager = FindAnyObjectByType<LocationManager>();
            if (!manager)
            {
#if UNITY_EDITOR
                if (Application.isPlaying || FindAnyObjectByType<Location>() != null)
#endif
                    Debug.LogError("Could not find a Location Manager. You have to add one to your scene!");
                return null;
            }

            return manager;
        }
        
#if UNITY_EDITOR
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void RefreshManagerInstance()
        {
            var manager = GetManager();
            if (manager != null)
                manager._locationsCache = null;
        }
#endif
    }
}