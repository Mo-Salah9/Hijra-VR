using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
public class nextscene : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine("enumerator");
    }
    private IEnumerator enumerator()
    {
        yield return new WaitForSeconds(75f);
        SceneManager.LoadScene(1);
    }
}
