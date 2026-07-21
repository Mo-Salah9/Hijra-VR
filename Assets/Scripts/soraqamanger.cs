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
    public GameObject idle2,idle3;
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
        animator1.SetBool("standup", true);
        animator2.SetBool("standup", true);
        animator2.gameObject.SetActive(false);
        idle2.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        animator1.SetBool("fall", true);
        idle2.GetComponent<Animator>().SetBool("fall",true);

        yield return new WaitForSeconds(4f);
        animator1.SetBool("standagain", true);
        idle2.gameObject.SetActive(false);
        idle3.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        AudioManager.Instance.PlayAlone("1");

        yield return null;
    }
}
