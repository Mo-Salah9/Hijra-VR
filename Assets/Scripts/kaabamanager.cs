using UnityEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;  // ✅ add this
using UnityEngine.XR;
using System.Collections.Generic;

[System.Serializable]
public class PathPointData
{
    public Transform point;
    public Vector3 rotation;
    public CanvasGroup ui;
    public AudioClip audioClip;
    public float stopDuration;
}

public class kaabamanager : MonoBehaviour
{
    private List<InputDevice> devices = new List<InputDevice>();

    public GameObject player;
    public Image image1, image2, image3;
    public PathPointData[] pathPoints;
    public float moveSpeed = 2f;

    void Start()
    {
        player.transform.position = new Vector3(-81.8000031F, -15.49438f, 201.100006F);
        player.transform.rotation = new Quaternion(0, 0.673657656F, 0, 0.739043534F);
        StartCoroutine(Entermasjed());
    }

    public IEnumerator enumerator()
    {
        AudioManager.Instance.PlayAlone("k2");
        //yield return new WaitForSeconds(10f);
        //image1.GetComponent<CanvasGroup>().DOFade(1, 2);
        //yield return new WaitForSeconds(10f);
        player.transform.DORotate(new Vector3(0, 175, 0), 1f);
        image1.GetComponent<CanvasGroup>().DOFade(0, 2);
        AudioManager.Instance.PlayAlone("k2");
        image2.GetComponent<CanvasGroup>().DOFade(1, 2);
        yield return new WaitForSeconds(13f);
        image2.GetComponent<CanvasGroup>().DOFade(0, 2);

        yield return MovePlayerAlongLocalPath();

        // ✅ After path is fully complete, wait 20 seconds then load Scene 1
        yield return new WaitForSeconds(30f);
        AudioManager.Instance.PlayAlone("gf");
        yield return new WaitForSeconds(18f);
        SceneManager.LoadScene(2);
    }
    private IEnumerator Entermasjed()
    {
        AudioManager.Instance.PlayAlone("e1");
        yield return new WaitForSeconds(2f);
         player.transform.DOMove (new Vector3(-25.3999996f, -15.49438f, 215.800003f), 25f,false);
        yield return new WaitForSeconds(28f);
        StartCoroutine(enumerator());

    }
    void Update()
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
    void LoadScene()
    {
        SceneManager.LoadScene("SampleScene");
    }
    private IEnumerator MovePlayerAlongLocalPath()
    {
        if (pathPoints == null || pathPoints.Length == 0)
            yield break;

        float playerLocalY = player.transform.localPosition.y;

        for (int i = 0; i < pathPoints.Length; i++)
        {
            PathPointData data = pathPoints[i];
            if (data.point == null) continue;

            // — Compute target local position —
            Vector3 localPoint = player.transform.parent != null
                ? player.transform.parent.InverseTransformPoint(data.point.position)
                : data.point.position;
            localPoint.y = playerLocalY;

            // — Duration based on distance and constant speed —
            float distance = Vector3.Distance(player.transform.localPosition, localPoint);
            float segmentDuration = distance / moveSpeed;

            Tween moveTween = player.transform
                .DOLocalMove(localPoint, segmentDuration)
                .SetEase(Ease.Linear);

            yield return moveTween.WaitForCompletion();

            // — Rotation —
            player.transform.DORotate(data.rotation, 0.5f);

            // — Audio —
            if (data.audioClip != null)
                AudioManager.Instance.PlayAlone(data.audioClip.name);

            // — UI Fade In —
            if (data.ui != null)
                data.ui.DOFade(1, 0.5f);

            // — Stop Duration —
            if (data.stopDuration > 0f)
                yield return new WaitForSeconds(data.stopDuration);

            // — UI Fade Out —
            if (data.ui != null)
                data.ui.DOFade(0, 0.5f);
        }

        // MovePlayerAlongLocalPath is done here — control returns to enumerator()
    }
}