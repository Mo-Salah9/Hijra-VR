using UnityEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;  // ✅ add this
using UnityEngine.XR;
using System.Collections.Generic;
using UnityEngine.Video;

public class Aliscenemanager : MonoBehaviour
{
    VideoPlayer p;  // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine("ie");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator ie()
    {
        yield return new WaitForSeconds (1f) ;
        p=GameObject.FindAnyObjectByType<VideoPlayer> ();
        p.playbackSpeed=0;
        AudioManager.Instance.Play("0");
        AudioManager.Instance.Play("1");
        yield return new WaitForSeconds(9f);
        p.playbackSpeed = 1;
        yield return new WaitForSeconds(12f);
        AudioManager.Instance.PlayAlone("2");
        yield return new WaitForSeconds(25.91f);
        p.playbackSpeed = 0;
        AudioManager.Instance.Play("0");

        AudioManager.Instance.Play("3");
        yield return new WaitForSeconds(18);
        p.playbackSpeed = 1;
        AudioManager.Instance.Play("5");
        yield return new WaitForSeconds(24);
        AudioManager.Instance.Play("4");


    }
}
