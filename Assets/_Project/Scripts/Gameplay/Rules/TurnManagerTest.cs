using UnityEngine;

public class TurnManagerTest : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("TurnManager test starting...");

        TurnManager turnManager = GetComponent<TurnManager>();
        if (turnManager == null)
        {
            Debug.LogError("TurnManager not found on this GameObject");
            return;
        }

        Debug.Log("TurnManager found");
        Debug.Log($"Max turns: {turnManager.maxTurns}");
        Debug.Log($"Shot timeout: {turnManager.shotTimeout} seconds");

        int currentPlayer = turnManager.GetCurrentPlayer();
        Debug.Log($"Current player: {currentPlayer}");

        int turnCount = turnManager.GetTurnCount();
        Debug.Log($"Turn count: {turnCount}");

        bool isShotInProgress = turnManager.IsShotInProgress();
        Debug.Log($"Is shot in progress: {isShotInProgress}");

        Debug.Log("TurnManager test setup complete");
    }
}