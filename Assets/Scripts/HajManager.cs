using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.XR;


public class HajManager : MonoBehaviour
{
    private List<InputDevice> devices = new List<InputDevice>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject player;
    public Image image1, image2, image3, imagem,logo;
    public GameObject Videobject;
    public VideoPlayer clip;
    public AudioSource audioSource;
    //public void step1()
    //{
    //    player.transform.position = new Vector3(0.365471601f, 0.295673728f, -0.584015489f);
    //    player.transform.rotation = new Quaternion(0, 0.707106829f, 0, 0.707106829f);
    private void Start()
    {
        //imagem.gameObject.SetActive(false);
        StartCoroutine(step1());
           audioSource.gameObject.SetActive(false);

}

//}
public void step2()
    {

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
    public IEnumerator step1()
    {
        //player.transform.position = new Vector3(0.365471601f, 0.295673728f, -0.584015489f);
        //player.transform.rotation = new Quaternion(0, 0.707106829f, 0, 0.707106829f);
        logo.gameObject.SetActive(true);
        yield return new WaitForSeconds(1);
        AudioManager.Instance.PlayAlone("Quraan");
        yield return new WaitForSeconds(15);
        logo.gameObject.SetActive(false);
        image1.GetComponent<CanvasGroup>().DOFade(1, 2);
        audioSource.gameObject.SetActive(true);
        AudioManager.Instance.PlayAlone("1");
        image1.GetComponent<CanvasGroup>().DOFade(1, 2);
        yield return new WaitForSeconds(20);
        image1.GetComponent<CanvasGroup>().alpha = 0;
        AudioManager.Instance.PlayAlone("2");
          image2.GetComponent<CanvasGroup>().DOFade(1, 2);
        yield return new WaitForSeconds(8);
        image2.GetComponent<CanvasGroup>().alpha = 0;

        image3.GetComponent<CanvasGroup>().DOFade(1, 2);
        ////AudioManager.Instance.PlayAlone("3");
        yield return new WaitForSeconds(10);
        audioSource.volume = 0.5f;
        yield return new WaitForSeconds(4);

        //image3.GetComponent<CanvasGroup>().alpha = 0;
        //Videobject.SetActive(true);
        //clip.Play();
        audioSource.volume = 1;
        yield return new WaitForSeconds(5);
        SceneManager.LoadScene(1);
    }
}
