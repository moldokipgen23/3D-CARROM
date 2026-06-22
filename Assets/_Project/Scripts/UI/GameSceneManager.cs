using UnityEngine;
using UnityEngine.UI;

public class GameSceneManager : MonoBehaviour
{
    [Header("Game References")]
    public GameManager gameManager;
    public StrikerController strikerController;
    public CoinSpawner coinSpawner;
    public TurnManager turnManager;
    public ScoreManager scoreManager;
    public BoardController boardController;
    public CarromCameraController cameraController;
    public GameHUD gameHUD;
    public ResultsScreen resultsScreen;
    
    [Header("UI References")]
    public Button pauseButton;
    public GameObject pauseMenu;
    
    private void Start()
    {
        InitializeGame();
        SetupUI();
    }
    
    private void InitializeGame()
    {
        Debug.Log("Game scene initializing...");
        
        // Initialize GameManager
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }
        if (gameManager != null)
            gameManager.SetState(GameState.InGame);
        
        // Initialize striker
        if (strikerController != null)
        {
            strikerController.gameObject.SetActive(true);
        }
        
        // Initialize coins
        if (coinSpawner != null)
        {
            coinSpawner.gameObject.SetActive(true);
            coinSpawner.SpawnCoins();
        }
        
        // Initialize turn manager
        if (turnManager != null)
        {
            turnManager.gameObject.SetActive(true);
        }
        
        // Initialize board
        if (boardController != null)
        {
            boardController.gameObject.SetActive(true);
        }
        
        // Initialize camera
        if (cameraController != null)
        {
            cameraController.gameObject.SetActive(true);
        }
        
        // Initialize UI
        if (gameHUD != null)
        {
            gameHUD.gameObject.SetActive(true);
        }
        
        // Setup event listeners
        SetupEventListeners();

        // Start the game AFTER everything is initialized so AI doesn't fire before coins spawn
        if (turnManager != null)
            turnManager.StartGame();
    }
    
    private void SetupUI()
    {
        if (pauseButton != null)
            pauseButton.onClick.AddListener(OnPauseClicked);

        if (pauseMenu != null)
            pauseMenu.SetActive(false);
    }
    
    private void SetupEventListeners()
    {
        if (turnManager != null)
        {
            turnManager.OnTurnChanged += OnTurnChanged;
            turnManager.OnScoreUpdated += OnScoreUpdated;
        }
        
        if (boardController != null)
        {
            boardController.CoinPocketed += OnCoinPocketed;
        }
        
        if (strikerController != null)
        {
            strikerController.OnShotFired += OnShotFired;
        }
    }
    
    private void OnTurnChanged(int player)
    {
        Debug.Log($"Turn changed to Player {player}");
        if (gameHUD != null)
        {
            gameHUD.UpdateHUD();
        }
    }
    
    private void OnScoreUpdated(int player, int score)
    {
        Debug.Log($"Score updated for Player {player}: {score}");
        if (gameHUD != null)
        {
            gameHUD.UpdateHUD();
        }
    }
    
    private void OnCoinPocketed(CoinType coinType)
    {
        Debug.Log($"Coin {coinType} pocketed in game");
        if (scoreManager != null)
        {
            int currentPlayer = turnManager.GetCurrentPlayer();
            scoreManager.AddCoins(currentPlayer, (int)coinType, 1);
        }
    }
    
    private void OnShotFired(Vector2 direction, float power)
    {
        Debug.Log($"Shot fired - Direction: {direction}, Power: {power:F1}");
        if (turnManager != null)
        {
            turnManager.StartShot();
        }
    }
    
    private void OnPauseClicked()
    {
        pauseMenu.SetActive(!pauseMenu.activeSelf);
    }
    
    private void OnDestroy()
    {
        // Cleanup event listeners
        if (turnManager != null)
        {
            turnManager.OnTurnChanged -= OnTurnChanged;
            turnManager.OnScoreUpdated -= OnScoreUpdated;
        }
        
        if (boardController != null)
        {
            boardController.CoinPocketed -= OnCoinPocketed;
        }
        
        if (strikerController != null)
        {
            strikerController.OnShotFired -= OnShotFired;
        }
    }
}