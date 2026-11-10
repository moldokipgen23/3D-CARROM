using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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

    private bool initialized = false;

    private void Awake()
    {
        ResolveReferences();
        EnsureScreenComponents();
    }

    private void Start()
    {
        if (initialized) return;
        initialized = true;
        StartCoroutine(InitializeGameRoutine());
    }

    private void ResolveReferences()
    {
        if (gameManager == null)
            gameManager = GameManager.Instance;
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>(true);

        if (strikerController == null)
            strikerController = StrikerController.Instance;
        if (strikerController == null)
            strikerController = FindObjectOfType<StrikerController>(true);

        if (coinSpawner == null)
            coinSpawner = FindObjectOfType<CoinSpawner>(true);

        if (turnManager == null)
            turnManager = TurnManager.Instance;
        if (turnManager == null)
            turnManager = FindObjectOfType<TurnManager>(true);

        if (scoreManager == null)
            scoreManager = ScoreManager.Instance;
        if (scoreManager == null)
            scoreManager = FindObjectOfType<ScoreManager>(true);

        if (boardController == null)
            boardController = FindObjectOfType<BoardController>(true);

        if (cameraController == null)
            cameraController = FindObjectOfType<CarromCameraController>(true);

        if (gameHUD == null)
            gameHUD = FindObjectOfType<GameHUD>(true);

        if (resultsScreen == null)
            resultsScreen = FindObjectOfType<ResultsScreen>(true);
    }

    private void EnsureScreenComponents()
    {
        RuntimeUI.EnsureEventSystem();

        if (gameHUD == null)
            gameHUD = gameObject.AddComponent<GameHUD>();

        if (resultsScreen == null)
            resultsScreen = gameObject.AddComponent<ResultsScreen>();
    }

    private IEnumerator InitializeGameRoutine()
    {
        Debug.Log("Game scene initializing...");

        if (gameManager != null)
            gameManager.SetState(GameState.InGame);

        // The striker used to be disabled by TurnManager.Start() because the
        // scene had player1 and player2 pointing at the same object.
        if (strikerController != null && !strikerController.gameObject.activeSelf)
            strikerController.gameObject.SetActive(true);

        // Let BoardSetup build the board and CoinSpawner run its own Start().
        yield return null;

        if (coinSpawner != null)
            coinSpawner.SpawnCoins();

        if (boardController != null)
            boardController.SetupPockets();

        // Wire the rule systems together even if the Inspector is empty.
        if (turnManager != null)
            turnManager.aiControlsPlayer2 = PlayerPrefs.GetInt("carrom_local_multiplayer", 0) == 0;

        if (scoreManager != null)
        {
            if (scoreManager.turnManager == null)
                scoreManager.turnManager = turnManager;
            if (scoreManager.gameHUD == null)
                scoreManager.gameHUD = gameHUD;
        }

        if (gameHUD != null)
        {
            if (gameHUD.turnManager == null)
                gameHUD.turnManager = turnManager;
            if (gameHUD.scoreManager == null)
                gameHUD.scoreManager = scoreManager;
        }

        if (resultsScreen != null && resultsScreen.scoreManager == null)
            resultsScreen.scoreManager = scoreManager;

        // One more fixed step so the coins settle onto the surface before play.
        yield return new WaitForFixedUpdate();

        if (turnManager != null)
            turnManager.StartGame();
        else
            Debug.LogError("GameSceneManager: no TurnManager found - the game cannot start");

        if (gameHUD != null)
            gameHUD.Refresh();

        Debug.Log("Game scene initialized");
    }
}
