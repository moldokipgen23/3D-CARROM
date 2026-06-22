using UnityEngine;
using System.Collections.Generic;

public class FoulDetector : MonoBehaviour
{
    [Header("Foul Detection Settings")]
    public float foulTimeout = 30f;
    public LayerMask coinLayer;
    
    [Header("References")]
    public TurnManager turnManager;
    public ScoreManager scoreManager;
    public CoinSpawner coinSpawner;
    
    private bool strikerPocketed = false;
    private bool anyCoinPocketed = false;
    private bool queenPocketed = false;
    private bool queenCovered = false;
    private float lastShotTime;
    private GameObject striker;
    
    // Track which coins were pocketed this turn
    private List<Coin> pocketedCoinsThisTurn = new List<Coin>();
    
    private void Start()
    {
        InitializeReferences();
        FindStriker();
    }
    
    private void InitializeReferences()
    {
        if (turnManager == null)
        {
            turnManager = FindObjectOfType<TurnManager>();
        }
        if (scoreManager == null)
        {
            scoreManager = FindObjectOfType<ScoreManager>();
        }
        if (coinSpawner == null)
        {
            coinSpawner = FindObjectOfType<CoinSpawner>();
        }
    }
    
    private void FindStriker()
    {
        GameObject[] strikers = GameObject.FindGameObjectsWithTag("Striker");
        if (strikers.Length > 0)
        {
            striker = strikers[0];
        }
        else
        {
            Debug.LogWarning("No striker found for foul detection");
        }
    }
    
    private void Update()
    {
        DetectFouls();
    }
    
    /// <summary>
    /// Call this when a shot is fired to reset tracking
    /// </summary>
    public void RegisterShot()
    {
        lastShotTime = Time.time;
        pocketedCoinsThisTurn.Clear();
        strikerPocketed = false;
        anyCoinPocketed = false;
        queenPocketed = false;
        queenCovered = false;
    }
    
    /// <summary>
    /// Call this when a coin is pocketed to track it
    /// </summary>
    public void RegisterPocketedCoin(Coin coin)
    {
        if (!pocketedCoinsThisTurn.Contains(coin))
        {
            pocketedCoinsThisTurn.Add(coin);
            
            if (coin.Type == CoinType.Queen)
            {
                queenPocketed = true;
            }
            else
            {
                anyCoinPocketed = true;
            }
        }
    }
    
    private void DetectFouls()
    {
        CheckStrikerPocketed();
        CheckNoCoinTouched();
        
        ApplyFoulsIfDetected();
    }
    
    private void CheckStrikerPocketed()
    {
        if (striker != null && striker.activeInHierarchy == false)
        {
            strikerPocketed = true;
            Debug.LogWarning("Foul: Striker pocketed");
        }
    }
    
    private void CheckNoCoinTouched()
    {
        float timeSinceLastShot = Time.time - lastShotTime;
        if (timeSinceLastShot > foulTimeout && !anyCoinPocketed && !queenPocketed)
        {
            Debug.LogWarning("Foul: No coin touched or pocketed within timeout");
            // Mark as foul but don't apply yet - wait for turn end
        }
    }
    
    /// <summary>
    /// Call this at the end of a turn to check and apply fouls
    /// </summary>
    public void EndTurnAndCheckFouls()
    {
        int currentPlayer = turnManager?.GetCurrentPlayer() ?? 1;
        bool foulDetected = false;
        
        // Foul 1: Striker pocketed
        if (strikerPocketed)
        {
            Debug.Log($"Player {currentPlayer} fouled: Striker pocketed");
            foulDetected = true;
        }
        // Foul 2: No coin pocketed
        else if (!anyCoinPocketed && !queenPocketed && Time.time - lastShotTime > foulTimeout)
        {
            Debug.Log($"Player {currentPlayer} fouled: No coin pocketed");
            foulDetected = true;
        }
        // Foul 3: Queen pocketed without cover
        else if (queenPocketed && !HasQueenCover(currentPlayer))
        {
            Debug.Log($"Player {currentPlayer} fouled: Queen pocketed without cover");
            foulDetected = true;
        }
        
        if (foulDetected)
        {
            ApplyFoulWithCoinReset(currentPlayer);
        }
        else if (queenPocketed)
        {
            // Queen was properly covered
            scoreManager?.RegisterQueenPocketed(currentPlayer);
        }
        
        // Reset for next turn
        pocketedCoinsThisTurn.Clear();
    }
    
    private void ApplyFoulsIfDetected()
    {
        // This is now handled in EndTurnAndCheckFouls()
    }
    
    /// <summary>
    /// Applies foul penalty AND resets all pocketed coins
    /// </summary>
    private void ApplyFoulWithCoinReset(int player)
    {
        // Apply score penalty
        scoreManager?.ApplyFoulPenalty(player);
        
        // Reset ALL coins that were pocketed this turn
        foreach (Coin coin in pocketedCoinsThisTurn)
        {
            if (coin != null && coin.IsPocketed)
            {
                coin.ResetCoin();
                Debug.Log($"Resetting pocketed coin: {coin.Type} due to foul");
            }
        }
        
        // Also reset the queen if it was pocketed
        if (queenPocketed)
        {
            ResetQueenToCenter();
        }
        
        // End the turn
        turnManager?.EndShot();
    }
    
    private bool HasQueenCover(int player)
    {
        // Queen cover rule: Player must pocket one of their own coins IMMEDIATELY after queen
        // Check if any non-queen coins were pocketed this turn
        foreach (Coin coin in pocketedCoinsThisTurn)
        {
            if (coin != null && coin.Type != CoinType.Queen)
            {
                queenCovered = true;
                return true;
            }
        }
        
        return false;
    }
    
    private void ResetQueenToCenter()
    {
        GameObject[] coins = GameObject.FindGameObjectsWithTag("Coin");
        foreach (GameObject coinObj in coins)
        {
            Coin coin = coinObj.GetComponent<Coin>();
            if (coin != null && coin.Type == CoinType.Queen)
            {
                coin.ResetCoin();
                // Position at center of board
                coin.transform.position = Vector3.zero;
                Debug.Log("Queen reset to center due to foul");
                break;
            }
        }
    }
}