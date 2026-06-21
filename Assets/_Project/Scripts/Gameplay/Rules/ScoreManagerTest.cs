using UnityEngine;

public class ScoreManagerTest : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("ScoreManager test starting...");

        ScoreManager scoreManager = GetComponent<ScoreManager>();
        if (scoreManager == null)
        {
            Debug.LogError("ScoreManager not found on this GameObject");
            return;
        }

        Debug.Log("ScoreManager found");
        Debug.Log($"White player score: {scoreManager.GetPlayerScore(1)}");
        Debug.Log($"Black player score: {scoreManager.GetPlayerScore(2)}");
        Debug.Log($"Queen player score: {scoreManager.GetPlayerScore(3)}");

        Debug.Log($"Win conditions - White: {scoreManager.whiteCoinsToWin}, Black: {scoreManager.blackCoinsToWin}, Queen: {scoreManager.queenToWin}");

        if (scoreManager.turnManager != null)
        {
            Debug.Log("TurnManager reference set");
        }
        else
        {
            Debug.LogWarning("No TurnManager reference set");
        }

        if (scoreManager.gameHUD != null)
        {
            Debug.Log("GameHUD reference set");
        }
        else
        {
            Debug.LogWarning("No GameHUD reference set");
        }

        Debug.Log("ScoreManager test setup complete");
    }
}