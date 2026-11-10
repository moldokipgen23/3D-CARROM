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
    public Text queenScoreText;

    [Header("References")]
    public ScoreManager scoreManager;

    private bool built = false;

    private void Start()
    {
        InitializeReferences();
        EnsureBuilt();
        HideResults();
    }

    private void InitializeReferences()
    {
        if (scoreManager == null)
        {
            scoreManager = ScoreManager.Instance;
            if (scoreManager == null)
                scoreManager = FindObjectOfType<ScoreManager>(true);
        }
    }

    private void EnsureBuilt()
    {
        if (built || resultsPanel != null) return;
        built = true;

        RuntimeUI.EnsureEventSystem();

        Canvas canvas = RuntimeUI.CreateCanvas("ResultsCanvas", 200);

        // Full-screen dimmer.
        Image dimmer = RuntimeUI.CreatePanel(canvas.transform, "Dimmer",
            new Color(0f, 0f, 0f, 0.7f));
        RuntimeUI.Stretch(dimmer.rectTransform, 0f, 0f, 0f, 0f);

        Image panel = RuntimeUI.CreatePanel(canvas.transform, "ResultsPanel",
            new Color(0.10f, 0.08f, 0.06f, 0.98f));
        RuntimeUI.SetRect(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760f, 620f));

        resultsPanel = panel.gameObject;

        Text title = RuntimeUI.CreateText(panel.transform, "Title", "GAME OVER", 54,
            TextAnchor.MiddleCenter, RuntimeUI.AccentGold, FontStyle.Bold);
        RuntimeUI.SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(700f, 70f));

        resultText = RuntimeUI.CreateText(panel.transform, "ResultText", "Player 1 wins!", 38,
            TextAnchor.MiddleCenter, RuntimeUI.TextLight, FontStyle.Bold);
        RuntimeUI.SetRect(resultText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0f, -120f), new Vector2(700f, 56f));

        player1ScoreText = RuntimeUI.CreateText(panel.transform, "P1Score", "You: 0", 30,
            TextAnchor.MiddleCenter, RuntimeUI.TextLight);
        RuntimeUI.SetRect(player1ScoreText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0f, -205f), new Vector2(600f, 40f));

        player2ScoreText = RuntimeUI.CreateText(panel.transform, "P2Score", "AI: 0", 30,
            TextAnchor.MiddleCenter, RuntimeUI.TextLight);
        RuntimeUI.SetRect(player2ScoreText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0f, -250f), new Vector2(600f, 40f));

        queenScoreText = RuntimeUI.CreateText(panel.transform, "QueenScore", "Queen: on board", 26,
            TextAnchor.MiddleCenter, RuntimeUI.AccentGold);
        RuntimeUI.SetRect(queenScoreText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0f, -298f), new Vector2(600f, 40f));

        playAgainButton = RuntimeUI.CreateButton(panel.transform, "PlayAgainButton", "PLAY AGAIN",
            RuntimeUI.ButtonGreen, RuntimeUI.TextLight);
        RuntimeUI.SetRect(playAgainButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 150f),
            new Vector2(520f, 88f));

        mainMenuButton = RuntimeUI.CreateButton(panel.transform, "MainMenuButton", "MAIN MENU",
            RuntimeUI.ButtonBrown, RuntimeUI.TextLight);
        RuntimeUI.SetRect(mainMenuButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 44f),
            new Vector2(520f, 88f));

        playAgainButton.onClick.AddListener(OnPlayAgainClicked);
        mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    /// <summary>Called by ScoreManager when the match ends.</summary>
    public void ShowResults(int winner, string message)
    {
        EnsureBuilt();
        InitializeReferences();

        if (resultsPanel != null)
            resultsPanel.SetActive(true);

        if (resultText != null)
            resultText.text = string.IsNullOrEmpty(message) ? "Player " + winner + " wins!" : message;

        if (scoreManager != null)
        {
            if (player1ScoreText != null)
                player1ScoreText.text = "You (white): " + scoreManager.GetPlayerScore(1);
            if (player2ScoreText != null)
                player2ScoreText.text = "AI (black): " + scoreManager.GetPlayerScore(2);
            if (queenScoreText != null)
                queenScoreText.text = scoreManager.GetQueenStatusText();
        }

        Debug.Log("Results screen shown: " + (resultText != null ? resultText.text : message));
    }

    public void HideResults()
    {
        if (resultsPanel != null)
            resultsPanel.SetActive(false);
    }

    private void OnPlayAgainClicked()
    {
        Time.timeScale = 1f;
        SceneFlow.LoadScene(SceneFlow.GAME);
    }

    private void OnMainMenuClicked()
    {
        Time.timeScale = 1f;
        SceneFlow.LoadScene(SceneFlow.MAIN_MENU);
    }
}
