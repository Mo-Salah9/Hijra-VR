using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CrizGames.Vour
{
    [SelectionBase]
    public class TeleportPoint : MonoBehaviour, IInteractable
    {
        public Location targetLocation;

        [Tooltip("Resets the player's rotation to the rotation of the teleport point indicated by the blue arrow.")]
        public bool resetPlayerRotation = false;
        
        [Tooltip("Called during teleport when both locations aren't visible.")]
        public UnityEvent onTeleport;
        
        private SpriteRenderer sprite;

        protected Color hoverColor = new Color(0.6f, 0.6f, 0.6f);
        private Color startColor;

        protected Location parentLocation;

        public enum TeleportType
        {
            SwitchLocation,
            UpdatePosition
        }
        public TeleportType teleportType;

        public virtual void Awake()
        {
            sprite = GetComponentInChildren<SpriteRenderer>();
            parentLocation = GetComponentInParent<Location>();

            if (sprite != null)
                startColor = sprite.color;

            if (teleportType == TeleportType.SwitchLocation && targetLocation == null)
                Debug.LogError("Target location is not assigned. Without a target location, the teleport point won't work.", this);
        }

        public void Interact()
        {
            Teleport();
        }

        public void Teleport()
        {
            StartCoroutine(TeleportIE());
        }

        private IEnumerator TeleportIE()
        {
            var manager = LocationManager.GetManager();
            
            yield return manager.PlayBlinkAnimation(1f);
            
            onTeleport?.Invoke();
            
            if (resetPlayerRotation)
                PlayerBase.Instance.ResetRotation(transform.eulerAngles);
            
            if(teleportType == TeleportType.SwitchLocation)
            {
                // Unload videos
                parentLocation.UnloadLinkedVideos(targetLocation);
				
                // Switch location view
                manager.SwitchCurrentLocationView(targetLocation);

                // Update location
                targetLocation.SetData();
                
                // Preload next videos
                targetLocation.PreloadLinkedVideos();

                // Wait until target location is ready
                LocationView l = LocationManager.GetManager().GetLocationView(targetLocation);
                if(!l.IsReady) 
                    yield return new WaitUntil(() => l.IsReady);

                // Set locations
                parentLocation.SetActive(false);
                targetLocation.SetActive(true);
            }
            else // TeleportType.UpdatePosition
            {
                var player = PlayerBase.Instance;
                var playerT = player.transform;
                var cam = player.cam.transform;
                
                // Move player to position of teleport point
                var camOffset = playerT.position - cam.position;
                playerT.position = transform.position + new Vector3(camOffset.x, 0, camOffset.z);
                
                yield return new WaitForSeconds(0.2f);
            }

            yield return manager.PlayBlinkAnimation(0f);
        }

        public void OnPointerHoverEnter()
        {
            if (sprite != null)
                sprite.color = hoverColor;
        }

        public void OnPointerHoverExit()
        {
            if (sprite != null)
                sprite.color = startColor;
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!resetPlayerRotation)
                return;

            var pos = transform.position;
            var dir = transform.forward * 0.5f;
            
            // Draw arrow pointing towards z-axis, useful for knowing where the player looks towards after rotation reset
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(pos, dir);
            
            var rightSide = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 180 + 20, 0) * new Vector3(0, 0, 0.15f);
            var leftSide = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 180 - 20, 0) * new Vector3(0, 0, 0.15f);
            Gizmos.DrawRay(pos + dir, rightSide);
            Gizmos.DrawRay(pos + dir, leftSide);
        }
#endif
    }
}