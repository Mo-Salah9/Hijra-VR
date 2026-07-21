using UnityEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;  // ✅ add this
using UnityEngine.XR;
using System.Collections.Generic;
using UnityEngine.Video;

public class soraqamanger : MonoBehaviour
{
    public Animator animator1;
    public Animator animator2;
    public GameObject hole1, hole2 , hole3;
    //public GameObject idle2,idle3;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine("soraqa");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator soraqa()
    {
        AudioManager.Instance.PlayAlone("0");
        yield return new WaitForSeconds(6.30f);
        animator1.SetBool("stop", true);
        animator2.SetBool("stop", true);
        yield return new WaitForSeconds(5f);
        //animator1.SetBool("standup", true);
        //animator2.SetBool("standup", true);
        hole1.SetActive(false);
        hole2.SetActive(true);
        yield return new WaitForSeconds(6f);
        hole2.SetActive(false);

        hole3.SetActive(true);
        AudioManager.Instance.PlayAlone("1");
        animator2.gameObject.SetActive(false);
        //idle2.gameObject.SetActive(true);
        //idle2.gameObject.transform.localPosition = new Vector3(0.0299999993f ,0f, 1.13f);
        yield return new WaitForSeconds(3f);
        animator1.SetBool("fall", true);
        //idle2.GetComponent<Animator>().SetBool("fall",true);

        yield return new WaitForSeconds(4f);
        animator1.SetBool("standagain", true);
        yield return new WaitForSeconds(0.5f);

        //idle2.gameObject.SetActive(false);

        //idle3.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
       
        yield return new WaitForSeconds(30f);
        animator1.SetBool("getback", true);
        //idle3.GetComponent<Animator>().SetBool("getback", true);
        yield return new WaitForSeconds(17f);
        SceneManager.LoadScene(4);
        yield return null;
    }
}
