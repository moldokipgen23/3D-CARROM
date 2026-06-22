using UnityEngine;
using System;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    
    [Header("Player Settings")]
    public int whitePlayerScore = 0;
    public int blackPlayerScore = 0;
    
    [Header("Game Settings")]
    public int coinsToWin = 9;
    
    [Header("References")]
    public TurnManager turnManager;
    public GameHUD gameHUD;
    
    private int[] playerScores = new int[2]; // 0: White (Player 1), 1: Black (Player 2)
    private bool[] queenPocketedByPlayer = new bool[2]; // Track which player has pocketed queen properly
    private bool gameEnded = false;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeScores();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        UpdateHUD();
    }
    
    private void InitializeScores()
    {
        playerScores[0] = 0;
        playerScores[1] = 0;
        queenPocketedByPlayer[0] = false;
        queenPocketedByPlayer[1] = false;
    }
    
    /// <summary>
    /// Add pocketed coins to player score
    /// </summary>
    public void AddCoins(int player, CoinType coinType, int count)
    {
        if (gameEnded) return;
        
        int playerIndex = player - 1;
        if (playerIndex < 0 || playerIndex >= 2) return;
        
        // Only count white/black coins, not queen
        if (coinType == CoinType.White || coinType == CoinType.Black)
        {
            playerScores[playerIndex] += count;
            Debug.Log($"Player {player} pocketed {count} coin(s). Total: {playerScores[playerIndex]}");
        }
        
        UpdateScore(player);
        CheckForWinCondition(player);
    }
    
    /// <summary>
    /// Register that a player has properly pocketed the queen (with cover)
    /// </summary>
    public void RegisterQueenPocketed(int player)
    {
        if (gameEnded) return;
        
        int playerIndex = player - 1;
        if (playerIndex < 0 || playerIndex >= 2) return;
        
        queenPocketedByPlayer[playerIndex] = true;
        Debug.Log($"Player {player} has properly pocketed the queen!");
        UpdateHUD();
    }
    
    /// <summary>
    /// Check if player has queen properly pocketed
    /// </summary>
    public bool HasQueenPocketed(int player)
    {
        int playerIndex = player - 1;
        if (playerIndex < 0 || playerIndex >= 2) return false;
        return queenPocketedByPlayer[playerIndex];
    }
    
    public void ApplyFoulPenalty(int player)
    {
        if (gameEnded) return;
        
        int playerIndex = player - 1;
        if (playerIndex < 0 || playerIndex >= 2) return;
        
        int coinsToReturn = 1;
        
        if (playerScores[playerIndex] >= coinsToReturn)
        {
            playerScores[playerIndex] -= coinsToReturn;
            Debug.Log($"Player {player} fouled - returned {coinsToReturn} coin(s)");
        }
        else
        {
            playerScores[playerIndex] = 0;
            Debug.Log($"Player {player} fouled - all coins returned");
        }
        
        UpdateScore(player);
    }
    
    public void ResetScores()
    {
        playerScores[0] = 0;
        playerScores[1] = 0;
        queenPocketedByPlayer[0] = false;
        queenPocketedByPlayer[1] = false;
        gameEnded = false;
        Debug.Log("All scores reset");
        UpdateHUD();
    }
    
    private void UpdateScore(int player)
    {
        UpdateHUD();
        OnScoreUpdated?.Invoke(player, playerScores[player - 1]);
    }
    
    private void CheckForWinCondition(int player)
    {
        int playerIndex = player - 1;
        if (playerIndex < 0 || playerIndex >= 2) return;
        
        int coinsPocketed = playerScores[playerIndex];
        bool hasQueen = queenPocketedByPlayer[playerIndex];
        
        // Win condition: Must have all 9 coins AND have properly pocketed the queen
        if (coinsPocketed >= coinsToWin && hasQueen)
        {
            EndGame(player, $"Player {player} wins!");
        }
        else if (coinsPocketed >= coinsToWin && !hasQueen)
        {
            Debug.Log($"Player {player} has all coins but needs to pocket queen properly!");
        }
    }
    
    private void EndGame(int winner, string message)
    {
        gameEnded = true;
        Debug.Log($"Game ended: {message}");
        
        ResultsScreen resultsScreen = FindObjectOfType<ResultsScreen>();
        if (resultsScreen != null)
        {
            resultsScreen.ShowResults(winner, message);
        }
        else
        {
            Debug.Log($"GAME OVER - {message}");
        }
    }
    
    public int GetPlayerScore(int player)
    {
        if (player < 1 || player > 2) return 0;
        return playerScores[player - 1];
    }
    
    public event Action<int, int> OnScoreUpdated;

    private void UpdateHUD()
    {
        if (gameHUD != null)
        {
            gameHUD.UpdateHUD();
        }
    }
}