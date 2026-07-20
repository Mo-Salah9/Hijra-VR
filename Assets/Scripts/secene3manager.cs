using UnityEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;  // ✅ add this
using UnityEngine.XR;
using System.Collections.Generic;
using UnityEngine.Video;


public class secene3manager : MonoBehaviour
{
    public Animator animator1;
    public  Animator animator2;
    public GameObject horse;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine("playscene");
    }

    // Update is called once per frame
    void Update()
    {
        

    }


    private IEnumerator playscene()
    {
        AudioManager.Instance.PlayAlone("0");
        yield return new WaitForSeconds(14);
        AudioManager.Instance.PlayAlone("1");
        yield return new WaitForSeconds(24);
        AudioManager.Instance.PlayAlone("2");
        yield return new WaitForSeconds(7);
        animator1.SetBool("go", true);
        animator2.SetBool("go", true);
        yield return new WaitForSeconds(7);
        horse.SetActive(false);
        yield return null;
    }
}

