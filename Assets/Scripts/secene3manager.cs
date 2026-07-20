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
        yield return null;
    }
}

