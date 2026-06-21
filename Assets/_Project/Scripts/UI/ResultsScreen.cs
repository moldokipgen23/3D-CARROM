using UnityEngine;
using UnityEngine.UI;

public class ResultsScreen : MonoBehaviour
{
    [Header("UI References")]
    public GameObject resultsPanel;
    public Text resultText;
    public Button playAgainButton;
    public Button mainMenuButton;
    
    [Header("Score Display")]
    public Text player1ScoreText;
    public Text player2ScoreText;
    public Text player3ScoreText;
    
    [Header("References")]
    public ScoreManager scoreManager;
    
    private void Start()
    {
        InitializeReferences();
        HideResults();
        
        if (playAgainButton != null)
        {
            playAgainButton.onClick.AddListener(OnPlayAgainClicked);
        }
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }
    }
    
    private void InitializeReferences()
    {
        if (scoreManager == null)
        {
            scoreManager = FindObjectOfType<ScoreManager>();
        }
    }
    
    public void ShowResults(int winner, string message)
    {
        if (resultsPanel != null)
        {
            resultsPanel.SetActive(true);
        }
        
        if (resultText != null)
        {
            resultText.text = message;
        }
        
        UpdateScoreDisplay();
        
        Debug.Log($"Results screen shown: {message}");
    }
    
    private void HideResults()
    {
        if (resultsPanel != null)
        {
            resultsPanel.SetActive(false);
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
    
    private void OnPlayAgainClicked()
    {
        Debug.Log("Play Again button clicked");
        HideResults();
        ResetGame();
    }
    
    private void OnMainMenuClicked()
    {
        Debug.Log("Main Menu button clicked");
        HideResults();
        LoadMainMenu();
    }
    
    private void ResetGame()
    {
        ScoreManager.Instance?.ResetScores();
        CoinSpawner coinSpawner = FindObjectOfType<CoinSpawner>();
        if (coinSpawner != null)
        {
            coinSpawner.ResetCoins();
        }
        TurnManager turnManager = FindObjectOfType<TurnManager>();
        if (turnManager != null)
        {
            turnManager.SwitchTurn();
        }
    }
    
    private void LoadMainMenu()
    {
        SceneFlow.LoadScene(SceneFlow.MAIN_MENU);
    }
}