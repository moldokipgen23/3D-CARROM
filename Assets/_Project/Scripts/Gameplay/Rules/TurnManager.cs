using UnityEngine;
using System;
using System.Collections;

public class TurnManager : MonoBehaviour
{
    [Header("Game Settings")]
    public int maxTurns = 100;
    public float shotTimeout = 5f;
    
    [Header("Player References")]
    public GameObject player1;
    public GameObject player2;
    
    public event Action<int> OnTurnChanged;
    public event Action<int, int> OnScoreUpdated;
    
    private int currentPlayer = 1;
    private int turnCount = 0;
    private bool isShotInProgress = false;
    private float shotStartTime;
    
    private void Start()
    {
        InitializePlayers();
        SwitchTurn();
    }
    
    private void InitializePlayers()
    {
        if (player1 != null)
        {
            player1.SetActive(true);
        }
        if (player2 != null)
        {
            player2.SetActive(false);
        }
    }
    
    public void StartShot()
    {
        if (!isShotInProgress)
        {
            isShotInProgress = true;
            shotStartTime = Time.time;
            Debug.Log($"Player {currentPlayer}'s turn started");
        }
    }
    
    public void EndShot()
    {
        if (isShotInProgress)
        {
            isShotInProgress = false;
            CheckShotTimeout();
            StartCoroutine(WaitForPhysicsThenSwitch());
        }
    }

    private IEnumerator WaitForPhysicsThenSwitch()
    {
        yield return new WaitForFixedUpdate();

        const float sleepThreshold = 0.02f;
        const float maxWait = 8f;
        float waited = 0f;

        while (waited < maxWait)
        {
            bool allSleeping = true;

            foreach (Rigidbody rb in FindObjectsOfType<Rigidbody>())
            {
                if (rb.linearVelocity.magnitude > sleepThreshold)
                {
                    allSleeping = false;
                    break;
                }
            }

            if (allSleeping) break;
            waited += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        SwitchTurn();
    }
    
    private void CheckShotTimeout()
    {
        float shotDuration = Time.time - shotStartTime;
        if (shotDuration > shotTimeout)
        {
            Debug.LogWarning($"Player {currentPlayer}'s shot timed out after {shotDuration:F1} seconds");
            ApplyFoulPenalty(currentPlayer);
        }
    }
    
    public void SwitchTurn()
    {
        currentPlayer = (currentPlayer == 1) ? 2 : 1;
        turnCount++;
        
        if (player1 != null)
        {
            player1.SetActive(currentPlayer == 1);
        }
        if (player2 != null)
        {
            player2.SetActive(currentPlayer == 2);
        }
        
        OnTurnChanged?.Invoke(currentPlayer);
        Debug.Log($"Turn switched to Player {currentPlayer}");
    }
    
    private void ApplyFoulPenalty(int player)
    {
        Debug.LogWarning($"Player {player} fouled - applying penalty");
        ScoreManager.Instance?.ApplyFoulPenalty(player);
    }
    
    public int GetCurrentPlayer()
    {
        return currentPlayer;
    }
    
    public int GetTurnCount()
    {
        return turnCount;
    }
    
    public bool IsShotInProgress()
    {
        return isShotInProgress;
    }
}