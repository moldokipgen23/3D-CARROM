using UnityEngine;

public class GameHUDTest : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("GameHUD test starting...");

        GameHUD gameHUD = GetComponent<GameHUD>();
        if (gameHUD == null)
        {
            Debug.LogError("GameHUD not found on this GameObject");
            return;
        }

        Debug.Log("GameHUD found");

        if (gameHUD.player1UI != null)
        {
            Debug.Log("Player 1 UI reference set");
        }
        else
        {
            Debug.LogWarning("No Player 1 UI reference set");
        }

        if (gameHUD.player2UI != null)
        {
            Debug.Log("Player 2 UI reference set");
        }
        else
        {
            Debug.LogWarning("No Player 2 UI reference set");
        }

        if (gameHUD.turnManager != null)
        {
            Debug.Log("TurnManager reference set");
        }
        else
        {
            Debug.LogWarning("No TurnManager reference set");
        }

        if (gameHUD.scoreManager != null)
        {
            Debug.Log("ScoreManager reference set");
        }
        else
        {
            Debug.LogWarning("No ScoreManager reference set");
        }

        Debug.Log("GameHUD test setup complete");
    }
}