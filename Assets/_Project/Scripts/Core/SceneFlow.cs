using UnityEngine;

public static class SceneFlow
{
    public const string BOOT = "Boot";
    public const string MAIN_MENU = "MainMenu";
    public const string GAME = "Game";
    public const string RESULTS = "Results";

    public static void LoadScene(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogError("Scene name cannot be null or empty");
            return;
        }

        if (!UnityEngine.SceneManagement.SceneManager.GetSceneByName(name).IsValid())
        {
            Debug.LogError($"Scene '{name}' not found in build settings");
            return;
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene(name);
    }
}