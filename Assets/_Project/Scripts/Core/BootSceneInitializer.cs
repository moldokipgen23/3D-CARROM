using UnityEngine;

public class BootSceneInitializer : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("Boot scene initializing...");
        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneFlow.MAIN_MENU);
    }
}
