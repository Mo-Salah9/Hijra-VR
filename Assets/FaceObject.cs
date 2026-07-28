using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Assertions;
//using Unity.VisualScripting;

public class FaceObject : MonoBehaviour
{
    public Transform head; // camera
    public Transform origin; // xr rig
    public Transform startTransform;
    private Animator animator;
    private void Start()
    {
        animator = GetComponent<Animator>();
        animator.enabled = false;
        Invoke(nameof(GoToStartView), 0.01f); // startTarget
    }

    public void GoToStartView()
    {
        GoToSpecficView(startTransform);
    }
    public void GoToStartViewDelayed()
    {
        Invoke(nameof(GoToStartView),2);
    }
    private void GoToSpecficView(Transform viewTransfrom)
    {
        animator.enabled=true;  
        Vector3 offset = head.position - origin.position;
        //offset.y = 0;
        origin.position = viewTransfrom.position - offset;

        Vector3 targetFwd = viewTransfrom.forward;
        Vector3 cameraFwd = head.forward;

        float angle = Vector3.SignedAngle(cameraFwd, targetFwd, Vector3.up);

        origin.RotateAround(head.position, Vector3.up, angle);
    }

}
