using UnityEngine;
using UnityEngine.Events;
#if VOUR_XRI
using UnityEngine.XR.Interaction.Toolkit.Interactables;
#endif

namespace CrizGames.Vour
{
#if VOUR_XRI
    public class VourXRInteractable : XRBaseInteractable
    {
        protected override void Awake()
        {
            base.Awake();

            allowGazeInteraction = true;
            allowGazeSelect = true;

            var i = GetComponent<IInteractable>() ?? GetComponentInParent<IInteractable>();
            SetEvent(hoverEntered, i.OnPointerHoverEnter);
            SetEvent(hoverExited, i.OnPointerHoverExit);
            SetEvent(selectEntered, i.Interact); // For Hands
            SetEvent(activated, i.Interact); // For Controllers
        }

        private static void SetEvent<T>(UnityEvent<T> unityEvent, UnityAction action)
        {
            unityEvent.AddListener(_ => action());
        }
    }
#else
    public class VourXRInteractable : MonoBehaviour {}
#endif
}