using UnityEngine;
#if VOUR_XRI
using UnityEngine.XR.Interaction.Toolkit.UI;
#endif

namespace CrizGames.Vour
{
#if VOUR_XRI
    public class VourTrackedDeviceGraphicRaycaster : TrackedDeviceGraphicRaycaster {}
#else
    public class VourTrackedDeviceGraphicRaycaster : MonoBehaviour {}
#endif
}