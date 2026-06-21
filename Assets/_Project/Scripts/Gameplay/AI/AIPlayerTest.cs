using UnityEngine;

public class AIPlayerTest : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("AIPlayer test starting...");

        AIPlayer aiPlayer = GetComponent<AIPlayer>();
        if (aiPlayer == null)
        {
            Debug.LogError("AIPlayer not found on this GameObject");
            return;
        }

        Debug.Log("AIPlayer found");
        Debug.Log($"Difficulty: {aiPlayer.difficulty}");
        Debug.Log($"Thinking delay: {aiPlayer.thinkingDelay} seconds");

        if (aiPlayer.strikerController != null)
        {
            Debug.Log("StrikerController reference set");
        }
        else
        {
            Debug.LogWarning("No StrikerController reference set");
        }

        if (aiPlayer.turnManager != null)
        {
            Debug.Log("TurnManager reference set");
        }
        else
        {
            Debug.LogWarning("No TurnManager reference set");
        }

        if (aiPlayer.scoreManager != null)
        {
            Debug.Log("ScoreManager reference set");
        }
        else
        {
            Debug.LogWarning("No ScoreManager reference set");
        }

        Debug.Log("AIPlayer test setup complete");
    }
}