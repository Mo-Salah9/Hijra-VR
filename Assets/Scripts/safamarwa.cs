using UnityEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.Video;

[System.Serializable]
public class PathPointData2
{
    public Transform point;
    public Vector3 rotation;
    public CanvasGroup ui;
    public AudioClip audioClip;
    public float stopDuration;
}   
public class safamarwa : MonoBehaviour
{
    public GameObject vo;
    public VideoPlayer vp;
    public GameObject player;
    public Image image1, image2, image3;
    public PathPointData2[] pathPoints;
    public GameObject charc , close;
    public float moveSpeed = 2f; // units per second — rename in Inspector too
    private List<InputDevice> devices = new List<InputDevice>();
    void Start()
    {
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);

        //player.transform.rotation = new Quaternion(0f, 0.793353379f, 0, 0.60876143f);
        //StartCoroutine(MovePlayerAlongLocalPath());
        player.transform.position = new Vector3(-11.1199999f, -5.4000001f, 99.4599991f);
        StartCoroutine(safamarwai());
    }

    public IEnumerator safamarwai()
    {
        player.transform.DOMove(new Vector3(-11.1199999f, -5.4000001f, 116f), 2);

        AudioManager.Instance.PlayAlone("safa1");
        yield return new WaitForSeconds(8f);

        AudioManager.Instance.PlayAlone("a");
        yield return new WaitForSeconds(18);

        player.transform.DORotateQuaternion(new Quaternion(0, -0.592927158f, 0, 0.805256128f), 1f);
        yield return new WaitForSeconds(1f);

        AudioManager.Instance.PlayAlone("safa3");

        player.transform.DOMove(new Vector3(-41.0999985f, -5.4000001f, 116f), 2f);
        yield return new WaitForSeconds(2f);

        player.transform.DORotateQuaternion(new Quaternion(0, 1, 0, 0), 1f);
        yield return new WaitForSeconds(1f);

        // ✅ WAIT until path movement finishes
        yield return StartCoroutine(MovePlayerAlongLocalPath());

        // ✅ THEN play video
        yield return new WaitForSeconds(3f);
        AudioManager.Instance.PlayAlone("Closure");
        yield return new WaitForSeconds(20f);
        charc.SetActive(false);
        vo.SetActive(true);
        vp.Play();
        yield return new WaitForSeconds(7f);
        vo.SetActive(false);
        close.SetActive(true);
        yield return new WaitForSeconds(13);
        AudioManager.Instance.PlayAlone("fmusic");





    }


    public IEnumerator enumerator()
    {
        AudioManager.Instance.PlayAlone("k1");
        yield return new WaitForSeconds(10f);
        image1.GetComponent<CanvasGroup>().DOFade(1, 2);
        yield return new WaitForSeconds(10f);
        player.transform.DORotate(new Vector3(0, 175, 0), 1f);
        image1.GetComponent<CanvasGroup>().DOFade(0, 2);
        AudioManager.Instance.PlayAlone("k2");
        image2.GetComponent<CanvasGroup>().DOFade(1, 2);
        yield return new WaitForSeconds(13f);
        image2.GetComponent<CanvasGroup>().DOFade(0, 2);
        yield return MovePlayerAlongLocalPath();
       
        //yield return new WaitForSeconds(10);
        //vo.SetActive(false);

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
            PathPointData2 data = pathPoints[i];
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
    }
}
