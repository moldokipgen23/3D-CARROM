using UnityEngine;

public class FoulDetectorTest : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("FoulDetector test starting...");

        FoulDetector foulDetector = GetComponent<FoulDetector>();
        if (foulDetector == null)
        {
            Debug.LogError("FoulDetector not found on this GameObject");
            return;
        }

        Debug.Log("FoulDetector found");
        Debug.Log($"Foul timeout: {foulDetector.foulTimeout} seconds");

        if (foulDetector.turnManager != null)
        {
            Debug.Log("TurnManager reference set");
        }
        else
        {
            Debug.LogWarning("No TurnManager reference set");
        }

        if (foulDetector.scoreManager != null)
        {
            Debug.Log("ScoreManager reference set");
        }
        else
        {
            Debug.LogWarning("No ScoreManager reference set");
        }

        Debug.Log("FoulDetector test setup complete");
    }
}