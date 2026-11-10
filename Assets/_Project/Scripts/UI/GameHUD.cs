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
    public Color player1TurnColor = new Color(0.96f, 0.94f, 0.88f);
    public Color player2TurnColor = new Color(0.35f, 0.35f, 0.35f);

    [Header("Coin Count Display")]
    public Text remainingCoinsText;

    [Header("References")]
    public TurnManager turnManager;
    public ScoreManager scoreManager;

    private Canvas canvas;
    private Text player1Label;
    private Text player2Label;
    private Text hintText;

    private void Start()
    {
        InitializeReferences();
        BuildUI();
        Subscribe();
        Refresh();
    }

    private void OnDestroy()
    {
        if (scoreManager != null)
            scoreManager.OnScoreUpdated -= HandleScoreUpdated;
        if (turnManager != null)
            turnManager.OnTurnChanged -= HandleTurnChanged;
    }

    private void InitializeReferences()
    {
        if (turnManager == null)
        {
            turnManager = TurnManager.Instance;
            if (turnManager == null)
                turnManager = FindObjectOfType<TurnManager>(true);
        }

        if (scoreManager == null)
        {
            scoreManager = ScoreManager.Instance;
            if (scoreManager == null)
                scoreManager = FindObjectOfType<ScoreManager>(true);
        }
    }

    private void Subscribe()
    {
        if (scoreManager != null)
            scoreManager.OnScoreUpdated += HandleScoreUpdated;

        if (turnManager != null)
            turnManager.OnTurnChanged += HandleTurnChanged;
    }

    private void HandleScoreUpdated(int player, int score)
    {
        Refresh();
    }

    private void HandleTurnChanged(int player)
    {
        Refresh();
    }

    private void BuildUI()
    {
        if (player1ScoreText != null && player2ScoreText != null)
        {
            // UI was wired in the Inspector - leave it alone.
            return;
        }

        RuntimeUI.EnsureEventSystem();

        canvas = RuntimeUI.CreateCanvas("GameHUDCanvas", 100);
        Transform root = canvas.transform;

        // Top bar background.
        Image topBar = RuntimeUI.CreatePanel(root, "TopBar", RuntimeUI.PanelDark);
        RuntimeUI.SetRect(topBar.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0f, 150f));

        // Player 1 (human / white).
        player1Label = RuntimeUI.CreateText(root, "Player1Label", "YOU  (WHITE)", 24,
            TextAnchor.MiddleLeft, RuntimeUI.TextLight, FontStyle.Bold);
        RuntimeUI.SetRect(player1Label.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(0f, 1f), new Vector2(30f, -18f), new Vector2(360f, 30f));

        player1ScoreText = RuntimeUI.CreateText(root, "Player1Score", "0", 52,
            TextAnchor.MiddleLeft, RuntimeUI.AccentGold, FontStyle.Bold);
        RuntimeUI.SetRect(player1ScoreText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(0f, 1f), new Vector2(28f, -48f), new Vector2(220f, 60f));

        player1UI = player1Label.gameObject;

        // Player 2 (AI / black).
        player2Label = RuntimeUI.CreateText(root, "Player2Label", "AI  (BLACK)", 24,
            TextAnchor.MiddleRight, RuntimeUI.TextLight, FontStyle.Bold);
        RuntimeUI.SetRect(player2Label.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(1f, 1f), new Vector2(-30f, -18f), new Vector2(360f, 30f));

        player2ScoreText = RuntimeUI.CreateText(root, "Player2Score", "0", 52,
            TextAnchor.MiddleRight, RuntimeUI.AccentGold, FontStyle.Bold);
        RuntimeUI.SetRect(player2ScoreText.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(1f, 1f), new Vector2(-28f, -48f), new Vector2(220f, 60f));

        player2UI = player2Label.gameObject;

        // Centre turn text.
        currentTurnText = RuntimeUI.CreateText(root, "TurnText", "Your turn", 30,
            TextAnchor.MiddleCenter, RuntimeUI.TextLight, FontStyle.Bold);
        RuntimeUI.SetRect(currentTurnText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0f, -22f), new Vector2(700f, 40f));

        // Thin coloured bar showing whose turn it is.
        turnIndicator = RuntimeUI.CreatePanel(root, "TurnIndicator", player1TurnColor);
        RuntimeUI.SetRect(turnIndicator.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0f, -66f), new Vector2(280f, 8f));

        // Queen status (reuses the legacy player3 field).
        player3ScoreText = RuntimeUI.CreateText(root, "QueenText", "Queen: on board", 22,
            TextAnchor.MiddleCenter, RuntimeUI.AccentGold, FontStyle.Normal);
        RuntimeUI.SetRect(player3ScoreText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0f, -88f), new Vector2(700f, 28f));

        // Bottom bar.
        Image bottomBar = RuntimeUI.CreatePanel(root, "BottomBar", RuntimeUI.PanelDark);
        RuntimeUI.SetRect(bottomBar.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f),
            new Vector2(0.5f, 0f), Vector2.zero, new Vector2(0f, 120f));

        remainingCoinsText = RuntimeUI.CreateText(root, "RemainingCoins", "Coins: 18", 26,
            TextAnchor.MiddleCenter, RuntimeUI.TextLight, FontStyle.Bold);
        RuntimeUI.SetRect(remainingCoinsText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f), new Vector2(0f, 62f), new Vector2(900f, 34f));

        hintText = RuntimeUI.CreateText(root, "Hint", "Drag from the striker toward your target, then release", 20,
            TextAnchor.MiddleCenter, new Color(0.8f, 0.78f, 0.72f), FontStyle.Normal);
        RuntimeUI.SetRect(hintText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f), new Vector2(0f, 28f), new Vector2(1000f, 28f));
    }

    public void Refresh()
    {
        if (player1ScoreText == null || player2ScoreText == null || currentTurnText == null)
            return;

        int p1Score = scoreManager != null ? scoreManager.GetPlayerScore(1) : 0;
        int p2Score = scoreManager != null ? scoreManager.GetPlayerScore(2) : 0;

        player1ScoreText.text = p1Score.ToString();
        player2ScoreText.text = p2Score.ToString();

        int current = turnManager != null ? turnManager.GetCurrentPlayer() : 1;

        if (scoreManager != null)
        {
            if (remainingCoinsText != null)
                remainingCoinsText.text = "Coins left: " + scoreManager.GetRemainingCoins();

            if (player3ScoreText != null)
                player3ScoreText.text = scoreManager.GetQueenStatusText();
        }

        bool aiTurn = turnManager != null && turnManager.aiControlsPlayer2 && current == 2;
        bool localMode = turnManager != null && !turnManager.aiControlsPlayer2;

        if (player2Label != null)
            player2Label.text = localMode ? "PLAYER 2  (BLACK)" : "AI  (BLACK)";

        if (scoreManager != null && scoreManager.IsGameEnded)
        {
            currentTurnText.text = "Game Over";
        }
        else if (aiTurn)
        {
            currentTurnText.text = "AI is thinking...";
        }
        else if (localMode)
        {
            currentTurnText.text = current == 1 ? "Player 1's turn" : "Player 2's turn";
        }
        else
        {
            currentTurnText.text = "Your turn";
        }

        if (turnIndicator != null)
            turnIndicator.color = current == 1 ? player1TurnColor : player2TurnColor;

        if (hintText != null)
            hintText.gameObject.SetActive(!aiTurn);
    }
}
