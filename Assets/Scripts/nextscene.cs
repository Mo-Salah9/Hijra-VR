using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using System.Collections.Generic;

public class nextscene : MonoBehaviour
{
    private List<InputDevice> devices = new List<InputDevice>();
    private void Update()
    {
        // 🔁 Re-fetch if lost / not initialized
        if (devices == null || devices.Count == 0)
        {
            InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);
        }

        if (devices.Count > 0)
        {
            bool aButtonPressed;

            if (devices[0].TryGetFeatureValue(CommonUsages.primaryButton, out aButtonPressed) && aButtonPressed)
            {
                Debug.Log("A button pressed");
                LoadScene();
            }
        }
    }
    private void Start()
    {
        StartCoroutine("enumerator");
    }
    private IEnumerator enumerator()
    {
        yield return new WaitForSeconds(75f);
        SceneManager.LoadScene(1);
    }

    void LoadScene()
    {
        SceneManager.LoadScene(0);
    }
}
