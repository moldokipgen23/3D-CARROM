using UnityEngine;
using UnityEngine.UI;

public class ResultsSceneManager : MonoBehaviour
{
    [Header("UI References")]
    public ResultsScreen resultsScreen;
    public Button playAgainButton;
    public Button mainMenuButton;
    
    private void Start()
    {
        InitializeResultsScreen();
        SetupButtonListeners();
    }
    
    private void InitializeResultsScreen()
    {
        Debug.Log("Results scene initializing...");
        
        // Show results with default values
        // In a real game, this would receive data from GameManager
        int winner = 1; // Default to Player 1
        string message = "Player 1 wins!";
        
        if (resultsScreen != null)
        {
            resultsScreen.ShowResults(winner, message);
        }
    }
    
    private void SetupButtonListeners()
    {
        if (playAgainButton != null)
        {
            playAgainButton.onClick.AddListener(OnPlayAgainClicked);
        }
        
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }
    }
    
    private void OnPlayAgainClicked()
    {
        Debug.Log("Play Again button clicked in results");
        ResetGameAndLoadMainMenu();
    }
    
    private void OnMainMenuClicked()
    {
        Debug.Log("Main Menu button clicked in results");
        LoadMainMenu();
    }
    
    private void ResetGameAndLoadMainMenu()
    {
        // Reset game state
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
        
        // Load main menu
        LoadMainMenu();
    }
    
    private void LoadMainMenu()
    {
        SceneFlow.LoadScene(SceneFlow.MAIN_MENU);
    }
}