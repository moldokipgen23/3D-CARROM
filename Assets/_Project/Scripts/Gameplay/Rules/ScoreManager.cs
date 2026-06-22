using UnityEngine;
using System;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    
    [Header("Player Settings")]
    public int whitePlayerScore = 0;
    public int blackPlayerScore = 0;
    public int queenPlayerScore = 0;
    
    [Header("Game Settings")]
    public int whiteCoinsToWin = 9;
    public int blackCoinsToWin = 9;
    public int queenToWin = 1;
    
    [Header("References")]
    public TurnManager turnManager;
    public GameHUD gameHUD;
    
    private int[] playerScores = new int[3]; // 0: White, 1: Black, 2: Queen
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
        playerScores[0] = whitePlayerScore;
        playerScores[1] = blackPlayerScore;
        playerScores[2] = queenPlayerScore;
    }
    
    public void AddCoins(int player, int coinType, int count)
    {
        if (gameEnded) return;
        
        switch (coinType)
        {
            case 0: // White
                playerScores[0] += count;
                break;
            case 1: // Black
                playerScores[1] += count;
                break;
            case 2: // Queen
                playerScores[2] += count;
                break;
        }
        
        UpdateScore(player);
        CheckForWinCondition(player);
    }
    
    public void ApplyFoulPenalty(int player)
    {
        if (gameEnded) return;
        
        int coinsToReturn = 1;
        int playerIndex = player - 1;
        
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
        playerScores[0] = whitePlayerScore;
        playerScores[1] = blackPlayerScore;
        playerScores[2] = queenPlayerScore;
        gameEnded = false;
        Debug.Log("All scores reset");
    }
    
    private void UpdateScore(int player)
    {
        UpdateHUD();
        OnScoreUpdated?.Invoke(player, playerScores[player - 1]);
    }
    
    private void CheckForWinCondition(int player)
    {
        // playerScores[0]=white coins pocketed, [1]=black coins pocketed, [2]=queen pocketed
        // Player 1 wins by pocketing all white coins; Player 2 wins by pocketing all black coins
        // Queen must also be pocketed (with cover) for the win to count
        bool queenPocketed = playerScores[2] >= queenToWin;

        if (playerScores[0] >= whiteCoinsToWin && queenPocketed)
        {
            EndGame(1, "Player 1 Wins!");
        }
        else if (playerScores[1] >= blackCoinsToWin && queenPocketed)
        {
            EndGame(2, "Player 2 Wins!");
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
    }
    
    public int GetPlayerScore(int player)
    {
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