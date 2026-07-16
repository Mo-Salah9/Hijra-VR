using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ssss : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine("playsounds");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private IEnumerator playsounds()
    {
        AudioManager.Instance.PlayAlone("s1");
        yield return new WaitForSeconds(33f);
        AudioManager.Instance.PlayAlone("s2");


        yield return null;
    }
}
