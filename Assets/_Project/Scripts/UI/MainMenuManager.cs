using UnityEngine;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("MainMenu loading... auto-transition to Game in 1s");
        StartCoroutine(LoadGameAfterDelay());
    }

    private IEnumerator LoadGameAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        SceneFlow.LoadScene(SceneFlow.GAME);
    }
}
