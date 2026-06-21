using UnityEngine;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    [Header("Player References")]
    public GameObject player1UI;
    public GameObject player2UI;
    
    [Header("Score Display")]
    public Text player1ScoreText;
    public Text player2ScoreText;
    public Text player3ScoreText;
    
    [Header("Turn Display")]
    public Text currentTurnText;
    public Image turnIndicator;
    public Color player1TurnColor = Color.white;
    public Color player2TurnColor = Color.gray;
    
    [Header("Coin Count Display")]
    public Text remainingCoinsText;
    
    [Header("References")]
    public TurnManager turnManager;
    public ScoreManager scoreManager;
    
    private void Start()
    {
        InitializeReferences();
        UpdateHUD();
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
    }
    
    private void Update()
    {
        UpdateHUD();
    }
    
    public void UpdateHUD()
    {
        UpdateTurnDisplay();
        UpdateScoreDisplay();
        UpdateCoinCountDisplay();
    }
    
    private void UpdateTurnDisplay()
    {
        if (turnManager == null) return;
        
        int currentPlayer = turnManager.GetCurrentPlayer();
        string turnText = $"Player {currentPlayer}'s Turn";
        
        if (currentTurnText != null)
        {
            currentTurnText.text = turnText;
        }
        
        if (turnIndicator != null)
        {
            turnIndicator.color = (currentPlayer == 1) ? player1TurnColor : player2TurnColor;
        }
        
        UpdatePlayerUIActiveState(currentPlayer);
    }
    
    private void UpdatePlayerUIActiveState(int currentPlayer)
    {
        if (player1UI != null)
        {
            player1UI.SetActive(currentPlayer == 1);
        }
        if (player2UI != null)
        {
            player2UI.SetActive(currentPlayer == 2);
        }
    }
    
    private void UpdateScoreDisplay()
    {
        if (scoreManager == null) return;
        
        if (player1ScoreText != null)
        {
            player1ScoreText.text = $"White: {scoreManager.GetPlayerScore(1)}";
        }
        if (player2ScoreText != null)
        {
            player2ScoreText.text = $"Black: {scoreManager.GetPlayerScore(2)}";
        }
        if (player3ScoreText != null)
        {
            player3ScoreText.text = $"Queen: {scoreManager.GetPlayerScore(3)}";
        }
    }
    
    private void UpdateCoinCountDisplay()
    {
        if (remainingCoinsText != null && scoreManager != null)
        {
            int totalCoins = scoreManager.GetPlayerScore(1) + scoreManager.GetPlayerScore(2) + scoreManager.GetPlayerScore(3);
            remainingCoinsText.text = $"Remaining: {totalCoins}";
        }
    }
}