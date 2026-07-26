using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.XR.Interaction.Toolkit;

public class FaceObject : MonoBehaviour
{
    [Header("Assign the Main Camera (HMD)")]
    public Transform xrCamera;

    [Header("Enable only when you want to lock the player's facing direction")]
    public bool compensateRotation = true;

    private float lastRigYaw;

    void LateUpdate()
    {
        if (!compensateRotation || xrCamera == null)
            return;

        float currentRigYaw = transform.eulerAngles.y;

        // How much the Timeline rotated the rig this frame
        float rigDelta = Mathf.DeltaAngle(lastRigYaw, currentRigYaw);

        if (Mathf.Abs(rigDelta) > 0.01f)
        {
            // Remove the headset's local yaw offset
            float cameraYaw = xrCamera.localEulerAngles.y;

            transform.Rotate(0f, -cameraYaw, 0f, Space.Self);
        }

        lastRigYaw = transform.eulerAngles.y;
    }

    public void RecenterFacing()
    {
        if (xrCamera == null)
            return;

        transform.Rotate(0f, -xrCamera.localEulerAngles.y, (float)Space.Self);
    }

}
