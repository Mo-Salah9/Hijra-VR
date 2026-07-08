using UnityEngine.EventSystems;

namespace CrizGames.Vour
{
    public class WebXRDesktopPlayer : DesktopPlayer
    {
#if ENABLE_INPUT_SYSTEM
        protected override void Update()
        {
            // Don't do any interaction as that is already handled by the WebXR player prefab
            // Only do hover & lookaround
            
            if (EventSystem.current != null)
                pointerOverUI = EventSystem.current.IsPointerOverGameObject();

            UpdateInputState(out var mousePos, out var canLook, out _);
            
            var ray = cam.ScreenPointToRay(mousePos);
            var interactable = RaycastInteractable(ray.origin, ray.direction);
            UpdateInteractable(interactable);

            if (canLook)
                Look();
        }
#endif
    }
}
