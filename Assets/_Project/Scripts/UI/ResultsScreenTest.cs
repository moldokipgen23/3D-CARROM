using UnityEngine;

public class ResultsScreenTest : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("ResultsScreen test starting...");

        ResultsScreen resultsScreen = GetComponent<ResultsScreen>();
        if (resultsScreen == null)
        {
            Debug.LogError("ResultsScreen not found on this GameObject");
            return;
        }

        Debug.Log("ResultsScreen found");

        if (resultsScreen.resultsPanel != null)
        {
            Debug.Log("Results panel reference set");
        }
        else
        {
            Debug.LogWarning("No results panel reference set");
        }

        if (resultsScreen.playAgainButton != null)
        {
            Debug.Log("Play Again button reference set");
        }
        else
        {
            Debug.LogWarning("No Play Again button reference set");
        }

        if (resultsScreen.mainMenuButton != null)
        {
            Debug.Log("Main Menu button reference set");
        }
        else
        {
            Debug.LogWarning("No Main Menu button reference set");
        }

        if (resultsScreen.scoreManager != null)
        {
            Debug.Log("ScoreManager reference set");
        }
        else
        {
            Debug.LogWarning("No ScoreManager reference set");
        }

        Debug.Log("ResultsScreen test setup complete");
    }
}