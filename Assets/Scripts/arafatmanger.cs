using UnityEngine;
using DG.Tweening;
using System.Collections;

public class arafatmanger : MonoBehaviour
{
    public GameObject player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player.transform.position = new Vector3(-183.029999f, -2.64512157f, -458.99585f);
        player.transform.rotation = new Quaternion(0, -0.878400385f, 0, 0.477925479f);
        StartCoroutine(enumerator());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator enumerator()
    {
        yield return new WaitForSeconds(4f);
        player.transform.DOMove(new Vector3(-197.399994f, 1, -468.399994f), 4f);
        yield return new WaitForSeconds(4f);
        player.transform.DORotateQuaternion ( new Quaternion(0, -1, 0, 0), 1f);
        yield return new WaitForSeconds(1f);
        player.transform.DOMove(new Vector3(-198.399994f, 11.23f, -495.799988f), 6f);
        yield return new WaitForSeconds(6f);
        player.transform.DORotateQuaternion(new Quaternion(0, -0.815885365f, 0, 0.578213751f), 1f);
        yield return new WaitForSeconds(1f);
        player.transform.DOMove(new Vector3(-217.199997f, 14.6999998f, -499.899994f), 4f);
        yield return new WaitForSeconds(4f);
        player.transform.DORotateQuaternion(new Quaternion(0, -0.736097634f, 0, 0.676875412f), 1f);
        yield return new WaitForSeconds(1f);





        yield return null;  
    }

}
