using UnityEngine;
using System.Collections;

public class ResultsSceneManager : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("Results scene... returning to MainMenu in 2s");
        StartCoroutine(ReturnToMenuAfterDelay());
    }

    private IEnumerator ReturnToMenuAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        SceneFlow.LoadScene(SceneFlow.MAIN_MENU);
    }
}
