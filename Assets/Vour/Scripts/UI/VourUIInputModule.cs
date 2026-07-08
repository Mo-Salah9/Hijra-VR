using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif
#if VOUR_XRI
using UnityEngine.XR.Interaction.Toolkit.UI;
#endif

namespace CrizGames.Vour
{
#if VOUR_XRI
    public class VourUIInputModule : XRUIInputModule {}
#elif ENABLE_INPUT_SYSTEM
    public class VourUIInputModule : InputSystemUIInputModule {}
#else
    public class VourUIInputModule : MonoBehaviour {}
#endif
}