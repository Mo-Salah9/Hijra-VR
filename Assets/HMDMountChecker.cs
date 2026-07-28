using UnityEngine;
using UnityEngine.XR;
using System;
using System.Collections.Generic;
using TMPro;


public class HMDMountChecker : MonoBehaviour
{
    public static event Action<bool> OnUserPresenceChanged;
    private InputDevice hmd;
    private bool lastPresence = true;
    public FaceObject faceObject;
 
    void Start()
    {
        // Get HMD device
        var headDevices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.Head, headDevices);
        if (headDevices.Count > 0)
            hmd = headDevices[0];
    }
    private void OnEnable()
    {
        OnUserPresenceChanged += HandleUserPresenceChanged;
    }

    private void OnDisable()
    {
        OnUserPresenceChanged -= HandleUserPresenceChanged;
    }

    private void HandleUserPresenceChanged(bool isPresent)
    {
        if (isPresent)
        {
            faceObject.GoToStartViewDelayed();
        }
    }
    void Update()
    {
        if (!hmd.isValid) return;

        if (hmd.TryGetFeatureValue(CommonUsages.userPresence, out bool userPresent))
        {
            if (userPresent != lastPresence)
            {
                lastPresence = userPresent;
                OnUserPresenceChanged?.Invoke(userPresent);
            }
        }
    }
}
