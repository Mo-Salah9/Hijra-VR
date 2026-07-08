using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

namespace CrizGames.Vour
{
    public class DesktopPlayer : PlayerBase
    {
        [SerializeField] private float mouseSensitivity = 30;
        [SerializeField] private float yOffset = 1.7f;
        public bool canMoveCam = true;

        private Vector2 startPos;
        private Vector3 camRot;
        
        private bool canClick = false;

#if ENABLE_INPUT_SYSTEM
        private Mouse mouse => Mouse.current;
        private TouchControl primaryTouch => Touchscreen.current.primaryTouch;
#endif
        
        public override void Init()
        {
            base.Init();
            camRot = cam.transform.eulerAngles;
        }

#if ENABLE_INPUT_SYSTEM
        protected virtual void Update()
        {
            if (EventSystem.current != null)
                pointerOverUI = EventSystem.current.IsPointerOverGameObject();

            UpdateInputState(out var mousePos, out var canLook, out var clicked);
            
            // Get Interactable
            var ray = cam.ScreenPointToRay(mousePos);
            var interactable = RaycastInteractable(ray.origin, ray.direction);
            UpdateInteractable(interactable);

            // Interact
            if (clicked)
                interactable?.Interact();
            
            // Look
            if (canLook)
                Look();
        }
        
        protected IInteractable RaycastInteractable(Vector3 pos, Vector3 dir)
        {
            if (pointerOverUI)
                return null;

            if (Physics.Raycast(pos, dir, out RaycastHit hit))
                return hit.collider.GetComponentInParent<IInteractable>();

            return null;
        }
        
        protected void UpdateInteractable(IInteractable interactable)
        {
            if (interactable != null)
            {
                if (interactable != CurrentInteractable)
                    interactable.OnPointerHoverEnter();

                if (CurrentInteractable != null && interactable != CurrentInteractable)
                    CurrentInteractable?.OnPointerHoverExit();
            }
            else
            {
                CurrentInteractable?.OnPointerHoverExit();
            }
            CurrentInteractable = interactable;
        }

        protected void UpdateInputState(out Vector2 mousePos, out bool canLook, out bool clicked)
        {
            clicked = false;
            canLook = !canClick;
            
            mousePos = Application.isMobilePlatform switch
            {
                true => primaryTouch.position.ReadValue(),
                false => mouse.position.ReadValue()
            };
            
            var pointerDown = Application.isMobilePlatform switch
            {
                true => primaryTouch.press.wasPressedThisFrame,
                false => mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame
            };
            
            var pointerUp = Application.isMobilePlatform switch
            {
                true => primaryTouch.press.wasReleasedThisFrame,
                false => mouse.leftButton.wasReleasedThisFrame
            };
            
            // Start isClick state
            if (pointerDown)
            {
                startPos = cam.ScreenToViewportPoint(mousePos);
                canClick = true;
            }

            // Check if pointer went over threshold (-> canClick = false)
            if (canClick)
            {
                var pos = Application.isMobilePlatform switch
                {
                    true => primaryTouch.position.ReadValue(),
                    false => mouse.position.ReadValue()
                };
                canClick = Vector2.Distance(startPos, cam.ScreenToViewportPoint(pos)) < 0.02f;
            }

            // Set clicked state to true on pointer up when it is possible
            if (pointerUp && canClick)
            {
                clicked = true;
                canClick = false;
            }
        }

        protected void Look()
        {
            if (!canMoveCam)
                return;

            var press = Application.isMobilePlatform switch
            {
                true => primaryTouch.press.isPressed,
                false => mouse.middleButton.isPressed || mouse.rightButton.isPressed
            };

            if (press)
            {
                var delta = Application.isMobilePlatform switch
                {
                    true => primaryTouch.delta.ReadValue(),
                    false => mouse.delta.ReadValue()
                };
                
                var lookVec = new Vector3(delta.y, -delta.x) / Screen.dpi * mouseSensitivity;
                
                camRot += lookVec;
                camRot.x = Mathf.Clamp(camRot.x, -90f, 90f);
            }
            
            cam.transform.eulerAngles = camRot;
        }
#endif

        public override void OnNewLocation(Location newLocation)
        {
            base.OnNewLocation(newLocation);
            
            // Lock camera position when lockCamera for new location is enabled
            // and when it is not a 360 or 180 location.
            var is360or180 = newLocation.displayType.Is360() || newLocation.displayType.Is180();
            canMoveCam = !newLocation.lockCamera || is360or180;
        }

        public override void ResetRotation(Vector3 newEulerAngles)
        {
            cam.transform.eulerAngles = camRot = newEulerAngles;
        }

        public override void SetCenterCam(bool center)
        {
            base.SetCenterCam(center);
            if (!center)
                cam.transform.localPosition = new Vector3(0, yOffset, 0);
        }
    }
}