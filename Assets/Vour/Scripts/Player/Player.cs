using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrizGames.Vour
{
    public enum PlayerMode
    {
        VR,
        Desktop // (or mobile)
    }
    
    public class Player : MonoBehaviour
    {
        public enum PlayerPlatformType
        {
            VR,
            WebXR,
            Desktop
        }

        public GameObject DesktopPlayer;
        public GameObject OpenXRPlayer;
        public GameObject WebXRPlayer;

        [Tooltip("When not in a 3D environment, the player should be centered to display the locations correctly.")]
        public bool CenterCamera = true;

        private void Awake()
        {
            var player = Instantiate(GetPlayerPrefab(), transform.position, transform.rotation).GetComponent<PlayerBase>();
            player.centerCamera = CenterCamera;
            player.Init();

            Destroy(gameObject);
        }

        private GameObject GetPlayerPrefab()
        {
            switch (GetPlayerPlatform())
            {
                case PlayerPlatformType.VR:
                    Debug.Log("VR Mode (OpenXR)");
                    return OpenXRPlayer;

                case PlayerPlatformType.WebXR:
                    Debug.Log("VR Mode (WebXR)");
                    return WebXRPlayer;

                case PlayerPlatformType.Desktop:
                default:
                    Debug.Log("Desktop/Mobile Mode");
                    return DesktopPlayer;
            }
        }

        private static PlayerPlatformType GetPlayerPlatform()
        {
#if VOUR_WEBXR && UNITY_WEBGL
            if (Application.platform == RuntimePlatform.WebGLPlayer)
                return PlayerPlatformType.WebXR;
#endif
            
            if (Utils.InVR())
                return PlayerPlatformType.VR;

            return PlayerPlatformType.Desktop;
        }
    }
}