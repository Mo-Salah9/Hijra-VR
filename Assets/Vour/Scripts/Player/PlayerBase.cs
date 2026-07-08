using UnityEngine;
using UnityEngine.Serialization;

namespace CrizGames.Vour
{
    public abstract class PlayerBase : MonoBehaviour
    {
        public static PlayerBase Instance;

        protected IInteractable CurrentInteractable;

        [HideInInspector] public Camera cam;

        [Header("Player Settings")]
        [FormerlySerializedAs("centerCam")]
        public bool centerCamera = true;

        private float startCamYPos;

        protected bool initialized = false;

        protected bool pointerOverUI = false;

        private void Start()
        {
            Init();
        }

        public virtual void Init()
        {
            if (initialized)
                return;

            cam = GetComponentInChildren<Camera>();
            startCamYPos = cam.transform.localPosition.y;
            SetCenterCam(centerCamera);

            Instance = this;
            initialized = true;
        }

        public void ResetRotation() => ResetRotation(Vector3.zero);
        public abstract void ResetRotation(Vector3 newEulerAngles);

        public virtual void SetCenterCam(bool center)
        {
            if (center)
                cam.transform.position = Vector3.zero;
            else
                cam.transform.localPosition = new Vector3(cam.transform.localPosition.x, startCamYPos, cam.transform.localPosition.z);
        }

        public virtual void OnNewLocation(Location newLocation)
        {
            // Reset player rotation if this is a non-360 location and not empty
            if (Application.isPlaying && !newLocation.displayType.Is360() && newLocation.locationType != LocationType.Empty)
                ResetRotation();
        }

        protected virtual void OnDestroy()
        {
            if(Instance == this)
                Instance = null;
        }
    }
}