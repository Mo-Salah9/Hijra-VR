using UnityEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using System.Collections.Generic;
using UnityEngine.Video;

public class lastscenemanger : MonoBehaviour
{
    public AudioSource audioSource1;
    public AudioSource audioSource2;
    public Animator animator1;
    public Canvas canvas;
    void Start()
    {
        audioSource1.volume = 0.06f;
        StartCoroutine(enumerator());
    }

    void Update()
    {

    }

    private IEnumerator enumerator()
    {
        yield return new WaitForSeconds(21);

        // Lerp volume from 0.02 to 1 over 2 seconds
        audioSource1.DOFade(1f, 2f);

        yield return new WaitForSeconds(39.01f);
        animator1.SetBool("stop", true);
        canvas.gameObject.SetActive(true);
        yield return null;
    }
}